using System;

namespace Poz1.MiBandCDK
{
    public struct MiBandMacAddressFilter
    {
        public const string MAC_ADDRESS_FILTER_1_1A = "88:0F:10";
        public const string MAC_ADDRESS_FILTER_1S = "C8:0F:10";
    }

    public enum MiBandHWVersion
    {
        MI1, MI1A, MI1S
    }

	public static class MiBandService
	{
		public static Guid GenericAccess = new Guid("00001800-0000-1000-8000-00805f9b34fb");
		public static Guid GenericAttribute = new Guid("00001801-0000-1000-8000-00805f9b34fb");
		public static Guid MainService = new Guid("0000fee0-0000-1000-8000-00805f9b34fb");
		public static Guid ServiceUnknown = new Guid("0000fee1-0000-1000-8000-00805f9b34fb");
		public static Guid HeartRateService = new Guid("0000180d-0000-1000-8000-00805f9b34fb");
		public static Guid ImmediateAlertService = new Guid("00001802-0000-1000-8000-00805f9b34fb");
	}

	public class MiBandCharacteristic
	{
		#region GenericAccess
		public static Guid DeviceName = new Guid("00002A00-0000-1000-8000-00805f9b34fb");
		public static Guid Appereance = new Guid("00002A01-0000-1000-8000-00805f9b34fb");
		public static Guid PeripheralPrivacyFlag = new Guid("00002A02-0000-1000-8000-00805f9b34fb");
		public static Guid PeripheralPreferredConnectionParameters = new Guid("00002A04-0000-1000-8000-00805f9b34fb");
		#endregion

		#region GenericAttributeService
		public static Guid ServiceChanged = new Guid("00002A05-0000-1000-8000-00805f9b34fb");
		#endregion

		#region MainService
		public static Guid DeviceInfo = new Guid("0000ff01-0000-1000-8000-00805f9b34fb");
		public static Guid DevicePName = new Guid("0000ff02-0000-1000-8000-00805f9b34fb");
		public static Guid GeneralNotification = new Guid("0000ff03-0000-1000-8000-00805f9b34fb");
		public static Guid UserInfo = new Guid("0000ff04-0000-1000-8000-00805f9b34fb");
		public static Guid ControlPoint = new Guid("0000ff05-0000-1000-8000-00805f9b34fb");
		public static Guid RealtimeSteps = new Guid("0000ff06-0000-1000-8000-00805f9b34fb");
		public static Guid ActivityData = new Guid("0000ff07-0000-1000-8000-00805f9b34fb");
		public static Guid FirmwareData = new Guid("0000ff08-0000-1000-8000-00805f9b34fb");
		public static Guid LeParams = new Guid("0000ff09-0000-1000-8000-00805f9b34fb");
		public static Guid DateTime = new Guid("0000ff0a-0000-1000-8000-00805f9b34fb");
		public static Guid Statistics = new Guid("0000ff0b-0000-1000-8000-00805f9b34fb");
		public static Guid Battery = new Guid("0000ff0c-0000-1000-8000-00805f9b34fb");
		public static Guid Test = new Guid("0000ff0d-0000-1000-8000-00805f9b34fb");
		public static Guid SensorData = new Guid("0000ff0e-0000-1000-8000-00805f9b34fb");
		public static Guid Pair = new Guid("0000ff0f-0000-1000-8000-00805f9b34fb");
		public static Guid Unknown00 = new Guid("0000ff10-0000-1000-8000-00805f9b34fb");
		public static Guid Unknown01 = new Guid("0000fec9-0000-1000-8000-00805f9b34fb");
		#endregion

		#region ServiceUnknown
		//MANY! fee1
		#endregion

		#region ImmediateAlertService
		public static Guid Vibration = new Guid("00002a06-0000-1000-8000-00805f9b34fb");
		#endregion
		#region HeartRateService
		public static Guid HeartRateControlPoint = new Guid("00002a39-0000-1000-8000-00805f9b34fb");
		public static Guid HeartRateMeasurement = new Guid("00002a37-0000-1000-8000-00805f9b34fb");
		#endregion
	}

	public static class MiBandDescriptor
	{
		public static Guid DescriptorUpdateNotification = new Guid("00002902-0000-1000-8000-00805f9b34fb");
	}

    public struct MiBandCommand
    {
        public static byte[] Pair = { 2 };

        public static byte[] VibrationWithLed = { 1 };
        public static byte[] Vibration10TimesWithLed = { 2 };
        public static byte[] VibrationWithoutLed = { 4 };
        public static byte[] StopVibration = { 0 };

        public static byte[] EnableRealtimeStepsNotifications = { 3, 1 };
        public static byte[] DisableRealtimeStepsNotifications = { 3, 0 };

        public static byte[] EnableSensorDataNotifications = { 18, 1 };
        public static byte[] DisableSensorDataNotifications = { 18, 0 };

        public static byte[] StartActivitySync = { 06 };
        public static byte[] StopActivitySync = { 10 };

        public static byte[] SetColorLed = { 14, 6, 1, 2, 1 };
        public static byte[] SetColorBlue = { 14, 0, 6, 6, 1 };
        public static byte[] SetColorOrange = { 14, 6, 2, 0, 1 };
        public static byte[] SetColorGreen = { 14, 4, 5, 0, 1 };

        public static byte[] StartHeartRateManual = { 21, 2, 1 };
        public static byte[] StopHeartRateManual = { 21, 2, 0 };

        public static byte[] StartHeartRateContinuous = { 21, 1, 1 };
        public static byte[] StoptHeartRateContinuous = { 21, 1, 0 };

        public static byte[] StartHeartRateSleep = { 21, 0, 1 };
        public static byte[] StoptHeartRateSleep = { 21, 0, 0 };

        //NewFirmware? Or when no UserInfo? 
        public static byte[] StartHeartRateRead = { 15, 2, 1 };

        public static byte[] Reboot = { 12 };
        public static byte[] RemoteDisconnect = { 1 };
        public static byte[] FactoryReset = { 9 };
        public static byte[] SelfTest = { 2 };
        
        public static byte[] EnableNotifications = { 0x01, 0x00 };
		public static byte[] DisableNotifications = {0x00, 0x00};
        /* COMMANDS: usually sent to UUID_CHARACTERISTIC_CONTROL_POINT characteristic */

        public static   byte SetTimer = 0x4;

        public static   byte SetFitnessGoal = 0x5;

     
        public static   byte SendFirmwareInfo = 0x7;

        public static   byte SendNotification = 0x8;

        //To upload firmware
        //public static   byte Sync = 0xb;
        //public static   byte COMMAND_STOP_SYNC_DATA = 0x11;

        public static byte[] SetWearLocation = { 15 };

     

        //public static   byte COMMAND_STOP_MOTOR_VIBRATE = 0x13;

        //public static   byte COMMAND_SET_REALTIME_STEPS_NOTIFICATION = 0x3;

        //public static   byte COMMAND_SET_REALTIME_STEP = 0x10;

        // Test HR
        public static   byte COMMAND_SET_HR_SLEEP = 0x0;
        public static   byte COMMAND_SET__HR_CONTINUOUS = 0x1;
        public static   byte COMMAND_SET_HR_MANUAL = 0x2;


        /* FURTHER COMMANDS: unchecked therefore left commented
        public static   byte COMMAND_FACTORY_RESET = 0x9t;
        public static   int COMMAND_SET_COLOR_THEME = et;
        public static   byte COMMAND_GET_SENSOR_DATA = 0x12t
        */
    }

    //TODO Implement!

    ////public static   byte ALIAS_LEN = 0xa;

    /////*NOTIFICATIONS: usually received on the UUID_CHARACTERISTIC_NOTIFICATION characteristic */

    ////public static   byte NOTIFY_NORMAL = 0x0;

    ////public static   byte NOTIFY_FIRMWARE_UPDATE_FAILED = 0x1;

    ////public static   byte NOTIFY_FIRMWARE_UPDATE_SUCCESS = 0x2;

    ////public static   byte NOTIFY_CONN_PARAM_UPDATE_FAILED = 0x3;

    ////public static   byte NOTIFY_CONN_PARAM_UPDATE_SUCCESS = 0x4;

    ////public static   byte NOTIFY_AUTHENTICATION_SUCCESS = 0x5;

    ////public static   byte NOTIFY_AUTHENTICATION_FAILED = 0x6;

    ////public static   byte NOTIFY_FITNESS_GOAL_ACHIEVED = 0x7;

    ////public static   byte NOTIFY_SET_LATENCY_SUCCESS = 0x8;

    ////public static   byte NOTIFY_RESET_AUTHENTICATION_FAILED = 0x9;

    ////public static   byte NOTIFY_RESET_AUTHENTICATION_SUCCESS = 0xa;

    ////public static   byte NOTIFY_FW_CHECK_FAILED = 0xb;

    ////public static   byte NOTIFY_FW_CHECK_SUCCESS = 0xc;

    ////public static   byte NOTIFY_STATUS_MOTOR_NOTIFY = 0xd;

    ////public static   byte NOTIFY_STATUS_MOTOR_CALL = 0xe;

    ////public static   byte NOTIFY_STATUS_MOTOR_DISCONNECT = 0xf;

    ////public static   byte NOTIFY_STATUS_MOTOR_SMART_ALARM = 0x10;

    ////public static   byte NOTIFY_STATUS_MOTOR_ALARM = 0x11;

    ////public static   byte NOTIFY_STATUS_MOTOR_GOAL = 0x12;

    ////public static   byte NOTIFY_STATUS_MOTOR_AUTH = 0x13;

    ////public static   byte NOTIFY_STATUS_MOTOR_SHUTDOWN = 0x14;

    ////public static   byte NOTIFY_STATUS_MOTOR_AUTH_SUCCESS = 0x15;

    ////public static   byte NOTIFY_STATUS_MOTOR_TEST = 0x16;

    ////// 0x18 is returned when we cancel data sync, perhaps is an ack for this message

    ////public static   byte NOTIFY_UNKNOWN = -0x1;

    ////public static   int NOTIFY_PAIR_CANCEL = 0xef;

    ////public static   int NOTIFY_DEVICE_MALFUNCTION = 0xff;

    ///* MESSAGES: unknown */

    //public static   byte MSG_CONNECTED = 0x0;

    //public static   byte MSG_DISCONNECTED = 0x1;

    //public static   byte MSG_CONNECTION_FAILED = 0x2;

    //public static   byte MSG_INITIALIZATION_FAILED = 0x3;

    //public static   byte MSG_INITIALIZATION_SUCCESS = 0x4;

    //public static   byte MSG_STEPS_CHANGED = 0x5;

    //public static   byte MSG_DEVICE_STATUS_CHANGED = 0x6;

    //public static   byte MSG_BATTERY_STATUS_CHANGED = 0x7;



    ///* CONNECTION: unknown
    //public static   CONNECTION_LATENCY_LEVEL_LOW = 0x0t;
    //public static   CONNECTION_LATENCY_LEVEL_MEDIUM = 0x1t;
    //public static   CONNECTION_LATENCY_LEVEL_HIGH = 0x2t;
    //*/

    ///* MODES: probably related to the sample data structure
    //*/

    //public static   byte MODE_REGULAR_DATA_LEN_BYTE = 0x0;

    //// was MODE_REGULAR_DATA_LEN_MINITE
    //public static   byte MODE_REGULAR_DATA_LEN_MINUTE = 0x1;

    ///* PROFILE: unknown
    //public static   PROFILE_STATE_UNKNOWN:I = 0x0
    //public static   PROFILE_STATE_INITIALIZATION_SUCCESS:I = 0x1
    //public static   PROFILE_STATE_INITIALIZATION_FAILED:I = 0x2
    //public static   PROFILE_STATE_AUTHENTICATION_SUCCESS:I = 0x3
    //public static   PROFILE_STATE_AUTHENTICATION_FAILED:I = 0x4
    //*/

    ///* TEST: unkown (maybe sent to UUID_CHARACTERISTIC_TEST characteristic?
    //public static   TEST_DISCONNECTED_REMINDER = 0x5t
    //public static   TEST_NOTIFICATION = 0x3t
    //public static   TEST_REMOTE_DISCONNECT = 0x1t
    //public static   TEST_SELFTEST = 0x2t
    //*/

}