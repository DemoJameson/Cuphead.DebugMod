using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Configuration;

namespace BepInEx.CupheadDebugMod.Components.RNG {
    internal class Utility {
        public static int GetUserPattern<T>(int value) where T : struct, IConvertible {
            if (value == 1)
                return Enum.GetValues(typeof(T)).Length - 2;
            return value - 2;
        }
    }
}
