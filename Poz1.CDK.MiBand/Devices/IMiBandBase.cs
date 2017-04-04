using Poz1.MiBandCDK.Model;
using Poz1.MiBandCDK.Services;
using System;
using System.Threading.Tasks;

namespace Poz1.MiBandCDK.Devices
{
    public interface IMiBandBase
    {
        VibrationService Vibration { get; }
        ActivityService Activity { get; }

        /// <summary>
        /// Establish a connection to MiBand
        /// </summary>
        /// <returns></returns>
        Task ConnectAsync();

        /// <summary>
        /// Once the Miband is actively paired with a device, other devices won't discover it
        /// </summary>
        /// <returns>True if pairing was Successful</returns>
        Task<bool> PairAsync();

        Task<DeviceInfo> GetDeviceInfoAsync();

        Task<BatteryInfo> GetBatteryInfoAsync();

        Task<string> GetDeviceNameAsync();

        Task SetDeviceNameAsync(string name);

        Task<UserInfo> GetUserInfoAsync();

        /// <summary>
        /// Sets the user info. MacAddress can be obtained from GetDeviceInfoAsync().
        /// </summary>
        /// <returns>The user info.</returns>
        /// <param name="user">User.</param>
        /// <param name="macAddress">MacAddress. This can be obtained from GetDeviceInfoAsync()</param>
        Task SetUserInfoAsync(UserInfo user, string macAddress);

        Task<DateTime> GetTimeAsync();

        Task SetTimeAsync(DateTime time);

       // Task SetStatisticsAsync(Statistics stats);

        Task FactoryReset();

        Task Reboot();

        Task SetLEParametersAsync(BLEConnectionSettings param);

        Task<BLEConnectionSettings> GetBLEConnectionSettingsAsync();

        Task SetWearLocationAsync(WearLocation location);
    }
}
