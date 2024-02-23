using System;
using System.Collections.Generic;
using HarmonyLib;

namespace BepInEx.CupheadDebugMod.Components;

[HarmonyPatch]
internal class GuaranteeLobberExSweetSpot {


    // A Lobber Ex Sweet Spot is when a Lobber Ex collides in-between an enemy and a floor.
    // Roughly half the times, it cause the explosion to happen twice. The other half, if will only happen once.
    // This is due to OnCollisionEnemy() and OnCollisionGround() getting called in different orders seemingly at random (I think it's single-threaded code so not a true race condition).
    // If OnCollisionEnemy() gets called first, then the double explosion will occur. If OnCollisionGround() gets called first, then it won't.
    // The reason why is because OnCollisionGround() lacks a check to make sure to not cause an explosion if it has already happened, but OnCollisionEnemy() does have one.

    [HarmonyPatch(typeof(WeaponBouncerProjectile), nameof(WeaponBouncerProjectile.Start))]
    [HarmonyPrefix]
    public static void StartFix(ref WeaponBouncerProjectile __instance) {
        if (Config.Settings.GuaranteeLobberExSweetSpot.Value && __instance.isEx) {
            weaponBouncerExtraProperties.SetProperty(__instance, "ranOnCollisionGround", false);
            weaponBouncerExtraProperties.SetProperty(__instance, "alreadyRanSweetSpotFix", false);
        }
    }

    [HarmonyPatch(typeof(WeaponBouncerProjectile), nameof(WeaponBouncerProjectile.OnCollisionGround))]
    [HarmonyPrefix]
    public static void OnCollisionGroundFix(ref WeaponBouncerProjectile __instance) {
        if (Config.Settings.GuaranteeLobberExSweetSpot.Value && __instance.isEx && !__instance.dead) {
            weaponBouncerExtraProperties.SetProperty(__instance, "ranOnCollisionGround", true);
        }
    }

    [HarmonyPatch(typeof(WeaponBouncerProjectile), nameof(WeaponBouncerProjectile.OnCollisionEnemy))]
    [HarmonyPrefix]
    public static void OnCollisionEnemyFix(ref WeaponBouncerProjectile __instance) {
        // Actual fix is here. If another collision type has been hit first, cause a second Lobber Ex explosion
        if (Config.Settings.GuaranteeLobberExSweetSpot.Value && __instance.isEx && (bool) weaponBouncerExtraProperties.GetProperty(__instance, "ranOnCollisionGround") && !(bool) weaponBouncerExtraProperties.GetProperty(__instance, "alreadyRanSweetSpotFix")) {
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