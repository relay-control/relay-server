using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Relay;

public class PanelHosting {
	const string DirectoryName = "Relay";
	internal static string PanelDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DirectoryName);

	public PanelHosting() {
		if (!Directory.Exists(PanelDirectory)) {
			Directory.CreateDirectory(PanelDirectory);
		}
	}

	public void Configure(IApplicationBuilder app) {
		app.UseStaticFiles(new StaticFileOptions() {
			RequestPath = "/panels",
			FileProvider = new PhysicalFileProvider(PanelDirectory),
			OnPrepareResponse = ctx => {
				ctx.Context.Response.Headers.Append("Cache-Control", "no-cache");
			}
		});
	}

	public static IEnumerable<string> GetPanels() {
		var panels = Enumerable.Empty<string>();
		if (Directory.Exists(PanelDirectory)) {
			panels = Directory.GetDirectories(PanelDirectory).Select(path => Path.GetFileName(path));
		}
		return panels;
	}
}
