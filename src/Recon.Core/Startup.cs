using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Recon.PluginInterface;

namespace Recon.Core
{
	class Startup
	{
		[ImportMany]
		IEnumerable<IPluginMiddleware> plugins;

		[ImportMany(typeof(IPluginController))]
		IEnumerable<IPluginController> controllers;

		public void ConfigureServices(IServiceCollection services)
		{
			var catalog = new DirectoryCatalog("plugins");
			CompositionContainer container = new CompositionContainer(catalog);
			container.SatisfyImportsOnce(this);

			var builder = services.AddMvcCore()
				.AddJsonFormatters();

			foreach (var item in controllers.Select(x => x.GetType().Assembly).Distinct())
			{
				builder.AddApplicationPart(item);
			}

			services.AddCors();

			services.AddSingleton<WebSocketCollection>();
			services.AddSingleton<JoystickManager>();

			services.AddSingleton<IInputMessageProcessor, KeyboardMessageProcessor>();
			services.AddSingleton<IInputMessageProcessor, JoystickMessageProcessor>();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseCors(builder => builder.AllowAnyOrigin());

			app.UseWebSockets();

			app.UseMvc();

			foreach (var plugin in plugins)
			{
				plugin.Configure(app);
			}

			//app.UseMiddleware<WebSocketMiddleware>();

			//app.Run(async (context) =>
			//{
			//	await context.Response.WriteAsync("Hello World!");
			//});
		}
	}
}
