using System.Collections.Generic;
using SuperNewRoles.Mode.PlusMode;

namespace SuperNewRoles.Mode;

enum PlusModeId
{
    No,
    NotSabotage,
    NotTaskWin
}
class PlusModeHandler
{
    public static List<PlusModeId> thisPlusModes;
    public static void ClearAndReload()
    {
        thisPlusModes = new List<PlusModeId>();
        foreach (PlusModeId mode in PlusModeIds.AsSpan())
        {
            if (IsMode(mode))
            {
                thisPlusModes.Add(mode);
            }
        }
    }
    public static List<PlusModeId> PlusModeIds = new()
        {
            PlusModeId.NotSabotage,
            PlusModeId.NotTaskWin
        };
    public static bool IsMode(PlusModeId Modeid)
    {
        return Modeid switch
        {
            PlusModeId.NotSabotage => PlusGameOptions.PlusGameOptionSetting.GetBool() && PlusGameOptions.NoSabotageModeSetting.GetBool(),
            PlusModeId.NotTaskWin => PlusGameOptions.PlusGameOptionSetting.GetBool() && PlusGameOptions.NoTaskWinModeSetting.GetBool(),
            _ => false,
        };
    }
}