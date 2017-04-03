using Poz1.MiBandCDK.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poz1.MiBandCDK.Services
{
    public class HeartRateService
    {
        MiBand band;

        public HeartRateService(MiBand band)
        {
            this.band = band;
        }

        /// <summary>
        /// Gets the HeartRate Scanner readings: Use SpotMode to get one reading, ContnousMode to get the readings obtained in 20Seconds of Scan.
        /// SleepMode is currently unsupported
        /// </summary>
        /// <returns>
        /// List of int each representing an HeartRate reading.
        /// </returns>
        public Task<List<int>> GetHertRateScan(HeartRateMode mode)
        {
            band.CheckBandConnection();

            var heartRateTCS = new TaskCompletionSource<List<int>>();
            var heartRateReadings = new List<int>();

            switch (mode)
            {
                case HeartRateMode.Spot:
                    {
                        //heartRateMode = HeartRateMode.Spot;
                        Task.Run(async () =>
                        {
                            try
                            {
                                var heartRateService = await band.device.GetServiceAsync(MiBandService.HeartRateService);

                                //Subscribe
                                var heartRateMeasurement = await heartRateService.GetCharacteristicAsync(MiBandCharacteristic.HeartRateMeasurement);
                                var descriptor = await heartRateMeasurement.GetDescriptorAsync(MiBandDescriptor.DescriptorUpdateNotification);

                                heartRateMeasurement.ValueUpdated += (o, args) =>
                                {
                                    var data = args.Characteristic.Value;
                                    if (data.Length == 2 && data[0] == 6)
                                    {
                                        int heartRate = data[1] & 0xFF;

                                        heartRateReadings.Add(heartRate);
                                        heartRateTCS.TrySetResult(heartRateReadings);
                                    }
                                    else
                                    {
                                        heartRateTCS.TrySetException(new Exception("HeartRate Data is Not Valid"));
                                    }
                                };

                                await descriptor.WriteAsync(MiBandCommand.EnableNotifications);
                                await heartRateMeasurement.StartUpdatesAsync();

                                //Write
                                var controlPoint = await heartRateService.GetCharacteristicAsync(MiBandCharacteristic.HeartRateControlPoint);
                                await controlPoint.WriteAsync(MiBandCommand.StartHeartRateManual);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e.Message);
                                throw;
                            }
                        });

                        return heartRateTCS.Task;
                    }

                case HeartRateMode.Countinous:
                    {
                        Task.Run(async () =>
                        {
                            var heartRateService = await band.device.GetServiceAsync(MiBandService.HeartRateService);

                            //Subscribe
                            var heartRateMeasurement = await heartRateService.GetCharacteristicAsync(MiBandCharacteristic.HeartRateMeasurement);
                            var descriptor = await heartRateMeasurement.GetDescriptorAsync(MiBandDescriptor.DescriptorUpdateNotification);

                            heartRateMeasurement.ValueUpdated += (o, args) =>
                            {
                                var data = args.Characteristic.Value;
                                if (data.Length == 2 && data[0] == 6)
                                {
                                    int heartRate = data[1] & 0xFF;

                                    heartRateReadings.Add(heartRate);
                                }
                                else
                                {
                                    heartRateTCS.TrySetException(new Exception("HeartRate Data is Not Valid"));
                                }
                            };

                            await descriptor.WriteAsync(MiBandCommand.EnableNotifications);
                            await heartRateMeasurement.StartUpdatesAsync();

                            //Write
                            var controlPoint = await heartRateService.GetCharacteristicAsync(MiBandCharacteristic.HeartRateControlPoint);
                            await controlPoint.WriteAsync(MiBandCommand.StartHeartRateContinuous);

                            //TODO: Change to something better
                            await Task.Delay(20000);
                            heartRateTCS.SetResult(heartRateReadings);
                        });

                        return heartRateTCS.Task;
                    }
            }

            return null;
        }

        //public async Task EnableHeartRateSleepSupport()
        //{
        //    await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.HeartRateControlPoint, MiBandCommand.StartHeartRateSleep);
        //}
        //public async Task DisableHeartRateSleepSupport()
        //{
        //    await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.HeartRateControlPoint, MiBandCommand.StoptHeartRateSleep);
        //}

    }
}
