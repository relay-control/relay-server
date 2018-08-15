using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Recon {
	class ReconApplicationContext : ApplicationContext {
		NotifyIcon notifyIcon = new NotifyIcon();

		public ReconApplicationContext() {
			MenuItem exitMenuItem = new MenuItem("Exit", Exit);

			//notifyIcon.Click += (s, e) => notifyIcon.ShowBalloonTip(0);
			notifyIcon.Text = "Recon";
			//notifyIcon.Icon = Properties.Resources.AppIcon;
			notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
			notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] { exitMenuItem });
			notifyIcon.BalloonTipTitle = "Balloon Tip Title";
			notifyIcon.BalloonTipText = "Balloon Tip Text.";
			notifyIcon.Visible = true;
			//notifyIcon.ShowBalloonTip(0);
		}

		void Exit(object sender, EventArgs e) {
			// We must manually tidy up and remove the icon before we exit.
			// Otherwise it will be left behind until the user mouses over.
			notifyIcon.Visible = false;

			Application.Exit();
		}
	}
}
