namespace UniApiRestService.Controllers;

using DigitaleDelta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services;

/// <summary>
/// 
/// </summary>
[ODataAttributeRouting]
[Route("odata/Observation")]
public class ObservationController : ODataController
{
	private readonly ObservationService _observationService;

	public ObservationController([FromServices] ObservationService observationService)
	{
		_observationService = observationService;
	}
	
	/// <summary>
	/// Retrieve observations.
	/// </summary>
	/// <param name="oDataQueryOptions"></param>
	/// <returns></returns>
	[EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All )]
	[HttpGet]
	public async Task<ActionResult<PageResult<Observation>>> Get(ODataQueryOptions<Observation> oDataQueryOptions)
	{
		var                     data       = await _observationService.QueryDataAsync(oDataQueryOptions).ConfigureAwait(false);
		
		oDataQueryOptions.ApplyTo(data.data.AsQueryable());
		var take = oDataQueryOptions.Top?.Value ?? 10000;
		if (take < data.data.Count)
		{
			var uri = new UriBuilder(oDataQueryOptions.Request.Scheme, oDataQueryOptions.Request.Host.Host, oDataQueryOptions.Request.Host.Port ?? 80, oDataQueryOptions.Request.Path);
			var queryParts = oDataQueryOptions.Request.QueryString.Value?.Split('&').ToList() ?? new List<string>();
			queryParts.RemoveAll(x => x.StartsWith("$skip"));
			queryParts.Add($"$skip={(oDataQueryOptions.Skip?.Value ?? 0) + take}");
			uri.Query = queryParts.Aggregate((x, y) => x + "&" + y);
			oDataQueryOptions.Request.ODataFeature().NextLink = uri.Uri;
		}
		return Ok(data.data);
	}
}