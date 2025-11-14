using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using HarmonyLib;
using UnityEngine;

namespace LetMeShop;

public class ReviveChecks
{
    private static Dictionary<string, object> _deathTimers = new Dictionary<string, object>();
    private static Dictionary<string, object> _deathStates = new Dictionary<string, object>();
    private static Dictionary<string, object> _invulnerableTimers = new Dictionary<string, object>();
    private static Dictionary<string, object> _invulnerableStates = new Dictionary<string, object>();

    public static void Update()
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer() || !SemiFunc.RunIsShop()) return;
        FetchPlayerStates();
        TryReviving();
    }

    private static void FetchPlayerStates()
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
        foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
        {
            _invulnerableTimers.TryAdd(player.steamID, 0);
            _deathStates.TryAdd(player.steamID, player.playerHealth.health <= 0);
            if ((float) _invulnerableTimers[player.steamID] > 0) _invulnerableTimers[player.steamID] = (float) _invulnerableTimers[player.steamID] - Time.deltaTime;
            _invulnerableStates[player.steamID] = (float) _invulnerableTimers[player.steamID] > 0;
        }
    }

    private static void TryReviving()
    {
        foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
        {
            _deathTimers.TryAdd(player.steamID, 0);
            bool dead = (bool) _deathStates[player.steamID];
            if (!dead) _deathTimers[player.steamID] = 0;
            if (!dead) continue;
            _deathTimers[player.steamID] = (float) _deathTimers[player.steamID] + Time.deltaTime;
            if ((float) _deathTimers[player.steamID] > LetMeShop.configRespawnTime.Value) player.Revive();
        }
    }


    [HarmonyPatch(typeof(RunManager), "ChangeLevel")]
    [HarmonyPrefix]
    private static bool ChangeLevelPatch()
    {
        return !RunManager.instance.allPlayersDead;
    }

    [HarmonyPatch(typeof(PlayerHealth), "Hurt")]
    [HarmonyPrefix]
    private static void HurtPatch(PlayerHealth __instance, ref int damage, bool savingGrace, int enemyIndex)
    {
        damage = (bool) _invulnerableStates[__instance.playerAvatar.steamID] ? 0 : damage;
    }

    [HarmonyPatch(typeof(PlayerHealth), "Death")]
    [HarmonyPrefix]
    private static void DeathPatch(PlayerHealth __instance)
    {
        if (!SemiFunc.RunIsShop()) return;
        _deathStates[__instance.playerAvatar.steamID] = true;
    }

    [HarmonyPatch(typeof(PlayerAvatar), "Revive")]
    [HarmonyPrefix]
    private static void RevivePatch(PlayerAvatar __instance, bool _revivedByTruck)
    {
        if (!SemiFunc.RunIsShop()) return;
        _deathStates[__instance.steamID] = false;
        _deathTimers[__instance.steamID] = 0;
        _invulnerableTimers[__instance.steamID] = LetMeShop.configRespawnInv.Value;
        __instance.playerHealth.health = (int) Math.Round(__instance.playerHealth.maxHealth * LetMeShop.configRespawnHealth.Value / 100);
    }
}