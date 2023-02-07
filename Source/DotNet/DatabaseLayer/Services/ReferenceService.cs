namespace DatabaseLayer.Services;

using Dapper;
using DatabaseModel.Models;
using Microsoft.OData.UriParser;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Npgsql;
using Utility;
using Utility.Models;

public class ReferenceService
{
	private readonly NpgsqlConnection _connection;
	private readonly Mapper           _mapper;
	private readonly Sorter           _sorter;
	private const    int              TakeDefault = 100;
	private const    int              TakeMax     = 1000;
	private const    string           SelectPart  = @"select count(*) over(), id, reference_type, organisation, code, uri, ST_AsText(geometry) AS geometry, display, description, external_key, taxon_rank, taxon_author, taxon_parent_author, cas_number, parameter_type, taxon_name_nl, taxon_status_code, taxon_parent_external_key, taxon_type_external_key, taxon_group_external_key from reference ";

	public ReferenceService(NpgsqlConnection connection)
	{
		_connection = connection;
		var mappings = new List<FieldMap?>
		{
			new("Id", "external_key", MapTypeEnum.Value, false),
			new("Type", "reference_type", MapTypeEnum.Value, false),
			new("Code", "code", MapTypeEnum.Value, false),
			new("Organisation", "organisation", MapTypeEnum.Value, false),
			new("Geometry", "geometry", MapTypeEnum.Value, false),
			new("TaxonType", "taxon_type_external_key", MapTypeEnum.Reference, false),
			new("TaxonGroup", "taxon_group_external_key", MapTypeEnum.Reference, false),
			new("TaxonStatusCode", "taxon_status_code", MapTypeEnum.Value, false),
			new("ParameterType", "parameter_type", MapTypeEnum.Value, false),
			new("Description", "description", MapTypeEnum.Value, false),
			new("TaxonRank", "taxon_rank", MapTypeEnum.Value, false),
			new("TaxonAuthor", "taxon_author", MapTypeEnum.Value, false),
			new("CasNumber", "cas_number", MapTypeEnum.Value, false),
			new("TaxonNameNl", "taxon_name_nl", MapTypeEnum.Value, false),
		}.ToDictionary(a => a?.FieldName ?? string.Empty, a => a);
		// Initialize the ODataToSqlMapper
		_mapper = new Mapper(mappings);
		_sorter = new Sorter(mappings);
	}

	public async Task<Dictionary<Guid, Reference>> GetReferencesAsync()
	{
		var dynamicReferences = await _connection.QueryAsync<dynamic>(SelectPart);
		var references = dynamicReferences.Select(Reference.FromDynamic).ToDictionary(item => item.ExternalKey, item => item);
		return references;
	}
	
	public async Task<Dictionary<long, Reference>> GetReferencesByIdAsync()
	{
		var dynamicReferences = await _connection.QueryAsync<dynamic>(SelectPart);
		var references        = dynamicReferences.Select(Reference.FromDynamic).ToDictionary(item => item.Id, item => item);
		return references;
	}
	
	public async Task<Dictionary<Guid, long>> GetIdsAsync()
	{
		return (await _connection.QueryAsync<(Guid, long)>("SELECT external_key, id FROM reference").ConfigureAwait(false)).ToDictionary(item => item.Item1, item => item.Item2);
	}
	
	public async Task<Dictionary<Guid, Geometry>> GetGeometriesAsync()
	{
		var reader = new WKTReader();
		var dynamicReferences = await _connection.QueryAsync<(Guid ExternalKey, string Geometry)>("SELECT external_key, ST_AsText(geometry) AS geometry FROM reference WHERE reference_type = 'measurementobject'").ConfigureAwait(false);
		var references        = dynamicReferences.ToDictionary(item => item.ExternalKey, item => reader.Read(item.Geometry));
		return references;
	}
	
	public async Task SyncReferencesAsync(Dictionary<Guid, Reference> references, Dictionary<Guid, Reference> items)
	{
		foreach (var item in items)
		{
			if (references.TryGetValue(item.Key, out var value))
			{
				item.Value.Id = value.Id;
				await _connection.ExecuteAsync(@"UPDATE reference SET code = @Code, description = @Description, organisation = @Organisation, reference_type = @ReferenceType, taxon_rank = @TaxonRank, taxon_author = @TaxonAuthor, taxon_parent_author = @TaxonParentAuthor, cas_number = @CasNumber, parameter_type = @ParameterType, taxon_name_nl = @TaxonNameNL, taxon_status_code = @TaxonStatusCode, taxon_parent_external_key = @TaxonParentExternalKey, taxon_type_external_key = @TaxonTypeExternalKey, taxon_group_external_key = @TaxonGroupExternalKey WHERE external_key = @ExternalKey", new { item.Value.Code, item.Value.Description, item.Value.Organisation, item.Value.ReferenceType, item.Value.ExternalKey, item.Value.TaxonRank, item.Value.TaxonAuthor, item.Value.TaxonParentAuthor, item.Value.CasNumber, item.Value.ParameterType, item.Value.TaxonNameNL, item.Value.TaxonStatusCode, item.Value.TaxonParentExternalKey, item.Value.TaxonTypeExternalKey, item.Value.TaxonGroupExternalKey }).ConfigureAwait(false);
			}
			else
			{
				await _connection.ExecuteAsync(@"INSERT INTO reference (code, description, organisation, reference_type, external_key, taxon_rank, taxon_author, taxon_parent_author, cas_number, parameter_type, taxon_name_nl, taxon_status_code, taxon_parent_external_key, taxon_type_external_key, taxon_group_external_key) 
																												VALUES (@Description, @Description, @Organisation, @ReferenceType, @ExternalKey, @TaxonRank, @TaxonAuthor, @TaxonParentAuthor, @CasNumber, @ParameterType, @TaxonNameNL, @TaxonStatusCode, @TaxonParentExternalKey, @TaxonTypeExternalKey, @TaxonGroupExternalKey)", new { item.Value.Code, item.Value.Description, item.Value.Organisation, item.Value.ReferenceType, item.Value.ExternalKey, item.Value.TaxonRank, item.Value.TaxonAuthor, item.Value.TaxonParentAuthor, item.Value.CasNumber, item.Value.ParameterType, item.Value.TaxonNameNL, item.Value.TaxonStatusCode, item.Value.TaxonParentExternalKey, item.Value.TaxonTypeExternalKey, item.Value.TaxonGroupExternalKey }).ConfigureAwait(false);
			}
			if (item.Value.Geometry != null)
			{
				var geo = new WKTReader().Read(item.Value.Geometry);
				geo.SRID = 4326;
				await _connection.ExecuteAsync("UPDATE reference SET geometry = @Geometry WHERE external_key = @ExternalKey", new { Geometry = geo.AsBinary(), item.Value.ExternalKey }).ConfigureAwait(false);
			}
			else
			{
				await _connection.ExecuteAsync("UPDATE reference SET geometry = null WHERE external_key = @ExternalKey", new { item.Value.ExternalKey }).ConfigureAwait(false);
			}
		}
	}

	public async Task<List<Reference>> GetParametersAsync()
	{
		var parameters = (await _connection.QueryAsync<dynamic>("SELECT id, code, external_key, reference_type FROM reference WHERE reference_type NOT IN ('quantity','measurementobject','analysismethod')").ConfigureAwait(false)).Select(Reference.FromDynamic).ToList();
		return parameters;
	}
		
	public async Task<List<Reference>> GetObservingProceduresAsync()
	{
		var parameters = (await _connection.QueryAsync<dynamic>("SELECT id, code, external_key, reference_type FROM reference WHERE reference_type = 'analysismethod'").ConfigureAwait(false)).Select(Reference.FromDynamic).ToList();
		return parameters;
	}
		
	public async Task<List<Reference>> GetQuantitiesProceduresAsync()
	{
		var parameters = (await _connection.QueryAsync<dynamic>("SELECT id, code, external_key, reference_type FROM reference WHERE reference_type = 'quantity'").ConfigureAwait(false)).Select(Reference.FromDynamic).ToList();
		return parameters;
	}

	public async Task<List<Reference>> GetFoiAsync()
	{
		var parameters = (await _connection.QueryAsync<dynamic>("SELECT id, code, external_key, reference_type FROM reference WHERE reference_type = 'measurementobject'").ConfigureAwait(false)).Select(Reference.FromDynamic).ToList();
		return parameters;
	}
		
	public async Task<List<Reference>> GetOrganisationsAsync()
	{
		var parameters = (await _connection.QueryAsync<dynamic>("SELECT id, code, external_key, reference_type FROM reference WHERE reference_type = 'organisation'").ConfigureAwait(false)).Select(Reference.FromDynamic).ToList();
		return parameters;
	}
	
	public async Task<List<string>> GetReferenceTypesForParametersAsync()
	{
		var parameters = (await _connection.QueryAsync<string>("SELECT DISTINCT CONCAT('parameter/', LOWER(REPLACE(reference_type,  ' ', '_'))) FROM reference WHERE reference_type NOT IN ('quantity','measurementobject','analysismethod')").ConfigureAwait(false)).OrderBy(a => a).ToList();
		return parameters;
	}
	
	public async Task<List<Reference>> GetDataAsync(FilterClause? filterClause, OrderByClause? orderByClause, int? skip = 0, int? take = TakeDefault)
	{
		take = take > TakeMax ? TakeMax : take;
		var whereStatement = filterClause == null ? " 1 = 1 " : _mapper.TranslateFilterClause(filterClause); 
		// Translate order by clause, using the mapper.
		var query          = $"SELECT count(*) over() as count, id, reference_type, organisation, code, uri, ST_AsText(geometry) as geometry, display, description, external_key, taxon_rank, taxon_author, taxon_parent_author, cas_number, parameter_type, taxon_name_nl, taxon_status_code, taxon_parent_external_key, taxon_type_external_key, taxon_group_external_key FROM reference where {whereStatement} limit {take ?? TakeDefault } offset {skip ?? 0} ";
		var data           = (await _connection.QueryAsync<dynamic>(query).ConfigureAwait(false)).Select(Reference.FromDynamic).ToList();
		return data;
	}
}
