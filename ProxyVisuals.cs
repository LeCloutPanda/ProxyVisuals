using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;

namespace ProxyVisuals;

public class ProxyVisuals : ResoniteMod
{
  public override string Name => "Proxy Visuals";
  public override string Author => "LeCloutPanda";
  public override string Version => "1.0.0";

  [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> ENABLED = new ModConfigurationKey<bool>("Enabled", "", () => false);
  [AutoRegisterConfigKey] private static readonly ModConfigurationKey<float> SIZE = new ModConfigurationKey<float>("Size multiplier", "", () => 1.0f);
  [AutoRegisterConfigKey] private static readonly ModConfigurationKey<colorX> BACKGROUNDCOLOR = new ModConfigurationKey<colorX>("Background Color", "", () => RadiantUI_Constants.Neutrals.DARK);
  private static ModConfiguration config;

  public override void OnEngineInit()
  {
    config = GetConfiguration();
    Harmony harmony = new Harmony("dev.lecloutpanda.proxyvisuals");
    harmony.PatchAll();
  }

  [HarmonyPatch]
  private static class ReferenceProxyPatch
  {
    [HarmonyPatch(typeof(InspectorHelper), nameof(InspectorHelper.SetupProxyVisual))]
    [HarmonyPostfix]
    private static void ModifyFinal(Slot slot)
    {
      if (!config.GetValue(ENABLED)) return;
      slot.ReferenceID.ExtractIDs(out var position, out var user);
      User userByAllocationID = slot.World.GetUserByAllocationID(user);
      if (userByAllocationID != slot.LocalUser) return;

      try
      {
        slot.LocalScale = slot.LocalScale * config.GetValue(SIZE);
        slot[0].GetComponentInChildren<Image>().Tint.Value = config.GetValue(BACKGROUNDCOLOR);
      }
      catch (Exception ex)
      {
        Error(ex);
        config.Set(ENABLED, false);
        Msg("Disabling mod to prevent further issues.");
      }
    }
  }
}