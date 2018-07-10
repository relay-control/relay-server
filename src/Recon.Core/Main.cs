using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Recon.PluginInterface;

namespace Recon.Core
{
	public class ReconServer
	{
		IWebHost host;

		public ReconServer(string[] args)
		{
			var builder = new WebHostBuilder();

			builder.UseKestrel((builderContext, options) =>
			{
				options.Configure(builderContext.Configuration.GetSection("Kestrel"));
			})
				//.ConfigureAppConfiguration((hostingContext, config) =>
				//{
				//	var env = hostingContext.HostingEnvironment;

				//	config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				//		  .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

				//	if (args != null)
				//	{
				//		config.AddCommandLine(args);
				//	}
				//})
				.ConfigureLogging((hostingContext, logging) =>
				{
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddConsole();
					logging.AddDebug();
				});
				//.UseDefaultServiceProvider((context, options) =>
				//{
				//	options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
				//});

			host = builder.UseStartup<Startup>()
				.UseUrls("http://0.0.0.0:32155/")
				.Build();
		}

		public void Run()
		{
			host.Run();
		}
	}
}
