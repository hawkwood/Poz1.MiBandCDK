using Poz1.MiBandCDK.Model;
using System.Threading.Tasks;

namespace Poz1.MiBandCDK.Services
{
    public class VibrationService
    {
        MiBand band;

        internal VibrationService(MiBand band)
        {
            this.band = band;
        }

        public async Task StartVibrationAsync(VibrationMode mode)
        {
            band.CheckBandConnection();

            byte[] data;

            switch (mode)
            {
                case VibrationMode.Vibration2TimesWithLed:
                    data = MiBandCommand.VibrationWithLed;
                    break;
                case VibrationMode.Vibration10TimesWithLed:
                    data = MiBandCommand.Vibration10TimesWithLed;
                    break;
                case VibrationMode.VibrationWithoutLed:
                    data = MiBandCommand.VibrationWithoutLed;
                    break;
                default:
                    return;
            }

            var mainService = await band.device.GetServiceAsync(MiBandService.ImmediateAlertService);
            var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.Vibration);
            await controlChar.WriteAsync(data);
        }

        public async Task StopVibrationAsync()
        {
            band.CheckBandConnection();

            var mainService = await band.device.GetServiceAsync(MiBandService.ImmediateAlertService);
            var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.Vibration);
            await controlChar.WriteAsync(MiBandCommand.StopVibration);
        }
    }
}
