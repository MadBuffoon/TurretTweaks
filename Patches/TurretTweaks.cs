using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using TurretTweaks.Tools;
using UnityEngine;

namespace TurretTweaks.Patches;


public class TurretTweaks
{
    internal static readonly Dictionary<string, TurretCache> OgTurretCache = new();
    
    [HarmonyPatch(typeof(Turret), nameof(Turret.RPC_SetTarget))]
    private static class TurretSetTargetUpdate
    {
        static void Prefix(Turret __instance)
        {
            ImFRIENDLYDAMMIT(__instance);
        }

        static void Postfix(Turret __instance)
        {
            ImFRIENDLYDAMMIT(__instance);
        }
        private static void ImFRIENDLYDAMMIT(Turret __instance)
        {
            if (TurretTweaksPlugin.EnableFriendlyConfig.Value)
            {
                if (!__instance.m_nview.IsValid())
                    return;
                if (__instance.m_target == null) return;


                if (!__instance.HasAmmo())
                {
                    if (!__instance.m_haveTarget)
                        return;
                }

                if (__instance.m_target.IsTamed())
                    __instance.m_target = null;
                if (!__instance.m_target.IsPlayer() && __instance.m_target != Player.m_localPlayer) return;
                if (__instance.m_target.IsPVPEnabled())
                {
                    return;
                }

                __instance.m_target = null;
            }
        }

    }
    
    [HarmonyPatch(typeof(Turret), nameof(Turret.ShootProjectile))]
    private static class TurretShoot
    {
        private static void Postfix(Turret __instance)
        {
            if (TurretTweaksPlugin.EnableNoAmmoCostConfigEntry.Value)
            {
                int a = __instance.m_nview.GetZDO().GetInt("ammo");
                int num = Mathf.Min(1,
                    __instance.m_maxAmmo == 0
                        ? __instance.m_lastAmmo.m_shared.m_attack.m_projectiles
                        : Mathf.Min(a, __instance.m_lastAmmo.m_shared.m_attack.m_projectiles));
                __instance.m_nview.GetZDO().Set("ammo", a + num);
            }
        }
        
    }
    
    [HarmonyPatch(typeof(Turret), nameof(Turret.Awake))]
    private static class TurretAwake
    {
        private static void Postfix(Turret __instance)
        {
            UpdateTurret(__instance);
        }
        
    }
    [HarmonyPatch(typeof(Turret), nameof(Turret.UpdateTarget))]
    private static class TurretUpdateTarget
    {
        private static void Postfix(Turret __instance)
        {
            UpdateTurret(__instance);
        }
        
    }
    
    [HarmonyPatch(typeof(Turret), nameof(Turret.RPC_AddAmmo))]
    private static class TurretAddAmmo
    {
        private static void Postfix(Turret __instance)
        {
            UpdateTurret(__instance);
        }
        
    }

    internal static void UpdateTurret(Turret __instance)
    {
        if (TurretTweaksPlugin.EnableConfig.Value)
        {
            if (__instance.m_name == null)
            {
                return;
            }

            if (!OgTurretCache.ContainsKey(__instance.m_name))
            {
                OgTurretCache.Add(__instance.m_name,
                    new TurretCache(__instance.m_name, __instance.m_turnRate, __instance.m_horizontalAngle,
                        __instance.m_verticalAngle, __instance.m_viewDistance, __instance.m_attackCooldown,
                        __instance.m_attackWarmup, __instance.m_maxAmmo));
            }

            __instance.m_turnRate = TurretTweaksPlugin.TurnRateConfig.Value;
            __instance.m_horizontalAngle = TurretTweaksPlugin.AngleHorizontalConfig.Value;
            __instance.m_verticalAngle = TurretTweaksPlugin.AngleVerticalConfig.Value;
            __instance.m_viewDistance = TurretTweaksPlugin.ViewDistanceConfig.Value;
            __instance.m_attackCooldown = TurretTweaksPlugin.AttackCoolDownConfig.Value;
            __instance.m_attackWarmup = TurretTweaksPlugin.AttackWarmUpConfig.Value;
            __instance.m_maxAmmo = TurretTweaksPlugin.MaxAmmoConfig.Value;
        }
        else
        {
            var ogTurretStats = OgTurretCache[__instance.m_name];
            __instance.m_turnRate = ogTurretStats.TurnRateOG;
            __instance.m_horizontalAngle = ogTurretStats.AngleHorizontalOG;
            __instance.m_verticalAngle = ogTurretStats.AngleVerticalOG;
            __instance.m_viewDistance = ogTurretStats.ViewDistanceOG;
            __instance.m_attackCooldown = ogTurretStats.AttackCoolDownOG;
            __instance.m_attackWarmup = ogTurretStats.AttackWarmUpOG;
            __instance.m_maxAmmo = ogTurretStats.MaxAmmoOG;
        }
        
    }
    
}

