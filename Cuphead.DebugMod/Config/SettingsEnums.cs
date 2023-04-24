using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace BepInEx.CupheadDebugMod.Config {
    public static class SettingsEnums {

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

        public enum FlyingBlimpPhaseBlimp2PatternsEasy {
            Random,
            Tornado1,
            Shoot1,
            Shoot2,
        }

        public enum FlyingBlimpPhaseBlimp3PatternsEasy {
            Random,
            Shoot1,
            Shoot2,
            Tornado1,
            Shoot3,
            Shoot4,
            Shoot5,
            Tornado2,
        }

        public enum FlyingBlimpPhaseBlimp2PatternsNormal {
            Random,
            Tornado1,
            Shoot1,
            Shoot2,
            Shoot3,
            Tornado2,
            Shoot4,
            Shoot5
        }
        public enum FlyingBlimpPhaseBlimp3PatternsNormal {
            Random,
            Shoot1,
            Tornado1,
            Shoot2,
            Shoot3,
            Tornado2,
        }

        public enum FlyingBlimpPhaseBlimp2PatternsHard {
            Random,
            Shoot1,
            Tornado1,
            Shoot2,
            Shoot3,
            Tornado2,
        }

        public enum FlyingBlimpPhaseBlimp3PatternsHard {
            Random,
            Shoot1,
            Shoot2,
            Tornado1,
            Shoot3,
            Shoot4,
            Shoot5,
            Tornado2
        }

        public enum FlowerPhaseGeneric1PatternsNormal {
            Random,
            HeadLunge,
            GatlingGun
        }

        public enum FlowerPhaseGeneric2PatternsNormal {
            Random,
            HeadLunge,
            PodHands
        }

        public enum FlowerPhaseGeneric3PatternsNormal {
            Random,
            PodHands,
            GatlingGun
        }

        public enum FlowerPhaseGenericHeadLungePatternsNormal {
            Random,
            Top1,
            Bottom1,
            Bottom2,
            Top2,
            Bottom3,
            Bottom4,
            Top3,
            Bottom5,
            Bottom6
        }

        public enum FlyingBirdPhaseFinalPatterns {
            Random,
            Garbage,
            Heart
        }

        public enum FlyingBirdPhaseFinalDirections {
            Random,
            Right,
            Left
        }

        public enum FlyingGeniePhaseOneTreasurePatterns {
            Random,
            Swords,
            Gems,
            Sphinx
        }

        public enum ClownDashDelaysEasy {
            Random,
            [Description("5B")]
            A_3_3,
            [Description("3B")]
            B_1_8,
            [Description("5B")]
            C_3_5,
            [Description("4B+")]
            D_3,
            [Description("2B")]
            E_1_5,
            [Description("4B")]
            F_2_7,
            [Description("4B+")]
            G_3,
            [Description("3B")]
            H_2,
            [Description("2B")]
            J_1_5
        }

        public enum ClownDashDelaysNormal {
            Random,
            [Description("5B")]
            A_3_3,
            [Description("3B++")]
            B_2_4,
            [Description("5B")]
            C_3_5,
            [Description("4B+")]
            D_3,
            [Description("2B+")]
            E_1_5,
            [Description("5B")]
            F_3_5,
            [Description("4B+")]
            G_3,
            [Description("5B+")]
            H_3_8,
            [Description("3B")]
            J_2,
            [Description("2B+")]
            K_1_5
        }

        public enum ClownDashDelaysHard {
            Random,
            [Description("5B")]
            A_3_3,
            [Description("5B")]
            B_3_6,
            [Description("3B")]
            C_2_2,
            [Description("5B")]
            D_3_5,
            [Description("4B+")]
            E_3,
            [Description("2B")]
            F_1_5,
            [Description("5B")]
            G_3_5,
            [Description("3B")]
            H_2,
            [Description("5B+")]
            J_3_8,
            [Description("3B")]
            K_2,
            [Description("2B")]
            L_1_5
        }

        public enum BeePhaseTwoPatternsEasy {
            Random,
            Orbs1,
            Triangles1,
            Orbs2,
            Triangles2,
            Triangles3,
            Orbs3,
            Triangles4,
            Orbs4,
            Triangles5,
            Orbs5,
            Orbs6
        }

        public enum BeePhaseTwoPatternsNormal {
            Random,
            Orbs,
            Triangles,
            Chain
        }

        public enum BeePhaseTwoPatternsHard {
            Random,
            Orbs,
            Chain1,
            Triangles,
            Chain2
        }

        public enum PiratePhaseOneGunPatternsNormal {
            Random,
            [Description("3-Pause-1")]
            ThreePOne,
            [Description("2-Pause-2")]
            TwoPTwo,
            [Description("1-Pause-3")]
            OnePThree
        }
        public enum FlyingMermaidPhaseOneFirstPatternsEasy {
            Random,
            Ghosts,
            Summon,
        }

        public enum FlyingMermaidPhaseOneSecondPatternsEasy {
            Random,
            Fish,
            Summon
        }

        public enum FlyingMermaidPhaseOnePatternsNormalHard {
            Random,
            Ghosts,
            Summon1,
            Fish,
            Summon2
        }

        public enum FlyingMermaidPhaseOneFishPatterns {
            Random,
            Yellow,
            Red
        }

        public enum FlyingMermaidPhaseOneSummonPatterns {
            Random,
            Seahorse,
            Pufferfish,
            Turtle
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

        public enum DevilPhaseOneDragonDirections {
            Random,
            Left,
            Right
        }

        public enum DevilPhaseOnePitchforkTypes {
            Random,
            Bouncer,
            Pinwheel,
            Ring
        }

        public enum DevilPhaseOneSpiderOffsets {
            Random,
            [Description("-150")]
            A_Neg150,
            [Description("50")]
            B_50,
            [Description("-50")]
            C_Neg50,
            [Description("300")]
            D_300,
            [Description("-200")]
            E_Neg200,
            [Description("50")]
            F_50,
            [Description("150")]
            G_150,
            [Description("-300")]
            H_Neg300,
            [Description("0")]
            I_0,
            [Description("100")]
            J_100,
            [Description("-50")]
            K_Neg50,
            [Description("200")]
            L_200,
            [Description("50")]
            M_50,
            [Description("0")]
            N_0,
            [Description("100")]
            O_100,
            [Description("-150")]
            P_Neg150,
            [Description("50")]
            Q_50,
            [Description("-250")]
            R_Neg250,
            [Description("200")]
            S_200,
            [Description("0")]
            T_0
        }
        public enum DevilPhaseTwoPatternsNormal {
            Random,
            BombEye01,
            SkullEye01,
            BombEye02,
            SkullEye02,
            BombEye03,
            SkullEye03,
            SkullEye04,
            BombEye04,
            SkullEye05,
            SkullEye06,
            SkullEye07,
            BombEye05,
            SkullEye08,
            BombEye06,
            SkullEye09,
            BombEye07,
            SkullEye10,
            SkullEye11
        }

        public enum DevilPhaseTwoPatternsHard {
            Random,
            BombEye01,
            SkullEye01,
            BombEye02,
            BombEye03,
            SkullEye02,
            BombEye04,
            SkullEye03,
            SkullEye04,
            BombEye05,
            SkullEye05,
            BombEye06,
            BombEye07,
            SkullEye06,
            BombEye08,
            SkullEye07,
            BombEye09,
            SkullEye08,
            SkullEye09,
        }

        public enum DevilPhaseTwoBombEyeDirections { 
            Random,
            Left,
            Right
        }

#if v1_3

        public enum SaltbakerPhaseOnePatterns {
            Random,
            Dough01,
            Limes01,
            Strawberries01,
            Sugarcubes01,
            Limes02,
            Dough02,
            Sugarcubes02,
            Strawberries02,
            Limes03,
            Dough03,
            Strawberries03,
            Sugarcubes03,
            Dough04,
            Limes04,
            Sugarcubes04,
            Strawberries04,
            Limes05,
            Sugarcubes05,
            Limes06,
            Strawberries05,
            Limes07
        }

        public enum SaltbakerPhaseThreeSawPatterns {
            Random,
            Left,
            Right
        }

#endif


    }
}
