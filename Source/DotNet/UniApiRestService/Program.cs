using System.Text.Json.Serialization;
using Dapper.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OData.Edm;
using UniApiRestService;
using UniApiRestService.OData;
using UniApiRestService.OpenApi;
using UniApiRestService.Services;
using static Dapper.SqlMapper;

var builder          = WebApplication.CreateBuilder(args);
var configuration    = builder.Configuration;

AddTypeHandler(new JsonTypeHandler<Dictionary<string, string>>());
var edmModel = UniApiModelBuilder.GetEdmModel();
SetBuilderServices(builder, edmModel, configuration);
SetAppProperties(builder).Run();

void SetBuilderServices(WebApplicationBuilder webApplicationBuilder, IEdmModel edmCsdlModel, IConfiguration configurationManager)
{
	var cache                    = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromHours(1) });
	webApplicationBuilder.Services.AddSingleton<IMemoryCache>(cache);
	webApplicationBuilder.Services.AddSingleton(_ => edmCsdlModel);
	webApplicationBuilder.Services.AddSingleton(_ => configurationManager);
	webApplicationBuilder.Services.AddScoped(_ => new ReferenceService(configurationManager, cache));
	webApplicationBuilder.Services.AddScoped(_ => new ObservationService(configurationManager, cache));
	webApplicationBuilder.Services.AddControllers(_ => _.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider())).AddOData(_ =>
	{
			_.EnableQueryFeatures().AddRouteComponents("odata", edmCsdlModel, services =>
			{
				services.AddSingleton<IFilterBinder, UniApiFilterBinder>();
				services.AddSingleton<ODataResourceSerializer, OmitNullResourceSerializer>();
			});
	}).AddJsonOptions(jsonOptions => { 
		jsonOptions.JsonSerializerOptions.NumberHandling         = JsonNumberHandling.AllowNamedFloatingPointLiterals;
		jsonOptions.JsonSerializerOptions.MaxDepth               = 1000;
		jsonOptions.JsonSerializerOptions.ReferenceHandler       = ReferenceHandler.Preserve;
		jsonOptions.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
	});
	webApplicationBuilder.Services.AddEndpointsApiExplorer();
	webApplicationBuilder.Services.AddSwaggerGen(_ =>
	{
		_.ResolveConflictingActions(act => act.First());
	});
	webApplicationBuilder.Services.AddRequestDecompression();
	webApplicationBuilder.Services.AddResponseCompression();
}

WebApplication SetAppProperties(WebApplicationBuilder applicationBuilder)
{
	var webApplication = applicationBuilder.Build();
	webApplication.UseDeveloperExceptionPage();
	webApplication.UseODataRouteDebug();
	webApplication.UseODataOpenApi();
	webApplication.UseODataQueryRequest();
	webApplication.UseSwagger();
	webApplication.UseSwaggerUI(_ =>
	{
		_.SwaggerEndpoint("/odata/$openapi", "OData raw OpenAPI");
		_.RoutePrefix = string.Empty;
		_.DocumentTitle = "UniApi Proof-of-Concept";
	});
	webApplication.UseReDoc(_ =>
	{
		_.RoutePrefix = "redoc";
		_.DocumentTitle = "UniApi Proof-of-Concept";
		_.SpecUrl = "/odata/$openapi";
	});
	webApplication.UseRouting();
	webApplication.MapControllers();
	webApplication.UseRequestDecompression();
	return webApplication;
}

// https://devblogs.microsoft.com/odata/tutorial-build-grpc-odata-in-asp-net-core/
// https://devblogs.microsoft.com/odata/customizing-filter-for-spatial-data-in-asp-net-core-odata-8/
// https://github.com/xuzhg/WebApiSample/tree/main/ODataSpatialSample
// https://medium.com/@nirinchev/dealing-with-spatial-data-in-odata-4e4051434ddb <--
// https://github.com/OData/WebApi/issues/438