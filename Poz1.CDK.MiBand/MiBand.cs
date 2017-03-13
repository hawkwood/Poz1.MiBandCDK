using Poz1.BLE.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Poz1.CDK.MiBand
{
    public class Band
    {
        private IDevice _band;
        private TaskCompletionSource<List<int>> _hrTCS;
        private TaskCompletionSource<List<List<ActivityData>>> _activityTCS;

        private List<int> _hrReadings;
        private HeartRateMode _hrMode;

        private bool _hrNotificationSubscribed;

        private List<byte> _activityDataBuffer = new List<byte>();
        private List<List<ActivityData>> _activities = new List<List<ActivityData>>();
        private int _totalBytes;
        private int _packageBytes;
        private bool _isLastPacket;
        private DateTime _packageTimeStamp;

        #region Events

        public event EventHandler DeviceDisconnected;

        public event EventHandler<CharacteristicEventArgs> NormalNotificationReceived;

        public event EventHandler<CharacteristicEventArgs> GravitySensorNotificationReceived;
        public event EventHandler<RealTimeStepsEventArgs> RealTimeStepsNotificationReceived;

        #endregion

        public Band(IDevice band)
        {
            _band = band;
            _band.DeviceDisconnected += (object sender, EventArgs e) => DeviceDisconnected?.Invoke(null, null);
            _band.CharacteristicChanged += OnBandCharacteristicChanged;
        }

        #region Public Methods
        public async Task ConnectAsync()
        {
            await _band.ConnectAsync();
        }

        /// <summary>
        /// Once the Miband is actively paired with a device, other devices won't discover it
        /// </summary>
        /// <returns>True if pairing was Successful</returns>
        public async Task<bool> PairAsync()
        {
            await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.Pair, MiBandCommand.Pair);
            var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.Pair);

            Debug.WriteLine("pair result " + ch.Value.ToString());

            if (ch.Value.Length == 1 && ch.Value[0] == 2)
            {
                return true;
            }
            else
            {
                Debug.WriteLine("Pairing Failed");
                return false;
            }
        }

        public async Task<DeviceInfo> GetDeviceInfoAsync()
        {
            var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.DeviceInfo);

            if (ch.Value.Length == 16 || ch.Value.Length == 20)
            {
                return new DeviceInfo(ch.Value);
            }
            else
            {
                throw new Exception("GetDeviceInfoAsync result format wrong!");
            }
        }
        public async Task<int> GetRealTimeStepsAsync()
        {
            var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.RealtimeSteps);
            return ch.Value[3] << 24 | (ch.Value[2] & 0xFF) << 16 | (ch.Value[1] & 0xFF) << 8 | (ch.Value[0] & 0xFF);
        }

        /// <summary>
        /// Always null
        /// </summary>
        /// <returns></returns>
        public async Task GetSensorDataAsync()
        {
            var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.SensorData);
        }
        //public async Task<ActivityData> GetActivityDataAsync()
        //{
        //    await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint, MiBandCommand.StartActivitySync);

        //    var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ActivityData);
        //    return new ActivityData(ch.Value);
        //}

        public async Task<BatteryInfo> GetBatteryInfoAsync()
        {
            var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.Battery);

            Debug.WriteLine("GetBatteryInfo result: " + ch.Value.ToString());

            if (ch.Value.Length == 10)
            {
                return new BatteryInfo(ch.Value);
            }
            else
            {
                throw new Exception("GetBatteryInfoAsync result format wrong!");
            }
        }


        public async Task<string> GetDeviceNameAsync()
        {
            var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.DeviceName);
            return Encoding.UTF8.GetString(ch.Value, 0, ch.Value.Length);
        }
        public async Task SetDeviceNameAsync(string deviceName)
        {
            var data = Encoding.UTF8.GetBytes(deviceName);
            await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.DeviceName, data);
        }

        public async Task<UserInfo> GetUserInfoAsync()
        {
            var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.UserInfo);
            if (ch.Value.Length == 20)
            {
                return new UserInfo(ch.Value);
            }
            else
            {
                throw new Exception("GetUserInfoAsync result format is wrong!");
            }
        }
        public async Task SetUserInfoAsync(UserInfo userInfo)
        {
            byte[] data = userInfo.ToByteArray(_band.MacAddress);
            await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.UserInfo, data);
        }

        public async Task<DateTime> GetTimeAsync()
        {
            var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.DateTime);
            if (ch.Value.Length == 12)
            {
                return new DateTime(ch.Value[0] + 2000, ch.Value[1], ch.Value[2], ch.Value[3], ch.Value[4], ch.Value[5]);
            }
            else
            {
                throw new Exception("GetTimeAsync result format is wrong!");
            }
        }
        public async Task SetTimeAsync(DateTime time)
        {
            byte[] data = new byte[]
            {
                (byte)(time.Year -2000),
                (byte)(time.Month),
                (byte)(time.Day),
                (byte)(time.Hour),
                (byte)(time.Minute),
                (byte)(time.Second),
                (byte) 0x0f,
                (byte) 0x0f,
                (byte) 0x0f,
                (byte) 0x0f,
                (byte) 0x0f,
                (byte) 0x0f};

            await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.DateTime, data);
        }

        public async Task<Statistics> GetStatisticsAsync()
        {
            var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.Statistics);
            if (ch.Value.Length == 5)
            {
                return new Statistics(ch.Value);
            }
            else
            {
                throw new Exception("GetStatisticsAsync result format is wrong!");
            }
        }
        public async Task SetStatisticsAsync(Statistics statistics)
        {
            byte[] data = statistics.ToByteArray();
            await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.Statistics, data);
        }

        public async Task FactoryReset()
        {
            await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint, MiBandCommand.FactoryReset);
        }

        public async Task Reboot()
        {
            await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint, MiBandCommand.Reboot);
        }

        public async Task SetLedColorAsync(LedColor color)
        {
            byte[] protocol;

            switch (color)
            {
                case LedColor.Red:
                    protocol = MiBandCommand.SetColorLed;
                    break;
                case LedColor.Blue:
                    protocol = MiBandCommand.SetColorBlue;
                    break;
                case LedColor.Green:
                    protocol = MiBandCommand.SetColorGreen;
                    break;
                case LedColor.Orange:
                    protocol = MiBandCommand.SetColorOrange;
                    break;
                default:
                    return;
            }

            await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint, protocol);
        }
      
        //public async Task SetLatencyAsync(Lantency latency)
        //{
        //    await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.LeParams, latency.Data);
        //}
        //public async Task<Lantency> GetLatencyAsync()
        //{
        //    var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.LeParams);
        //    if (ch.Value.Length == 12)
        //    {
        //        return Lantency.FromByteData(ch.Value);
        //    }
        //    else
        //    {
        //        throw new Exception("GetLantencyAsync result format is wrong!");
        //    }
        //}

        //public async Task<int> GetFitnessGoal()
        //{
        //    var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint);
        //    if (ch.Value.Length == 4)
        //    {
        //        return (ch.Value[1] << 8) & 0xFF | (ch.Value[0] & 0xFF);
        //    }
        //    else
        //    {
        //        throw new Exception("GetFitnessGoal result format is wrong!");
        //    }
        //}
        public async Task SetFitnessGoalAsync(int fitnessGoal)
        {
            byte[] data = { MiBandCommand.SetFitnessGoal, 0, (byte)(fitnessGoal & 0xff), (byte)(((int)fitnessGoal >> 8) & 0xff) };
            await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint, data);
        }

        public async Task SetWearLocationAsync(WearLocation location)
        {
            byte[] data = { MiBandCommand.SetWearLocation[0], (byte)location };
            await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint, data);
        }
        //public async Task<WearLocation> GetWearLocation()
        //{
        //    var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint);
        //    if (ch.Value.Length == 2)
        //    {
        //        return (WearLocation)ch.Value[1];
        //    }
        //    else
        //    {
        //        throw new Exception("GetFitnessGoal result format is wrong!");
        //    }
        //}

        public async Task GetUnkonw()
        {

            var ch = await _band.SubscribeCharacteristic(MiBandService.MainService, MiBandCharacteristic.Unknown00, MiBandDescriptor.DescriptorUpdateNotification);

            var ch2 = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.Unknown01);
        }

        #region ToBeImplemented

        //private void queueAlarm(Alarm alarm, TransactionBuilder builder, BluetoothGattCharacteristic characteristic)
        //{
        //    byte[] alarmCalBytes = MiBandDateConverter.calendarToRawBytes(alarm.getAlarmCal());

        //    byte[] alarmMessage = new byte[]{
        //        MiBandService.COMMAND_SET_TIMER,
        //        (byte) alarm.getIndex(),
        //        (byte) (alarm.isEnabled() ? 1 : 0),
        //        alarmCalBytes[0],
        //        alarmCalBytes[1],
        //        alarmCalBytes[2],
        //        alarmCalBytes[3],
        //        alarmCalBytes[4],
        //        alarmCalBytes[5],
        //        (byte) (alarm.isSmartWakeup() ? 30 : 0),
        //        (byte) alarm.getRepetitionMask()
        //};
        //    builder.write(characteristic, alarmMessage);
        //}
        /**
   * Fetch the events from the android device calendars and set the alarms on the miband.
   */
        //private void sendCalendarEvents()
        //{
        //    try
        //    {
        //        TransactionBuilder builder = performInitialized("Send upcoming events");
        //        BluetoothGattCharacteristic characteristic = getCharacteristic(MiBandService.UUID_CHARACTERISTIC_CONTROL_POINT);

        //        SharedPreferences prefs = PreferenceManager.getDefaultSharedPreferences(getContext());
        //        int availableSlots = Integer.parseInt(prefs.getString(MiBandConst.PREF_MIBAND_RESERVE_ALARM_FOR_CALENDAR, "0"));

        //        if (availableSlots > 0)
        //        {
        //            CalendarEvents upcomingEvents = new CalendarEvents();
        //            List<CalendarEvents.CalendarEvent> mEvents = upcomingEvents.getCalendarEventList(getContext());

        //            int iteration = 0;
        //            for (CalendarEvents.CalendarEvent mEvt : mEvents)
        //            {
        //                if (iteration >= availableSlots || iteration > 2)
        //                {
        //                    break;
        //                }
        //                int slotToUse = 2 - iteration;
        //                Calendar calendar = Calendar.getInstance();
        //                calendar.setTimeInMillis(mEvt.getBegin());
        //                byte[] calBytes = MiBandDateConverter.calendarToRawBytes(calendar);

        //                byte[] alarmMessage = new byte[]{
        //                    MiBandService.COMMAND_SET_TIMER,
        //                    (byte) slotToUse,
        //                    (byte) 1,
        //                    calBytes[0],
        //                    calBytes[1],
        //                    calBytes[2],
        //                    calBytes[3],
        //                    calBytes[4],
        //                    calBytes[5],
        //                    (byte) 0,
        //                    (byte) 0
        //            };
        //                builder.write(characteristic, alarmMessage);
        //                iteration++;
        //            }
        //            builder.queue(getQueue());
        //        }
        //    }
        //    catch (IOException ex)
        //    {
        //        LOG.error("Unable to send Events to MI device", ex);
        //    }
        //}

        #endregion

        #region Vibration
        public async Task StartVibrationAsync(VibrationMode mode)
        {
            byte[] protocol;

            switch (mode)
            {
                case VibrationMode.VibrationWithLed:
                    protocol = MiBandCommand.VibrationWithLed;
                    break;
                case VibrationMode.Vibration10TimesWithLed:
                    protocol = MiBandCommand.Vibration10TimesWithLed;
                    break;
                case VibrationMode.VibrationWithoutLed:
                    protocol = MiBandCommand.VibrationWithoutLed;
                    break;
                default:
                    return;
            }

            await _band.WriteCharacteristicAsync(MiBandService.ImmediateAlertService, MiBandCharacteristic.Vibration, protocol);
        }
        public async Task StopVibrationAsync()
        {
            await _band.WriteCharacteristicAsync(MiBandService.ImmediateAlertService, MiBandCharacteristic.Vibration, MiBandCommand.StopVibration);
        }
        #endregion

        #region GravitySensor
        public async Task EnableGravitySensorNotifications()
        {
            if (await _band.SubscribeCharacteristic(MiBandService.MainService, MiBandCharacteristic.SensorData, MiBandDescriptor.DescriptorUpdateNotification))
            {
                await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint, MiBandCommand.EnableSensorDataNotifications);
            }
        }

        public async Task DisableGravitySensorNotifications()
        {
            await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint, MiBandCommand.DisableSensorDataNotifications);
            await _band.UnsubscribeCharacteristic(MiBandService.MainService, MiBandCharacteristic.SensorData, MiBandDescriptor.DescriptorUpdateNotification);
        }

        #endregion

        #region RealTimeStepsSensor
        public async Task EnableRealtimeStepsNotifications()
        {
            if (await _band.SubscribeCharacteristic(MiBandService.MainService, MiBandCharacteristic.RealtimeSteps, MiBandDescriptor.DescriptorUpdateNotification))
            {
                await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint, MiBandCommand.EnableRealtimeStepsNotifications);
            }
        }

        public async Task DisableRealtimeStepsNotifications()
        {
            await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint, MiBandCommand.DisableRealtimeStepsNotifications);
            await _band.UnsubscribeCharacteristic(MiBandService.MainService, MiBandCharacteristic.RealtimeSteps, MiBandDescriptor.DescriptorUpdateNotification);
        }

        #endregion

        #region HeartRateSensor

        /// <summary>
        /// Gets the HeartRate Scanner readings: Use SpotMode to get one reading, ContnousMode to get the readings obtained in 20Seconds of Scan.
        /// SleepMode is currently unsupported
        /// </summary>
        /// <returns>
        /// List of int eache representing an HeartRate reading.
        /// </returns>
        public Task<List<int>> GetHertRateScan(HeartRateMode mode)
        {
            _hrTCS = new TaskCompletionSource<List<int>>();
            _hrReadings = new List<int>();

            switch (mode)
            {
                case HeartRateMode.Spot:
                    {
                        _hrMode = HeartRateMode.Spot;
                        Task.Run(async () =>
                        {
                            if (await _band.SubscribeCharacteristic(MiBandService.HeartRateService, MiBandCharacteristic.HeartRateMeasurement, MiBandDescriptor.DescriptorUpdateNotification))
                            {
                                await _band.WriteCharacteristicAsync(MiBandService.HeartRateService, MiBandCharacteristic.HeartRateControlPoint, MiBandCommand.StartHeartRateManual);
                            }
                        });

                        return _hrTCS.Task;
                    }
                case HeartRateMode.Countinous:
                    {
                        _hrMode = HeartRateMode.Countinous;
                        Task.Run(async () =>
                        {
                            if (!_hrNotificationSubscribed)
                                _hrNotificationSubscribed = await _band.SubscribeCharacteristic(MiBandService.HeartRateService, MiBandCharacteristic.HeartRateMeasurement, MiBandDescriptor.DescriptorUpdateNotification);

                            await _band.WriteCharacteristicAsync(MiBandService.HeartRateService, MiBandCharacteristic.HeartRateControlPoint, MiBandCommand.StartHeartRateContinuous);

                            await Task.Delay(20000);
                            _hrTCS.SetResult(_hrReadings);
                        });

                        return _hrTCS.Task;
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

        #endregion

        #region Activities

        public Task<List<List<ActivityData>>> GetActivitiesAsync()
        {
            _activityTCS = new TaskCompletionSource<List<List<ActivityData>>>();

            Task.Run(async () =>
            {
                if (await _band.SubscribeCharacteristic(MiBandService.MainService, MiBandCharacteristic.ActivityData, MiBandDescriptor.DescriptorUpdateNotification))
                {
                    await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint, MiBandCommand.StartActivitySync);
                }
            });

            return _activityTCS.Task;
        }

        // ONE ACTIVITY == 1 MINUTE
        private int OrganizeActivityData(byte[] data)
        {
            if (data.Length == 11)
            {
                if (_activityDataBuffer.Count != 0)
                {
                    ParseActivityList(_activityDataBuffer, _packageTimeStamp);
                    _activityDataBuffer.Clear();
                }
                // byte 0 is the data type: 1 means that each minute is represented by a triplet of bytes
                int dataType = data[0];
                // byte 1 to 6 represent a timestamp
                _packageTimeStamp = new DateTime(data[1] + 2000, data[2], data[3], data[4], data[5], data[6]);

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
            return 1;
        }
        private void ParseActivityList(List<byte> list, DateTime timeStamp)
        {
            var activitiesList = new List<ActivityData>();
            var activitiesNumber = list.Count / 4;

            for (int i = 0; i < list.Count; i = i + 4)
            {
                var activity = new ActivityData(new byte[] { list[i], list[i + 1], list[i + 2], list[i + 3] }, timeStamp.AddMinutes(activitiesList.Count));
                activitiesList.Add(activity);
            }

            _activities.Add(activitiesList);

            if (_isLastPacket)
                _activityTCS.SetResult(_activities);
        }
    
        #endregion

        public async Task EnableNotificationsAsync()
        {
            await _band.SubscribeCharacteristic(MiBandService.MainService, MiBandCharacteristic.GeneralNotification, MiBandDescriptor.DescriptorUpdateNotification);
        }
        #endregion
        private void OnBandCharacteristicChanged(object sender, CharacteristicEventArgs e)
        {
            switch (e.Characteristic.Guid.ToString())
            {
                case MiBandCharacteristic.GeneralNotification:
                    {
                        NormalNotificationReceived?.Invoke(null, new CharacteristicEventArgs(e.Characteristic));
                        break;
                    }
                case MiBandCharacteristic.SensorData:
                    {
                        GravitySensorNotificationReceived?.Invoke(null, new CharacteristicEventArgs(e.Characteristic));
                        break;
                    }
                case MiBandCharacteristic.RealtimeSteps:
                    {
                        var data = e.Characteristic.Value;
                        if (data.Length == 4)
                        {
                            int steps = data[3] << 24 | (data[2] & 0xFF) << 16 | (data[1] & 0xFF) << 8 | (data[0] & 0xFF);
                            RealTimeStepsNotificationReceived?.Invoke(null, new RealTimeStepsEventArgs(steps));
                        }

                        break;
                    }
                case MiBandCharacteristic.HeartRateMeasurement:
                    {
                        var data = e.Characteristic.Value;
                        if (data.Length == 2 && data[0] == 6)
                        {
                            int heartRate = data[1] & 0xFF;

                            _hrReadings.Add(heartRate);

                            if (_hrMode == HeartRateMode.Spot)
                                _hrTCS.TrySetResult(_hrReadings);
                        }
                        else
                        {
                            _hrTCS.TrySetException(new Exception("HeartRate Data is Not Valid"));
                        }
                        break;
                    }
                case MiBandCharacteristic.ActivityData:
                    {
                        var data = e.Characteristic.Value;
                        var result = OrganizeActivityData(data);
                        break;
                    }
                default:
                    {
                        Debug.WriteLine("Unkown Characteristic");
                        break;
                    }
            }
        }

        public async Task TestAPI()
        {
            await ConnectAsync();

            //await EnableNotificationsAsync();
            //var t = await EnableActivityDataNotifications();

            //var x = await GetDeviceInfoAsync();
            //var y = await GetDeviceNameAsync();
            //var z = await GetActivityDataAsync();
            //var t = await GetBatteryInfoAsync();
            //var u = await GetStatisticsAsync();
            //var w = await GetTimeAsync();
            //var p = await GetUserInfoAsync();
        }
    }
}
