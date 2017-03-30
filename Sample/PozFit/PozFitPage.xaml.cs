using System.Collections.Generic;
using Plugin.BLE;
using Poz1.MiBandCDK;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace PozFit
{
	public partial class PozFitPage : ContentPage
	{
        public ObservableCollection<IDevice> BLEDevicesList { get; private set; }

        public Command<IDevice> ConnectToDeviceCommand {
            get
            {
                return new Command<IDevice>(async device =>
                {
                    if (device.Name == MiBandVersion.MI1.ToString() || device.Name == MiBandVersion.MI1A.ToString() || device.Name == MiBandVersion.MI1S.ToString())
                    {
                        var band = new MiBand(device);
                        await band.TestAPI();
                    }
                });
            }
        }

		public PozFitPage()
		{
            InitializeComponent();

            BLEDevicesList = new ObservableCollection<IDevice>();
            BindingContext = this;
        }

        protected async override void OnAppearing()
		{
			base.OnAppearing();

			var ble = CrossBluetoothLE.Current;
			var adapter = CrossBluetoothLE.Current.Adapter;

            adapter.DeviceDiscovered += (object sender, DeviceEventArgs e) =>
                {
                    BLEDevicesList.Add(e.Device);
                };

			await adapter.StartScanningForDevicesAsync();
		}
    }
}
