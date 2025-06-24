using System;
using System.Collections.Generic;
using BepInEx.CupheadDebugMod.Components.RNG;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

namespace BepInEx.CupheadDebugMod.Components;

[HarmonyPatch]
internal class GuaranteeLobberExSweetSpot {


    private static bool alreadyRanNeverCrit;
    private static bool triedToDie;

    // A Lobber Ex Sweet Spot is when a Lobber Ex collides in-between an enemy and a floor.
    // Roughly half the times, it cause the explosion to happen twice. The other half, if will only happen once.
    // This is due to OnCollisionEnemy() and OnCollisionGround() getting called in different orders seemingly at random (I think it's single-threaded code so not a true race condition).
    // If OnCollisionEnemy() gets called first, then the double explosion will occur. If OnCollisionGround() gets called first, then it won't.
    // The reason why is because OnCollisionGround() lacks a check to make sure to not cause an explosion if it has already happened, but OnCollisionEnemy() does have one.

    [HarmonyPatch(typeof(WeaponBouncerProjectile), nameof(WeaponBouncerProjectile.Start))]
    [HarmonyPrefix]
    public static void StartFix(ref WeaponBouncerProjectile __instance) {
        if (__instance.isEx) {
            weaponBouncerExtraProperties.SetProperty(__instance, "ranOnCollisionGround", false);
            weaponBouncerExtraProperties.SetProperty(__instance, "ranOnCollisionEnemy", false);
            weaponBouncerExtraProperties.SetProperty(__instance, "alreadyRanSweetSpotFix", false);
            weaponBouncerExtraProperties.SetProperty(__instance, "ranHitGround", false);
        }
    }

    [HarmonyPatch(typeof(WeaponBouncerProjectile), nameof(WeaponBouncerProjectile.OnCollisionGround))]
    [HarmonyPrefix]
    public static void OnCollisionGroundFix(ref WeaponBouncerProjectile __instance) {
        if (__instance.isEx && !__instance.dead) {
            weaponBouncerExtraProperties.SetProperty(__instance, "ranOnCollisionGround", true);
        }
    }


    [HarmonyPatch(typeof(WeaponBouncerProjectile), nameof(WeaponBouncerProjectile.HitGround))]
    [HarmonyPrefix]
    public static void ReallyJankPrefixBecauseILManipulatorIsWeird(ref WeaponBouncerProjectile __instance) {
        triedToDie = false;
    }

    [HarmonyPatch(typeof(WeaponBouncerProjectile), nameof(WeaponBouncerProjectile.HitGround))]
    [HarmonyILManipulator]
    public static void HitGroundFix(ILContext il) {
        ILCursor ilCursor = new(il);
        ILLabel newLabel = null;
        while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().Contains("isEx"))) {
            ilCursor.Index++;
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Emit(OpCodes.Call, typeof(GuaranteeLobberExSweetSpot).GetMethod(nameof(setTriedToDie), BindingFlags.Static | BindingFlags.Public));
            ilCursor.Index--;
            newLabel = ilCursor.DefineLabel();
            ilCursor.MarkLabel(newLabel);
            ilCursor.Index -= 4;
            ilCursor.Remove();
            ilCursor.Emit(OpCodes.Bge, newLabel ?? throw new NullReferenceException());
            ilCursor.Index -= 5;
            ilCursor.Remove();
            ilCursor.Emit(OpCodes.Ble, newLabel ?? throw new NullReferenceException());
            break;
        }
    }

    public static void setTriedToDie() {
        triedToDie = true;
    }

    [HarmonyPatch(typeof(WeaponBouncerProjectile), nameof(WeaponBouncerProjectile.HitGround))]
    [HarmonyPostfix]
    public static void ReallyJankPostfixBecauseILManipulatorIsWeird(ref WeaponBouncerProjectile __instance) {
#if !v1_0
        weaponBouncerExtraProperties.SetProperty(__instance, "ranHitGround", true);
#endif

        if (triedToDie) {
            if (GuaranteeLobberExCrit.Value != LobberCritSettings.Never) {
                __instance.Die();
            }
            // if there was an attempt to make the lobber EX explode, and there's already been an enemy explosion and a ground explosion, then don't make this explosion happen
            else if (!((bool) weaponBouncerExtraProperties.GetProperty(__instance, "ranHitGround")) || !((bool) weaponBouncerExtraProperties.GetProperty(__instance, "ranOnCollisionEnemy"))) {
                __instance.Die();
            }
        }

        // if on v1.2+, we want the lobber to never explode when hitting the ground in this scenario, so set ranHitGround immediately.
        // if on v1.0/v1.1, we set this later so that one single Grond explosion occurs.
#if v1_0
        weaponBouncerExtraProperties.SetProperty(__instance, "ranHitGround", true);
#endif
    }



    [HarmonyPatch(typeof(WeaponBouncerProjectile), nameof(WeaponBouncerProjectile.OnCollisionEnemy))]
    [HarmonyPrefix]
    public static void OnCollisionEnemyFix(ref WeaponBouncerProjectile __instance) {
        if (__instance.isEx) {
            weaponBouncerExtraProperties.SetProperty(__instance, "ranOnCollisionEnemy", true);
        }
        // Actual fix is here. If another collision type has been hit first, cause a second Lobber Ex explosion
        if (GuaranteeLobberExCrit.Value == LobberCritSettings.Always && __instance.isEx && (bool) weaponBouncerExtraProperties.GetProperty(__instance, "ranOnCollisionGround") && !(bool) weaponBouncerExtraProperties.GetProperty(__instance, "alreadyRanSweetSpotFix")) {
            __instance.Die();
            weaponBouncerExtraProperties.SetProperty(__instance, "alreadyRanSweetSpotFix", true);
        }
    }

    private static ExtraPropertyManager weaponBouncerExtraProperties = new ExtraPropertyManager();
}




// Utility class to essentially manage additional properties for WeaponBouncerProjectile at run-time
// ConditionalWeakTable would've been more convenient, but it's not available on .NET 3.5
public class ExtraPropertyManager {
    private readonly Dictionary<WeakReference, Dictionary<string, object>> properties = new Dictionary<WeakReference, Dictionary<string, object>>();

    public void SetProperty(object obj, string propertyName, object value) {
        Cleanup();

        var weakRef = GetWeakReference(obj);

        if (!properties.ContainsKey(weakRef)) {
            properties[weakRef] = new Dictionary<string, object>();
        }

        properties[weakRef][propertyName] = value;
    }

    public object GetProperty(object obj, string propertyName) {
        Cleanup();

        var weakRef = GetWeakReference(obj);

        if (properties.TryGetValue(weakRef, out var objProperties)) {
            if (objProperties.ContainsKey(propertyName)) {
                return objProperties[propertyName];
            }
        }

        return null;
    }

    private WeakReference GetWeakReference(object obj) {
        foreach (var key in properties.Keys) {
            if (key.Target == obj) {
                return key;
            }
        }

        return new WeakReference(obj);
    }

    private void Cleanup() {
        var keysToRemove = new List<WeakReference>();

        foreach (var key in properties.Keys) {
            if (!key.IsAlive) {
                keysToRemove.Add(key);
            }
        }

        foreach (var key in keysToRemove) {
            properties.Remove(key);
        }
    }
}