using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace Recon.PluginInterface
{
	[InheritedExport]
	public interface IPluginMiddleware
	{
		void Configure(IApplicationBuilder app);
	}

	[InheritedExport]
	//[Route("api/[controller]")]
	//[ApiController]
	public interface IPluginController { }
}
