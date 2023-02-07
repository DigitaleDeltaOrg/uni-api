//-----------------------------------------------------------------------------
// <copyright file="OpenApiDocumentExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace UniApiRestService.OpenApi;

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData;
using Microsoft.OpenApi.OData.Edm;

/// <summary>
/// Provides methods for <see cref="OpenApiDocument"/>.
/// </summary>
internal static class OpenApiDocumentExtensions
{
    public static OpenApiDocument CreateDocument(HttpContext context, string prefixName)
    {
        IDictionary<string, ODataPath> templateToPathDict = new Dictionary<string, ODataPath>();
        var                            provider           = new ODataOpenApiPathProvider();
        IEdmModel?                     model              = null;
        var                            dataSource         = context.RequestServices.GetRequiredService<EndpointDataSource>();
        foreach (var endpoint in dataSource.Endpoints)
        {
            var metadata = endpoint.Metadata.GetMetadata<IODataRoutingMetadata>();
            if (metadata == null)
            {
                continue;
            }

            if (metadata.Prefix != prefixName)
            {
                continue;
            }
            model = metadata.Model;

            var routeEndpoint = endpoint as RouteEndpoint;
            if (routeEndpoint == null)
            {
                continue;
            }

            // get rid of the prefix
            var    length            = prefixName.Length;
            var routePathTemplate = routeEndpoint.RoutePattern.RawText?.Substring(length);
            routePathTemplate = routePathTemplate?.StartsWith("/") == false ? routePathTemplate : "/" + routePathTemplate;

            if (templateToPathDict.TryGetValue(routePathTemplate, out var pathValue))
            {
                var methods = GetHttpMethods(endpoint);
                foreach (var method in methods)
                {
                    pathValue.HttpMethods.Add(method);
                }

                continue;
            }

            var path = metadata.Template.Translate();
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (path == null)
            {
                continue;
            }

            path.PathTemplate = routePathTemplate;
            provider.Add(path);

            var method1 = GetHttpMethods(endpoint);
            foreach (var method in method1)
            {
                path.HttpMethods.Add(method);
            }
            templateToPathDict[routePathTemplate] = path;
        }

        var settings = new OpenApiConvertSettings
        {
            PathProvider = provider,
            ServiceRoot  = BuildAbsolute(context, prefixName)
        };

        return model.ConvertToOpenApi(settings);
    }

    private static IEnumerable<string> GetHttpMethods(Endpoint endpoint)
    {
        var methodMetadata = endpoint.Metadata.GetMetadata<HttpMethodMetadata>();
        if (methodMetadata != null)
        {
            return methodMetadata.HttpMethods;
        }

        throw new Exception();
    }

    internal static Uri BuildAbsolute(HttpContext context, string prefix)
    {
        var request = context.Request;
        return new Uri(UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase) + prefix);
    }
}