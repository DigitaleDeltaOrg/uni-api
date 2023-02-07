namespace UniApiRestService.Services;

using DatabaseModel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Spatial;
using Npgsql;

/// <summary>
/// 
/// </summary>
public class ReferenceService
{
	private  readonly NpgsqlConnection                        _connection;
	private readonly  IMemoryCache                            _memoryCache;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="configuration"></param>
	/// <param name="memoryCache"></param>
	public ReferenceService([FromServices] IConfiguration configuration, [FromServices] IMemoryCache memoryCache)
	{
		_connection  = new NpgsqlConnection(configuration.GetConnectionString("postgres"));
		_memoryCache = memoryCache;
	}
	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="options"></param>
	/// <returns></returns>
	public async Task<(long count, IQueryable<DigitaleDelta.Reference> data)> QueryDataAsync(ODataQueryOptions<DigitaleDelta.Reference> options)
	{
		var references = await ReferenceCache.GetReferencesByKeyFromCacheAsync(_memoryCache, _connection).ConfigureAwait(false);
		var data       = (await new DatabaseLayer.Services.ReferenceService(_connection).GetDataAsync(options.Filter?.FilterClause, options.OrderBy?.OrderByClause, options.Skip?.Value, options.Top?.Value)).ToList();
		return (data.FirstOrDefault()?.Count ?? 0, data.Select(reference => DatabaseReferenceToODataReference(reference, references)!).AsQueryable());
	}

	private static DigitaleDelta.Reference? DatabaseReferenceToODataReference(Reference? reference, Dictionary<Guid, Reference>? references)
	{
		if (reference == null)
		{
			return null;
		}

		GeometryPoint? point = null;
		if (!string.IsNullOrEmpty(reference.Geometry))
		{
			var geoPoint = new NetTopologySuite.IO.WKTReader().Read(reference.Geometry);
			point = GeometryPoint.Create(CoordinateSystem.Geometry(4258), geoPoint.Coordinate.X, geoPoint.Coordinate.Y, null, null);
		}

		// Type and name are not mapped. 
		// Internal references are not mapped either.
		// Geometry needs to be converted to SQL geometry, not NetTopologySuite geometry.
		return new DigitaleDelta.Reference
		{
			Id              = reference.ExternalKey.ToString(),
			Type            = reference.ReferenceType,
			Organisation    = reference.Organisation ?? string.Empty,
			Code            = reference.Code,
			Href            = reference.Uri ?? string.Empty,
			Geometry        = point,
			Description     = reference.Description,
			ExternalKey     = reference.ExternalKey.ToString(),
			TaxonType       = DatabaseReferenceToODataReference(GetReferenceFromCache(reference.TaxonTypeExternalKey, references), references),
			TaxonGroup      = DatabaseReferenceToODataReference(GetReferenceFromCache(reference.TaxonGroupExternalKey, references), references),
			TaxonParent     = DatabaseReferenceToODataReference(GetReferenceFromCache(reference.TaxonParentExternalKey, references), references),
			TaxonRank       = reference.TaxonRank,
			TaxonAuthors    = reference.TaxonAuthor,
			CasNumber       = reference.CasNumber,
			ParameterType   = reference.ParameterType,
			TaxonNameNl     = reference.TaxonNameNL,
			TaxonStatusCode = reference.TaxonStatusCode,
			TaxonTypeId     = reference.TaxonTypeExternalKey == null ? null : reference.TaxonTypeExternalKey.ToString(),
			TaxonGroupId    = reference.TaxonGroupExternalKey == null ? null : reference.TaxonGroupExternalKey.ToString(),
			TaxonParentId   = reference.TaxonParentExternalKey == null ? null : reference.TaxonParentExternalKey.ToString()
		};
	}
	
	private static Reference? GetReferenceFromCache(Guid? reference, IReadOnlyDictionary<Guid, Reference>? references)
	{
		if (reference == null || references == null)
		{
			return null;
		}
		
		return references.TryGetValue(reference.Value, out var value) ? value : null;
	}
}