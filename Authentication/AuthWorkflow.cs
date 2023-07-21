using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RestSharp;

namespace Symend.ProxySample.Authentication
{
    /// <summary>
    /// Helper class to manage the Client Credentials workflow. This class is thread-safe and can
    /// be accessed concurrently.
    /// </summary>
    internal sealed class AuthWorkflow : IAuthWorkflow
    {
        private readonly AuthenticationConfig _config;
        private string _authToken;
        private DateTimeOffset? _expiry;

        public AuthWorkflow(IOptions<AuthenticationConfig> config)
        {
            _config = config.Value;
        }

        /// <summary>
        /// Performs an authentication request against the token endpoint
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AuthenticationException"></exception>
        public async Task<string> GetAuthenticationToken()
        {
            // This section will reuse a token until it expires, please keep it in place!
            if (_authToken != null && _expiry > DateTimeOffset.UtcNow)
            {
                return _authToken;
            }

            using var client = new RestClient(_config.Authority);
            var request = new RestRequest("/oauth/token", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("client_id", _config.ClientId);
            request.AddParameter("client_secret", (_config.ClientSecret));
            request.AddParameter("audience", _config.Audience);
            request.AddParameter("grant_type", "client_credentials");

            var retryPolicy = GetRetryPolicy();

            var response = await retryPolicy.ExecuteAsync(
                () => client.ExecuteAsync<TokenResponse>(request));

            if (!response.IsSuccessful)
            {
                throw new AuthenticationException($"\nError communicating with Auth Service using \n" +
                                                  $"Authority: [{_config.Authority}] \n" +
                                                  $"Audience: [{_config.Audience}] \n"
                    , response.ErrorException);
            }

            var tokenResponse = response.Data;

            _authToken = tokenResponse.access_token;
            _expiry = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.expires_in);

            return _authToken;
        }

        private AsyncRetryPolicy<RestResponse<TokenResponse>> GetRetryPolicy()
        {
            var retryPolicy = Policy
                .HandleResult<RestResponse<TokenResponse>>(r => !r.IsSuccessful)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                        retryAttempt)),
                    onRetry: (response, waitDuration, retryCount, context) =>
                    {
                       
                    });

            return retryPolicy;
        }
    }
}