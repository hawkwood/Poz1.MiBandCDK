using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BLE;
using Poz1.MiBandCDK;
using Xamarin.Forms;

namespace PozFit
{
	public partial class PozFitPage : ContentPage
	{
		public PozFitPage()
		{
			InitializeComponent();
		}

		protected async override void OnAppearing()
		{
			base.OnAppearing();

			var ble = CrossBluetoothLE.Current;
			var adapter = CrossBluetoothLE.Current.Adapter;

			var deviceList = new List<Plugin.BLE.Abstractions.Contracts.IDevice>();

			adapter.DeviceDiscovered += async (s, a) =>
			{
				deviceList.Add(a.Device);

				if (a.Device.Name == "MI1S") 
				{
					var band = new MiBand(a.Device);

					await band.ConnectAsync();

					await band.EnableGravitySensorNotifications();
					//var info = await band.GetDeviceInfoAsync();
					//var steps = await band.GetStepsAsync();
					//var battery = await band.GetBatteryInfoAsync();
					//var deviceName = await band.GetDeviceNameAsync();
					//var userInfo = await band.GetUserInfoAsync();
					//var time = await band.GetTimeAsync();
					//var bleSettings = await band.GetBLEConnectionSettingsAsync();
				}
			};

			await adapter.StartScanningForDevicesAsync();
		}
	}
}
