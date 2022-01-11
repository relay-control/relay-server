using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApplication;

namespace Relay;

class Program {
	static void Main(string[] args) {
		Console.Title = "Relay";
		var app = new WebApplication.WebApplication(32155);
		app.builder.ConfigureWebHost(webBuilder => {
			webBuilder.UseStartup<Startup>();
		});
		app.builder.ConfigureLogging(logging => {
			//logging.SetMinimumLevel(LogLevel.Debug);
		});
		app.Run();
		//Console.ReadKey(true);

		Application.SetHighDpiMode(HighDpiMode.SystemAware);
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		Application.Run(new RelayApplicationContext());
	}
}
