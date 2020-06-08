using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace WebApp
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddLogging()
				.AddMvc(c => c.EnableEndpointRouting = false)
				// Explicitly add WebbApplication assembly as application part
				// This is required because WebApplication isn't executing assembly
				// when being hosted as Azure Function
				.AddApplicationPart(Assembly.Load("WebApp"));

			// Register the Swagger services
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "Serverless App", Version = "v1" });
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			// Register the Swagger generator and the Swagger UI middlewares
			app.UseSwagger();

			app.UseSwaggerUI(c =>
			{
				c.RoutePrefix = string.Empty;
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Serverless App V1");
			});

			app.UseHttpsRedirection();

			app.UseMvc();
		}
	}
}
