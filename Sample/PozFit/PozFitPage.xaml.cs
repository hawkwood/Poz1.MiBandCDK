using Plugin.BLE;
using Poz1.MiBandCDK;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Poz1.MiBandCDK.Model;
using System;

namespace PozFit
{
	public partial class PozFitPage : MasterDetailPage
	{
        public ObservableCollection<IDevice> BLEDevicesList { get; private set; }
		public ObservableCollection<Command> BLESDKCommands { get; private set; }

		public Command OnAppearingCommand
        {
            get
            {
                return new Command(async () =>
                {

                    var ble = CrossBluetoothLE.Current;
                    var adapter = CrossBluetoothLE.Current.Adapter;

                    adapter.DeviceDiscovered += (object sender, DeviceEventArgs e) =>
                    {
                        BLEDevicesList.Add(e.Device);
                    };

                    await adapter.StartScanningForDevicesAsync();
                });
            }
        }

        public Command<IDevice> ConnectToDeviceCommand {
            get
            {
                return new Command<IDevice>(async device =>
                {
					try
					{
						var miDevice = (MiBandModel)Enum.Parse(typeof(MiBandModel), device.Name);
						switch (miDevice)
						{
							case MiBandModel.MI1:
								{
									var band = new MiBand(device);
									await band.ConnectAsync();
									await band.StartVibrationAsync(VibrationMode.Vibration2TimesWithLed);
									break;
								}
						}
					}
					catch 
					{
						System.Diagnostics.Debug.WriteLine("Not a Supported MiBand device");
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
    }
}
