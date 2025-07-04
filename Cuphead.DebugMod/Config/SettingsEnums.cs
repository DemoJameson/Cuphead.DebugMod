﻿using System.ComponentModel;

namespace BepInEx.CupheadDebugMod.Config {
    public static class SettingsEnums {

        public enum LobberCritSettings {
            Random,
            Always,
            Never
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

        public enum FrogsPhaseOnePatterns {
            Random,
            Punches,
            Fireflies
        }

        public enum FrogsPhaseOneFirefliesPatternsEasy {
            Random,
            [Description("2-1-1 (Slow)")]
            Two_One_One_Slow,
            [Description("1-1-2")]
            One_One_Two,
            [Description("2-1-1 (Fast)")]
            Two_One_One_Fast
        }
        public enum FrogsPhaseOneFirefliesPatternsNormal {
            Random,
            [Description("2-2-1-2")]
            Two_Two_One_Two,
            [Description("2-2-2")]
            Two_Two_Two,
            [Description("2-1-2-2")]
            Two_One_Two_Two,
            [Description("2-2-1")]
            Two_Two_One,
        }

        public enum FrogsPhaseOneFirefliesPatternsHard {
            Random,
            [Description("2-2-1")]
            Two_Two_One,
            [Description("1-1-2")]
            One_One_Two,
            [Description("2-2 (Fast)")]
            Two_Two_Fast,
            [Description("2-1-1")]
            Two_One_One,
            [Description("1-2-2")]
            One_Two_Two,
            [Description("2-2 (Slow)")]
            Two_Two_Slow,
            [Description("1-2-1")]
            One_Two_One
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

        public enum BaronessMinibossesEasy {
            Random,
            Gumball,
            Waffle,
            CandyCorn,
            Jawbreaker
        }

        public enum BaronessMinibossesNormal {
            Random,
            Gumball,
            Waffle,
            CandyCorn,
            Cupcake,
            Jawbreaker
        }

        public enum BaronessMinibossesHard {
            Random,
            Gumball,
            Waffle,
            CandyCorn,
            Cupcake,
            Jawbreaker
        }

        public enum FlyingBirdPhaseOneDirections {
            Random,
            Up,
            Down
        }


        public enum FlyingBirdPhaseOnePatternsEasy {
            Random,
            [Description("Eggs")]
            Eggs1,
            [Description("Lasers")]
            Lasers1,
            [Description("Eggs")]
            Eggs2,
            [Description("Eggs")]
            Eggs3,
            [Description("Lasers")]
            Lasers2
        }

        public enum FlyingBirdPhaseTwoPatternsEasy {
            Random,
            [Description("Eggs")]
            Eggs1,
            [Description("Eggs")]
            Eggs2,
            [Description("Eggs")]
            Eggs3,
            [Description("Lasers")]
            Lasers1,
            [Description("Eggs")]
            Eggs4,
            [Description("Eggs")]
            Eggs5,
            [Description("Eggs")]
            Eggs6,
            [Description("Eggs")]
            Eggs7,
            [Description("Lasers")]
            Lasers2
        }

        public enum FlyingBirdPhaseOnePatternsNormal {
            Random,
            [Description("Eggs")]
            Eggs1,
            [Description("Lasers")]
            Lasers1
        }

        public enum FlyingBirdPhaseTwoPatternsNormal {
            Random,
            [Description("Eggs")]
            Eggs1,
            [Description("Eggs")]
            Eggs2,
            [Description("Eggs")]
            Eggs3,
            [Description("Lasers")]
            Lasers1,
            [Description("Eggs")]
            Eggs4,
            [Description("Eggs")]
            Eggs5,
            [Description("Eggs")]
            Eggs6,
            [Description("Eggs")]
            Eggs7,
            [Description("Lasers")]
            Lasers2
        }

        public enum FlyingBirdPhaseOnePatternsHard {
            Random,
            [Description("Eggs")]
            Eggs01,
            [Description("Eggs")]
            Eggs02,
            [Description("Eggs")]
            Eggs03,
            [Description("Lasers")]
            Lasers01,
            [Description("Eggs")]
            Eggs04,
            [Description("Eggs")]
            Eggs05,
            [Description("Lasers")]
            Lasers02,
            [Description("Eggs")]
            Eggs06,
            [Description("Eggs")]
            Eggs07,
            [Description("Eggs")]
            Eggs08,
            [Description("Lasers")]
            Lasers03,
            [Description("Eggs")]
            Eggs09,
            [Description("Eggs")]
            Eggs10,
            [Description("Lasers")]
            Lasers04,
            [Description("Eggs")]
            Eggs11,
            [Description("Eggs")]
            Eggs12,
            [Description("Eggs")]
            Eggs13,
            [Description("Lasers")]
            Lasers05,
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

        public enum BeePhaseTwoOrbsDirections {
            Random,
            Left,
            Right
        }

        public enum BeePhaseTwoTrianglesDirections {
            Random,
            Left,
            Right
        }

        public enum RobotPhaseFinalGemColors {
            Random,
            Red,
            Blue
        }

        public enum MousePhaseOnePatternsEasy {
            Random,
            Dash1,
            Catapult1,
            CherryBomb1,
            Dash2,
            CherryBomb2,
            Catapult2,
            Dash3,
            Catapult3
        }

        public enum MousePhaseOnePatternsNormal {
            Random,
            CherryBomb1,
            Dash1,
            Catapult1,
            Dash2,
            CherryBomb2,
            Catapult2,
            Dash3,
            CherryBomb3,
            Catapult3,
            CherryBomb4,
            Dash4,
            CherryBomb5,
            Catapult4,
            Dash5
        }

        public enum MousePhaseOnePatternsHard {
            Random,
            CherryBomb1,
            Catapult1,
            Dash1,
            CherryBomb2,
            Dash2,
            Catapult2,
            Dash3,
            Catapult3,
            CherryBomb3,
            Dash4
        }

        public enum PiratePhaseThreeGunPatternsEasy {
            Random,
            [Description("1-2")]
            One_Two,
            [Description("2-1")]
            Two_One,
            [Description("3")]
            Three
        }

        public enum PiratePhaseFourGunPatternsEasy {
            Random,
            [Description("1-1 #1")]
            One_One_1,
            [Description("1-1 #2")]
            One_One_2,
            [Description("2 (Double as likely!)")]
            Two,
            [Description("1-1 Longer")]
            One_One_Longer,
            [Description("1-1 Shorter")]
            One_One_Shorter
        }

        public enum PiratePhaseSevenGunPatternsEasy {
            Random,
            [Description("2-1-2 #1")]
            Two_One_Two_1,
            [Description("2-2-1 (Double as likely!)")]
            Two_Two_One,
            [Description("3")]
            Three,
            [Description("1-2-2")]
            One_Two_Two,
            [Description("1-3")]
            One_Three,
            [Description("2-1-2 #2")]
            Two_One_Two_2,
            [Description("4")]
            Four
        }

        public enum PiratePhaseOneGunPatternsNormal {
            Random,
            [Description("3-1")]
            Three_One,
            [Description("2-2")]
            Two_Two,
            [Description("1-3")]
            One_Three
        }

        public enum PiratePhaseTwoGunPatternsNormal {
            Random,
            [Description("1-1-2")]
            One_One_Two,
            [Description("2-1-1 (Bugged!)")]
            Two_One_One
        }

        public enum PiratePhaseThreeGunPatternsNormal {
            Random,
            [Description("2-1")]
            Two_One,
            [Description("1-2")]
            One_Two,
        }

        public enum PiratePhaseOneGunPatternsHard {
            Random,
            [Description("2-3 Longer")]
            Two_Three_Longer,
            [Description("2-3 Shorter")]
            Two_Three_Shorter,
            [Description("3-2 #1")]
            Three_Two_1,
            [Description("3-2 #2")]
            Three_Two_2,
            [Description("4")]
            Four
        }

        public enum PiratePhaseTwoGunPatternsHard {
            Random,
            [Description("3-1")]
            Three_One,
            [Description("4 (Double as likely!)")]
            Four,
            [Description("2-2")]
            Two_Two,
            [Description("1-3")]
            One_Three
        }

        public enum PiratePhaseThreeGunPatternsHard {
            Random,
            [Description("2 (Double as likely!)")]
            Two,
            [Description("3")]
            Three,
            [Description("4")]
            Four
        }

        public enum PiratePhaseFourPatternsEasy {
            Random,
            Peashot,
            Shark
        }

        public enum PiratePhaseSevenPatternsEasy {
            Random,
            [Description("Peashot")]
            Peashot01,
            [Description("Peashot")]
            Peashot02,
            [Description("Peashot")]
            Peashot03,
            [Description("Shark")]
            Shark01,
            [Description("Peashot")]
            Peashot04,
            [Description("Peashot")]
            Peashot05,
            [Description("Shark")]
            Shark02,
            [Description("Peashot")]
            Peashot06,
            [Description("Peashot")]
            Peashot07,
            [Description("Shark")]
            Shark03,
            [Description("Peashot")]
            Peashot08,
            [Description("Peashot")]
            Peashot09,
            [Description("Peashot")]
            Peashot10,
            [Description("Shark")]
            Shark04,
        }

        public enum PiratePhaseTwoPatternsNormalHard {
            Random,
            [Description("Peashot -> Shark")]
            PeashotShark,
            Shark,
            [Description("Peashot -> Squid")]
            PeashotSquid,
            Squid,
            [Description("Peashot -> Dogfish")]
            PeashotDogfish,
            Dogfish
        }

        public enum PiratePhaseThreePatternsNormalHard {
            Random,
            [Description("Peashot -> Shark")]
            PeashotShark,
            Shark,
            [Description("Peashot -> Squid")]
            PeashotSquid,
            Squid,
            [Description("Peashot -> Dogfish")]
            PeashotDogfish,
            Dogfish
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

        public enum TrainPumpkinStartingDirections {
            Random,
            Left,
            Right
        }

        public enum TrainStartingGhouls {
            Random,
            Left,
            Right
        }

        public enum DicePalaceHeartPositions1 {
            Random,
            [Description("1")]
            One,
            [Description("2")]
            Two,
            [Description("3")]
            Three
        }

        public enum DicePalaceHeartPositions2 {
            Random,
            [Description("4")]
            Four,
            [Description("5")]
            Five,
            [Description("6")]
            Six
        }

        public enum DicePalaceHeartPositions3 {
            Random,
            [Description("7")]
            Seven,
            [Description("8")]
            Eight,
            [Description("9")]
            Nine
        }

        public enum DicePalaceCigarSpitAttackCountsNormal {
            Random,
            [Description("1")]
            One1,
            [Description("2")]
            Two1,
            [Description("1")]
            One2,
            [Description("2")]
            Two2,
            [Description("2")]
            Two3,
            [Description("3")]
            Three1,
            [Description("1")]
            One3,
            [Description("2")]
            Two4,
        }

        public enum DicePalaceCigarSpitAttackCountsHard {
            Random,
            [Description("2")]
            Two1,
            [Description("1")]
            One1,
            [Description("3")]
            Three1,
            [Description("2")]
            Two2,
            [Description("3")]
            Three2,
            [Description("1")]
            One2,
            [Description("3")]
            Three3,
            [Description("3")]
            Three4,
            [Description("2")]
            Two3,
        }

        public enum DicePalaceRabbitPatterns {
            Random,
            [Description("Wand")]
            Wand01,
            [Description("Parry")]
            Parry01,
            [Description("Wand")]
            Wand02,
            [Description("Wand")]
            Wand03,
            [Description("Parry")]
            Parry02,
            [Description("Wand")]
            Wand04,
            [Description("Wand")]
            Wand05,
            [Description("Parry")]
            Parry03,
            [Description("Wand")]
            Wand06,
            [Description("Parry")]
            Parry04,
            [Description("Wand")]
            Wand07,
            [Description("Parry")]
            Parry05,
            [Description("Wand")]
            Wand08,
            [Description("Wand")]
            Wand09,
            [Description("Parry")]
            Parry06,
            [Description("Wand")]
            Wand10,
            [Description("Wand")]
            Wand11,
            [Description("Wand")]
            Wand12,
            [Description("Parry")]
            Parry07,
            [Description("Wand")]
            Wand13,
            [Description("Wand")]
            Wand14,
            [Description("Parry")]
            Parry08
        }

        public enum DicePalaceRabbitParryDirections {
            Random,
            Top,
            Bottom
        }

        public enum DicePalaceRoulettePatterns {
            Random,
            Twirl,
            Marble
        }

        public enum DicePalaceRouletteTwirlAmountsNormal {
            Random,
            [Description("4")]
            Four,
            [Description("5")]
            Five
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

        public enum DevilPhaseOneSpiderHopCounts {
            Random,
            [Description("3")]
            Num_3,
            [Description("4")]
            Num_4,
            [Description("5")]
            Num_5

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
