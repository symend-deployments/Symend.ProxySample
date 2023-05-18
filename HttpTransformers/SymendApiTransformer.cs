using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Symend.ProxySample.Authentication;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

namespace Symend.ProxySample.HttpTransformers;

public class SymendApiTransformer : HttpTransformer, ISymendApiTransformer
{
    private readonly IAuthWorkflow _authWorkflow;
    private readonly IOptions<ApiProxyConfig> _apiProxyConfig;

    public SymendApiTransformer(IAuthWorkflow authWorkflow, IConfiguration configuration, IOptions<ApiProxyConfig> apiProxyConfig)
    {
        _authWorkflow = authWorkflow;
        _apiProxyConfig = apiProxyConfig;
    }
    
    public override async ValueTask TransformRequestAsync(HttpContext httpContext,
        HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken)
    {
        // Copy all request headers
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);
        
        var queryContext = new QueryTransformContext(httpContext.Request);
        
        StringValues targetApi;
        if (queryContext.Collection.TryGetValue("url", out targetApi))
        {
            var token = await _authWorkflow.GetAuthenticationToken();
            proxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            proxyRequest.Headers.Add("User-Agent", "SymProxySample/0.0.1");
            
            // Must clear Accept headers or you may receive JSON response instead of text/html as defined below
            proxyRequest.Headers.Accept.Clear();
            proxyRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

            // POI: Specify target organization from AppSettings
            proxyRequest.Headers.Add("X-SYM-OrganizationId", _apiProxyConfig.Value.Headers.OrganizationId);
            proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(targetApi[0]?.ToString() ?? string.Empty, PathString.Empty, QueryString.Empty);
        }
        proxyRequest.Headers.Host = null;
    }
}