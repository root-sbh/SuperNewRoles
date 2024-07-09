
using System;
using System.Linq;
using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate.Pteranodontwo;

public class Pteranodontwo : RoleBase, ICrewmate, ICustomButton, IFixedUpdaterMe, IFixedUpdaterAll, IRpcHandler, IWrapUpHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(Pteranodontwo),
        (p) => new Pteranodontwo(p),
        RoleId.Pteranodon,
        "Pteranodon",
        new(17, 128, 45, byte.MaxValue),
        new(RoleId.Pteranodon, TeamTag.Crewmate),
        TeamRoleType.Crewmate,
        TeamType.Crewmate
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.Pteranodon, 406000, false,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.Pteranodon, introSound: RoleTypes.Crewmate);
    public static CustomOption PteranodonPlayerCount;
    public static CustomOption PteranodonCoolTime;

    public bool IsPteranodonNow;
    public Vector2 StartPosition;
    public Vector2 TargetPosition;
    public Vector2 CurrentPosition;
    public float Timer;
    public static Dictionary<byte, (float, float, Vector2)> UsingPlayers;
    public const float StartTime = 2f;
    public static FollowerCamera FCamera
    {
        get
        {
            if (_camera == null)
                _camera = Camera.main.GetComponent<FollowerCamera>();
            return _camera;
        }
    }
    public static FollowerCamera _camera;

    public CustomButtonInfo[] CustomButtonInfos => throw new System.NotImplementedException();

    private static void CreateOption()
    {
        PteranodonPlayerCount = CustomOption.Create(Optioninfo.OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], Optioninfo.RoleOption);
        PteranodonCoolTime = CustomOption.Create(Optioninfo.OptionId + 2, false, CustomOptionType.Crewmate, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, Optioninfo.RoleOption);
    }

    public void FixedUpdateAllDefault()
    {
        //これが呼ばれた時点で呼ばれた人(this.Player)はPteranodonなので、UsingPlayersを使う必要ないのでは？
        foreach (var data in UsingPlayers.ToArray())
        {
            PlayerControl target = ModHelpers.PlayerById(data.Key);
            if (target.IsDead())
            {
                UsingPlayers.Remove(data.Key);
                continue;
            }
            float NewTimer = data.Value.Item2 - Time.fixedDeltaTime;
            Vector3 pos = data.Value.Item3;
            target.MyPhysics.Animations.PlayIdleAnimation();
            float tarpos = data.Value.Item1;
            if ((NewTimer + 0.025f) > (StartTime / 2f))
            {
                pos.y += (((NewTimer + 0.025f) - (StartTime / 2)) * 4f) * Time.fixedDeltaTime;
            }
            else
            {
                pos.y -= (((StartTime / 2) - (NewTimer + 0.025f)) * 4f) * Time.fixedDeltaTime;
            }
            pos.x += tarpos * Time.fixedDeltaTime * 0.5f;
            if (NewTimer <= 0)
            {
                UsingPlayers.Remove(data.Key);
            }
            target.NetTransform.SnapTo(pos);
            UsingPlayers[data.Key] = (data.Value.Item1, NewTimer, pos);
        }
    }
    public void FixedUpdateMeDefaultAlive()
    {
        if (IsPteranodonNow)
        {
            Timer -= Time.fixedDeltaTime;
            Vector3 pos = CurrentPosition;
            PlayerControl.LocalPlayer.MyPhysics.Animations.PlayIdleAnimation();
            float tarpos = (TargetPosition.x - StartPosition.x);
            if ((Timer + 0.025f) > (StartTime / 2f))
            {
                pos.y += (((Timer + 0.025f) - (StartTime / 2)) * 4f) * Time.fixedDeltaTime;
            }
            else
            {
                pos.y -= (((StartTime / 2) - (Timer + 0.025f)) * 4f) * Time.fixedDeltaTime;
            }
            pos.x += tarpos * Time.fixedDeltaTime * 0.5f;
            if (Timer <= 0)
            {
                IsPteranodonNow = false;
                PlayerControl.LocalPlayer.Collider.enabled = true;
                PlayerControl.LocalPlayer.moveable = true;

                Vector3 position = PlayerControl.LocalPlayer.transform.position;
                byte[] buff = new byte[sizeof(float) * 3];
                Buffer.BlockCopy(BitConverter.GetBytes(position.x), 0, buff, 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(position.y), 0, buff, 1 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(position.z), 0, buff, 2 * sizeof(float), sizeof(float));

                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.PteranodonSetStatus);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(false);
                writer.Write(false);
                writer.Write(0f);
                writer.Write(buff.Length);
                writer.Write(buff);
                writer.EndRPC();
            }
            PlayerControl.LocalPlayer.NetTransform.SnapTo(pos);
            CurrentPosition = pos;
            FCamera.transform.position = Vector3.Lerp(FCamera.centerPosition, (Vector2)FCamera.Target.transform.position + FCamera.Offset, 5f * Time.deltaTime);
        }
    }

    public void RpcReader(MessageReader reader)
    {
        throw new System.NotImplementedException();
    }

    public void OnWrapUp()
    {
        IsPteranodonNow = false;
        PlayerControl.LocalPlayer.Collider.enabled = true;
    }

    public Pteranodontwo(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleAnimation))]
    private static class PlayerPhysicsHandleAnimationPatch
    {
        private static void Postfix(PlayerPhysics __instance)
        {
            //LocalPlayerだろうがこのDictionaryに突っ込めば全部解決……だけどこのDictionaryを使いたくないので要検討
            if (UsingPlayers.ContainsKey(__instance.myPlayer.PlayerId))
            {
                __instance.GetSkin().SetIdle(__instance.FlipX);
            }
        }
    }
}