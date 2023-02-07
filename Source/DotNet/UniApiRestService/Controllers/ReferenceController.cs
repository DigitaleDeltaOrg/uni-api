namespace UniApiRestService.Controllers;

using DigitaleDelta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using OData;
using Services;

/// <summary>
/// 
/// </summary>
[ODataAttributeRouting]
[Route("odata/Reference")]
public class ReferenceController : ODataController
{
	private readonly ReferenceService _referenceService;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="referenceService"></param>
	public ReferenceController([FromServices] ReferenceService referenceService)
	{
		_referenceService = referenceService;
	}
	
	// GET
	/// <summary>
	/// Retrieve references
	/// </summary>
	/// <param name="oDataQueryOptions"></param>
	/// <returns></returns>
	[EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All )]
	[HttpGet]
	public async Task<ActionResult<PageResult<Reference>>> Get(ODataQueryOptions<Reference> oDataQueryOptions)
	{
		var data    = await _referenceService.QueryDataAsync(oDataQueryOptions).ConfigureAwait(false);
		oDataQueryOptions.Request.ODataFeature().TotalCount = data.count;
		oDataQueryOptions.ApplyTo(data.data);

		return Ok(data.data);
	}
}