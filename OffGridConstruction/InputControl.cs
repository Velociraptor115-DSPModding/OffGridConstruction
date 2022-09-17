using HarmonyLib;
using UnityEngine;

namespace DysonSphereProgram.Modding.OffGridConstruction;

public static class InputControl
{
  public static Quaternion extraRot = Quaternion.identity;
  public static float yaw;
  public static float segmentModifier = 1f;

  public static bool IsGridControlHeld;
  public static bool IsUsingEquator = true;
  public static bool hadTargetLastUpdate = false;

  [HarmonyPostfix]
  [HarmonyPatch(typeof(VFInput), nameof(VFInput.OnUpdate))]
  public static void Update()
  {
    var hadTarget = hadTargetLastUpdate;
    hadTargetLastUpdate = false;
    IsGridControlHeld = KeyBinds.GridControl.IsActive;
    if (!IsGridControlHeld)
      return;
    
    var localPlanet = GameMain.localPlanet;
    if (localPlanet == null)
      return;
    
    IsGridControlHeld = false;
    
    InternalUpdate(hadTarget, localPlanet.realRadius + 0.2f);
    
    GridRotation.SetRotationOverride(extraRot);
    IsGridControlHeld = true;
  }

  private static void InternalUpdate(bool hadTarget, float groundHeight)
  {
    if (VFInput._buildModeKey)
    {
      extraRot = Quaternion.identity;
      yaw = 0;
    }

    if (VFInput._jump.onDown)
      IsUsingEquator = !IsUsingEquator;

    if (VFInput._ignoreGrid)
    {
      const int layerMask = 8720;
      var mouseRay = GameCamera.main.ScreenPointToRay(Input.mousePosition);
      var castGround = Physics.Raycast(mouseRay, out var raycastHit, 400f, layerMask, QueryTriggerInteraction.Collide);
      if (!castGround)
      {
        var ray = new Ray(mouseRay.GetPoint(200f), -mouseRay.direction);
        castGround = Physics.Raycast(ray, out raycastHit, 200f, layerMask, QueryTriggerInteraction.Collide);
      }

      if (!castGround)
        return;

      var castGroundPosRaw = raycastHit.point;
      var castGroundPos = castGroundPosRaw.normalized * groundHeight;
      
      if (VFInput._rotate)
      {
        yaw += 1f;
      }

      if (VFInput._counterRotate)
      {
        yaw -= 1f;
      }
      
      extraRot = Quaternion.AngleAxis(yaw, castGroundPos) * Quaternion.FromToRotation(Vector3.up, castGroundPos);
      if (IsUsingEquator)
        extraRot *= Quaternion.FromToRotation(Vector3.up, Vector3.right);
    }
    else
    {
      var raycastLogic = GameMain.mainPlayer.controller.cmd.raycast;
      var castObjectPos = raycastLogic.castObjectPos;
      var hasTarget = castObjectPos != Vector3.zero && raycastLogic.castItemProto != null;
      if (hasTarget)
      {
        if (hadTarget)
        {
          if (VFInput._rotate.onDown)
          {
            yaw += 90f;
            yaw = Mathf.Repeat(yaw, 360f); ;
          }
          if (VFInput._counterRotate.onDown)
          {
            yaw -= 90f;
            yaw = Mathf.Repeat(yaw, 360f);
          }
        }
        else
        {
          yaw = GetYawFromObjectRot(castObjectPos, raycastLogic.castObjectRot);
        }
        extraRot = raycastLogic.castObjectRot;
        if (IsUsingEquator)
          extraRot *= Quaternion.FromToRotation(Vector3.up, Vector3.right);
        hadTargetLastUpdate = true;
      }
    }
    
    if (VFInput._moveRight)
    {
      extraRot = Quaternion.AngleAxis(0.1f, Vector3.right) * extraRot;
    }

    if (VFInput._moveLeft)
    {
      extraRot = Quaternion.AngleAxis(-0.1f, Vector3.right) * extraRot;
    }

    if (VFInput._moveForward)
    {
      extraRot = Quaternion.AngleAxis(-0.1f, Vector3.forward) * extraRot;
    }

    if (VFInput._moveBackward)
    {
      extraRot = Quaternion.AngleAxis(0.1f, Vector3.forward) * extraRot;
    }
  }

  [HarmonyPostfix]
  [HarmonyPatch(typeof(VFInput), nameof(VFInput._rotate), MethodType.Getter)]
  [HarmonyPatch(typeof(VFInput), nameof(VFInput._counterRotate), MethodType.Getter)]
  [HarmonyPatch(typeof(VFInput), nameof(VFInput._moveForward), MethodType.Getter)]
  [HarmonyPatch(typeof(VFInput), nameof(VFInput._moveBackward), MethodType.Getter)]
  [HarmonyPatch(typeof(VFInput), nameof(VFInput._moveLeft), MethodType.Getter)]
  [HarmonyPatch(typeof(VFInput), nameof(VFInput._moveRight), MethodType.Getter)]
  [HarmonyPatch(typeof(VFInput), nameof(VFInput._jump), MethodType.Getter)]
  public static void ReturnDefaultIfGridControlIsPressed(ref VFInput.InputValue __result)
  {
    if (!IsGridControlHeld)
      return;
    __result = default;
  }
  
  [HarmonyPostfix]
  [HarmonyPatch(typeof(VFInput), nameof(VFInput._buildModeKey), MethodType.Getter)]
  [HarmonyPatch(typeof(VFInput), nameof(VFInput._ignoreGrid), MethodType.Getter)]
  public static void ReturnFalseIfGridControlIsPressed(ref bool __result)
  {
    if (!IsGridControlHeld)
      return;
    __result = false;
  }

  public static float GetYawFromObjectRot(Vector3 pos, Quaternion rot)
  {
    pos.Normalize();
    Vector3 lhs = Vector3.Cross(pos, Vector3.up).normalized;
    Vector3 forward;
    if (lhs.sqrMagnitude < 0.0001f)
    {
      float d = Mathf.Sign(pos.y);
      lhs = Vector3.right * d;
      forward = Vector3.forward * d;
    }
    else
    {
      forward = Vector3.Cross(lhs, pos).normalized;
    }
  
    var orig = Quaternion.Inverse(Quaternion.LookRotation(forward, pos)) * rot;
    Plugin.Log.LogDebug(orig.eulerAngles);
    return orig.eulerAngles.y;
  }
}