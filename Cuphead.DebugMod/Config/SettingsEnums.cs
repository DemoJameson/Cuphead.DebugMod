using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BepInEx.CupheadDebugMod.Config {
    public static class SettingsEnums {

        public enum FrogsPhaseOnePatterns {
            Random,
            Punches,
            Fireflies
        }

        #if v1_3

        private enum RelicLevels {
            Default,
            BrokenRelic,
            [Description("Cursed Relic 1")]
            CursedRelic1,
            [Description("Cursed Relic 2")]
            CursedRelic2,
            [Description("Cursed Relic 3")]
            CursedRelic3,
            [Description("Cursed Relic 4")]
            CursedRelic4,
            DivineRelic
        }

        #endif


    }
}
