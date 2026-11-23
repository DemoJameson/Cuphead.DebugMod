// TEMPORARY Forest Follies RNG diagnostics — commented out after the investigation
// (kept for future runs). Re-enable by changing "#if false" to "#if true".
#if false
using System;
using System.Reflection;
using System.Text;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// TEMPORARY Forest Follies RNG diagnostics — remove once the investigation is done.
// Everything it logs is prefixed with "[ForestDiag]" so it can be filtered out of
// BepInEx/LogOutput.log. Three streams:
//   1. a one-shot level dump (all spawners and Forest enemies with their serialized
//      randomness data: side modes, cycle strings, MinMaxes, trigger types),
//   2. a runtime trace of ground-enemy drops, including the silent GoToGround destroys,
//   3. the mushroom's pink-cadence and fire-timing rolls as they happen.
[HarmonyPatch]
internal class ForestDiagnostics : PluginComponent {

    private const bool Enabled = true;

    // ------------------------------------------------------------------
    // 1. Level dump. Level.Start runs after every Awake of the scene, so
    //    spawner rolls and the shared pink counter are already in place.
    //    The acorn-spawner check keeps this Forest Follies only.
    // ------------------------------------------------------------------
    [HarmonyPatch(typeof(Level), "Start")]
    [HarmonyPostfix]
    private static void LevelStartDump() {
        if (!Enabled || UnityEngine.Object.FindObjectOfType<ForestPlatformingLevelAcornSpawner>() == null) {
            return;
        }

        Logger.LogInfo("[ForestDiag] ===== Forest Follies level dump =====");

        foreach (PlatformingLevelEnemySpawner spawner in UnityEngine.Object.FindObjectsOfType<PlatformingLevelEnemySpawner>()) {
            Logger.LogInfo($"[ForestDiag] Spawner {spawner.GetType().Name} @ {spawner.transform.position}");
            DumpFields(spawner);
            ForestPlatformingLevelAcornSpawner acorn = spawner as ForestPlatformingLevelAcornSpawner;
            if (acorn != null) {
                Logger.LogInfo("[ForestDiag]   rolled starts: leftRightIndex=" + GetField(acorn, "leftRightIndex") + " yIndex=" + GetField(acorn, "yIndex"));
            }
        }

        foreach (AbstractPlatformingLevelEnemy enemy in UnityEngine.Object.FindObjectsOfType<AbstractPlatformingLevelEnemy>()) {
            StringBuilder line = new StringBuilder();
            line.Append("[ForestDiag] Enemy ").Append(enemy.GetType().Name)
                .Append(" @ ").Append(enemy.transform.position)
                .Append(" id=").Append(enemy.ID)
                .Append(" startCondition=").Append(GetField(enemy, "_startCondition"))
                .Append(" startDelay=").Append(GetField(enemy, "_startDelay"));
            Logger.LogInfo(line.ToString());

            if (GetField(enemy, "_startCondition") + "" == "TriggerVolume") {
                Logger.LogInfo("[ForestDiag]   triggerRect pos=" + GetField(enemy, "_triggerPosition") + " size=" + GetField(enemy, "_triggerSize"));
            }

            PlatformingLevelShootingEnemy shooter = enemy as PlatformingLevelShootingEnemy;
            if (shooter != null) {
                Logger.LogInfo(
                    "[ForestDiag]   shooting: triggerType=" + GetField(shooter, "_triggerType")
                    + " initialShotDelay=" + MinMaxStr(GetField(shooter, "_initialShotDelay"))
                    + " projectileDelayRange=" + MinMaxStr(shooter.Properties.ProjectileDelay)
                    + " aimMode=" + shooter.Properties.ProjectileAimMode
                    + " arcMinSpeed=" + shooter.Properties.ArcProjectileMinSpeed
                    + " projectileSpeed=" + shooter.Properties.ProjectileSpeed
                    + " pinkNumber=" + MinMaxStr(shooter.Properties.MushroomPinkNumber));
            }
        }

        Logger.LogInfo("[ForestDiag] shared pink counter after all Awakes = " + ForestPlatformingLevelMushroomProjectile.numUntilPink);
        Logger.LogInfo("[ForestDiag] =====================================");
    }

    // ------------------------------------------------------------------
    // 2. Ground-enemy drop trace. GoToGround is where the game silently
    //    destroys a dropped grunt (no ground under the drop point or the
    //    pit-edge check), so the postfix detects the destroy via Unity's
    //    fake-null.
    // ------------------------------------------------------------------
    [HarmonyPatch(typeof(PlatformingLevelGroundMovementEnemy), "GoToGround")]
    [HarmonyPrefix]
    private static void GoToGroundPrefix(PlatformingLevelGroundMovementEnemy __instance) {
        if (!Enabled) {
            return;
        }
        Logger.LogInfo($"[ForestDiag] {__instance.GetType().Name} drop @ {__instance.transform.position} facing={__instance.direction}");
    }

    [HarmonyPatch(typeof(PlatformingLevelGroundMovementEnemy), "GoToGround")]
    [HarmonyPostfix]
    private static void GoToGroundPostfix(PlatformingLevelGroundMovementEnemy __instance) {
        if (!Enabled) {
            return;
        }
        if (__instance == null) {
            Logger.LogInfo("[ForestDiag]   -> DESTROYED in GoToGround (no ground under the drop point or pit-edge check)");
        } else {
            Logger.LogInfo($"[ForestDiag]   -> landed at y={__instance.transform.position.y}");
        }
    }

    // ------------------------------------------------------------------
    // 3. Mushroom roll traces. These call sites are the future control
    //    points of the pink-cadence and fire-timing spots.
    // ------------------------------------------------------------------
    [HarmonyPatch(typeof(ForestPlatformingLevelMushroom), "Awake")]
    [HarmonyILManipulator]
    private static void MushroomAwakeManipulator(ILContext il) {
        TraceIntRoll(il, "pink cadence roll (mushroom Awake)");
    }

    [HarmonyPatch(typeof(ForestPlatformingLevelMushroomProjectile), "Start")]
    [HarmonyILManipulator]
    private static void MushroomProjectileStartManipulator(ILContext il) {
        TraceIntRoll(il, "pink cadence re-roll (projectile hit zero)");
    }

    // the per-instance shot cadence, rolled once per shooting enemy
    [HarmonyPatch(typeof(PlatformingLevelShootingEnemy), "OnStart")]
    [HarmonyPostfix]
    private static void ShootingOnStartPostfix(PlatformingLevelShootingEnemy __instance) {
        if (!Enabled) {
            return;
        }
        Logger.LogInfo("[ForestDiag] " + __instance.GetType().Name + " @ " + __instance.transform.position
            + " OnStart: projectileDelay=" + GetField(__instance, "_projectileDelay")
            + " triggerType=" + GetField(__instance, "_triggerType"));
    }

    // the first-shot delay, rolled once per shooting enemy in its trigger coroutine
    [HarmonyPatch(typeof(PlatformingLevelShootingEnemy), "ranged_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void InitialDelayRangedManipulator(ILContext il) {
        TraceFloatRoll(il, "initial-shot-delay roll (ranged)");
    }

    [HarmonyPatch(typeof(PlatformingLevelShootingEnemy), "triggerVolumes_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void InitialDelayVolumesManipulator(ILContext il) {
        TraceFloatRoll(il, "initial-shot-delay roll (triggerVolumes)");
    }

    [HarmonyPatch(typeof(PlatformingLevelShootingEnemy), "onscreen_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void InitialDelayOnscreenManipulator(ILContext il) {
        TraceFloatRoll(il, "initial-shot-delay roll (onscreen)");
    }

    [HarmonyPatch(typeof(PlatformingLevelShootingEnemy), "indefinite_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void InitialDelayIndefiniteManipulator(ILContext il) {
        TraceFloatRoll(il, "initial-shot-delay roll (indefinite)");
    }

    // per-shot projectile speed (only fires for Arc/Spread aim modes)
    [HarmonyPatch(typeof(PlatformingLevelShootingEnemy), "Shoot")]
    [HarmonyILManipulator]
    private static void ShootManipulator(ILContext il) {
        TraceFloatRoll(il, "projectile-speed roll (arc/spread)");
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static void TraceIntRoll(ILContext il, string label) {
        if (!Enabled) {
            return;
        }
        ILCursor cursor = new(il);
        while (cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Callvirt && i.Operand.ToString().Contains("MinMax::RandomInt"))) {
            cursor.EmitDelegate<Func<int, int>>(roll => {
                Logger.LogInfo("[ForestDiag] " + label + " = " + roll);
                return roll;
            });
            cursor.Index++;
        }
    }

    private static void TraceFloatRoll(ILContext il, string label) {
        if (!Enabled) {
            return;
        }
        ILCursor cursor = new(il);
        while (cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Callvirt && i.Operand.ToString().Contains("MinMax::RandomFloat"))) {
            cursor.EmitDelegate<Func<float, float>>(roll => {
                Logger.LogInfo("[ForestDiag] " + label + " = " + roll.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
                return roll;
            });
            cursor.Index++;
        }
    }

    private static void DumpFields(object obj) {
        for (Type type = obj.GetType(); type != null && type != typeof(UnityEngine.Object); type = type.BaseType) {
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
                if (field.FieldType.IsSubclassOf(typeof(Delegate))) {
                    continue;
                }
                Logger.LogInfo("[ForestDiag]   " + field.Name + " = " + Fmt(field.GetValue(obj)));
            }
        }
    }

    private static string Fmt(object value) {
        if (value == null) {
            return "null";
        }
        if (value is UnityEngine.Object unityObject) {
            return unityObject == null ? "null" : "(" + unityObject.GetType().Name + ")";
        }
        if (value is MinMax minMax) {
            return MinMaxStr(minMax);
        }
        if (value is string text) {
            return "\"" + text + "\"";
        }
        return value.ToString();
    }

    private static string MinMaxStr(object value) {
        MinMax minMax = value as MinMax;
        return minMax != null ? "MinMax(" + minMax.min + ", " + minMax.max + ")" : value == null ? "null" : value.ToString();
    }

    private static object GetField(object obj, string name) {
        for (Type type = obj.GetType(); type != null; type = type.BaseType) {
            FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field != null) {
                return field.GetValue(obj);
            }
        }
        return "(missing " + name + ")";
    }
}
#endif