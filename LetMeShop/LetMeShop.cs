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

    public static NetworkedEvent sendStates;
    public static NetworkedEvent sendConfig;
    public static float[] configList = new float[3];
    
    private void Awake()
    {
        Instance = this;

        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;
        
        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");

        Harmony.CreateAndPatchAll(typeof(ReviveChecks));
        
        sendStates = new NetworkedEvent("SendStates", ReviveChecks.GetStates);
        sendConfig = new NetworkedEvent("SendConfig", GetHostConfig);
        
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
        configList[0] = configRespawnTime.Value;
        configList[1] = configRespawnHealth.Value;
        configList[2] = configRespawnInv.Value;
        sendConfig.RaiseEvent(configList, REPOLib.Modules.NetworkingEvents.RaiseOthers, SendOptions.SendReliable);
    }

    private static void GetHostConfig(EventData data)
    {
        float[] config = data.CustomData as float[];
        hostRespawnTime = config[0];
        hostRespawnHealth = config[1];
        hostRespawnInv = config[2];
    }
}