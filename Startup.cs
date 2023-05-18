using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Symend.ProxySample.Authentication;
using Symend.ProxySample.HttpTransformers;
using Yarp.ReverseProxy.Forwarder;

namespace Symend.ProxySample;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<AuthenticationConfig>(Configuration.GetSection("AuthConfig"));
        services.Configure<ApiProxyConfig>(Configuration.GetSection("ApiProxyConfig"));
        services.AddSingleton<SymendApiTransformer>();
        services.AddSingleton<IAuthWorkflow,AuthWorkflow>();
        services.AddHttpForwarder();
    }

    public void Configure(IApplicationBuilder app, IHttpForwarder forwarder, IWebHostEnvironment env)
    {
        var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.All,
            UseCookies = false,
            ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
            ConnectTimeout = TimeSpan.FromSeconds(15)
        });
        
        var transformer = app.ApplicationServices.GetRequiredService<SymendApiTransformer>();
        var requestConfig = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.Map("/{**catch-all}", async httpContext =>
            {
                var apiConfig = Configuration
                    .GetSection("ApiProxyConfig")
                    .Get<ApiProxyConfig>();
                
                var error = await forwarder.SendAsync(httpContext, apiConfig.ApiUrl,
                    httpClient, requestConfig, transformer);
       
                if (error != ForwarderError.None)
                {
                    var errorFeature = httpContext.GetForwarderErrorFeature();
                    var exception = errorFeature.Exception;
                }
            });
        });
    }
}