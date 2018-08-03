using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Recon.PluginInterface;

namespace Recon.Core {
	class Startup {
		[ImportMany]
		IEnumerable<IPluginMiddleware> plugins;

		[ImportMany(typeof(IPluginController))]
		IEnumerable<IPluginController> controllers;

		public void ConfigureServices(IServiceCollection services) {
			var builder = services.AddMvcCore()
				.AddJsonFormatters();

			if (Directory.Exists("plugins")) {
				var catalog = new DirectoryCatalog("plugins");
				var container = new CompositionContainer(catalog);
				container.SatisfyImportsOnce(this);

				foreach (var item in controllers.Select(x => x.GetType().Assembly).Distinct()) {
					builder.AddApplicationPart(item);
				}
			}

			services.AddCors();

			services.AddSingleton<WebSocketManager>();
			services.AddSingleton<JoystickManager>();

			services.AddSingleton<InputMessageProcessor>();

			services.AddSingleton<IInputMessageProcessor, KeyboardMessageProcessor>();
			services.AddSingleton<IInputMessageProcessor, JoystickMessageProcessor>();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseCors(builder => builder.AllowAnyOrigin());
			app.UseWebSockets();
			app.UseMvc();

			if (Directory.Exists("plugins")) {
				foreach (var plugin in plugins) {
					plugin.Configure(app);
				}
			}

			//app.Run(async (context) =>
			//{
			//	await context.Response.WriteAsync("Hello World!");
			//});
		}
	}
}
