using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApp;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace FuncApp
{
	public static class WebAppProxy
	{
		private static readonly Lazy<(RequestDelegate requestDelegate, IServiceProvider serviceProvider)> Init =
			new Lazy<(RequestDelegate requestDelegate, IServiceProvider serviceProvider)>(() =>
			{
				/* Instantiate standard ASP.NET Core Startup class */

				var webHostBuilder = WebHost
					.CreateDefaultBuilder(Array.Empty<string>())
					.UseStartup<Startup>();

				var webHost = webHostBuilder.Build();

				// see https://github.com/dotnet/aspnetcore/blob/master/src/Hosting/Hosting/src/Internal/WebHost.cs
				var serviceCollection =
					(ServiceCollection)webHost
						?.GetType()
						.GetField("_applicationServiceCollection", BindingFlags.Instance | BindingFlags.NonPublic)
						?.GetValue(webHost);

				/* Initialize DI container */
				var serviceProvider = serviceCollection.BuildServiceProvider();

				var startup = new Startup(serviceProvider.GetRequiredService<IConfiguration>());

				/* Add web app services into DI container */
				startup.ConfigureServices(serviceCollection);

				/* Initialize Application builder */
				var appBuilder = new ApplicationBuilder(serviceProvider, new FeatureCollection());

				/* Configure the HTTP request pipeline */
				startup.Configure(appBuilder, serviceProvider.GetRequiredService<IWebHostEnvironment>());

				/* Build request handling function */
				var requestHandler = appBuilder.Build();

				return (requestHandler, serviceProvider);
			});

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

			var prerequisites = Init.Value;

			/* Set DI container for HTTP Context */
			req.HttpContext.RequestServices = prerequisites.serviceProvider;

			/* Handle HTTP request */
			await prerequisites.requestDelegate(req.HttpContext);

			/* This dummy result does nothing, HTTP response is already set by requestHandler */
			return new EmptyResult();
		}
	}
}
