using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Recon.Core;

namespace Recon
{
	class Program
	{
		static void Main(string[] args)
		{
			ReconServer server = new ReconServer(new string[]{});
			Task.Run(() => server.Run());

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new ReconApplicationContext());
		}
	}
}
