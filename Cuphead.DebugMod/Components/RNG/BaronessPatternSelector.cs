using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog).
[HarmonyPatch]
internal class BaronessPatternSelector : PluginComponent {

    private static Level.Mode Mode => Level.ScoringData.difficulty;

    // The waffle rolls its path once in Awake (pathA = Random.Range(0, 2) == 0); the roll
    // is consumed as-is, so no pre-increment compensation.
    [HarmonyPatch(typeof(BaronessLevelWaffle), "Awake")]
    [HarmonyPostfix]
    private static void WafflePathManipulator(ref bool ___pathA) {
        int pick = RngState.Index(RngSpots.BaronessWafflePath, Mode, ___pathA ? 0 : 1, 2, false);
        ___pathA = pick == 0;
    }

    // The miniboss picks are three rolls without replacement inside pickminibosses_cr;
    // the pick list is read back through the static PICKED_BOSSES by SetUpTimeline, so
    // replacing that property right before SetUpTimeline (the same injection point the
    // legacy patch used) decides the whole lineup. Each slot resolves its own spot; -1
    // means unpinned and draws from the remaining pool without replacement, exactly like
    // vanilla. Pinned slots may repeat each other, which vanilla can never produce.
    [HarmonyPatch(typeof(BaronessLevel), nameof(BaronessLevel.pickminibosses_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void MinibossSelectionManipulator(ILContext il) {
        ILCursor cursor = new(il);
        while (cursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("SetUpTimeline"))) {
            cursor.Index = cursor.Index - 2;
            cursor.Emit(OpCodes.Call, typeof(BaronessPatternSelector).GetMethod(nameof(SetMinibossSelectionWrapper), BindingFlags.Static | BindingFlags.Public));
            cursor.Index = cursor.Index + 3;
        }
    }

    public static void SetMinibossSelectionWrapper() {
        Level.Mode mode = Level.ScoringData.difficulty;
        string[] roster = RngSpots.BaronessMinibossRoster(mode);
        if (roster.Length == 0) {
            return;
        }

        int slot1 = RngState.Index(RngSpots.BaronessMiniboss1, mode, -1, roster.Length, false);
        int slot2 = RngState.Index(RngSpots.BaronessMiniboss2, mode, -1, roster.Length, false);
        int slot3 = RngState.Index(RngSpots.BaronessMiniboss3, mode, -1, roster.Length, false);

        List<string> pool = new List<string>(roster);
        string[] picked = new string[3];
        int[] slots = new[] { slot1, slot2, slot3 };
        for (int i = 0; i < 3; i++) {
            if (slots[i] >= 0 && slots[i] < roster.Length) {
                picked[i] = roster[slots[i]];
                pool.Remove(picked[i]); // one instance per placed pick; a repeat pin just leaves the pool untouched
            }
        }
        for (int i = 0; i < 3; i++) {
            if (picked[i] == null) {
                int index = UnityEngine.Random.Range(0, pool.Count);
                picked[i] = pool[index];
                pool.RemoveAt(index);
            }
        }

        BaronessLevel.PICKED_BOSSES = new List<string>(picked);
    }

    // ------------------------------------------------------------------
    // Shotgun: the baroness pops out of the castle to shoot during the last
    // miniboss (shoot_cr runs once; its loop only walks the rolled pattern).
    // ------------------------------------------------------------------

    // One pop-up timing pattern and its start index, both once per fight. The pattern
    // pick goes through ArrayExtensions.GetRandom, the start through Random.Range; the
    // start delegate needs the entry count of the pattern the fight actually picked, so
    // the pattern pick stashes its index.
    private static int chosenShotgunPattern = -1;

    [HarmonyPatch(typeof(BaronessLevelCastle), "shoot_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void ShotgunManipulator(ILContext il) {
        ILCursor cursor = new(il);
        while (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("ArrayExtensions::GetRandom"))) {
            cursor.EmitDelegate<Func<string, string>>(rolled => {
                string[] patterns = RngSpots.BaronessShotgunPatterns(Mode);
                int vanilla = Array.IndexOf(patterns, rolled);
                if (vanilla < 0) {
                    return rolled;
                }
                int pick = RngState.Index(RngSpots.BaronessShotgunPattern, Mode, vanilla, patterns.Length, false);
                chosenShotgunPattern = pick;
                return patterns[pick];
            });
            cursor.Index++;
        }

        cursor.Index = 0;
        if (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            cursor.EmitDelegate<Func<int, int>>(rolled =>
                // the rolled start is consumed directly (the loop advances the index at its
                // end), so no pre-increment compensation
                RngState.Index(RngSpots.BaronessShotgunStart, Mode, rolled,
                    RngSpots.BaronessShotgunStartCount(Mode, chosenShotgunPattern), false));
        }

        // the per-shot delay: the MinMax is consumed through its implicit float operator
        cursor.Index = 0;
        while (cursor.TryGotoNext(MoveType.After, i =>
                i.Operand?.ToString().Contains("MinMax::op_Implicit") == true)) {
            cursor.EmitDelegate<Func<float, float>>(rolled =>
                RngState.Float(RngSpots.BaronessShotgunDelay, Mode, rolled));
            cursor.Index++;
        }
    }

    // ------------------------------------------------------------------
    // Final phase (chase): the castle tosses its head along a fixed script
    // (final_shoot_cr runs once).
    // ------------------------------------------------------------------

    [HarmonyPatch(typeof(BaronessLevelCastle), "final_shoot_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void HeadTossManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            cursor.EmitDelegate<Func<int, int>>(rolled =>
                // the rolled start is consumed directly (the loop advances the index at its
                // end), so no pre-increment compensation
                RngState.Index(RngSpots.BaronessHeadTossStart, Mode, rolled,
                    RngSpots.BaronessHeadTossEntries(Mode).Length, false));
        }

        cursor.Index = 0;
        if (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Callvirt && i.Operand.ToString().Contains("MinMax::RandomFloat"))) {
            cursor.EmitDelegate<Func<float, float>>(rolled =>
                RngState.Float(RngSpots.BaronessHeadTossDelay, Mode, rolled));
        }
    }

    // ------------------------------------------------------------------
    // Jellybeans: the spawner coroutine is replaced wholesale (the
    // StartJellybeans prefix skips the original and starts a faithful copy)
    // because both control points live inside it and the old compiler's
    // iterator IL proved too fragile to hook. The copy behaves identically
    // to the game when nothing is configured: the top-of-loop delay roll
    // stays where vanilla makes it (closed iterations still consume a
    // roll), and the resolver only overrides at the actual spawn point, so
    // delay sequences advance once per real bean.
    // ------------------------------------------------------------------

    [HarmonyPatch(typeof(BaronessLevelCastle), nameof(BaronessLevelCastle.StartJellybeans))]
    [HarmonyPrefix]
    private static bool JellybeanSpawnerManipulator(BaronessLevelCastle __instance) {
        __instance.StartCoroutine(JellybeanSpawnerReplacement(__instance));
        return false;
    }

    // A faithful copy of the game's spawnJellybeans_cr with the two jellybean spots
    // resolved inside it. The castle's private helpers and fields are reached through
    // reflection so the copy stays identical across game versions (jellyIsPink only
    // exists up to 1.2, and SpawnJellyBeans differs internally between versions).
    private static IEnumerator JellybeanSpawnerReplacement(BaronessLevelCastle castle) {
        LevelProperties.Baroness properties = Traverse.Create(castle).Property("properties").GetValue<LevelProperties.Baroness>();
        LevelProperties.Baroness.Jellybeans p = properties.CurrentState.jellybeans;

        Traverse changeDelayField = Traverse.Create(castle).Field("jellyChangeDelay");
        Traverse pinkField = Traverse.Create(castle).Field("jellyIsPink");
        MethodInfo spawnBean = AccessTools.Method(typeof(BaronessLevelCastle), "SpawnJellyBeans");
        BaronessLevelJellybeans greenPrefab = Traverse.Create(castle).Field("greenJellyPrefab").GetValue<BaronessLevelJellybeans>();
        BaronessLevelJellybeans pinkPrefab = Traverse.Create(castle).Field("pinkJellyPrefab").GetValue<BaronessLevelJellybeans>();

        // the once-per-fight pattern pick
        string[] patterns = p.typeArray;
        int vanillaPattern = Array.IndexOf(patterns, patterns.GetRandom<string>());
        int patternPick = RngState.Index(RngSpots.BaronessJellybeanPattern, Mode, vanillaPattern, patterns.Length, false);
        string[] typePattern = patterns[patternPick].Split(new[] { ',' });

        float change = 0f;
        while (castle.state != BaronessLevelCastle.State.ChaseIntro) {
            for (int i = 0; i < typePattern.Length; i++) {
                BaronessLevelJellybeans toSpawn = null;
                float beanSpawnDelay = p.spawnDelay.RandomFloat();
                if (typePattern[i][0] == 'R') {
                    toSpawn = greenPrefab;
                    if (pinkField.FieldExists()) {
                        pinkField.SetValue(false);
                    }
                } else if (typePattern[i][0] == 'P') {
                    toSpawn = pinkPrefab;
                    if (pinkField.FieldExists()) {
                        pinkField.SetValue(true);
                    }
                }

                if ((BaronessLevelCastle.CURRENT_MINI_BOSS != null && castle.state == BaronessLevelCastle.State.Idle)
                    || castle.state == BaronessLevelCastle.State.EasyFinal) {
                    // the delay resolver runs here - once per real bean, never on the
                    // skipped iterations that still burn vanilla rolls
                    float beanDelay = RngState.Float(RngSpots.BaronessJellybeanDelay, Mode, beanSpawnDelay);
                    spawnBean.Invoke(castle, new object[] { toSpawn });
                    yield return CupheadTime.WaitForSeconds(castle, beanDelay - change);
                } else {
                    yield return null;
                }
                if (changeDelayField.GetValue<bool>()) {
                    change += beanSpawnDelay - beanSpawnDelay * (1f - p.spawnDelayChangePercentage / 100f);
                    changeDelayField.SetValue(false);
                }
            }
        }
    }

    // ------------------------------------------------------------------
    // Candy corn: the turn decision re-rolls its pattern on every pass
    // through the castle (switch_cr runs once per pass) while the position
    // inside it is rolled once (first pass) and then walked. The walked
    // index can also run past a shorter pattern's length, which breaks the
    // corn's turning for the rest of the fight - exactly as it can in
    // vanilla, so the start pin is deliberately not validated.
    // ------------------------------------------------------------------

    [HarmonyPatch(typeof(BaronessLevelCandyCorn), "switch_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void CornTurnManipulator(ILContext il) {
        ILCursor cursor = new(il);

        // the per-pass pattern pick
        while (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("ArrayExtensions::GetRandom"))) {
            cursor.EmitDelegate<Func<string, string>>(rolled => {
                string[] patterns = RngSpots.BaronessCornTurnPatterns(Mode);
                int vanilla = Array.IndexOf(patterns, rolled);
                if (vanilla < 0) {
                    return rolled;
                }
                int pick = RngState.Index(RngSpots.BaronessCornTurnPattern, Mode, vanilla, patterns.Length, false);
                return patterns[pick];
            });
            cursor.Index++;
        }

        // the once-per-fight start (first pass only)
        cursor.Index = 0;
        if (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            cursor.EmitDelegate<Func<int, int>>(rolled =>
                // consumed directly; deliberately not validated against the pattern
                // length so the vanilla fight-breaking state stays forceable
                RngState.Index(RngSpots.BaronessCornTurnStart, Mode, rolled,
                    RngSpots.BaronessCornTurnMaxIndex(Mode), false));
        }
    }

    // the mini corn spawn delay: rolled per spawned mini (the wait lives inside the
    // spawn branch, so closed frames never reach it)
    [HarmonyPatch(typeof(BaronessLevelCandyCorn), "spawnMinis_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void CornMiniDelayManipulator(ILContext il) {
        ILCursor cursor = new(il);
        while (cursor.TryGotoNext(MoveType.After, i =>
                i.Operand?.ToString().Contains("MinMax::op_Implicit") == true)) {
            cursor.EmitDelegate<Func<float, float>>(rolled =>
                RngState.Float(RngSpots.BaronessCornTurnMiniDelay, Mode, rolled));
            cursor.Index++;
        }
    }

    // ------------------------------------------------------------------
    // Waffle: the split cycle (check_attack_cr runs once per split - the
    // loop closes itself through waffle_pieces reassembling the waffle).
    // ------------------------------------------------------------------

    // the delay between reassembling and the next split
    [HarmonyPatch(typeof(BaronessLevelWaffle), "check_attack_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void WaffleSplitDelayManipulator(ILContext il) {
        ILCursor cursor = new(il);
        while (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Callvirt && i.Operand.ToString().Contains("MinMax::RandomFloat"))) {
            cursor.EmitDelegate<Func<float, float>>(rolled =>
                RngState.Float(RngSpots.BaronessWaffleSplitDelay, Mode, rolled));
            cursor.Index++;
        }
    }

    // which half detaches first (diagFirst = Random.Range(0, 2) == 0); only observable
    // on Expert - Regular's explodeTwoDuration is 0.001s, so the halves detach
    // simultaneously there
    [HarmonyPatch(typeof(BaronessLevelWaffle), "attack_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void WaffleSplitOrderManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            cursor.EmitDelegate<Func<int, int>>(rolled =>
                RngState.Index(RngSpots.BaronessWaffleSplitOrder, Mode, rolled, 2, false));
        }
    }
}