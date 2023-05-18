using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Symend.ProxySample.HttpTransformers;

public interface ISymendApiTransformer
{
    ValueTask TransformRequestAsync(HttpContext httpContext,
        HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken);
}