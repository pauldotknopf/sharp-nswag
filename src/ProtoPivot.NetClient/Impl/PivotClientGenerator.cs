using System.Reflection;
using Microsoft.AspNetCore.Routing;
using ProtoPivot.Services;

namespace ProtoPivot.NetClient.Impl;

public class PivotClientGenerator : IPivotClientGenerator
{
    private readonly IPivotServiceDefinitionBuilder _serviceDefinitionBuilder;
    private readonly IPivotClientInvoker _pivotClientInvoker;
    private readonly LinkGenerator _linkGenerator;

    public PivotClientGenerator(IPivotServiceDefinitionBuilder serviceDefinitionBuilder,
        IPivotClientInvoker pivotClientInvoker,
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
        private PivotServiceDefinition _serviceDefinition;
        private HttpClient _httpClient;
        private IPivotClientInvoker _pivotClientInvoker;
        // ReSharper enable NotAccessedField.Local
        
        // ReSharper disable once UnusedMember.Global
        public void Internal_Init(PivotServiceDefinition serviceDefinition, HttpClient httpClient, IPivotClientInvoker pivotClientInvoker)
        {
            _serviceDefinition = serviceDefinition;
            _httpClient = httpClient;
            _pivotClientInvoker = pivotClientInvoker;
        }

        public override object Invoke(MethodInfo method, object[] args)
        {
            return null;
        }

        public override async Task InvokeAsync(MethodInfo method, object[] args)
        {
            var route = _serviceDefinition.Routes.Single(x => x.MethodInfo == method);
            await _pivotClientInvoker.Invoke(_serviceDefinition, route, _httpClient, args);
        }

        public override async Task<T1> InvokeAsyncT<T1>(MethodInfo method, object[] args)
        {
            var route = _serviceDefinition.Routes.Single(x => x.MethodInfo == method);
            var result = await _pivotClientInvoker.Invoke(_serviceDefinition, route, _httpClient, args);
            return (T1)result;
        }
    }
}