namespace UniApiRestService.OData;

using System.Reflection;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;

public class UniApiEnableQueryAttribute : EnableQueryAttribute
{
	public override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
	{
		// Remove skip, as it is taken care of by the backend.
		var parser = typeof(ODataQueryOptions).GetField("_queryOptionParser", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(queryOptions) as ODataQueryOptionParser;
		typeof(ODataQueryOptions).GetProperty("Top")?.SetValue(queryOptions, new TopQueryOption("0", queryOptions.Context, parser), null);
		typeof(ODataQueryOptions).GetProperty("OrderBy")?.SetValue(queryOptions, new OrderByQueryOption("0", queryOptions.Context, parser), null);

		return base.ApplyQuery(queryable, queryOptions);
	}
}