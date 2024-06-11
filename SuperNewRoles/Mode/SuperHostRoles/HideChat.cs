using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles;
public static class HideChat
{
    public static bool HideChatEnabled = true;
    private static bool SerializeByHideChat = false;
    public static bool CanSerializeGameData => !HideChatEnabled || SerializeByHideChat || !RoleClass.IsMeeting;
    public static void OnStartMeeting()
    {
        _ = new LateTask(() => DesyncSetDead(null), 1.5f);
    }
    private static void DesyncSetDead(CustomRpcSender? sender)
    {
        PlayerData<AliveState> AliveStates = new();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            AliveStates[player.PlayerId] = new(player.Data);
        }
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (AliveStates[player] == null ||
                AliveStates[player].IsDead ||
                AliveStates[player].Disconnected)
                continue;
            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
            if (player.IsMod()) continue;
            foreach (PlayerControl player2 in PlayerControl.AllPlayerControls)
            {
                player2.Data.IsDead = true;
            }
            player.Data.IsDead = false;

            // シリアライズ
            SerializeByHideChat = true;
            RPCHelper.RpcSyncGameData(sender, player.GetClientId());
            SerializeByHideChat = false;
        }
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            player.Data.IsDead = AliveStates[player.PlayerId].IsDead;
            player.Data.Disconnected = AliveStates[player.PlayerId].Disconnected;
        }
    }
    public static void OnAddChat(PlayerControl player, string message, bool isAdd)
    {
        if (!RoleClass.IsMeeting || !HideChatEnabled || player.IsDead())
            return;
        CustomRpcSender sender = new("HideChatSender", sendOption: Hazel.SendOption.Reliable, false);
        if (isAdd)
        {
            player.Data.IsDead = false;
            SerializeByHideChat = true;
            RPCHelper.RpcSyncGameData(sender: sender);
            SerializeByHideChat = false;
            foreach (PlayerControl target in PlayerControl.AllPlayerControls)
            {
                if (player == target || target.IsDead())
                    continue;
                if (target.PlayerId == 0)
                    continue;
                if (target.IsMod())
                    continue;
                player.RPCSendChatPrivate(message, target, sender);
                sender.EndMessage();
            }
        }
        DesyncSetDead(sender);
        sender.SendMessage();
    }
}
public class AliveState
{
    public bool IsDead { get; }
    public bool Disconnected { get;}
    public AliveState(GameData.PlayerInfo player)
    {
        IsDead = player.IsDead;
        Disconnected = player.Disconnected;
    }
}