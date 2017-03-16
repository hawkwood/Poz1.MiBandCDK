using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Poz1.MiBandCDK.Model;

namespace Poz1.MiBandCDK
{
    public class MiBand
    {
		private IDevice band;
		private TaskCompletionSource<List<int>> heartRateTCS;
		private TaskCompletionSource<List<List<ActivityData>>> activityTCS;

		private List<int> heartRateReadings;
		private HeartRateMode heartRateMode;

        //private bool _hrNotificationSubscribed;

        private List<byte> _activityDataBuffer = new List<byte>();
        private List<List<ActivityData>> _activities = new List<List<ActivityData>>();
        private int _totalBytes;
        private int _packageBytes;
        private bool _isLastPacket;
        private DateTime _packageTimeStamp;

        #region Events

		public event EventHandler<CharacteristicBase> GeneralNotificationReceived;

        public event EventHandler<CharacteristicBase> GravitySensorNotificationReceived;
        public event EventHandler<RealTimeStepsEventArgs> RealTimeStepsNotificationReceived;

        #endregion

        public MiBand(IDevice band)
        {
            this.band = band;
        }

        #region Public Methods
        public async Task ConnectAsync()
        {
			await CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(band);
            //await _band.ConnectAsync();
        }

        /// <summary>
        /// Once the Miband is actively paired with a device, other devices won't discover it
        /// </summary>
        /// <returns>True if pairing was Successful</returns>
        public async Task<bool> PairAsync()
        {
			try
			{
				var mainService = await band.GetServiceAsync(MiBandService.MainService);
				var pairChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.Pair);
				await pairChar.WriteAsync(MiBandCommand.Pair);

				var ch = await pairChar.ReadAsync();

				Debug.WriteLine("pair result " + ch.ToString());

				if (ch.Length == 1 && ch[0] == 2)
				{
					return true;
				}
				else
				{
					Debug.WriteLine("Pairing Failed");
					return false;
				}
			}
			catch (Exception e) 
			{
				return false;
			}
        }

        public async Task<DeviceInfo> GetDeviceInfoAsync()
        {
			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var deviceInfo = await mainService.GetCharacteristicAsync(MiBandCharacteristic.DeviceInfo);

			var ch = await deviceInfo.ReadAsync();

			if (ch.Length == 16 || ch.Length == 20)
            {
                return new DeviceInfo(ch);
            }
            else
            {
                throw new Exception("GetDeviceInfoAsync result format wrong!");
            }
        }

        public async Task<int> GetRealTimeStepsAsync()
        {
			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var realtimeSteps = await mainService.GetCharacteristicAsync(MiBandCharacteristic.RealtimeSteps);

			var ch = await realtimeSteps.ReadAsync();

            return ch[3] << 24 | (ch[2] & 0xFF) << 16 | (ch[1] & 0xFF) << 8 | (ch[0] & 0xFF);
        }

        /// <summary>
        /// Always null
        /// </summary>
        /// <returns></returns>
        //public async Task GetSensorDataAsync()
        //{
        //    //var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.SensorData);
        //}
        //public async Task<ActivityData> GetActivityDataAsync()
        //{
        //    await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint, MiBandCommand.StartActivitySync);

        //    var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ActivityData);
        //    return new ActivityData(ch.Value);
        //}

        public async Task<BatteryInfo> GetBatteryInfoAsync()
        {
            //var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.Battery);

			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var battery = await mainService.GetCharacteristicAsync(MiBandCharacteristic.Battery);

			var ch = await battery.ReadAsync();

            Debug.WriteLine("GetBatteryInfo result: " + ch.ToString());

            if (ch.Length == 10)
            {
                return new BatteryInfo(ch);
            }
            else
            {
                throw new Exception("GetBatteryInfoAsync result format wrong!");
            }
        }


        public async Task<string> GetDeviceNameAsync()
        {
			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var deviceName = await mainService.GetCharacteristicAsync(MiBandCharacteristic.DeviceName);

			var ch = await deviceName.ReadAsync();

            return Encoding.UTF8.GetString(ch, 0, ch.Length);
        }

        public async Task SetDeviceNameAsync(string name)
        {
            var data = Encoding.UTF8.GetBytes(name);

			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var deviceName = await mainService.GetCharacteristicAsync(MiBandCharacteristic.DeviceName);

			var ch = await deviceName.WriteAsync(data);
        }

        public async Task<UserInfo> GetUserInfoAsync()
        {

			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var userInfo = await mainService.GetCharacteristicAsync(MiBandCharacteristic.UserInfo);

			var ch = await userInfo.ReadAsync();

            if (ch.Length == 20)
            {
                return new UserInfo(ch);
            }
            else
            {
                throw new Exception("GetUserInfoAsync result format is wrong!");
            }
        }

   //     public async Task SetUserInfoAsync(UserInfo userInfo)
   //     {
   //         byte[] data = userInfo.ToByteArray(_band.MacAddress);

			//var mainService = await _band.GetServiceAsync(MiBandService.MainService);
			//var userInfo = await mainService.GetCharacteristicAsync(MiBandCharacteristic.UserInfo);

			//var ch = await userInfo.WriteAsync(data);
   //         await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.UserInfo, data);
   //     }

        public async Task<DateTime> GetTimeAsync()
        {
			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var dateTime = await mainService.GetCharacteristicAsync(MiBandCharacteristic.DateTime);

			var ch = await dateTime.ReadAsync();

            if (ch.Length == 12)
            {
                return new DateTime(ch[0] + 2000, ch[1], ch[2], ch[3], ch[4], ch[5]);
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

			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var dateTime = await mainService.GetCharacteristicAsync(MiBandCharacteristic.DateTime);

			await dateTime.WriteAsync(data);
        }

        public async Task<Statistics> GetStatisticsAsync()
        {
			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var statistics = await mainService.GetCharacteristicAsync(MiBandCharacteristic.Statistics);

			var ch = await statistics.ReadAsync();

            if (ch.Length == 5)
            {
                return new Statistics(ch);
            }
            else
            {
                throw new Exception("GetStatisticsAsync result format is wrong!");
            }
        }

        public async Task SetStatisticsAsync(Statistics stats)
        {
			byte[] data = stats.ToByteArray();

			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var statistics = await mainService.GetCharacteristicAsync(MiBandCharacteristic.Statistics);

			await statistics.WriteAsync(data);
        }

        public async Task FactoryReset()
        {
			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);
			await controlChar.WriteAsync(MiBandCommand.FactoryReset);
        }

        public async Task Reboot()
        {
			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);
			await controlChar.WriteAsync(MiBandCommand.Reboot);        
		}

        public async Task SetLedColorAsync(LedColor color)
        {
			byte[] data;

            switch (color)
            {
                case LedColor.Red:
                    data = MiBandCommand.SetColorLed;
                    break;
                case LedColor.Blue:
                    data = MiBandCommand.SetColorBlue;
                    break;
                case LedColor.Green:
                    data = MiBandCommand.SetColorGreen;
                    break;
                case LedColor.Orange:
                    data = MiBandCommand.SetColorOrange;
                    break;
                default:
                    return;
            }

			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);
			await controlChar.WriteAsync(data);
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

			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);
			await controlChar.WriteAsync(data);
		}

        public async Task SetWearLocationAsync(WearLocation location)
        {
            byte[] data = { MiBandCommand.SetWearLocation[0], (byte)location };

			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);
			await controlChar.WriteAsync(data);
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

            //var ch = await _band.SubscribeCharacteristic(MiBandService.MainService, MiBandCharacteristic.Unknown00, MiBandDescriptor.DescriptorUpdateNotification);

            //var ch2 = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.Unknown01);
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
			byte[] data;

            switch (mode)
            {
                case VibrationMode.VibrationWithLed:
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

			var mainService = await band.GetServiceAsync(MiBandService.ImmediateAlertService);
			var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.Vibration);
			await controlChar.WriteAsync(data);
        }

        public async Task StopVibrationAsync()
        {
			var mainService = await band.GetServiceAsync(MiBandService.ImmediateAlertService);
			var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.Vibration);
			await controlChar.WriteAsync(MiBandCommand.StopVibration);
        }
        #endregion

        #region GravitySensor
        public async Task EnableGravitySensorNotifications()
        {
			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var controlPoint = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);


			var sensorData = await mainService.GetCharacteristicAsync(MiBandCharacteristic.SensorData);
			var descriptor = await sensorData.GetDescriptorAsync(MiBandDescriptor.DescriptorUpdateNotification);

			await descriptor.WriteAsync(MiBandCommand.EnableNotifications);

			sensorData.ValueUpdated += (o, args) =>
			{
				var bytes = args.Characteristic.Value;
			};

			await sensorData.StartUpdatesAsync();
			await controlPoint.WriteAsync(MiBandCommand.EnableSensorDataNotifications);
        }

        //public async Task DisableGravitySensorNotifications()
        //{
        //    await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint, MiBandCommand.DisableSensorDataNotifications);
        //    await _band.UnsubscribeCharacteristic(MiBandService.MainService, MiBandCharacteristic.SensorData, MiBandDescriptor.DescriptorUpdateNotification);
        //}

        #endregion

        #region RealTimeStepsSensor
        public async Task EnableRealtimeStepsNotifications()
        {
			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var controlPoint = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);

			var realTimeSteps = await mainService.GetCharacteristicAsync(MiBandCharacteristic.RealtimeSteps);
			var descriptor = await realTimeSteps.GetDescriptorAsync(MiBandDescriptor.DescriptorUpdateNotification);

			await descriptor.WriteAsync(MiBandCommand.EnableNotifications);

			realTimeSteps.ValueUpdated += (o, args) =>
			{
				var data = args.Characteristic.Value;
				if (data.Length == 4)
				{
					int steps = data[3] << 24 | (data[2] & 0xFF) << 16 | (data[1] & 0xFF) << 8 | (data[0] & 0xFF);
					RealTimeStepsNotificationReceived?.Invoke(null, new RealTimeStepsEventArgs(steps));
				}
			};

			await realTimeSteps.StartUpdatesAsync();
			await controlPoint.WriteAsync(MiBandCommand.EnableRealtimeStepsNotifications);
        }

        //public async Task DisableRealtimeStepsNotifications()
        //{
        //    await _band.WriteCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.ControlPoint, MiBandCommand.DisableRealtimeStepsNotifications);
        //    await _band.UnsubscribeCharacteristic(MiBandService.MainService, MiBandCharacteristic.RealtimeSteps, MiBandDescriptor.DescriptorUpdateNotification);
        //}

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
            heartRateTCS = new TaskCompletionSource<List<int>>();
            heartRateReadings = new List<int>();

			switch (mode)
            {
                case HeartRateMode.Spot:
                    {
                        heartRateMode = HeartRateMode.Spot;
						Task.Run(async () =>
                        {
							try
							{
								var heartRateService = await band.GetServiceAsync(MiBandService.HeartRateService);

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

										if (heartRateMode == HeartRateMode.Spot)
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
								throw;
							}
                        });

                        return heartRateTCS.Task;
                    }
                case HeartRateMode.Countinous:
                    {
                        heartRateMode = HeartRateMode.Countinous;
                        Task.Run(async () =>
                        {
							var heartRateService = await band.GetServiceAsync(MiBandService.HeartRateService);

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

									if (heartRateMode == HeartRateMode.Spot)
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
							await controlPoint.WriteAsync(MiBandCommand.StartHeartRateContinuous);

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

        #endregion

        #region Activities

        public Task<List<List<ActivityData>>> GetActivitiesAsync()
        {
            activityTCS = new TaskCompletionSource<List<List<ActivityData>>>();

            Task.Run(async () =>
            {
				try
				{
					var mainService = await band.GetServiceAsync(MiBandService.MainService);
					var controlPoint = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);


					var activityData = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ActivityData);
					var descriptor = await activityData.GetDescriptorAsync(MiBandDescriptor.DescriptorUpdateNotification);

					await descriptor.WriteAsync(MiBandCommand.EnableNotifications);

					activityData.ValueUpdated += (o, args) =>
					{
						var bytes = args.Characteristic.Value;
						ReadActivityData(args.Characteristic.Value);
					};

					await activityData.StartUpdatesAsync();
					await controlPoint.WriteAsync(MiBandCommand.StartActivitySync);

				}
				catch (Exception e) 
				{
					Debug.WriteLine(e.Message);
					throw e;
				}
            });

           return activityTCS.Task;
        }

        // ONE ACTIVITY == 1 MINUTE
		private int ReadActivityData(byte[] data)
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

				Debug.WriteLine(dataType + "   Has to be 1");

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
                activityTCS.SetResult(_activities);
        }
    
        #endregion

        public async Task EnableNotificationsAsync()
        {
            //await _band.SubscribeCharacteristic(MiBandService.MainService, MiBandCharacteristic.GeneralNotification, MiBandDescriptor.DescriptorUpdateNotification);
        }
		#endregion
		//private void OnBandCharacteristicChanged(object sender, CharacteristicEventArgs e)
		//{
		//    switch (e.Characteristic.Guid.ToString())
		//    {
		//        case MiBandCharacteristic.GeneralNotification:
		//            {
		//                NormalNotificationReceived?.Invoke(null, new CharacteristicEventArgs(e.Characteristic));
		//                break;
		//            }
		//        case MiBandCharacteristic.SensorData:
		//            {
		//                GravitySensorNotificationReceived?.Invoke(null, new CharacteristicEventArgs(e.Characteristic));
		//                break;
		//            }
		//        case MiBandCharacteristic.RealtimeSteps:
		//            {
		//                var data = e.Characteristic.Value;
		//                if (data.Length == 4)
		//                {
		//                    int steps = data[3] << 24 | (data[2] & 0xFF) << 16 | (data[1] & 0xFF) << 8 | (data[0] & 0xFF);
		//                    RealTimeStepsNotificationReceived?.Invoke(null, new RealTimeStepsEventArgs(steps));
		//                }

		//                break;
		//            }
		//        case MiBandCharacteristic.HeartRateMeasurement:
		//            {
		//                var data = e.Characteristic.Value;
		//                if (data.Length == 2 && data[0] == 6)
		//                {
		//                    int heartRate = data[1] & 0xFF;

		//                    _hrReadings.Add(heartRate);

		//                    if (_hrMode == HeartRateMode.Spot)
		//                        _hrTCS.TrySetResult(_hrReadings);
		//                }
		//                else
		//                {
		//                    _hrTCS.TrySetException(new Exception("HeartRate Data is Not Valid"));
		//                }
		//                break;
		//            }
		//        case MiBandCharacteristic.ActivityData:
		//            {
		//                var data = e.Characteristic.Value;
		//                var result = OrganizeActivityData(data);
		//                break;
		//            }
		//        default:
		//            {
		//                Debug.WriteLine("Unkown Characteristic");
		//                break;
		//            }
		//    }
		//}

		public async Task TestAPI()
		{
			try
			{
				await ConnectAsync();
				await EnableNotificationsAsync();
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}

			try
			{
				var t = await GetActivitiesAsync();
				Debug.WriteLine(t);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}

			try
			{
				var t = await GetDeviceInfoAsync();
				Debug.WriteLine(t);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}

			try
			{
				var t = await GetDeviceNameAsync();
				Debug.WriteLine(t);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}

			try
			{
				var t = await GetBatteryInfoAsync();
				Debug.WriteLine(t);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}

			try
			{
				var t = await GetStatisticsAsync();
				Debug.WriteLine(t);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}

			try
			{
				var t = await GetTimeAsync();
				Debug.WriteLine(t);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}

			try
			{
				var t = await GetUserInfoAsync();
				Debug.WriteLine(t);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
		}
    }
}
