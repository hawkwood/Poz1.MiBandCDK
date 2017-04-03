using Plugin.BLE.Abstractions.EventArgs;
using Poz1.MiBandCDK.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Poz1.MiBandCDK.Services
{
    public class ActivityService
    {
        MiBand band;

        public event EventHandler<RealTimeStepsEventArgs> RealTimeStepsNotificationReceived;

        public ActivityService(MiBand band)
        {
            this.band = band;
        }

        public async Task<int> GetStepsAsync()
        {
            band.CheckBandConnection();

            var mainService = await band.device.GetServiceAsync(MiBandService.MainService);
            var realtimeSteps = await mainService.GetCharacteristicAsync(MiBandCharacteristic.RealtimeSteps);

            var ch = await realtimeSteps.ReadAsync();

            return ch[3] << 24 | (ch[2] & 0xFF) << 16 | (ch[1] & 0xFF) << 8 | (ch[0] & 0xFF);
        }

        public Task<List<List<ActivityData>>> GetActivitiesAsync()
        {
            band.CheckBandConnection();

            var activityTCS = new TaskCompletionSource<List<List<ActivityData>>>();
            int _totalBytes = 0;
            int _packageBytes = 0;
            bool _isLastPacket = false;
            DateTime _packageTimeStamp;

            Task.Run(async () =>
            {
                var _activityDataBuffer = new List<byte>();
                var _activities = new List<List<ActivityData>>();

                try
                {
                    var mainService = await band.device.GetServiceAsync(MiBandService.MainService);
                    var controlPoint = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);


                    var activityData = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ActivityData);
                    var descriptor = await activityData.GetDescriptorAsync(MiBandDescriptor.DescriptorUpdateNotification);

                    await descriptor.WriteAsync(MiBandCommand.EnableNotifications);

                    activityData.ValueUpdated += (o, args) =>
                    {
                        var data = args.Characteristic.Value;
                        if (data.Length == 11)
                        {
                            // byte 1 to 6 represent a timestamp
                            _packageTimeStamp = new DateTime(data[1] + 2000, data[2], data[3], data[4], data[5], data[6]);

                            if (_activityDataBuffer.Count != 0)
                            {
                                var activitiesList = new List<ActivityData>();
                                //var activitiesNumber = _activityDataBuffer.Count / 4;

                                for (int i = 0; i < _activityDataBuffer.Count; i = i + 4)
                                {
                                    var activity = new ActivityData(new byte[] { _activityDataBuffer[i], _activityDataBuffer[i + 1], _activityDataBuffer[i + 2], _activityDataBuffer[i + 3] }, _packageTimeStamp.AddMinutes(activitiesList.Count));
                                    activitiesList.Add(activity);
                                }

                                _activities.Add(activitiesList);

                                if (_isLastPacket)
                                    activityTCS.SetResult(_activities);

                                //ParseActivityList(_activityDataBuffer, _packageTimeStamp);
                                _activityDataBuffer.Clear();
                            }
                            // byte 0 is the data type: 1 means that each minute is represented by a triplet of bytes
                            int dataType = data[0];

                            Debug.WriteLine(dataType + "   Has to be 1");

                            // counter of all data held by the band
                            if (_totalBytes == 0)
                                _totalBytes = ((data[7] & 0xff) | ((data[8] & 0xff) << 8)) * 4;
                            // counter of this data block
                            _packageBytes = ((data[9] & 0xff) | ((data[10] & 0xff) << 8)) * 4;

                            if (_totalBytes == _packageBytes)
                            {
                                _isLastPacket = true;
                            }
                        }
                        else
                        {
                            foreach (var item in data)
                            {
                                _activityDataBuffer.Add(item);
                            }

                            _totalBytes = _totalBytes - data.Length;
                        }
                    };

                    await activityData.StartUpdatesAsync();
                    await controlPoint.WriteAsync(MiBandCommand.StartActivitySync);

                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);

                    if (!band.IsConnected)
                        activityTCS.TrySetException(new InvalidOperationException("Please Connect to MiBand first!"));

                    activityTCS.TrySetException(e);
                }
            });

            return activityTCS.Task;
        }

        EventHandler<CharacteristicUpdatedEventArgs> realTimeStepsUpdated;

        public async Task EnableRealtimeStepsNotifications()
        {
            band.CheckBandConnection();

            var mainService = await band.device.GetServiceAsync(MiBandService.MainService);
            var controlPoint = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);

            var realTimeSteps = await mainService.GetCharacteristicAsync(MiBandCharacteristic.RealtimeSteps);
            var descriptor = await realTimeSteps.GetDescriptorAsync(MiBandDescriptor.DescriptorUpdateNotification);

            await descriptor.WriteAsync(MiBandCommand.EnableNotifications);

            realTimeStepsUpdated = (o, args) =>
            {
                var data = args.Characteristic.Value;
                if (data.Length == 4)
                {
                    int steps = data[3] << 24 | (data[2] & 0xFF) << 16 | (data[1] & 0xFF) << 8 | (data[0] & 0xFF);
                    RealTimeStepsNotificationReceived?.Invoke(null, new RealTimeStepsEventArgs(steps));
                }
            };

            realTimeSteps.ValueUpdated += realTimeStepsUpdated;

            await realTimeSteps.StartUpdatesAsync();
            await controlPoint.WriteAsync(MiBandCommand.EnableRealtimeStepsNotifications);
        }

        public async Task DisableRealtimeStepsNotifications()
        {
            band.CheckBandConnection();

            var mainService = await band.device.GetServiceAsync(MiBandService.MainService);
            var controlPoint = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);
            var realTimeSteps = await mainService.GetCharacteristicAsync(MiBandCharacteristic.RealtimeSteps);

            await realTimeSteps.StopUpdatesAsync();
            realTimeSteps.ValueUpdated -= realTimeStepsUpdated;

            var descriptor = await realTimeSteps.GetDescriptorAsync(MiBandDescriptor.DescriptorUpdateNotification);
            await descriptor.WriteAsync(MiBandCommand.DisableNotifications);
            await controlPoint.WriteAsync(MiBandCommand.DisableRealtimeStepsNotifications);
        }
    }
}
