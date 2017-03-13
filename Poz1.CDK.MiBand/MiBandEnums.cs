namespace Poz1.CDK.MiBand
{
    public enum MiBandModel
    {
        MI1,
        MI1A,
        MI1S
    }

    public enum MiBandActivity
    {
        Silent, //NotSure
        Walking,
        Running,
        NotWorn,
        LightSleep,
        DeepSleep,
        Charging,
        OnBed
    }

    public enum LedColor
    {
        Red,
        Blue,
        Orange,
        Green
    }

    public enum VibrationMode
    {
        VibrationWithLed,
        Vibration10TimesWithLed,
        VibrationWithoutLed
    }

    public enum HeartRateMode
    {
        Spot,
        Countinous,
        Sleep
    }

    public enum NotificationType
    {
        Standard,
        RealTimeSteps,
        ActivityData,
        Battery,
        SensorData,
        HeartRateMeasurement
    }

    //Normal should not exist
    public enum BatteryStatus
    {
        Normal,
        Low,
        Charging,
        Full,
        NotCharging
    }

    public enum Gender
    {
        Male,
        Female
    }

    public enum WearLocation
    {
        Left,
        Right,
        Neck
    }
}