using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.FileProviders;

namespace FuncApp
{
	public class ServerLessHostEnvironment : IWebHostEnvironment
	{
		public ServerLessHostEnvironment(ExecutionContext context)
		{
			WebRootPath = context.FunctionAppDirectory;
			ContentRootPath = context.FunctionAppDirectory;
		}

		public string EnvironmentName { get; set; }
		public string ApplicationName { get; set; }
		public string ContentRootPath { get; set; }
		public string WebRootPath { get; set; }
		public IFileProvider ContentRootFileProvider { get; set; }
		public IFileProvider WebRootFileProvider { get; set; }
	}
}
