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
        public enum FrogsPhaseFinalPatterns {
            Random,
            Snake,
            Tiger,
            Bison
        }
        public enum FlyingBlimpPhaseBlimp2Patterns {
            Random,
            Tornado1,
            Shoot1,
            Shoot2,
            Shoot3,
            Tornado2,
            Shoot4,
            Shoot5
        }
        public enum FlyingBlimpPhaseBlimp3Patterns {
            Random,
            Shoot1,
            Tornado1,
            Shoot2,
            Shoot3,
            Tornado2,
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
