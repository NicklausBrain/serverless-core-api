using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
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
                .AddEnvironmentVariables()
                .Build();

            var hostingEnvironment = new HostingEnvironment()
            {
                ContentRootPath = context.FunctionAppDirectory
            };

            /* Add required services into DI container */
            var services = new ServiceCollection();
            services.AddSingleton<DiagnosticSource>(new DiagnosticListener("Microsoft.AspNetCore"));
            services.AddSingleton<ObjectPoolProvider>(new DefaultObjectPoolProvider());
            services.AddSingleton<IHostingEnvironment>(hostingEnvironment);

            /* Instantiate standard ASP.NET Core Startup class */
            var startup = new Startup(config.GetWebJobsRootConfiguration());

            /* Add web app services into DI container */
            startup.ConfigureServices(services);

            /* Initialize DI container */
            var serviceProvider = services.BuildServiceProvider();

            /* Initialize Application builder */
            var appBuilder = new ApplicationBuilder(serviceProvider);

            /* Configure the HTTP request pipeline */
            startup.Configure(appBuilder, hostingEnvironment);

            /* Build request handling function */
            var requestHandler = appBuilder.Build();

            /* Set DI container for HTTP Context */
            req.HttpContext.RequestServices = serviceProvider;

            /* Handle HTTP request */
            await requestHandler(req.HttpContext);

            /* This dummy result does nothing, HTTP response is already set by requestHandler */
            return new EmptyResult();
        }
    }
}
