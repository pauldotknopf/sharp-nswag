using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Pivotte.Services;

namespace Pivotte.NetClient.Impl;

public class PivotteClientGenerator : IPivotteClientGenerator
{
    private readonly IPivotteServiceDefinitionBuilder _serviceDefinitionBuilder;
    private readonly IPivotteClientInvoker _pivotClientInvoker;
    private readonly LinkGenerator _linkGenerator;

    public PivotteClientGenerator(IPivotteServiceDefinitionBuilder serviceDefinitionBuilder,
        IPivotteClientInvoker pivotClientInvoker,
        LinkGenerator linkGenerator)
    {
        _serviceDefinitionBuilder = serviceDefinitionBuilder;
        _pivotClientInvoker = pivotClientInvoker;
        _linkGenerator = linkGenerator;
    }

    public T Generate<T>(HttpClient client)
    {
        var definition = _serviceDefinitionBuilder.BuildServiceDefinition<T>();
        var proxy = (dynamic)DispatchProxyAsync.Create<T, RealProxyLoggingDecorator<T>>()!;
        proxy.Internal_Init(definition, client, _pivotClientInvoker);
        return proxy;
    }

    public class RealProxyLoggingDecorator<T> : DispatchProxyAsync
    {
        // ReSharper disable NotAccessedField.Local
        private PivotteServiceDefinition _serviceDefinition;
        private HttpClient _httpClient;
        private IPivotteClientInvoker _pivotteClientInvoker;
        // ReSharper enable NotAccessedField.Local
        
        // ReSharper disable once UnusedMember.Global
        public void Internal_Init(PivotteServiceDefinition serviceDefinition, HttpClient httpClient, IPivotteClientInvoker pivotteClientInvoker)
        {
            _serviceDefinition = serviceDefinition;
            _httpClient = httpClient;
            _pivotteClientInvoker = pivotteClientInvoker;
        }

        public override object Invoke(MethodInfo method, object[] args)
        {
            return null;
        }

        public override async Task InvokeAsync(MethodInfo method, object[] args)
        {
            var route = _serviceDefinition.Routes.Single(x => x.MethodInfo == method);
            await _pivotteClientInvoker.Invoke(_serviceDefinition, route, _httpClient, args);
        }

        public override async Task<T1> InvokeAsyncT<T1>(MethodInfo method, object[] args)
        {
            var route = _serviceDefinition.Routes.Single(x => x.MethodInfo == method);
            var result = await _pivotteClientInvoker.Invoke(_serviceDefinition, route, _httpClient, args);
            return (T1)result;
        }
    }
}