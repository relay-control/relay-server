using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Recon.PluginInterface;

namespace Recon.PanelHosting
{
	public static class PanelPath
	{
		public static string Path3 => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Recon");
	}

	public class PanelHosting : IPluginMiddleware
    {
		string folder = PanelPath.Path3;

		public void Configure(IApplicationBuilder app)
		{
			app.UseStaticFiles(new StaticFileOptions()
			{
				RequestPath = new PathString("/panels"),
				FileProvider = new PhysicalFileProvider(folder),
				//OnPrepareResponse = ctx =>
				//{
				//	ctx.Context.Response.Headers.Append("Cache-Control", "no-cache");
				//}
			});
		}
	}

	[Route("api/[controller]")]
	[ApiController]
	public class PanelsController : ControllerBase, IPluginController
	{
		string panelDirectory = PanelPath.Path3;

		public PanelsController()
		{
			Console.WriteLine("heelo everyone");
		}

		// GET: api/<controller>
		[HttpGet]
		public IEnumerable<string> Get()
		{
			var panels = Directory.Exists(panelDirectory) ? from path in Directory.GetDirectories(panelDirectory) select Path.GetFileName(path) : Enumerable.Empty<string>();
			return panels;
		}

		// GET api/<controller>/5
		[HttpGet("{id}")]
		public string Get(int id)
		{
			return "value[" + id + "]";
		}
	}
}
