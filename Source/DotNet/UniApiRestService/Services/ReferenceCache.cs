namespace UniApiRestService.Services;

using DatabaseModel.Models;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;

public abstract record ReferenceCache
{
	public static async Task<Dictionary<long, Reference>?> GetReferencesFromCacheAsync(IMemoryCache cache, NpgsqlConnection connection)
	{
		var references = await cache.GetOrCreateAsync<Dictionary<long, Reference>>("referencesById", async cacheEntry =>
		{
			cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
			return await new DatabaseLayer.Services.ReferenceService(connection).GetReferencesByIdAsync().ConfigureAwait(false);
		});
		return references;
	}
	
	public static async Task<Dictionary<Guid, Reference>?> GetReferencesByKeyFromCacheAsync(IMemoryCache cache, NpgsqlConnection connection)
	{
		var references = await cache.GetOrCreateAsync<Dictionary<Guid, Reference>>("referencesByKey", async cacheEntry =>
		{
			cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
			return await new DatabaseLayer.Services.ReferenceService(connection).GetReferencesAsync().ConfigureAwait(false);
		});
		return references;
	}
}