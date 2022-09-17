using HarmonyLib;
using UnityEngine;

namespace DysonSphereProgram.Modding.OffGridConstruction;

public class BuildGridCamera : MonoBehaviour
{
  Camera camera;

  void Start()
  {
    camera = gameObject.GetComponent<Camera>();
    
    camera.farClipPlane = GameCamera.main.farClipPlane;
    camera.fieldOfView = GameCamera.main.fieldOfView;
    camera.focalLength = GameCamera.main.focalLength;
    camera.nearClipPlane = GameCamera.main.nearClipPlane;

    camera.renderingPath = RenderingPath.Forward;
    camera.clearFlags = CameraClearFlags.Nothing;
    camera.backgroundColor = GameCamera.main.backgroundColor;

    camera.cullingMask = 1 << 14;
  }

  public void FrameLogic()
  {
    if (!camera)
      return;
    camera.transform.position = GridRotation.PrefixPatch(GameCamera.main.transform.position);
    camera.transform.rotation = GridRotation.PrefixPatch(GameCamera.main.transform.rotation);
    camera.focalLength = GameCamera.main.focalLength;
    camera.fieldOfView = GameCamera.main.fieldOfView;
    camera.nearClipPlane = GameCamera.main.nearClipPlane;
    camera.farClipPlane = GameCamera.main.farClipPlane;
  }

  Vector3 sunlightDir;
  void OnPreRender()
  {
    sunlightDir = Shader.GetGlobalVector("_Global_SunDir");
    Shader.SetGlobalVector("_Global_SunDir", GridRotation.PrefixPatch(sunlightDir));
  }

  void OnPostRender()
  {
    Shader.SetGlobalVector("_Global_SunDir", sunlightDir);
  }
}

public static class BuildGridCameraPatch
{
  static BuildGridCamera buildGridCamera;
  
  [HarmonyPostfix]
  [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.FrameLogic))]
  static void FrameLogicPostfix()
  {
    GameCamera.main.cullingMask &= ~(1 << 14);
    if (buildGridCamera)
      buildGridCamera.FrameLogic();
  }

  [HarmonyPostfix]
  [HarmonyPatch(typeof(UIRoot), nameof(UIRoot._OnCreate))]
  public static void Create(UIRoot __instance)
  {
    if (buildGridCamera)
      return;
    var go = new GameObject("Build Grid Camera");
    go.AddComponent<Camera>();
    go.AddComponent<BuildGridCamera>();
    buildGridCamera = go.GetComponent<BuildGridCamera>();
    var index = __instance.transform.GetSiblingIndex();
    go.transform.SetSiblingIndex(index + 1);
  }

  [HarmonyPrefix]
  [HarmonyPatch(typeof(UIRoot), nameof(UIRoot._OnDestroy))]
  public static void Destroy()
  {
    if (buildGridCamera && buildGridCamera.gameObject)
      Object.Destroy(buildGridCamera.gameObject);
    buildGridCamera = null;
  }
}