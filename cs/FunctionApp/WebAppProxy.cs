using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using WebApplication;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace FunctionApp
{
    public static class WebAppProxy
    {
        [FunctionName(nameof(WebAppProxy))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get", "post","put", "patch",
                Route = "{*any}")]
            HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton(new DiagnosticListener(
                "Microsoft.AspNetCore"));
            services.AddSingleton<DiagnosticSource>(new DiagnosticListener(
                "Microsoft.AspNetCore"));
            services.AddSingleton<ObjectPoolProvider>(new DefaultObjectPoolProvider());

            var startup = new Startup(config.GetWebJobsRootConfiguration());
            startup.ConfigureServices(services);
            var appBuilder = new ApplicationBuilder(services.BuildServiceProvider());
            startup.Configure(appBuilder, new HostingEnvironment());

            var requestHandler = appBuilder.Build();

            await requestHandler(req.HttpContext);

            return (ActionResult)new OkObjectResult($"Hello, world");
        }
    }
}
