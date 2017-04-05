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
using PozFit.Controls;
using System.Reflection;

namespace PozFit
{
	public partial class PozFitPage : MasterDetailPage, INotifyPropertyChanged
	{
        public ObservableCollection<IDevice> BLEDevicesList { get; private set; }
        public ObservableCollection<MainMenuGroup> BandInfos { get; private set; }

        private bool isScanning;
        public bool IsScanning
        {
            get { return isScanning; }
            set
            {
                if (value != isScanning)
                {
                    isScanning = value;
                    OnPropertyChanged();
                }
            }
        }

        public Command BLEScanCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await BLEScan();
                });
            }
        }

        public Command OnAppearingCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if(BLEDevicesList.Count == 0)
                        await BLEScan();
                });
            }
        }
        public Command<IDevice> ConnectToDeviceCommand {
            get
            {
                return new Command<IDevice>(async bleDevice =>
                {
                if (bleDevice.Name == MiBandModel.MI1.ToString() || bleDevice.Name == MiBandModel.MI1A.ToString() || bleDevice.Name == MiBandModel.MI1S.ToString())
                {
                    try
                        {
                            var band = MiBandFactory.Create<IMiBand1S>(bleDevice);
                            await band.ConnectAsync();

                            await GetBandInfos(band);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }
                });
            }
        }

		public PozFitPage()
		{
            InitializeComponent();

            BLEDevicesList = new ObservableCollection<IDevice>();
            BandInfos = new ObservableCollection<MainMenuGroup>();

            BindingContext = this;
        }

        async Task BLEScan()
        {
            IsScanning = true;

            var ble = CrossBluetoothLE.Current;
            var adapter = CrossBluetoothLE.Current.Adapter;

            adapter.DeviceDiscovered += (object sender, DeviceEventArgs e) =>
            {
                BLEDevicesList.Add(e.Device);
            };

            await adapter.StartScanningForDevicesAsync();

            IsScanning = false;
        }

        async Task GetBandInfos(IMiBandBase band)
        {
            AddInfo(await band.GetDeviceInfoAsync());
            AddInfo(await band.GetBatteryInfoAsync());
            AddInfo(await band.GetUserInfoAsync());
            AddInfo(await band.GetTimeAsync());
            AddInfo(await band.GetBLEConnectionSettingsAsync());
        }

        void AddInfo(object info)
        {
            var type = info.GetType();
            var group = new MainMenuGroup(type.Name);

            if (type != typeof(DateTime))
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                foreach (PropertyInfo propInfo in typeInfo.DeclaredProperties)
                {
                    group.Add(new MainMenuItem() { Property = propInfo.Name, Value = propInfo.GetValue(info).ToString() });
                }
            }
            else
            {
                var dateInfo = (DateTime)info;
                group.Add(new MainMenuItem() { Property = "Date", Value = dateInfo.Date.ToString() });
                group.Add(new MainMenuItem() { Property = "Time", Value = dateInfo.TimeOfDay.ToString() });
            }

            BandInfos.Add(group);
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void OnPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
