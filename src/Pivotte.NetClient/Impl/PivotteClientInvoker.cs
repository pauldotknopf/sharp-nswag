using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Options;
using Pivotte.Services;

namespace Pivotte.NetClient.Impl;

public class PivotteClientInvoker : IPivotteClientInvoker
{
    private readonly IOptions<MvcOptions> _mvcOptions;
    private readonly OutputFormatterSelector _formatterSelector;

    public PivotteClientInvoker(IOptions<MvcOptions> mvcOptions,
        OutputFormatterSelector formatterSelector)
    {
        _mvcOptions = mvcOptions;
        _formatterSelector = formatterSelector;
    }

    public async Task<object> Invoke(PivotteServiceDefinition serviceDefinition, PivotteRouteDefinition routeDefinition,
        ApiDescription apiDescription, HttpClient client, object[] args)
    {
        var routeTemplate = routeDefinition.Route;

        var queryParameters = new Dictionary<string, string>();

        HttpContent httpContent = null;
        int index = 0;
        foreach (var parameter in apiDescription.ParameterDescriptions)
        {
            if (parameter.Source == BindingSource.Query)
            {
                queryParameters[parameter.Name] = args[index]?.ToString();
            }

            if (parameter.Source == BindingSource.Body)
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Response.Body = new MemoryStream();

                var outputContext = new OutputFormatterWriteContext(
                    httpContext,
                    (stream, encoding) => throw new NotSupportedException("don't support raw binary writing"),
                    parameter.Type,
                    args[index]);
                outputContext.ContentType = apiDescription.SupportedRequestFormats.First().MediaType;

                var mediaTypes = new MediaTypeCollection();
                foreach (var type in apiDescription.SupportedRequestFormats)
                {
                    mediaTypes.Add(type.MediaType);
                }

                var outputFormatter = _formatterSelector.SelectFormatter(outputContext,
                    _mvcOptions.Value.OutputFormatters,
                    mediaTypes);

                if (outputFormatter == null)
                {
                    throw new NotSupportedException("no output formatter found");
                }

                Encoding encoding;
                if (outputFormatter is TextOutputFormatter textOutputFormatter)
                {
                    encoding = textOutputFormatter.SelectCharacterEncoding(outputContext);
                }
                else
                {
                    encoding = Encoding.UTF8;
                }

                await outputFormatter.WriteAsync(outputContext);

                if (string.IsNullOrEmpty(httpContext.Response.ContentType))
                {
                    throw new NotSupportedException("output formatter didn't set content type");
                }

                httpContext.Response.Body.Position = 0;
                using StreamReader reader = new(httpContext.Response.Body, encoding);
                httpContent = new StringContent(
                    await reader.ReadToEndAsync(),
                    encoding,
                    MediaTypeHeaderValue.Parse(httpContext.Response.ContentType));
            }

            if (parameter.Source == BindingSource.Path)
            {
                routeTemplate = routeTemplate.Replace($"{{{parameter.Name}}}", args[index].ToString());
            }

            index++;
        }

        if (queryParameters.Count > 0)
        {
            routeTemplate += "?" + string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"));
        }

        var request = new HttpRequestMessage(new HttpMethod(routeDefinition.Verb), routeTemplate);
        request.Content = httpContent;

        var response = await client.SendAsync(request);

        foreach (var responseType in apiDescription.SupportedResponseTypes)
        {
            if (responseType.StatusCode == (int)response.StatusCode)
            {
                var format = responseType.ApiResponseFormats.FirstOrDefault(x =>
                    x.MediaType == response.Content.Headers.ContentType.MediaType);
                if (format == null)
                {
                    // there is no response
                }
                else
                {
                    var modelState = new ModelStateDictionary();
                    var inputFormatterContext = new InputFormatterContext(new DefaultHttpContext
                        {
                            HttpContext =
                            {
                                Request =
                                {
                                    ContentType = format.MediaType,
                                    Body = response.Content.ReadAsStream()
                                }
                            }
                        },
                        "model",
                        modelState,
                        responseType.ModelMetadata,
                        (stream, encoding) => throw new NotSupportedException());
                    var inputFormatter =
                        _mvcOptions.Value.InputFormatters.FirstOrDefault(x => x.CanRead(inputFormatterContext));

                    if (inputFormatter == null)
                    {
                        throw new NotSupportedException($"no input formatter found for request {format.MediaType}");
                    }

                    var result = await inputFormatter.ReadAsync(inputFormatterContext);

                    if (result.HasError)
                    {
                        // Don't return type if there was errors.
                        return null;
                    }

                    return result.Model;
                }
            }
        }

        return null;
    }
}