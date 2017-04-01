using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Poz1.MiBandCDK.Model;

namespace Poz1.MiBandCDK
{
	public class MiBand
	{
		IDevice band;

		public bool IsConnected { get; private set; }

		public event EventHandler<byte[]> GeneralNotificationReceived;
		public event EventHandler<byte[]> GravitySensorNotificationReceived;
		public event EventHandler<RealTimeStepsEventArgs> RealTimeStepsNotificationReceived;
        
		public MiBand(IDevice band)
		{
			this.band = band;
		}
        
        public async Task ConnectAsync()
        {
			try
			{
				await CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(band);
				IsConnected = true;
			}
			catch { throw; }
        }

        /// <summary>
        /// Once the Miband is actively paired with a device, other devices won't discover it
        /// </summary>
        /// <returns>True if pairing was Successful</returns>
        public async Task<bool> PairAsync()
        {
            CheckBandConnection();

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
				Debug.WriteLine(e.Message);
				throw;			
			}
        }

        public async Task<DeviceInfo> GetDeviceInfoAsync()
        {
            CheckBandConnection();

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

		public async Task<int> GetStepsAsync()
        {
            CheckBandConnection();

            var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var realtimeSteps = await mainService.GetCharacteristicAsync(MiBandCharacteristic.RealtimeSteps);

			var ch = await realtimeSteps.ReadAsync();

            return ch[3] << 24 | (ch[2] & 0xFF) << 16 | (ch[1] & 0xFF) << 8 | (ch[0] & 0xFF);
        }

        public async Task<BatteryInfo> GetBatteryInfoAsync()
        {
            CheckBandConnection();

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
            CheckBandConnection();

            var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var deviceName = await mainService.GetCharacteristicAsync(MiBandCharacteristic.DevicePName);

			var ch = await deviceName.ReadAsync();

            return Encoding.UTF8.GetString(ch, 0, ch.Length);
        }

        public async Task SetDeviceNameAsync(string name)
        {
            CheckBandConnection();

            var data = Encoding.UTF8.GetBytes(name);

			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var deviceName = await mainService.GetCharacteristicAsync(MiBandCharacteristic.DeviceName);

			await deviceName.WriteAsync(data);
        }

        public async Task<UserInfo> GetUserInfoAsync()
        {
            CheckBandConnection();

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

		/// <summary>
		/// Sets the user info.
		/// </summary>
		/// <returns>The user info.</returns>
		/// <param name="user">User.</param>
		/// <param name="macAddress">MacAddress. This can be obtained from GetDeviceInfoAsync()</param>
        public async Task SetUserInfoAsync(UserInfo user, string macAddress)
        {
            CheckBandConnection();

            byte[] data = user.ToByteArray(macAddress);

			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var userInfo = await mainService.GetCharacteristicAsync(MiBandCharacteristic.UserInfo);

			await userInfo.WriteAsync(data);
        }

        public async Task<DateTime> GetTimeAsync()
        {
            CheckBandConnection();

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
            CheckBandConnection();

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

   //     public async Task<Statistics> GetStatisticsAsync()
   //     {
			//var mainService = await band.GetServiceAsync(MiBandService.MainService);
			//var statistics = await mainService.GetCharacteristicAsync(MiBandCharacteristic.Statistics);

			//var ch = await statistics.ReadAsync();

   //         if (ch.Length == 5)
   //         {
   //             return new Statistics(ch);
   //         }
   //         else
   //         {
   //             throw new Exception("GetStatisticsAsync result format is wrong!");
   //         }
   //     }

        public async Task SetStatisticsAsync(Statistics stats)
        {
            CheckBandConnection();

            byte[] data = stats.ToByteArray();

			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var statistics = await mainService.GetCharacteristicAsync(MiBandCharacteristic.Statistics);

			await statistics.WriteAsync(data);
        }

        public async Task FactoryReset()
        {
            CheckBandConnection();

            var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);
			await controlChar.WriteAsync(MiBandCommand.FactoryReset);
        }

        public async Task Reboot()
        {
            CheckBandConnection();

            var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);
			await controlChar.WriteAsync(MiBandCommand.Reboot);        
		}

        public async Task SetLedColorAsync(LedColor color)
        {
            CheckBandConnection();

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
      
		public async Task SetLEParametersAsync(BLEConnectionSettings param)
        {
            CheckBandConnection();

            var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var bleParams = await mainService.GetCharacteristicAsync(MiBandCharacteristic.LeParams);
			await bleParams.WriteAsync(param.ToByteArray());
        }

		public async Task<BLEConnectionSettings> GetBLEConnectionSettingsAsync()
        {
            CheckBandConnection();

            var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var bleParams = await mainService.GetCharacteristicAsync(MiBandCharacteristic.LeParams);
			var ch = await bleParams.ReadAsync();

            if (ch.Length == 12)
            {
				return new BLEConnectionSettings(ch);
            }
            else
            {
                throw new Exception("GetLantencyAsync result format is wrong!");
            }
        }

		//TODO: find a way to get this.
   //     public async Task<int> GetFitnessGoal()
   //     {
			//var mainService = await band.GetServiceAsync(MiBandService.MainService);
			//var controlPoint = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);

			//var ch = await controlPoint.ReadAsync();
   //         if (ch.Length == 4)
   //         {
   //             return (ch[1] << 8) & 0xFF | (ch[0] & 0xFF);
   //         }
   //         else
   //         {
   //             throw new Exception("GetFitnessGoal result format is wrong!");
   //         }
   //     }

        public async Task SetFitnessGoalAsync(int fitnessGoal)
        {
            CheckBandConnection();

            byte[] data = { MiBandCommand.SetFitnessGoal, 0, (byte)(fitnessGoal & 0xff), (byte)(((int)fitnessGoal >> 8) & 0xff) };

			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);
			await controlChar.WriteAsync(data);
		}

        public async Task SetWearLocationAsync(WearLocation location)
        {
            CheckBandConnection();

            byte[] data = { MiBandCommand.SetWearLocation[0], (byte)location };

			var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);
			await controlChar.WriteAsync(data);
		}

		//Porcone
   //     public async Task<WearLocation> GetWearLocationAsync()
   //     {
			//var mainService = await band.GetServiceAsync(MiBandService.MainService);
			//var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);
			//var ch = await controlChar.ReadAsync();

			//if (ch.Length == 2)
   //         {
   //             return (WearLocation)ch[1];
   //         }
   //         else
   //         {
   //             throw new Exception("GetFitnessGoal result format is wrong!");
   //         }
   //     }

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

		  /// <summary>
        /// Always null
        /// </summary>
        /// <returns></returns>
        //public async Task GetSensorDataAsync()
        //{
        //    //var ch = await _band.ReadCharacteristicAsync(MiBandService.MainService, MiBandCharacteristic.SensorData);
        //}
		       
        #endregion

        #region Vibration
        public async Task StartVibrationAsync(VibrationMode mode)
        {
            CheckBandConnection();

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

                var mainService = await band.GetServiceAsync(MiBandService.ImmediateAlertService);
                var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.Vibration);
                await controlChar.WriteAsync(data);
        }

        public async Task StopVibrationAsync()
        {
            CheckBandConnection();

            var mainService = await band.GetServiceAsync(MiBandService.ImmediateAlertService);
			var controlChar = await mainService.GetCharacteristicAsync(MiBandCharacteristic.Vibration);
			await controlChar.WriteAsync(MiBandCommand.StopVibration);
        }
        #endregion

        #region GravitySensor

		EventHandler<CharacteristicUpdatedEventArgs> gravitySensorUpdated;

        public async Task EnableGravitySensorNotifications()
        {
            CheckBandConnection();

            var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var controlPoint = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);

			var sensorData = await mainService.GetCharacteristicAsync(MiBandCharacteristic.SensorData);
			var descriptor = await sensorData.GetDescriptorAsync(MiBandDescriptor.DescriptorUpdateNotification);

			await descriptor.WriteAsync(MiBandCommand.EnableNotifications);

			gravitySensorUpdated = (o, args) =>
			{
				GravitySensorNotificationReceived?.Invoke(this, args.Characteristic.Value);

				//		 private void handleSensorData(byte[] value)
				//{
				//	int counter = 0, step = 0, axis1 = 0, axis2 = 0, axis3 = 0;
				//	if ((value.length - 2) % 6 != 0)
				//	{
				//		LOG.warn("GOT UNEXPECTED SENSOR DATA WITH LENGTH: " + value.length);
				//		for (byte b : value)
				//		{
				//			LOG.warn("DATA: " + String.format("0x%4x", b));
				//		}
				//	}
				//	else
				//	{
				//		counter = (value[0] & 0xff) | ((value[1] & 0xff) << 8);
				//		for (int idx = 0; idx < ((value.length - 2) / 6); idx++)
				//		{
				//			step = idx * 6;
				//			axis1 = (value[step + 2] & 0xff) | ((value[step + 3] & 0xff) << 8);
				//			axis2 = (value[step + 4] & 0xff) | ((value[step + 5] & 0xff) << 8);
				//			axis3 = (value[step + 6] & 0xff) | ((value[step + 7] & 0xff) << 8);
				//		}
				//		LOG.info("READ SENSOR DATA VALUES: counter:" + counter + " step:" + step + " axis1:" + axis1 + " axis2:" + axis2 + " axis3:" + axis3 + ";");
				//	}
				//}
			};

			sensorData.ValueUpdated += gravitySensorUpdated;

			await sensorData.StartUpdatesAsync();
			await controlPoint.WriteAsync(MiBandCommand.EnableSensorDataNotifications);
        }

        public async Task DisableGravitySensorNotifications()
        {
            CheckBandConnection();

            var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var controlPoint = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);
			var sensorData = await mainService.GetCharacteristicAsync(MiBandCharacteristic.SensorData);

			await sensorData.StopUpdatesAsync();
			sensorData.ValueUpdated -= gravitySensorUpdated;

			var descriptor = await sensorData.GetDescriptorAsync(MiBandDescriptor.DescriptorUpdateNotification);
			await descriptor.WriteAsync(MiBandCommand.DisableNotifications);
			await controlPoint.WriteAsync(MiBandCommand.DisableSensorDataNotifications);
        }

		#endregion

		#region RealTimeStepsSensor

		EventHandler<CharacteristicUpdatedEventArgs> realTimeStepsUpdated;

        public async Task EnableRealtimeStepsNotifications()
        {
            CheckBandConnection();

            var mainService = await band.GetServiceAsync(MiBandService.MainService);
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
            CheckBandConnection();

            var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var controlPoint = await mainService.GetCharacteristicAsync(MiBandCharacteristic.ControlPoint);
			var realTimeSteps = await mainService.GetCharacteristicAsync(MiBandCharacteristic.RealtimeSteps);

			await realTimeSteps.StopUpdatesAsync();
			realTimeSteps.ValueUpdated -= realTimeStepsUpdated;

			var descriptor = await realTimeSteps.GetDescriptorAsync(MiBandDescriptor.DescriptorUpdateNotification);
			await descriptor.WriteAsync(MiBandCommand.DisableNotifications);
			await controlPoint.WriteAsync(MiBandCommand.DisableRealtimeStepsNotifications);
		}

        #endregion

        #region HeartRateSensor

        /// <summary>
        /// Gets the HeartRate Scanner readings: Use SpotMode to get one reading, ContnousMode to get the readings obtained in 20Seconds of Scan.
        /// SleepMode is currently unsupported
        /// </summary>
        /// <returns>
        /// List of int each representing an HeartRate reading.
        /// </returns>
        public Task<List<int>> GetHertRateScan(HeartRateMode mode)
        {
            CheckBandConnection();

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

        #endregion

        public Task<List<List<ActivityData>>> GetActivitiesAsync()
        {
            CheckBandConnection();

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
					var mainService = await band.GetServiceAsync(MiBandService.MainService);
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

					if (!IsConnected)
						activityTCS.TrySetException(new InvalidOperationException("Please Connect to MiBand first!"));
					
					activityTCS.TrySetException(e);
				}
            });

           return activityTCS.Task;
        }

		EventHandler<CharacteristicUpdatedEventArgs> generalNotificationRecevived;

		public async Task EnableGeneralNotificationsAsync()
        {
            CheckBandConnection();

            var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var generalNotification = await mainService.GetCharacteristicAsync(MiBandCharacteristic.GeneralNotification);
		
			generalNotificationRecevived = (o, args) =>
			{
				GeneralNotificationReceived?.Invoke(this, args.Characteristic.Value);
			};

			generalNotification.ValueUpdated += generalNotificationRecevived;

			await generalNotification.StartUpdatesAsync();
        }

		public async Task DisableGeneralNotifications()
		{
            CheckBandConnection();

            var mainService = await band.GetServiceAsync(MiBandService.MainService);
			var generalNotification = await mainService.GetCharacteristicAsync(MiBandCharacteristic.GeneralNotification);

			generalNotification.ValueUpdated -= generalNotificationRecevived;

			await generalNotification.StopUpdatesAsync();
		}

        private void CheckBandConnection()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Please connect to MiBand first");
        }

		public async Task TestAPI()
		{
			try
			{
				await ConnectAsync();
				await EnableGeneralNotificationsAsync();
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
			 
			try
			{
				var t = await GetBLEConnectionSettingsAsync();
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
				//var t = await GetStatisticsAsync();
				//Debug.WriteLine(t);
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
