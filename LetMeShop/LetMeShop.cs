using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ExitGames.Client.Photon;
using HarmonyLib;
using REPOLib.Modules;
using UnityEngine;
using ReviveChecks = LetMeShop.ReviveChecks;

namespace LetMeShop;

[BepInPlugin("endersaltz.LetMeShop", "LetMeShop", "1.0")]

public class LetMeShop : BaseUnityPlugin
{
    internal static LetMeShop Instance { get; private set; } = null!;
    internal new ManualLogSource Logger => Instance._logger;
    private ManualLogSource _logger => base.Logger;

    public static ConfigEntry<float> configRespawnTime = null!;
    public static ConfigEntry<float> configRespawnHealth = null!;
    public static ConfigEntry<float> configRespawnInv = null!;
    public static bool invulnerable;
    public static int respawnHealth;
    
    public static NetworkedEvent sendInvulnerableStatus;
    public static NetworkedEvent sendRespawnHealth;
    
    private void Awake()
    {
        Instance = this;

        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;
        
        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");

        Harmony.CreateAndPatchAll(typeof(ReviveChecks));
        
        sendInvulnerableStatus = new NetworkedEvent("SendInvulnerableStatus", GetInvulnerableStatus);
        sendRespawnHealth = new NetworkedEvent("SendRespawnHealth", GetRespawnHealth);
        
        configRespawnTime = Config.Bind(
            "General",
            "RespawnTime",
            4.5f,
            "Times above 4.5 won't respawn players if all players die at once/in singleplayer."
            );
        configRespawnHealth = Config.Bind(
            "General",
            "RespawnHealth",
            10f,
            "Amount, in percent, the player should respawn at."
            );
        configRespawnInv = Config.Bind(
            "General",
            "RespawnInv",
            3f,
            "Amount of time the player becomes invulnerable after respawning."
            );
    }

    private void Update()
    {
        ReviveChecks.Update();
    }

    private static void GetInvulnerableStatus(EventData data)
    {
        Dictionary<string, bool> statuses = (Dictionary<string, bool>)data.CustomData;
        statuses.TryAdd(PlayerAvatar.instance.steamID, false);
        invulnerable = statuses[PlayerAvatar.instance.steamID];
    }

    private static void GetRespawnHealth(EventData data)
    {
        respawnHealth = (int) Math.Round(PlayerAvatar.instance.playerHealth.maxHealth * (float) data.CustomData / 100);
    }
}