namespace Relay;

class RelayApplicationContext : ApplicationContext {
	readonly NotifyIcon notifyIcon = new();

	public RelayApplicationContext() {
		var menu = new ContextMenuStrip {
			RenderMode = ToolStripRenderMode.System,
			DefaultDropDownDirection = ToolStripDropDownDirection.BelowRight,
		};

		var exitMenuItem = new ToolStripMenuItem("Exit", null, Exit);
		menu.Items.Add(exitMenuItem);

		notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		notifyIcon.Text = "Relay server";
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
