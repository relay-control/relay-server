using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Recon {
	class ReconApplicationContext : ApplicationContext {
		readonly NotifyIcon notifyIcon = new();

		public ReconApplicationContext() {
			var menu = new ContextMenuStrip {
				RenderMode = ToolStripRenderMode.System,
				DefaultDropDownDirection = ToolStripDropDownDirection.BelowRight
			};

			var exitMenuItem = new ToolStripMenuItem("Exit", null, Exit);
			menu.Items.Add(exitMenuItem);

			notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
			notifyIcon.Text = "Recon server";
			notifyIcon.ContextMenuStrip = menu;
			notifyIcon.Visible = true;

			Application.ApplicationExit += OnApplicationExit;
		}

		private void OnApplicationExit(object sender, EventArgs e) {
			notifyIcon.Visible = false;
		}

		void Exit(object sender, EventArgs e) {
			Application.Exit();
		}
	}
}
