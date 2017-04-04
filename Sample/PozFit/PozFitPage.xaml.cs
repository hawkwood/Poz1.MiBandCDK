using Plugin.BLE;
using Poz1.MiBandCDK;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Poz1.MiBandCDK.Model;
using System;
using Poz1.MiBandCDK.Devices;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace PozFit
{
	public partial class PozFitPage : MasterDetailPage, INotifyPropertyChanged
	{
        public ObservableCollection<IDevice> BLEDevicesList { get; private set; }

        private DeviceInfo deviceInfo;
        public DeviceInfo DeviceInfo
        {
            get { return deviceInfo; }
            set
            {
                if(value != deviceInfo)
                {
                    deviceInfo = value;
                    OnPropertyChanged();
                }
            }
        }

        private BatteryInfo batteryInfo;
        public BatteryInfo BatteryInfo
        {
            get { return batteryInfo; }
            set
            {
                if (value != batteryInfo)
                {
                    batteryInfo = value;
                    OnPropertyChanged();
                }
            }
        }

        private UserInfo userInfo;
        public UserInfo UserInfo
        {
            get { return userInfo; }
            set
            {
                if (value != userInfo)
                {
                    userInfo = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime bandTime;
        public DateTime BandTime
        {
            get { return bandTime; }
            set
            {
                if (value != bandTime)
                {
                    bandTime = value;
                    OnPropertyChanged();
                }
            }
        }

        private BLEConnectionSettings bleConnection;
        public BLEConnectionSettings BleConnection
        {
            get { return bleConnection; }
            set
            {
                if (value != bleConnection)
                {
                    bleConnection = value;
                    OnPropertyChanged();
                }
            }
        }

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
                return new Command<IDevice>(async bleDevice =>
                {
                    //if (bleDevice.Name == MiBandModel.MI1.ToString() || bleDevice.Name == MiBandModel.MI1A.ToString() || bleDevice.Name == MiBandModel.MI1S.ToString())
                    //{
                        try
                        {
                            var band = MiBandFactory.Create<IMiBand1S>(bleDevice);
                            await band.ConnectAsync();

                       var t = await band.HeartRate.GetHertRateScan(HeartRateMode.Spot);
                            //await GetBandInfos(band);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    //}
                });
            }
        }

		public PozFitPage()
		{
            InitializeComponent();

            BLEDevicesList = new ObservableCollection<IDevice>();
            BindingContext = this;
        }

        async Task GetBandInfos(IMiBandBase band)
        {
            DeviceInfo = await band.GetDeviceInfoAsync();
            BatteryInfo = await band.GetBatteryInfoAsync();
            UserInfo = await band.GetUserInfoAsync();
            BandTime = await band.GetTimeAsync();
            BleConnection = await band.GetBLEConnectionSettingsAsync();
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void OnPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
