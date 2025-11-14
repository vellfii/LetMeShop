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
    public static float hostRespawnTime;
    public static float hostRespawnHealth;
    public static float hostRespawnInv;
    
    public static NetworkedEvent sendRespawnTime;
    public static NetworkedEvent sendRespawnHealth;
    public static NetworkedEvent sendRespawnInv;
    
    private void Awake()
    {
        Instance = this;

        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;
        
        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");

        Harmony.CreateAndPatchAll(typeof(ReviveChecks));
        
        sendRespawnTime = new NetworkedEvent("SendRespawnTime", GetHostRespawnTime);
        sendRespawnHealth = new NetworkedEvent("SendRespawnHealth", GetHostRespawnHealth);
        sendRespawnInv = new NetworkedEvent("SendRespawnInv", GetHostRespawnInv);
        
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
        if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
        sendRespawnTime.RaiseEvent(configRespawnTime.Value, REPOLib.Modules.NetworkingEvents.RaiseOthers, SendOptions.SendReliable);
        sendRespawnHealth.RaiseEvent(configRespawnHealth.Value, REPOLib.Modules.NetworkingEvents.RaiseOthers, SendOptions.SendReliable);
        sendRespawnInv.RaiseEvent(configRespawnInv.Value, REPOLib.Modules.NetworkingEvents.RaiseOthers, SendOptions.SendReliable);
    }

    private static void GetHostRespawnTime(EventData data)
    {
        hostRespawnTime = (float) data.CustomData;
        Instance.Logger.LogInfo(hostRespawnTime);
    }
    
    private static void GetHostRespawnHealth(EventData data)
    {
        hostRespawnHealth = (float) data.CustomData;
    }
    
    private static void GetHostRespawnInv(EventData data)
    {
        hostRespawnInv = (float) data.CustomData;
    }
}