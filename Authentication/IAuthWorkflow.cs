using System.Threading.Tasks;

namespace Symend.ProxySample.Authentication;

public interface IAuthWorkflow
{
    /// <summary>
    /// Performs an authentication request against the token endpoint
    /// </summary>
    /// <returns></returns>
    /// <exception cref="AuthenticationException"></exception>
    Task<string> GetAuthenticationToken();
}