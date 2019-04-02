using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

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
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var services = new ServiceCollection();
            services.AddSingleton(new DiagnosticListener(
                "Microsoft.AspNetCore"));
            services.AddSingleton<DiagnosticSource>(new DiagnosticListener(
                "Microsoft.AspNetCore"));
            services.AddSingleton<ObjectPoolProvider>(new DefaultObjectPoolProvider());

            services.AddLogging();
            services.AddMvcCore().AddApplicationPart(Assembly.Load("WebApplication"));

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var requestHandler = new ApplicationBuilder(serviceProvider).UseMvc().Build();

            await requestHandler(req.HttpContext);

            return (ActionResult)new OkObjectResult($"Hello, world");
        }
    }
}
