using Poz1.MiBandCDK.Services;

namespace Poz1.MiBandCDK.Devices
{
    public interface IMiBand1S : IMiBandBase
    {
        HeartRateService HeartRate { get; }
    }
}
