namespace DatabaseLayer.Services;

using System.Globalization;
using System.Text.Json;
using Microsoft.OData.UriParser;
using Dapper;
using DatabaseModel.Models;
using NetTopologySuite.IO;
using Npgsql;
using Utility;
using Utility.Models;

public class ObservationService
{
	private readonly NpgsqlConnection _connection;
	private const    string           Delimiter    = "\t";
	private readonly CultureInfo      _cultureInfo = new("en-US");
	private readonly Mapper           _mapper;
	private readonly Sorter           _sorter;
	private const    string           Null        = "\\N";
	private const    int              TakeDefault = 10000;
	private const    int              TakeMax     = 100000;

	public ObservationService(NpgsqlConnection connection)
	{
		_connection = connection;
		var mappings = new List<FieldMap?>
		{
			new("Id", "external_key", MapTypeEnum.Value, false),
			new("Type", "observation_type", MapTypeEnum.Value, false),
			new("ResultTime", "observation_type", MapTypeEnum.Value, false),
			new("PhenomenonTime", "observation_type", MapTypeEnum.Value, false),
			new("ValidTime", "observation_type", MapTypeEnum.Value, false),
			new("Foi/Code", "foi_id", MapTypeEnum.Reference, true, "foi_id in (select id from reference where code"),
			new("Foi/Geometry", "foi_id", MapTypeEnum.Reference, false, "foi_id in (select id from reference where geometry"),
			new("Foi/Description", "foi_id", MapTypeEnum.Reference, false, "foi_id in (select id from reference where description"),
			new("Foi/Organisation", "foi_id", MapTypeEnum.Reference, false, "foi_id in (select id from reference where organisation"),
			new("ObservedProperty/Code", "observed_property_id", MapTypeEnum.Reference, false, "observed_property_id in (select id from reference where code"),
			new("ObservedProperty/Type", "observed_property_id", MapTypeEnum.Reference, false, "observed_property_id in (select id from reference where type"),
			new("ObservedProperty/Description", "observed_property_id", MapTypeEnum.Reference, false, "observed_property_id in (select id from reference where description"),
			new("ObservedProperty/Organisation", "observed_property_id", MapTypeEnum.Reference, false, "observed_property_id in (select id from reference where organisation"),
			new("Host/Code", "host_id", MapTypeEnum.Reference, false, "host_id in (select id from reference where code"),
			new("Host/Type", "host_id", MapTypeEnum.Reference, false, "host_id in (select id from reference where type"),
			new("Host/Description", "host_id", MapTypeEnum.Reference, false, "host_id in (select id from reference where description"),
			new("Host/Organisation", "host_id", MapTypeEnum.Reference, false, "host_id in (select id from reference where organisation"),
			new("Observer/Code", "observer_id", MapTypeEnum.Reference, false, "observer_id in (select id from reference where code"),
			new("Observer/Type", "observer_id", MapTypeEnum.Reference, false, "observer_id in (select id from reference where type"),
			new("Observer/Description", "observer_id", MapTypeEnum.Reference, false, "observer_id in (select id from reference where description"),
			new("Observer/Organisation", "observer_id", MapTypeEnum.Reference, false, "observer_id in (select id from reference where organisation"),
			new("Result/Truth", "result_truth", MapTypeEnum.Value, false),
			new("Result/Count", "result_count", MapTypeEnum.Value, false),
			new("Result/Measure/Uom/Code", "result_uom_id", MapTypeEnum.Reference, true, "result_uom_id in (select id from reference where code"),
			new("Result/Measure/Uom/Value", "result_uom_id", MapTypeEnum.Reference, true, "result_uom_id in (select id from reference where code"),
			new("Parameter/type", "id", MapTypeEnum.Reference, true, "cast(parameter->>'taxontype' as bigint) in (select id from reference where parameter_type"),
			new("Parameter/taxontype", "id", MapTypeEnum.Reference, true, "cast(parameter->>'taxontype' as bigint) in (select id from reference where taxon_type"),
			new("Parameter/taxongroup", "id", MapTypeEnum.Reference, true, "cast(parameter->>'taxongroup' as bigint) in (select id from reference where taxon_group"),
			new("Parameter/organisation", "id", MapTypeEnum.Reference, true, "cast(parameter->>'organisation' as bigint) in (select id from reference where organisation"),
			new("Parameter/analysemethode", "id", MapTypeEnum.Reference, true, "cast(parameter->>'Analyse Methode' as bigint) in (select id from reference where reference_type = 'method' and code"),
			new("Parameter/monstermethode", "id", MapTypeEnum.Reference, true, "cast(parameter->>'Monster Methode' as bigint) in (select id from reference where reference_type = 'method' and code"),
			new("Parameter/levensvorm", "id", MapTypeEnum.Reference, true, "cast(parameter->>'Levensvorm' as bigint) in (select id from reference where reference_type = 'Levensvorm' and code"),
			new("Parameter/geslacht", "id", MapTypeEnum.Reference, true, "cast(parameter->>'Geslacht' as bigint) in (select id from reference where reference_type = 'Geslacht' and code"),
			new("Parameter/verschijningsvorm", "id", MapTypeEnum.Reference, true, "cast(parameter->>'Verschijningsvorm' as bigint) in (select id from reference where reference_type = 'Verschijningsvorm' and code"),
			new("Parameter/compartment", "id", MapTypeEnum.Reference, true, "cast(parameter->>'compartment' as bigint) in (select id from reference where reference_type = 'compartment' and code"),
		}.ToDictionary(a => a?.FieldName ?? string.Empty, a => a);
		// Initialize the ODataToSqlMapper
		_mapper = new Mapper(mappings);
		_sorter = new Sorter(mappings);
	}

	public async Task<Observation> GetAsync(long id)
	{
		const string sqlGetObservation           = "SELECT * FROM observation WHERE id = @id";
		var          observation                 = Observation.FromDynamic(await _connection.QueryFirstOrDefaultAsync<dynamic>(sqlGetObservation, new { id }).ConfigureAwait(false));
		return observation;
	}

	public async Task RemoveAsync(Guid[] guids)
	{
		const int batchSize = 1000;
		var skip = 0;
		do
		{
			var batch = guids.Skip(skip).Take(batchSize).ToArray();
			if (batch.Length == 0)
			{
				break;
			}
			
			await _connection.ExecuteAsync("DELETE FROM observation WHERE id in @guids ", new { guids }).ConfigureAwait(false);
			skip += batchSize;
		} while (skip < guids.Length);
	}
	
	private string ValueOrNull(double? value)
	{
		return !value.HasValue ?  Null : value.Value.ToString(_cultureInfo);
	}
	
	private static string ValueOrNull(Guid? value)
	{
		return value.HasValue ? value.Value.ToString() :  Null;
	}

	private static string ValueOrNull(string? value)
	{
		return string.IsNullOrEmpty(value) ?  Null  : value ;
	}
	
	private static string ValueOrNull(bool? value)
	{
		return value.HasValue ? $"{value.Value.ToString().ToUpper()}" :  Null;
	}

	private static string ValueOrNull(long? value)
	{
		return value.HasValue ? value.Value.ToString() : Null;
	}
	
	private static string ValueOrNull(DateTime? value)
	{
		return value.HasValue ? $"{value.Value:yyyy-MM-dd HH:mm:ss}" : Null;
	}
	
	public async Task InsertObservationsAsync(List<Observation> observations)
	{
		try
		{
			var headers = new List<string>
			{
				"id",
				"related_observation_id",
				"relation_role",
				"observation_type",
				"phenomenon_time_start",
				"phenomenon_time_end",
				"result_time",
				"observed_property_id",
				"observing_procedure_id",
				"observer_id",
				"host_id",
				"foi_id",
				"result_measure",
				"result_uom_id",
				"result_truth",
				"result_vocab",
				"result_term",
				"result_timeseries",
				"result_count",
				"result_geometry",
				"result_text",
				"valid_time_start",
				"valid_time_end",
				"parameter",
				"metadata",
				"base_measure",
				"base_uom_id"
			};
			var payload = new List<string>(observations.Count + 1);

			foreach (var observation in observations)
			{
				var geo  = observation.ResultGeometry == null ? null : Convert.ToHexString(new WKBWriter().Write(observation.ResultGeometry));
				var data = new List<string>
				{
					ValueOrNull(observation.Id),
					ValueOrNull(observation.RelatedObservationId),
					ValueOrNull(observation.RelationRole),
					ValueOrNull(observation.ObservationType),
					ValueOrNull(observation.PhenomenonStart),
					ValueOrNull(observation.PhenomenonStart),
					ValueOrNull(observation.ResultTime),
					ValueOrNull(observation.FeatureOfInterestId),
					ValueOrNull(observation.ResultMeasure),
					ValueOrNull(observation.ResultUomId),
					ValueOrNull(observation.ResultTruth),
					ValueOrNull(observation.ResultVocab),
					ValueOrNull(observation.ResultTerm),
					ValueOrNull(observation.ResultTimeseries),
					ValueOrNull(observation.ResultCount),
					ValueOrNull(geo),
					ValueOrNull(observation.ResultText),
					ValueOrNull(observation.ValidTimeStart),
					ValueOrNull(observation.ValidTimeEnd),
					ValueOrNull(observation.Parameter == null ? Null : JsonSerializer.Serialize(observation.Parameter, new JsonSerializerOptions { WriteIndented = false })),
					ValueOrNull(observation.Metadata == null ? Null : JsonSerializer.Serialize(observation.Metadata, new JsonSerializerOptions { WriteIndented   = false })),
					ValueOrNull(observation.BaseMeasure),
					ValueOrNull(observation.BaseUomId)
				};

				//Console.WriteLine(string.Join(Delimiter, data));
				payload.Add(string.Join(Delimiter, data));
			}

			await SendBulkAsync("observation", headers, payload).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
	}

	private async Task SendBulkAsync(string tableName, IEnumerable<string> columns, IEnumerable<string> content)
	{
		_connection.Open();
		try
		{
			var             command = $"COPY {tableName} ({string.Join(",", columns)}) FROM STDIN;";
			await using var writer  = await _connection.BeginTextImportAsync(command).ConfigureAwait(false);
			foreach (var line in content)
			{
				await writer.WriteLineAsync(line).ConfigureAwait(false);
				//Console.WriteLine(line);
			}
			writer.Close();
			await _connection.CloseAsync();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
	}
	
	public async Task<List<Observation>> GetObservationsAsync(FilterClause? filterClause, OrderByClause? orderByClause, int? skip = 0, int? take = TakeDefault)
	{
		take = take > TakeMax ? TakeMax : take;

		var whereStatement = filterClause == null ? " 1 = 1 " : _mapper.TranslateFilterClause(filterClause);
	//	var orderBy        = orderByClause == null ? " ctid " : _sorter.TranslateOrderByClause(orderByClause);
		var query = $"SELECT 0 AS count, id, related_observation_id, observation_type, phenomenon_time_start, phenomenon_time_end, valid_time_start, valid_time_end, observed_property_id, observing_procedure_id, host_id, observer_id, result_uom_id, ST_AsText(result_geometry) as result_geometry, result_count, result_measure, result_term, result_vocab, result_timeseries, result_complex, parameter, metadata, result_time, foi_id, result_truth, result_text, relation_role, base_measure, base_uom_id FROM observation where {whereStatement}  limit {(take ?? TakeDefault) + 1} offset {skip ?? 0} ";
		var data  = (await _connection.QueryAsync<dynamic>(query).ConfigureAwait(false)).Select(Observation.FromDynamic).ToList();
		return data;
	}
}

