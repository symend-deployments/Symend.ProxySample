 using Microsoft.AspNetCore.Hosting;
 using Microsoft.Extensions.Configuration;
 using Microsoft.Extensions.Hosting;

 namespace Symend.ProxySample;
 
 public class Program
 {
     public static void Main(string[] args)
     {
         var myHostBuilder = Host.CreateDefaultBuilder(args);
         myHostBuilder.ConfigureWebHostDefaults(webHostBuilder =>
         {
             webHostBuilder.ConfigureAppConfiguration((hostContext, config) =>
             {
                 var env = hostContext.HostingEnvironment;
                 config.SetBasePath(env.ContentRootPath);
                 config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                 config.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false);
                 config.AddEnvironmentVariables();
             });
             webHostBuilder.UseStartup<Startup>();
         });
         var myHost = myHostBuilder.Build();
         myHost.Run();
     }
 }