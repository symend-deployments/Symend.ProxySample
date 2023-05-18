namespace Symend.ProxySample;

public class ApiProxyConfig
{
    public Headers Headers { get; set; }
    public string ApiUrl { get; set; } 
}

public class Headers
{
    public string OrganizationId { get; set; }
}