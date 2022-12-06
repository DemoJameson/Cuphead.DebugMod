using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public enum DevilPhaseOnePatterns {
            Random,
            Head1,
            Clap1,
            Pitchfork1,
            Clap2,
            Head2,
            Clap3,
            Pitchfork2
        }

        public enum DevilPhaseOneHeadTypes {
            Random,
            Dragon,
            Spider
        }

        public enum DevilPhaseOnePitchforkTypes {
            Random,
            Bouncer,
            Pinwheel,
            Ring
        }

        public enum DevilPhaseOneDragonDirections {
            Random,
            Left,
            Right
        }

        public enum FlyingMermaidPhaseOnePatterns {
            Random,
            Ghosts,
            Summon1,
            Fish,
            Summon2
        }

#if v1_3

        public enum RelicLevels {
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
