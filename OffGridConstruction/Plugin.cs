using System;
using BepInEx;
using BepInEx.Logging;
using CommonAPI;
using CommonAPI.Systems;
using HarmonyLib;

namespace DysonSphereProgram.Modding.OffGridConstruction
{
  [BepInAutoPlugin("dev.raptor.dsp.OffGridConstruction", "OffGridConstruction")]
  [BepInProcess("DSPGAME.exe")]
  [BepInDependency(CommonAPIPlugin.GUID)]
  [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomKeyBindSystem))]
  public partial class Plugin : BaseUnityPlugin
  {
    private Harmony _harmony;
    internal static ManualLogSource Log;

    private void Awake()
    {
      Plugin.Log = Logger;
      _harmony = new Harmony(Plugin.Id);
      _harmony.Patch(
        AccessTools.Method(typeof(BuildTool_Click), nameof(BuildTool_Click.DeterminePreviews))
        , transpiler: new HarmonyMethod(typeof(OffGridConstruction), nameof(OffGridConstruction.PatchToPerformSteppedRotate))
      );
      _harmony.PatchAll(typeof(OffGridConstruction));
      _harmony.PatchAll(typeof(BuildGridCameraPatch));
      var uiRoot = UIRoot.instance;
      if (uiRoot && uiRoot.created)
        BuildGridCameraPatch.Create(uiRoot);
      _harmony.PatchAll(typeof(GridRotationPatches));
      _harmony.PatchAll(typeof(InputControl));
      KeyBinds.RegisterKeyBinds();
      Logger.LogInfo("OffGridConstruction Awake() called");
    }

    private void OnDestroy()
    {
      Logger.LogInfo("OffGridConstruction OnDestroy() called");
      _harmony?.UnpatchSelf();
      BuildGridCameraPatch.Destroy();
      Plugin.Log = null;
    }
  }
}

namespace System.Runtime.CompilerServices
{
  public record IsExternalInit;
}