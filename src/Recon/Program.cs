using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Recon.Core;

namespace Recon {
	class Program {
		static void Main(string[] args) {
			var builder = new WebHostBuilder()
				.UseKestrel((builderContext, options) => {
					//options.Listen(IPAddress.Loopback, 32155);
					//options.Configure(builderContext.Configuration.GetSection("Kestrel"));
				})
				.ConfigureLogging((hostingContext, logging) => {
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddConsole();
					logging.AddDebug();
				})
				//.UseDefaultServiceProvider((context, options) => {
				//	 options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
				// });
				.UseStartup<Startup>()
				.UseUrls("http://0.0.0.0:32155/");
			
			host = builder.Build();
			
			Task.Factory.StartNew(() => host.Run(), TaskCreationOptions.LongRunning);

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new ReconApplicationContext());
		}
	}
}
