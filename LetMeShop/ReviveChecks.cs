using System;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using HarmonyLib;
using UnityEngine;

namespace LetMeShop;

public class ReviveChecks
{
    private static Dictionary<string, float> _deathTimers = new Dictionary<string, float>();
    private static Dictionary<string, bool> _deathStates = new Dictionary<string, bool>();
    private static Dictionary<string, float> _invulnerableTimers = new Dictionary<string, float>();

    public static void Update()
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer() || !SemiFunc.RunIsShop()) return;
        FetchPlayerStates();
        TryReviving();
        UpdateInvulnerable();
    }

    private static void FetchPlayerStates()
    {
        foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
        {
            _deathStates.TryAdd(player.steamID, player.playerHealth.health <= 0);
            _deathStates[player.steamID] = player.playerHealth.health <= 0;
        }
    }

    private static void TryReviving()
    {
        foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
        {
            _invulnerableTimers.TryAdd(player.steamID, 0);
            _deathTimers.TryAdd(player.steamID, 0);
            bool dead = _deathStates[player.steamID];
            if (!dead) _deathTimers[player.steamID] = 0;
            if (!dead) continue;
            _deathTimers[player.steamID] += Time.deltaTime;
            float timer = _deathTimers[player.steamID];
            if (timer > LetMeShop.configRespawnTime.Value)
            {
                player.Revive();
                _deathStates[player.steamID] = false;
                _deathTimers[player.steamID] = 0;
                _invulnerableTimers[player.steamID] = LetMeShop.configRespawnInv.Value;
                player.playerHealth.health = (int) Math.Round(player.playerHealth.maxHealth * LetMeShop.configRespawnHealth.Value / 100);
            }
        }
    }

    private static void UpdateInvulnerable()
    {
        foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
        {
            if (_invulnerableTimers[player.steamID] > 0) _invulnerableTimers[player.steamID] -= Time.deltaTime;
            player.playerHealth.godMode = _invulnerableTimers[player.steamID] > 0;
            if (player.playerHealth.health <= 0 && player.playerHealth.godMode) player.playerHealth.health = 1;
        }
    }


    [HarmonyPatch(typeof(RunManager), "ChangeLevel")]
    [HarmonyPrefix]
    private static bool ChangeLevelPatch()
    {
        return !RunManager.instance.allPlayersDead;
    }
}