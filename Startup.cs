using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace Relay {
	public class Startup {
		internal static string PanelDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Relay");

		public void ConfigureServices(IServiceCollection services) {
			services.AddCors(options => {
				options.AddDefaultPolicy(builder => builder.AllowAnyOrigin());
			});
			services.AddRouting();
			services.AddSignalR();

			services.AddSingleton<InputProcessor>();
			services.AddSingleton<JoystickCollection>();

			services.AddSingleton<MacroProcessor>();
			services.AddSingleton<IInputProcessor, KeyboardProcessor>();
			services.AddSingleton<IInputProcessor, ButtonProcessor>();
			services.AddSingleton<IInputProcessor, AxisProcessor>();
			services.AddSingleton<IInputProcessor, CommandProcessor>();

			services.AddSingleton<PanelHosting>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			app.UseCors();
			app.UseRouting();

			// hosting
			app.UseStaticFiles(new StaticFileOptions() {
				RequestPath = "/panels",
				FileProvider = new PhysicalFileProvider(PanelDirectory),
				OnPrepareResponse = ctx => {
					ctx.Context.Response.Headers.Append("Cache-Control", "no-cache");
				}
			});
			//

			app.UseEndpoints(endpoints => {
				endpoints.MapHub<InputHub>("/inputhub");
			});
		}
	}
}
