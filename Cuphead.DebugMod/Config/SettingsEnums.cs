using System.ComponentModel;

namespace BepInEx.CupheadDebugMod.Config {
    public static class SettingsEnums {

        public enum LobberCritSettings {
            Random,
            Always,
            Never
        }

        public enum RngOverride {
            Custom,
            Vanilla,
            [Description("Perfect RNG (1.1+ Any%)")]
            PerfectRng11
        }

#if v1_3
        public enum RelicLevels {
            Default,
            [Description("Broken Relic")]
            BrokenRelic,
            [Description("Cursed Relic 1")]
            CursedRelic1,
            [Description("Cursed Relic 2")]
            CursedRelic2,
            [Description("Cursed Relic 3")]
            CursedRelic3,
            [Description("Cursed Relic 4")]
            CursedRelic4,
            [Description("Divine Relic")]
            DivineRelic
        }
#endif

        public enum DragonPhaseOnePatternsEasy {
            Random,
            Peashot1,
            Meteor1,
            Peashot2,
            Peashot3,
            Meteor2,
            Peashot4,
            Meteor3,
            Peashot5,
            Peashot6,
            Peashot7,
            Meteor4
        }

        public enum DragonPhaseTwoPatternsEasy {
            Random,
            Peashot1,
            Meteor1,
            Peashot2,
            Peashot3,
            Meteor2,
            Peashot4,
            Meteor3,
            Peashot5,
            Peashot6,
            Meteor4
        }

        public enum DragonPhaseThreePatternsNormal {
            Random,
            Meteor,
            Peashot
        }

        public enum DragonPhaseOnePatternsHard {
            Random,            
            Peashot,
            Meteor
        }

        public enum DragonPhaseTwoPatternsHard {
            Random,
            Meteor,
            Peashot
        }

        public enum DragonPhaseOneMeteorPatternsEasy {
            Random,
            [Description("Up-Down (Thrice as likely!)")]
            UD,
            [Description("Down-Up-Down (Thrice as likely!)")]
            DUD,
            [Description("Down-Up-Down (Double as likely!)")]
            UDU,
            [Description("Down-Down")]
            DD,
            [Description("Up-Up")]
            UU,
        }

        public enum DragonPhaseTwoMeteorPatternsEasy {
            Random,
            [Description("Up-Down (Triple as likely!)")]
            UD,
            [Description("Down-Up-Down (Triple as likely!)")]
            DUD,
            [Description("Down-Up-Down (Double as likely!)")]
            UDU,
            [Description("Down-Down")]
            DD,
            [Description("Up-Up")]
            UU,
        }

        public enum DragonPhaseTwoMeteorPatternsNormal {
            Random,
            [Description("Up-Down-Up-Down")]
            UDUD,
            [Description("Down-Up-Down")]
            DUD,
            [Description("Down-Up-Down-Up")]
            DUDU,
            [Description("Up-Down-Up")]
            UDU
        }

        public enum DragonPhaseThreeMeteorPatternsNormal {
            Random,
            [Description("Up-Both")]
            UB,
            [Description("Both-Down")]
            BD,
            [Description("Down-Both")]
            DB,
            [Description("Both-Up")]
            BU
        }

        public enum DragonPhaseOneMeteorPatternsHard {
            Random,
            [Description("Up-Down (Double as likely!)")]
            UD,
            [Description("Down-Up-Down")]
            DUD,
            [Description("Down-Up")]
            DU,
            [Description("Up-Down-Up")]
            UDU
        }

        public enum DragonPhaseTwoMeteorPatternsHard {
            Random,
            [Description("Up-Up-Down (Double as likely!)")]
            UUD,
            [Description("Down-Up-Down (Double as likely!)")]
            DUU,
            [Description("Down-Up-Both")]
            DUB
        }

        public enum DragonPhaseOneLaserPatternsEasy {
            Random,
            [Description("2 (Faster) #1")]
            Two_1,
            [Description("2 (Slower)")]
            Two_2,
            [Description("1 (Faster)")]
            One_1,
            [Description("2 (Faster) #2")]
            Two_3,
            [Description("1 (Slower)")]
            One_2
        }

        public enum DragonPhaseTwoLaserPatternsEasy {
            Random,
            [Description("2 (Faster) #1")]
            Two_1,
            [Description("2 (Slower)")]
            Two_2,
            [Description("1 (Faster)")]
            One_1,
            [Description("2 (Faster) #2")]
            Two_3,
            [Description("1 (Slower)")]
            One_2
        }

        public enum DragonPhaseOneLaserPatternsNormal {
            Random,
            [Description("3")]
            Three,
            [Description("2 (Slower)")]
            Two_1,
            [Description("2 (Faster)")]
            Two_2
        }

        public enum DragonPhaseThreeLaserPatternsNormal {
            Random,
            [Description("2")]
            Two,
            [Description("1 (Double as likely!)")]
            One
        }

        public enum DragonPhaseOneLaserPatternsHard {
            Random,
            [Description("2 #1")]
            Two_1,
            [Description("2 #2")]
            Two_2,
            [Description("3")]
            Three
        }

        public enum DragonPhaseTwoLaserPatternsHard {
            Random,
            [Description("2 #1")]
            Two_1,
            [Description("2 #2")]
            Two_2,
            [Description("3")]
            Three
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

        public enum SallyStageplayPatternsEasy {
            Random,
            Jump1,
            Kiss1,
            Teleport1,
            Jump2,
            Kiss2,
            Jump3,
            Teleport2,
            Kiss3,
            Jump4,
            Teleport3
        }

        public enum SallyStageplayPatternsNormalHard {
            Random,
            Jump1,
            Kiss1,
            Teleport1,
            Jump2,
            Kiss2,
            Jump3,
            Teleport2
        }

        public enum SallyStageplayJumpTypesEasy {
            Random,
            DiveKick1,
            DiveKick2,
            DoubleJump1,
            DiveKick3,
            DiveKick4,
            DiveKick5,
            DoubleJump2
        }

        public enum SallyStageplayJumpTypesNormalHard {
            Random,
            DiveKick1,
            DiveKick2,
            DoubleJump1,
            DiveKick3,
            DoubleJump2,
            DiveKick4,
            DiveKick5,
            DiveKick6,
            DoubleJump3
        }

        public enum SallyStageplayJumpCountsEasy {
            Random,
            [Description("1")]
            One_1,
            [Description("3")]
            Three_1,
            [Description("2")]
            Two_1,
            [Description("3")]
            Three_2,
            [Description("1")]
            One_2,
            [Description("2")]
            Two_2,
            [Description("3")]
            Three_3,
            [Description("2")]
            Two_3,
            [Description("2")]
            Two_4
        }

        public enum SallyStageplayJumpCountsNormalHard {
            Random,
            [Description("1")]
            One_1,
            [Description("3")]
            Three_1,
            [Description("2")]
            Two_1,
            [Description("3")]
            Three_2,
            [Description("1")]
            One_2,
            [Description("2")]
            Two_2,
            [Description("3")]
            Three_3,
            [Description("2")]
            Two_3,
            [Description("2")]
            Two_4,
            [Description("3")]
            Three_4
        }

        public enum SallyStageplayTeleportOffsetsEasy {
            Random,
            [Description("0")]
            A_0,
            [Description("100")]
            B_100,
            [Description("-100")]
            C_Neg100,
            [Description("0")]
            D_0,
            [Description("50")]
            E_50,
            [Description("0")]
            F_0,
            [Description("-50")]
            G_Neg50
        }

        public enum SallyStageplayTeleportOffsetsNormalHard {
            Random,
            [Description("0")]
            A_0,
            [Description("100")]
            B_100,
            [Description("-100")]
            C_Neg100,
            [Description("0")]
            D_0,
            [Description("200")]
            E_200,
            [Description("-200")]
            F_Neg200,
            [Description("50")]
            G_50,
            [Description("-200")]
            H_Neg100,
            [Description("0")]
            I_0,
            [Description("150")]
            J_150
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

        public enum MouseCherryBombPatternsNormal {
            Random,
            [Description("2-2-2 #1")]
            Two_Two_Two_1,
            [Description("3-2")]
            Three_Two,
            [Description("2-2-2 #2")]
            Two_Two_Two_2,
            [Description("2-3")]
            Two_Three
        }

        public enum MouseCherryBombPatternsHard {
            Random,
            [Description("3-3 #1")]
            Three_Three_1,
            [Description("4-2")]
            Four_Two,
            [Description("2-2-2 #1")]
            Two_Two_Two_1,
            [Description("3-3 #2")]
            Three_Three_2,
            [Description("2-4")]
            Two_Four,
            [Description("2-2-2 #2")]
            Two_Two_Two_2
        }

        public enum MouseCatapultPatternsEasy {
            Random,
            CGGGN,
            GGGNC,
            CGGNG,
            BGGCG
        }

        public enum MouseCatapultPatternsNormal {
            Random,
            BNGCG,
            CGPGC,
            PGGCN,
            BPGGN
        }

        public enum MouseCatapultPatternsHard {
            Random,
            BNGCG,
            CBGGN,
            NGGCB,
            CGBGN,
            NBGGC
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
            Peashot01,
            Peashot02,
            Peashot03,
            Shark01,
            Peashot04,
            Peashot05,
            Shark02,
            Peashot06,
            Peashot07,
            Shark03,
            Peashot08,
            Peashot09,
            Peashot10,
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

        public enum DicePalaceChipsPatternsNormal {
            Random,
            [Description("1-2-3-4,5-6-7-8")] OneTwoThreeFour_FiveSixSevenEight,
            [Description("1-8,2-7,3-4-5-6")] OneEight_TwoSeven_ThreeFourFiveSix,
            [Description("3-4-5,1-2-6-7-8")] ThreeFourFive_OneTwoSixSevenEight,
            [Description("5-6-7-8,1-2-3-4")] FiveSixSevenEight_OneTwoThreeFour,
            [Description("5-6-7,1-2-8,3-4")] FiveSixSeven_OneTwoEight_ThreeFour,
            [Description("1-2-3-4-5-6,7-8")] OneTwoThreeFourFiveSix_SevenEight,
            [Description("1-2-8,7-6,3-4-5")] OneTwoEight_SevenSix_ThreeFourFive,
            [Description("2-3-4-5,1-6-7-8")] TwoThreeFourFive_OneSixSevenEight,
            [Description("8-5,2-3-4,1-6-7")] EightFive_TwoThreeFour_OneSixSeven,
            [Description("1-8,2-3-4-5,6-7")] OneEight_TwoThreeFourFive_SixSeven,
            [Description("3-4-5,1-6-7,2-8")] ThreeFourFive_OneSixSeven_TwoEight,
            [Description("6-7-8,3-4-5,1-2")] SixSevenEight_ThreeFourFive_OneTwo,
            [Description("5-6-7,1-8,2-3-4")] FiveSixSeven_OneEight_TwoThreeFour,
            [Description("1-2-7-8,3-4-5-6")] OneTwoSevenEight_ThreeFourFiveSix
        }

        public enum DicePalaceChipsPatternsHard {
            Random,
            [Description("1-2-8,3-4-5,6-7")] OneTwoEight_ThreeFourFive_SixSeven,
            [Description("1-2,3-8,4-5-6-7")] OneTwo_ThreeEight_FourFiveSixSeven,
            [Description("1-2-3-4,5-6-7-8")] OneTwoThreeFour_FiveSixSevenEight,
            [Description("2-4-6-1,3-8,5-7")] TwoFourSixOne_ThreeEight_FiveSeven,
            [Description("1-7-8,2-3,4-5-6")] OneSevenEight_TwoThree_FourFiveSix,
            [Description("2-3-8,1-5-6,4-7")] TwoThreeEight_OneFiveSix_FourSeven,
            [Description("3-4-5-6,1-2-7-8")] ThreeFourFiveSix_OneTwoSevenEight,
            [Description("4-5-6-7,1-2-3-8")] FourFiveSixSeven_OneTwoThreeEight,
            [Description("5-6-7-8,1-2-3-4")] FiveSixSevenEight_OneTwoThreeFour_1,
            [Description("2-3-8-1,4-5-6-7")] TwoThreeEightOne_FourFiveSixSeven,
            [Description("3-4-5,1-2-6-7-8")] ThreeFourFive_OneTwoSixSevenEight,
            [Description("1-8,3-4-5,2-6-7")] OneEight_ThreeFourFive_TwoSixSeven,
            [Description("4-5-6,1-2-3-7-8")] FourFiveSix_OneTwoThreeSevenEight,
            [Description("5-6-7-8,1-2-3-4")] FiveSixSevenEight_OneTwoThreeFour_2,
            [Description("2-3-4,1-5-6-7-8")] TwoThreeFour_OneFiveSixSevenEight,
            [Description("3-4-5-6,1-8,2-7")] ThreeFourFiveSix_OneEight_TwoSeven,
            [Description("1-2-3-8,4-5-6-7")] OneTwoThreeEight_FourFiveSixSeven
        }

        public enum DicePalaceRabbitPatterns {
            Random,
            Wand01,
            Parry01,
            Wand02,
            Wand03,
            Parry02,
            Wand04,
            Wand05,
            Parry03,
            Wand06,
            Parry04,
            Wand07,
            Parry05,
            Wand08,
            Wand09,
            Parry06,
            Wand10,
            Wand11,
            Wand12,
            Parry07,
            Wand13,
            Wand14,
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

#if v1_3
        public enum  OldManPhaseOnePatterns {
            Random,
            Spit01,
            Camel01,
            Spit02,
            Duck01
        }
  
        public enum OldManPhaseOnePlatformRemoveOrdersNormal {
            Random,
            [Description("0,2")]
            Order01,
            [Description("0,3")]
            Order02,
            [Description("0,4")]
            Order03,
            [Description("1,3")]
            Order04,
            [Description("3,1")]
            Order05,
            [Description("2,0")]
            Order06,
            [Description("3,0")]
            Order07,
            [Description("2,4")]
            Order08,
            [Description("0,1")]
            Order09,
            [Description("0,2")]
            Order10,
            [Description("0,3")]
            Order11
        }

        public enum OldManPhaseTwoLeftPuppetPatternsEasy {
            Random,
            Low01,
            Mid01,
            High01,
            Low02,
            Mid02,
            Low03,
            High02,
            Mid03,
            Low04
        }

        public enum OldManPhaseTwoRightPuppetPatternsEasy {
            Random,
            High01,
            High02,
            Low01,
            Mid01,
            High03,
            Low02,
            Mid02,
            Low03
        }

        public enum OldManPhaseTwoLeftNormalRightHardPuppetPatterns {
            Random,
            Low01,
            Mid01,
            High01,
            Low02,
            Mid02,
            Low03,
            High02,
            Mid03,
            Low04
        }

        public enum OldManPhaseTwoRightNormalLeftHardPuppetPatterns {
            Random,
            High01,
            High02,
            Low01,
            Mid01,
            High03,
            Low02,
            Mid02,
            Low03
        }
          
        public enum SnowCultPhaseOnePatterns {
            Random,
            Cones01,
            Whale01,
            Cards01,
            Cones02,
            Cards02,
            Whale02,
            Cones03,
            Cards03,
            Cones04,
            Whale03
        }

        public enum SnowCultPhaseOneStartPositions {
            Random,
            [Description("150")]
            A_150,
            [Description("0")]
            B_0,
            [Description("-100")]
            C_Neg100,
            [Description("50")]
            D_50,
            [Description("100")]
            E_100,
            [Description("-50")]
            F_Neg50,
            [Description("-150")]
            G_Neg150,
            [Description("0")]
            H_0,
            [Description("-150")]
            I_Neg150,
            [Description("-50")]
            J_Neg50,
            [Description("150")]
            K_150,
            [Description("-100")]
            L_Neg100,
            [Description("100")]
            M_100
        }

        public enum SnowCultPhaseTwoPatterns {
            Random,
            Roll01,
            Jump01,
            Fridge01,
            Roll02,
            Slam01,
            Roll03,
            Fridge02,
            Slam02,
            Roll04,
            Roll05,
            Jump02,
            Fridge03,
            Slam03,
            Roll06,
            Fridge04,
            Roll07,
            Jump03,
            Slam04,
            Fridge05
        }
          
        public enum SnowCultPhaseThreePatterns {
            Random,
            Mouth01,
            Switch01,
            Eye01,
            Switch02,
            Shard01,
            Eye02,
            Switch03,
            Mouth02,
            Switch04,
            Eye03,
            Switch05,
            Shard02,
            Switch06,
            Mouth03,
            Eye04,
            Switch07
        }

        public enum AirplanePhaseOnePatterns {
            Random,
            Parachute,
            Cat
        }

        public enum AirplanePhaseOneSides {
            Random,
            Left01,
            Right01,
            Right02,
            Left02,
            Right03,
            Left03,
            Left04,
            Right04,
            Left05,
            Right05
        }

        public enum AirplanePhaseOneParryPatterns {
            Random,
            Regular01,
            Regular02,
            Parry01,
            Regular03,
            Parry02
        }

        public enum AirplanePhaseOneParryPatternsSimple {
            Random,
            Regular01,
            Parry01,
            Regular02,
            Parry02
        }

        public enum AirplanePhaseTwoTerrierRotationDirections {
            Random,
            Clockwise,
            Counterclockwise
        }
  
        public enum AirplaneTurretPatterns {
            Random,
            [Description("TopL-MidL, MidR, TopR-Bot")]
            A_B_D_C_E01,
            [Description("MidR, TopL-TopR, Bot")]
            D_A_C_E02,
            [Description("TopL, MidL-TopR, Bot")]
            A_B_C_E03,
            [Description("MidL-MidR, TopL, MidL-Bot")]
            B_D_A_B_E04,
            [Description("TopR, TopL-MidR, Bot")]
            C_A_D_E05,
            [Description("TopL, MidL-MidR, Bot")]
            A_B_D_E06,
            [Description("TopL-TopR, MidL, MidR-Bot")]
            A_C_B_D_E07,
            [Description("MidL, TopR-MidR, Bot")]
            B_C_D_E08,
            [Description("MidR, TopL-MidL, Bot")]
            D_A_B_E09,
            [Description("TopR-MidR, MidL, TopL-Bot")]
            C_D_B_A_E10,
            [Description("TopL, MidL-TopR, Bot")]
            A_B_C_E11,
            [Description("TopR, TopL-MidR, Bot")]
            C_A_D_E12 
        }

        public enum AirplaneTurretPatternsSimple {
            Random,
            [Description("Bot")]
            E01,
            [Description("TopL, MidL, TopR-MidR")]
            A_B_C_D02,
            [Description("TopL, TopR, MidL-MidR")]
            A_C_B_D03,
            [Description("Bot")]
            E04,
            [Description("MidL, TopL, TopR-MidR")]
            B_A_C_D05,
            [Description("MidL, TopR, TopL-MidL")]
            B_C_A_B06,
            [Description("Bot")]
            E07,
            [Description("TopL, MidR, MidL-TopR")]
            A_D_B_C08,
            [Description("MidL, TopR, TopL-MidR")]
            B_C_A_D09,
            [Description("Bot")]
            E10,
            [Description("MidR, MidL, TopL-TopR")]
            D_B_A_C11,
            [Description("TopR, TopL, MidL-MidR")]
            C_A_B_D12
        }

        public enum AirplaneTurretPatternsHard {
            Random,
            [Description("MidR-Bot, TopL-TopR, TopL-MidL-TopR")]
            D_E_A_C_A_B_C01,
            [Description("TopL-MidL, TopR-MidR, MidL-Bot")]
            A_B_C_D_B_E02,
            [Description("TopL-TopR, MidL-Bot, TopL-TopR-MidR")]
            A_C_B_E_A_C_D03,
            [Description("TopR-MidR, TopL-MidL, TopR-Bot")]
            C_D_A_B_C_E04,
            [Description("MidL-MidR, TopL-Bot, TopL-MidL-MidR")]
            B_D_A_E_A_B_D05,
            [Description("TopL-MidR, MidL-TopR, TopL-Bot")]
            A_D_B_C_A_E06,
            [Description("MidL-Bot, TopL-MidR, MidL-TopR-MidR")]
            B_E_A_D_B_C_D07,
            [Description("MidL-TopR, TopL-MidR, MidL-Bot")]
            B_C_A_D_B_E08
        }

        public enum RumRunnersPhaseOneSpiderActionsEasyNormal {
            Random,
            Caterpillar01,
            Grubs01,
            None01,
            Mine01,
            Grubs02,
            Mine02,
            None02,
            Grubs03,
            Caterpillar02,
            Mine03,
            Grubs04,
            None03
        }

        public enum RumRunnersPhaseOneSpiderActionsEasy {
            Random,
            Grubs01,
            None01,
            Mine01,
            Grubs02,
            Mine02,
            Mine03,
            None02,
            Grubs03,
            Mine04,
            Mine05,
            Grubs04,
            Mine06,
            None03
        }

        public enum RumRunnersPhaseOneSpiderActionsHard {
            Random,
            Caterpillar01,
            Grubs01,
            Mine01,
            Caterpillar02,
            None01,
            Grubs02,
            Mine02,
            Caterpillar03,
            None02,
            Mine03,
            Caterpillar04,
            Grubs03,
            Mine04,
            None03
        }

        public enum RumRunnersPhaseOneSpiderInitialActions {
            Random,
            None,
            Mine
        }

        public enum RumRunnersPhaseOneSpiderPositionsEasy {
            Random,
            Low01,
            Mid01,
            High01,
            Mid02
        }

        public enum RumRunnersPhaseOneSpiderPositionsNormalHard {
            Random,
            Low01,
            High01,
            Mid01,
            Low02,
            Mid02,
            High02,
            Mid03,
            High03,
            Low03,
            Low04,
            High04,
            Mid04,
            Low05,
            Mid05,
            High05,
            High06,
            Mid06,
            Low06,
            High07
        }

        public enum RumRunnersPhaseOneSpiderActionPositions {
            Random,
            [Description("15")]
            Position15,
            [Description("85")]
            Position85,
            [Description("50")]
            Position50,
            [Description("75")]
            Position75,
            [Description("25")]
            Position25,
            [Description("35")]
            Position35,
            [Description("65")]
            Position65
        }

        public enum RumRunnersPhaseOneMinePlacementsEasy {
            Random,
            [Description("0,7")] Zero_Seven,
            [Description("3,9,12")] Three_Nine_Twelve,
            [Description("2,14")] Two_Fourteen,
            [Description("4,5,11")] Four_Five_Eleven,
            [Description("6,13")] Six_Thirteen,
            [Description("1,8,10")] One_Eight_Ten,
            [Description("3,11")] Three_Eleven,
            [Description("4,7,13")] Four_Seven_Thirteen,
            [Description("1,10")] One_Ten,
            [Description("0,8,12")] Zero_Eight_Twelve,
            [Description("5,13")] Five_Thirteen,
            [Description("2,6,9")] Two_Six_Nine,
            [Description("4,12")] Four_Twelve,
            [Description("0,9,11")] Zero_Nine_Eleven,
            [Description("7,14")] Seven_Fourteen,
            [Description("2,5,10")] Two_Five_Ten,
            [Description("3,6")] Three_Six,
            [Description("1,8,13")] One_Eight_Thirteen
        }

        public enum RumRunnersPhaseOneMinePlacementsNormal {
            Random,
            [Description("0,3,7,11")] Zero_Three_Seven_Eleven,
            [Description("1,5,13")] One_Five_Thirteen,
            [Description("4,6,10,12")] Four_Six_Ten_Twelve,
            [Description("2,8,14")] Two_Eight_Fourteen,
            [Description("1,3,7,11")] One_Three_Seven_Eleven,
            [Description("0,9,13")] Zero_Nine_Thirteen,
            [Description("2,5,12")] Two_Five_Twelve,
            [Description("3,7,9,10")] Three_Seven_Nine_Ten,
            [Description("4,8,11")] Four_Eight_Eleven,
            [Description("1,6,8,14")] One_Six_Eight_Fourteen,
            [Description("2,9,12")] Two_Nine_Twelve
        }

        public enum RumRunnersPhaseOneMinePlacementsHard {
            Random,
            [Description("1,3,7,11")] One_Three_Seven_Eleven,
            [Description("2,5,9,10")] Two_Five_Nine_Ten,
            [Description("0,4,6,8,12")] Zero_Four_Six_Eight_Twelve,
            [Description("13,14,1,7")] Thirteen_Fourteen_One_Seven,
            [Description("4,6,9,11")] Four_Six_Nine_Eleven,
            [Description("4,8,10,14")] Four_Eight_Ten_Fourteen,
            [Description("0,2,5,8,12")] Zero_Two_Five_Eight_Twelve,
            [Description("3,6,8,14")] Three_Six_Eight_Fourteen,
            [Description("1,5,9,13")] One_Five_Nine_Thirteen,
            [Description("0,4,7,10,12")] Zero_Four_Seven_Ten_Twelve,
            [Description("2,3,5,11")] Two_Three_Five_Eleven,
            [Description("7,9,10,13")] Seven_Nine_Ten_Thirteen,
            [Description("0,6,2,8,14")] Zero_Six_Two_Eight_Fourteen
        }

        public enum RumRunnersPhaseThreeSnoutActionsNormal {
            Random,
            [Description("LQ,F,LT")] LQ_F_LT,
            [Description("Q,T")] Q_T,
            [Description("LQ,Q,LF,Q,T")] LQ_Q_LF_Q_T,
            [Description("Q,LF,Q,T")] Q_LF_Q_T,
            [Description("F,LQ,Q,LT")] F_LQ_Q_LT,
            [Description("Q,LT")] Q_LT,
            [Description("LF,Q,LQ,Q,T")] LF_Q_LQ_Q_T,
            [Description("Q,F,LQ,LT")] Q_F_LQ_LT,
            [Description("Q,Q,LT")] Q_Q_LT
        }

        public enum RumRunnersPhaseThreeSnoutActionsHard {
            Random,
            [Description("F,LQ,LT")] F_LQ_LT,
            [Description("Q,LT")] Q_LT,
            [Description("LQ,Q,T")] LQ_Q_T,
            [Description("LF,Q,T")] LF_Q_T,
            [Description("Q,LQ,Q,LT")] Q_LQ_Q_LT,
            [Description("Q,T")] Q_T,
            [Description("Q,LF,Q,T")] Q_LF_Q_T,
            [Description("Q,LQ,LT")] Q_LQ_LT,
            [Description("LQ,F,T")] LQ_F_T,
            [Description("LQ,LT")] LQ_LT
        }
      
        public enum RumRunnersPhaseThreeSnoutPositions {
            Random,
            Low01,
            Mid01,
            High01,
            Low02,
            High02,
            Low03,
            Low04,
            Mid02,
            High03,
            Mid03,
            Mid04,
            Low05,
            High04,
            High05,
            Mid05,
            Low06,
            Mid06
        }

        public enum SaltbakerPhaseOneTablePatterns {
            Random,
            Limes,
            Sugarcubes
        }

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

        public enum SaltbakerParryStringPatterns {
            Random,
            [Description("Regular")]
            R01,
            [Description("Regular")]
            R02,
            [Description("Regular")]
            R03,
            [Description("Parry")]
            P01,
            [Description("Regular")]
            R04,
            [Description("Regular")]
            R05,
            [Description("Regular")]
            R06,
            [Description("Parry")]
            P02,
            [Description("Regular")]
            R07,
            [Description("Regular")]
            R08,
            [Description("Regular")]
            R09,
            [Description("Regular")]
            R10,
            [Description("Parry")]
            P03
        }

        public enum SaltbakerPhaseThreeSawPatterns {
            Random,
            Left,
            Right
        }

#endif


    }
}
