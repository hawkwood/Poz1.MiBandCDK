using Poz1.MiBandCDK.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poz1.MiBandCDK.Devices
{
    public interface IMiBand1 : IMiBandBase
    {
        bool IsConnected { get; }
        Task SetLedColorAsync(LedColor color);
    }
}
