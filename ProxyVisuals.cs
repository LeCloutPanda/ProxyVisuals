using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using Renderite.Shared;

namespace ProxyVisuals;

public class ProxyVisuals : ResoniteMod
{
  public override string Name => "Proxy Visuals";
  public override string Author => "LeCloutPanda";
  public override string Version => "1.0.2";

  [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> ENABLED = new ModConfigurationKey<bool>("Enabled", "", () => false);
  [AutoRegisterConfigKey] private static readonly ModConfigurationKey<float> SIZE = new ModConfigurationKey<float>("Size multiplier", "", () => 1.0f);
  [AutoRegisterConfigKey] private static readonly ModConfigurationKey<colorX> BACKGROUNDCOLOR = new ModConfigurationKey<colorX>("Background Color", "", () => RadiantUI_Constants.Neutrals.DARK);
  [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> DISABLE_BACKGROUND = new ModConfigurationKey<bool>("Disable Background", "", () => false);
  [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> DiSABLE_FAILSAFE = new ModConfigurationKey<bool>("Disable Error Handling", "", () => true);
  private static ModConfiguration config;

  public override void OnEngineInit()
  {
    config = GetConfiguration();
    Harmony harmony = new Harmony("dev.lecloutpanda.proxyvisuals");
    harmony.PatchAll();
  }

  [HarmonyPatch]
  private static class InspectorHelperPatch
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
        if (config.GetValue(DISABLE_BACKGROUND))
        {
          slot[0].GetComponentInChildren<Image>().Enabled = false;
          Text text = slot[0].GetComponentInChildren<Text>();
          UI_TextUnlitMaterial material = text.Slot.AttachComponent<UI_TextUnlitMaterial>();
          material.ZWrite.Value = ZWrite.On;
          text.Materials[0] = material;
        }
        else
        {
          slot[0].GetComponentInChildren<Image>().Tint.Value = config.GetValue(BACKGROUNDCOLOR);
        }
      }
      catch (Exception ex)
      {
        Error(ex);
        if (!config.GetValue(DiSABLE_FAILSAFE)) return;
        config.Set(ENABLED, false);
        Msg("Disabling mod to prevent further issues.");
      }
    }
  }
}
