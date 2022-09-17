using System.Collections.Generic;
using UnityEngine;
using CommonAPI.Systems;

namespace DysonSphereProgram.Modding.OffGridConstruction;

public record KeyBind(string Id, string Description, CombineKey DefaultBinding, int ConflictGroup)
{
  public bool IsActive => CustomKeyBindSystem.GetKeyBind(Id)?.keyValue ?? false;
}

public static class KeyBinds
{
  public static readonly KeyBind GridControl = new(
    nameof(GridControl)
    , "Control Build Grid (Hold)"
    , new CombineKey((int) KeyCode.Z, 8, ECombineKeyAction.LongPress, false)
    , KeyBindConflict.CAMERA_1 | KeyBindConflict.CAMERA_2 | KeyBindConflict.MOVEMENT | KeyBindConflict.BUILD_MODE_1 | KeyBindConflict.KEYBOARD_KEYBIND
  );

  private static readonly KeyBind[] keyBinds = {
    GridControl
  };

  public static void RegisterKeyBinds()
  {
    foreach (var keyBind in keyBinds)
    {
      if (!CustomKeyBindSystem.HasKeyBind(keyBind.Id))
      {
        var builtinKey = new BuiltinKey
        {
          name = keyBind.Id,
          id = 0,
          key = keyBind.DefaultBinding,
          canOverride = true,
          conflictGroup = keyBind.ConflictGroup
        };
        if (builtinKey.key.action == ECombineKeyAction.LongPress)
          CustomKeyBindSystem.RegisterKeyBind<HoldKeyBind>(builtinKey);
        else
          CustomKeyBindSystem.RegisterKeyBind<PressKeyBind>(builtinKey);
        ProtoRegistry.RegisterString("KEY" + keyBind.Id, keyBind.Description);
      }
    }
  }
}