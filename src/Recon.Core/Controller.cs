using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Recon.Core
{
	[Route("api/[controller]")]
	[ApiController]
	public class TestController : ControllerBase
	{
		Joystick joystick = new Joystick();

		public TestController(WebsocketCollection greeter)
		{
		}

		// GET: api/<controller>
		[HttpGet]
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}

		// GET api/<controller>/5
		[HttpGet("{id}")]
		public string Get(int id)
		{
			return "value" + id;
		}

		// GET: /api/requestdevice/ 
		[HttpGet("RequestDevice/{deviceID}")]
		public Dictionary<string, dynamic> Get(uint deviceID)
		{
			var deviceData = new Dictionary<string, dynamic>();
			deviceData["token"] = HttpContext.TraceIdentifier;
			deviceData["id"] = deviceID;
			deviceData["acquired"] = joystick.AcquireDevice(deviceID);
			if (deviceData["acquired"])
			{
				deviceData["numButtons"] = joystick.GetDeviceNumButtons(deviceID);
				deviceData["numContPovs"] = joystick.GetDeviceNumContPovs(deviceID);
				deviceData["numDiscPovs"] = joystick.GetDeviceNumDiscPovs(deviceID);

				deviceData["axes"] = new Dictionary<int, bool>();
				int[] axes = (int[])Enum.GetValues(typeof(HID_USAGES));
				for (int i = 0; i < axes.Length; i++)
				{
					deviceData["axes"][i + 1] = joystick.IsDeviceAxisEnabled(deviceID, axes[i]);
				}
			}

			return deviceData;
		}
	}
}
