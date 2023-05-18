namespace Symend.ProxySample.Authentication;

public class AuthenticationConfig
{
    /// <summary>
    /// Seconds from Expiry to pre-emptively renew the token
    /// </summary>
    public double TokenExpiryGracePeriodSeconds { get; set; }
    public string Authority { get; set; }
    public string Audience { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}