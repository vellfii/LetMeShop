using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

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
    
    private void Awake()
    {
        Instance = this;

        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;
        
        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");

        Harmony.CreateAndPatchAll(typeof(ReviveChecks));
        
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
}