namespace UniApiRestService.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using DigitaleDelta;
using UniApiExportFormats;

public class ObservationService
{
	private readonly NpgsqlConnection _connection;
	private readonly  IMemoryCache     _memoryCache;

	public ObservationService([FromServices] IConfiguration configuration, [FromServices] IMemoryCache memoryCache)
	{
		_connection = new NpgsqlConnection(configuration.GetConnectionString("postgres"));
		_memoryCache = memoryCache;
	}

	public async Task<(long count, List<Observation> data)> QueryDataAsync(ODataQueryOptions<Observation> options, string? alternateFormat = null)
	{
		var references = await ReferenceCache.GetReferencesFromCacheAsync(_memoryCache, _connection).ConfigureAwait(false);
		if (references == null)
		{
			return (0, Array.Empty<Observation>().ToList());
		}

		var observations = await new DatabaseLayer.Services.ObservationService(_connection).GetObservationsAsync(options.Filter?.FilterClause, options.OrderBy?.OrderByClause, options.Skip?.Value, options.Top?.Value);
		var response     = await new UniApiExport().GenerateExportDataAsync(observations, references).ConfigureAwait(false);
		return (observations.FirstOrDefault()?.Count ?? 0, response);
	}
}