using Plugin.BLE.Abstractions.Contracts;
using Poz1.MiBandCDK.Devices;
using Poz1.MiBandCDK.Model;
using System;

namespace Poz1.MiBandCDK
{
    public static class MiBandFactory
    {
        public static T Create<T>(IDevice device) where T : class, IMiBandBase
        {
            try
            {
                return new MiBand(device) as T;
            }
            catch
            {
                throw new InvalidOperationException("This device is either a not supported MiBand version or not a MiBand");
            }
        }
    }
}
