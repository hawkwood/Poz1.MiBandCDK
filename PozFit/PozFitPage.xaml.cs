using System.Collections.Generic;
using Plugin.BLE;
using Poz1.CDK.MiBand;
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
					var band = new Band(a.Device);

					await band.ConnectAsync();

					var time = await band.GetHertRateScan(HeartRateMode.Spot);
				}
			};

			await adapter.StartScanningForDevicesAsync();
		}
	}
}
