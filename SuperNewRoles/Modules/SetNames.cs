using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sentry.Unity.NativeUtils;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Modules;

public class SetNamesClass
{
    public static string QuarreledSuffix = ModHelpers.Cs(RoleClass.Quarreled.color, " ○");
    public static string LoversSuffix = ModHelpers.Cs(RoleClass.Lovers.color, " ♥");
    public static string DemonSuffix = ModHelpers.Cs(RoleClass.Demon.color, " ▲");
    public static string ArsonistSuffix = ModHelpers.Cs(RoleClass.Arsonist.color, " §");
    public static string MoiraSuffix = " (→←)";
    public static string SatsumaimoCrewSuffix = ModHelpers.Cs(Palette.White, " (C)");
    public static string SatsumaimoMadSuffix = ModHelpers.Cs(RoleClass.ImpostorRed, " (M)");
    public static string PartTimerSuffix = ModHelpers.Cs(RoleClass.PartTimer.color, "◀");
    public static string BulletSuffix = ModHelpers.Cs(WaveCannonJackal.Roleinfo.RoleColor, "☆");

    public static Dictionary<int, string> PlayerNameTexts = new();
    public static Dictionary<int, PlayerSuffixBuilder> PlayerNameSuffixes = new();
    public static Dictionary<int, Color> PlayerNameColors = new();
    public static HashSet<int> PlayerNameColorUpdated = new();
    public static HashSet<int> PlayerRoleInfoUpdated = new();

    public static void ApplyPlayerNameView(PlayerControl p)
    {
        if (PlayerNameTexts.TryGetValue(p.PlayerId, out string n)) p.NameText().text = $"{(ModHelpers.HidePlayerName(PlayerControl.LocalPlayer, p) ? string.Empty : n)}{PlayerNameSuffixes[p.PlayerId].Suffix}";
        if (PlayerNameColors.TryGetValue(p.PlayerId, out Color c)) p.NameText().color = c;
    }
    public static void ApplyAllMeetingPlayerNameView()
    {
        foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
        {
            if (PlayerNameTexts.TryGetValue(player.TargetPlayerId, out string n)) player.NameText.text = $"{n}{PlayerNameSuffixes[player.TargetPlayerId].Suffix}";
            if (PlayerNameColors.TryGetValue(player.TargetPlayerId, out Color c)) player.NameText.color = c;
        }
    }
    public static void SetPlayerNameColor(PlayerControl player, Color? c = null)
    {
        if (PlayerNameColorUpdated.Contains(player.PlayerId)) return;
        var role = player.GetRole();
        if (role == RoleId.DefaultRole || (role == RoleId.Bestfalsecharge && player.IsAlive())) return;

        InnerSetPlayerNameColor(player, c ?? CustomRoles.GetRoleColor(player));

        PlayerNameColorUpdated.Add(player.PlayerId);
    }
    public static void InitPlayerNameColors(PlayerControl player)
    {
        if (PlayerNameColorUpdated.Contains(player.PlayerId)) return;
        if ((PlayerControl.LocalPlayer.IsImpostor() && (player.IsImpostor() || player.IsRole(RoleId.Spy, RoleId.Egoist))) || (ModeHandler.IsMode(ModeId.HideAndSeek) && player.IsImpostor()))
            InnerSetPlayerNameColor(player, RoleClass.ImpostorRed);
        else
            InnerSetPlayerNameColor(player, Color.white);
    }
    private static void InnerSetPlayerNameColor(PlayerControl p, Color c)
    {
        if (p.IsBot()) return;
        PlayerNameColors[p.PlayerId] = c;
    }

    public static Dictionary<byte, TextMeshPro> PlayerInfos = new();
    public static Dictionary<byte, TextMeshPro> MeetingPlayerInfos = new();
    public static void SetPlayerRoleInfoView(PlayerControl p, Color roleColors, string roleNames, Dictionary<string, (Color, bool)> attributeRoles, Color? GhostRoleColor = null, string GhostRoleNames = "")
    {
        if (!PlayerInfos.TryGetValue(p.PlayerId, out TextMeshPro playerInfo) || playerInfo == null)
        {
            playerInfo = UnityEngine.Object.Instantiate(p.NameText(), p.NameText().transform.parent);
            playerInfo.fontSize *= 0.75f;
            playerInfo.gameObject.name = "Info";
            PlayerInfos[p.PlayerId] = playerInfo;
        }
        // Set the position every time bc it sometimes ends up in the wrong place due to camoflauge
        playerInfo.transform.localPosition = p.NameText().transform.localPosition + Vector3.up * 0.2f;

        PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
        if ((!MeetingPlayerInfos.TryGetValue(p.PlayerId, out TextMeshPro meetingInfo) || meetingInfo == null) && playerVoteArea != null)
        {
            meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
            meetingInfo.transform.localPosition += Vector3.down * 0.1f;
            meetingInfo.fontSize = 1.5f;
            meetingInfo.gameObject.name = "Info";
            MeetingPlayerInfos[p.PlayerId] = meetingInfo;
        }

        // Set player name higher to align in middle
        if (meetingInfo != null && playerVoteArea != null)
        {
            var playerName = playerVoteArea.NameText;
            playerName.transform.localPosition = new Vector3(0.3384f, 0.0311f + 0.0683f, -0.1f);
        }

        StringBuilder playerInfoTextBuilder = new();
        string playerInfoText = string.Empty;
        if (GhostRoleNames != string.Empty)
        {
            playerInfoTextBuilder.Append($"{CustomOptionHolder.Cs((Color)GhostRoleColor, GhostRoleNames)}({CustomOptionHolder.Cs(roleColors, roleNames)})");
        }
        else
        {
            playerInfoTextBuilder.Append($"{CustomOptionHolder.Cs(roleColors, roleNames)}");
        }

        if (attributeRoles.Count != 0)
        {
            foreach (var kvp in attributeRoles)
            {
                if (!kvp.Value.Item2) continue;
                playerInfoTextBuilder.Append($" + {CustomOptionHolder.Cs(kvp.Value.Item1, kvp.Key)}");
            }
        }

        try
        {
            if (p.IsUseTaskTrigger())
            {
                var (complete, all) = TaskCount.TaskDateNoClearCheck(p.Data);
                playerInfoTextBuilder.Append(ModHelpers.Cs(Color.yellow, $"({(RoleHelpers.IsComms() ? "?" : complete)}/{all})"));
            }
        }
        catch { }

        playerInfoText = playerInfoTextBuilder.ToString();
        playerInfo.text = playerInfoText;
        if (playerInfo.gameObject.active != p.Visible) playerInfo.gameObject.SetActive(p.Visible);
        if (meetingInfo != null)
        {
            meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? string.Empty : playerInfoText.Trim();
            p.NameText().color = roleColors;
        }
    }
    public static void SetPlayerRoleInfo(PlayerControl p)
    {
        if (p.IsBot()) return;
        //SetNamesUpdate.Postfixの1ループ中に、各プレイヤー1回のみ呼び出されるようにする
        if (PlayerRoleInfoUpdated.Contains(p.PlayerId)) return;
        PlayerRoleInfoUpdated.Add(p.PlayerId);
        string roleNames;
        Color roleColors;
        string GhostroleNames = "";
        Color? GhostroleColors = null;

        var role = p.GetRole();
        if (role == RoleId.DefaultRole || (role == RoleId.Bestfalsecharge && p.IsAlive()))
        {
            if (p.IsImpostor())
            {
                roleNames = "ImpostorName";
                roleColors = RoleClass.ImpostorRed;
            }
            else
            {
                roleNames = "CrewmateName";
                roleColors = RoleClass.CrewmateWhite;
            }
        }
        else if (role == RoleId.Stefinder && RoleClass.Stefinder.IsKill)
        {
            roleNames = IntroData.StefinderIntro.Name;
            roleColors = RoleClass.ImpostorRed;
        }
        else if (p.IsPavlovsTeam())
        {
            var introData = IntroData.PavlovsdogsIntro;
            roleNames = $"{introData.Name}{(role == RoleId.Pavlovsdogs ? "(D)" : "(O)")}";
            roleColors = RoleClass.Pavlovsdogs.color;
        }
        else
        {
            roleNames = CustomRoles.GetRoleName(role, p);
            roleColors = CustomRoles.GetRoleColor(role, p);
        }

        var GhostRole = p.GetGhostRole();
        if (GhostRole != RoleId.DefaultRole)
        {
            GhostroleNames = CustomRoles.GetRoleName(GhostRole, p);
            GhostroleColors = CustomRoles.GetRoleColor(GhostRole, p);
        }

        Dictionary<string, (Color, bool)> attributeRoles = new(AttributeRoleNameSet(p));

        SetPlayerRoleInfoView(p, roleColors, roleNames, attributeRoles, GhostroleColors, GhostroleNames);
    }

    /// <summary>
    /// 重複役職の役職名を追加する。
    /// key = 役職名, value.Item1 = 役職カラー, value.Item2 = 役職名の表示条件を達しているか,
    /// </summary>
    /// <param name="player">役職名を表示したいプレイヤー</param>
    /// <param name="seePlayer">役職名を見るプレイヤー</param>
    internal static Dictionary<string, (Color, bool)> AttributeRoleNameSet(PlayerControl player, PlayerControl seePlayer = null)
    {
        Dictionary<string, (Color, bool)> attributeRoles = new();
        if (player.IsHauntedWolf())
        {
            if (seePlayer == null) seePlayer = PlayerControl.LocalPlayer;
            var isSeeing = seePlayer.IsDead() || seePlayer.IsRole(RoleId.God, RoleId.Marlin);
            attributeRoles.Add(ModTranslation.GetString("HauntedWolfName"), (HauntedWolf.RoleData.color, isSeeing));
        }
        return attributeRoles;
    }

    /// <summary>
    /// 死亡後, 全員の役職が見る事ができるか判定する。
    /// 見る事ができない状態は, 見る事ができる状態より優先して反映する。
    /// </summary>
    /// <param name="target">役職を見ようとしているプレイヤー (nullなら処理者本人)</param>
    /// <returns> true:見られる / false:見られない </returns>
    public static bool CheckCanGhostSeeRoles(PlayerControl target = null)
    {
        if (target == null) target = PlayerControl.LocalPlayer;
        if (target.IsDead())
        {
            if (target.GetRoleBase() is INameHandler INameHandler) // 役職の個別判定
            {
                if (!INameHandler.CanGhostSeeRole) return false;  // 死後 役職をみえない役職の場合, 最優先で判定する。
            }

            // ゲーム設定による, 基本的な判定
            // (役職の個別判定で死後役職が見られる場合は, ゲーム設定による判定を優先する)
            if (!Mode.PlusMode.PlusGameOptions.PlusGameOptionSetting.GetBool()) return true; // 上位設定 "ゲームオプション" が有効か
            else
            {
                if (!Mode.PlusMode.PlusGameOptions.CanNotGhostSeeRole.GetBool()) return true;
                else if (Mode.PlusMode.PlusGameOptions.OnlyImpostorGhostSeeRole.GetBool()) return target.IsImpostor();
            }
        }
        return false;
    }

    //TODO: 本来なら毎フレームではなくRole変更やSabotage、タスク完了など、名前/RoleInfoの変更時のみ呼び出されるべき
    public static void Postfix(PlayerControl __instance)
    {
        RoleId LocalRole = PlayerControl.LocalPlayer.GetRole();
        bool CanGhostSeeRoles = CheckCanGhostSeeRoles();
        bool CanSeeAllRole =
            CanGhostSeeRoles ||
            Debugger.canSeeRole ||
            (PlayerControl.LocalPlayer.GetRoleBase() is INameHandler nameHandler &&
            nameHandler.AmAllRoleVisible);
        bool CanSeeImpostor =
            Madmate.CheckImpostor(PlayerControl.LocalPlayer) ||
            LocalRole == RoleId.MadKiller ||
            LocalRole == RoleId.Marlin ||
            (RoleClass.Demon.IsCheckImpostor && LocalRole == RoleId.Demon) ||
            (LocalRole == RoleId.Safecracker && Safecracker.CheckTask(__instance, Safecracker.CheckTasks.CheckImpostor));

        PlayerNameColorUpdated.Clear();
        PlayerRoleInfoUpdated.Clear();

        CheckSuffixes();
        CustomRoles.NameHandler(CanSeeAllRole);

        switch (LocalRole)
        {
            case RoleId.PartTimer:
                if (RoleClass.PartTimer.IsLocalOn)
                {
                    if (CustomOptionHolder.PartTimerIsCheckTargetRole.GetBool())
                    {
                        SetPlayerRoleInfo(RoleClass.PartTimer.CurrentTarget);
                        SetPlayerNameColor(RoleClass.PartTimer.CurrentTarget);
                        PlayerNameColorUpdated.Add(RoleClass.PartTimer.CurrentTarget.PlayerId);
                    }
                    else
                        PlayerNameSuffixes[RoleClass.PartTimer.CurrentTarget.PlayerId].PartTimer = true;
                }
                break;
            case RoleId.TheFirstLittlePig:
            case RoleId.TheSecondLittlePig:
            case RoleId.TheThirdLittlePig:
                foreach (var players in TheThreeLittlePigs.TheThreeLittlePigsPlayer)
                {
                    if (!players.Contains(PlayerControl.LocalPlayer)) continue;
                    foreach (PlayerControl p in players)
                    {
                        SetPlayerRoleInfo(p);
                        SetPlayerNameColor(p);
                        PlayerNameColorUpdated.Add(p.PlayerId);
                    }
                    break;
                }
                break;
            case RoleId.OrientalShaman:
                foreach (var date in OrientalShaman.OrientalShamanCausative)
                {
                    if (date.Key != PlayerControl.LocalPlayer.PlayerId) continue;
                    SetPlayerRoleInfo(ModHelpers.PlayerById(date.Value));
                    SetPlayerNameColor(ModHelpers.PlayerById(date.Value));
                    PlayerNameColorUpdated.Add(date.Value);
                }
                break;
            case RoleId.ShermansServant:
                foreach (var date in OrientalShaman.OrientalShamanCausative)
                {
                    if (date.Value != PlayerControl.LocalPlayer.PlayerId) continue;
                    SetPlayerRoleInfo(ModHelpers.PlayerById(date.Key));
                    SetPlayerNameColor(ModHelpers.PlayerById(date.Key));
                    PlayerNameColorUpdated.Add(date.Value);
                }
                break;
            case RoleId.Pokerface:
                Pokerface.PokerfaceTeam team = Pokerface.GetPokerfaceTeam(PlayerControl.LocalPlayer.PlayerId);
                if (team != null)
                {
                    foreach (PlayerControl member in team.TeamPlayers)
                    {
                        SetPlayerNameColor(member, Pokerface.RoleData.color);
                    }
                }
                break;
            case RoleId.Stefinder:
                //case RoleId.Stefinder1:
                if (RoleClass.Stefinder.IsKill) SetPlayerNameColor(PlayerControl.LocalPlayer, Color.red);
                break;
            case RoleId.SatsumaAndImo:
                SatsumaAndImo satsumaAndImo = RoleBaseManager.GetLocalRoleBase<SatsumaAndImo>();
                if (satsumaAndImo == null) return;
                if (satsumaAndImo.TeamState == SatsumaAndImo.SatsumaTeam.Crewmate)
                {//名前に(C)をつける
                    PlayerNameSuffixes[PlayerControl.LocalPlayer.PlayerId].SatsumaimoCrew = true;
                }
                else if (satsumaAndImo.TeamState == SatsumaAndImo.SatsumaTeam.Madmate)
                {
                    PlayerNameSuffixes[PlayerControl.LocalPlayer.PlayerId].SatsumaimoMad = true;
                }
                break;
        }
        foreach (byte MyPartTimer in RoleClass.PartTimer.Data.WhereSelect(x => x.Value == CachedPlayer.LocalPlayer.PlayerId, x => x.Key))
        {
            SetPlayerRoleInfo(ModHelpers.GetPlayerControl(MyPartTimer));
            SetPlayerNameColor(ModHelpers.GetPlayerControl(MyPartTimer));
        }

        ReadOnlySpan<CachedPlayer> Players = CachedPlayer.AllPlayers.AsSpan();
        foreach (PlayerControl player in Players)
        {
            if (!PlayerNameTexts.ContainsKey(player.PlayerId)) PlayerNameTexts[player.PlayerId] = player.CurrentOutfit.PlayerName;
            if (!PlayerNameSuffixes.ContainsKey(player.PlayerId)) PlayerNameSuffixes[player.PlayerId] = new(player);

            InitPlayerNameColors(player);

            var check = CheckView(player);

            if (check.Item1) SetPlayerNameColor(player, check.Item2);
            if (check.Item3) SetPlayerRoleInfo(player);

            CheckPlayerSuffix(player);

            ApplyPlayerNameView(player);
        }

        if (MeetingHud.Instance) ApplyAllMeetingPlayerNameView();
        return;

        //return TupleValue(NameColorflg, SetColor(null = DefaultRoleColor), RoleInfoflg)
        ValueTuple<bool, Color?, bool> CheckView(PlayerControl player)
        {
            if (player == PlayerControl.LocalPlayer) return (true, null, true);
            if (CanSeeAllRole) return (true, null, true);
            if (LocalRole == RoleId.God && (RoleClass.IsMeeting || player.IsAlive())) return (true, null, true);

            (bool, Color?, bool) check = (false, null, false);

            if (CanSeeImpostor) check.Item1 = true;
            switch (LocalRole)
            {
                case RoleId.Finder:
                    if (RoleClass.Finder.IsCheck && player.IsMadRoles())
                    {
                        check.Item1 = true;
                        check.Item2 = Color.red;
                    }
                    break;
                case RoleId.Dependents:
                    if (RoleClass.Vampire.VampirePlayer.Contains(player)) check.Item1 = true;
                    break;
                case RoleId.Vampire:
                    if (RoleClass.Dependents.DependentsPlayer.Contains(player)) check.Item1 = true;
                    break;
                case RoleId.Fox:
                case RoleId.FireFox:
                    if (RoleClass.Fox.FoxPlayer.Contains(player) || FireFox.FireFoxPlayer.Contains(player))
                    {
                        if (FireFox.FireFoxIsCheckFox.GetBool() || player.IsRole(PlayerControl.LocalPlayer.GetRole()))
                        {
                            check = (true, null, true);
                        }
                    }
                    break;
                case RoleId.OrientalShaman:
                    if (OrientalShaman.IsKiller(player)) check.Item1 = true;
                    break;
            }
            if (PlayerControl.LocalPlayer.IsImpostor())
            {
                if (RoleClass.SideKiller.MadKillerPlayer.Contains(player))
                {
                    check.Item1 = true;
                    check.Item2 = RoleClass.ImpostorRed;
                }
            }
            if ((PlayerControl.LocalPlayer.IsJackalTeam() && !PlayerControl.LocalPlayer.IsFriendRoles()) ||
                JackalFriends.CheckJackal(PlayerControl.LocalPlayer))
            {
                if (player.IsJackalTeam() && (!player.IsFriendRoles() || player.IsRole(RoleId.Bullet)))
                {
                    check = (true, null, true);
                }
            }
            if (Sabotage.SabotageManager.thisSabotage == Sabotage.SabotageManager.CustomSabotage.CognitiveDeficit &&
                ModeHandler.IsMode(ModeId.Default) && PlayerControl.LocalPlayer.IsImpostor() &&
                player.IsAlive() && !Sabotage.CognitiveDeficit.Main.OKPlayers.IsCheckListPlayerControl(player) &&
                !(player.IsImpostor() || player.IsRole(RoleId.MadKiller)))
            {
                check.Item1 = true;
                check.Item2 = new Color32(18, 112, 214, byte.MaxValue);
            }
            if (!RoleClass.Camouflager.IsCamouflage && (RoleClass.Celebrity.ChangeRoleView ? RoleClass.Celebrity.ViewPlayers : RoleClass.Celebrity.CelebrityPlayer).Contains(player))
            {
                check.Item1 = true;
                check.Item2 = RoleClass.Celebrity.color;
            }
            if (!RoleClass.Camouflager.IsCamouflage && PlayerControl.LocalPlayer.IsPavlovsTeam())
            {
                if (RoleClass.Pavlovsdogs.PavlovsdogsPlayer.Contains(player) || RoleClass.Pavlovsowner.PavlovsownerPlayer.Contains(player))
                {
                    check = (true, RoleClass.Pavlovsdogs.color, true);
                }
            }

            return check;
        }

        void CheckPlayerSuffix(PlayerControl player)
        {
            if (!RoleClass.Camouflager.IsCamouflage || RoleClass.Camouflager.ArsonistMark)
                if (LocalRole == RoleId.God || LocalRole == RoleId.Arsonist || CanGhostSeeRoles)
                    if (Arsonist.IsViewIcon(player)) PlayerNameSuffixes[player.PlayerId].Arsonist = true;
            if (!RoleClass.Camouflager.IsCamouflage || RoleClass.Camouflager.DemonMark)
                if (LocalRole == RoleId.God || LocalRole == RoleId.Demon || CanGhostSeeRoles)
                    if (Demon.IsViewIcon(player)) PlayerNameSuffixes[player.PlayerId].Demon = true;
            if (!RoleClass.Camouflager.IsCamouflage || RoleClass.Camouflager.QuarreledMark)
                if ((CanGhostSeeRoles || LocalRole == RoleId.God) && RoleClass.Quarreled.QuarreledPlayer.Count > 0)
                {
                    foreach (List<PlayerControl> ps in RoleClass.Quarreled.QuarreledPlayer.AsSpan())
                    {
                        if (ps.Contains(player) && !player.Data.Disconnected) PlayerNameSuffixes[player.PlayerId].Quarreled = true;
                    }
                }
            if (!RoleClass.Camouflager.IsCamouflage || RoleClass.Camouflager.LoversMark)
                if ((CanGhostSeeRoles || LocalRole == RoleId.God) && RoleClass.Lovers.LoversPlayer.Count > 0)
                {
                    foreach (List<PlayerControl> ps in RoleClass.Lovers.LoversPlayer.AsSpan())
                    {
                        if (ps.Contains(player) && !player.Data.Disconnected) PlayerNameSuffixes[player.PlayerId].Lovers = true;
                    }
                }
            if (RoleClass.Jumbo.BigPlayer.Contains(player) && RoleClass.Jumbo.JumboSize.ContainsKey(player.PlayerId))
                PlayerNameSuffixes[player.PlayerId].Jumbo = true;
        }
        void CheckSuffixes()
        {
            //----Suffixes----
            //Moira
            if (RoleClass.Camouflager.IsCamouflage) return;
            if (!Moira.AbilityUsedUp || Moira.AbilityUsedThisMeeting) return;
            if (Moira.Player is null) return;
            PlayerNameSuffixes[Moira.Player.PlayerId].Moira = true;
            //Quarreled
            if (!RoleClass.Camouflager.IsCamouflage || RoleClass.Camouflager.QuarreledMark)
            {
                if (PlayerControl.LocalPlayer.IsQuarreled() && PlayerControl.LocalPlayer.IsAlive())
                {
                    PlayerControl side = PlayerControl.LocalPlayer.GetOneSideQuarreled();
                    PlayerNameSuffixes[PlayerControl.LocalPlayer.PlayerId].Quarreled = true;
                    if (!side.Data.Disconnected) PlayerNameSuffixes[side.PlayerId].Quarreled = true;
                }
            }
            //Lovers
            if (!RoleClass.Camouflager.IsCamouflage || RoleClass.Camouflager.LoversMark)
            {
                if ((PlayerControl.LocalPlayer.IsLovers() || (PlayerControl.LocalPlayer.IsFakeLovers() && !PlayerControl.LocalPlayer.IsFakeLoversFake())) && PlayerControl.LocalPlayer.IsAlive())
                {
                    PlayerControl side = PlayerControl.LocalPlayer.GetOneSideLovers();
                    if (side == null) side = PlayerControl.LocalPlayer.GetOneSideFakeLovers();
                    PlayerNameSuffixes[PlayerControl.LocalPlayer.PlayerId].Lovers = true;
                    if (!side.Data.Disconnected) PlayerNameSuffixes[side.PlayerId].Lovers = true;
                }
            }
            //SatsumaAndImo:自分以外
            if (CanGhostSeeRoles || LocalRole == RoleId.God)
            {
                foreach (SatsumaAndImo player in RoleBaseManager.GetRoleBases<SatsumaAndImo>())
                {
                    //クルーなら
                    if (player.TeamState == SatsumaAndImo.SatsumaTeam.Crewmate)
                    {//名前に(C)をつける
                        PlayerNameSuffixes[player.Player.PlayerId].SatsumaimoCrew = true;
                    }
                    else if (player.TeamState == SatsumaAndImo.SatsumaTeam.Madmate)
                    {
                        PlayerNameSuffixes[player.Player.PlayerId].SatsumaimoMad = true;
                    }
                }
            }
        }
    }
    public class PlayerSuffixBuilder
    {
        private byte playerid;
        private bool changed;
        private bool quarreled;
        public bool Quarreled
        {
            set
            {
                if (value == quarreled) return;
                quarreled = value;
                changed = true;
            }
        }
        private bool lovers;
        public bool Lovers
        {
            set
            {
                if (value == lovers) return;
                lovers = value;
                changed = true;
            }
        }
        private bool demon;
        public bool Demon
        {
            set
            {
                if (value == demon) return;
                demon = value;
                changed = true;
            }
        }
        private bool arsonist;
        public bool Arsonist
        {
            set
            {
                if (value == arsonist) return;
                arsonist = value;
                changed = true;
            }
        }
        private bool moira;
        public bool Moira
        {
            set
            {
                if (value == moira) return;
                moira = value;
                changed = true;
            }
        }
        private bool satsumaimoCrew;
        public bool SatsumaimoCrew
        {
            set
            {
                if (value == satsumaimoCrew) return;
                satsumaimoCrew = value;
                satsumaimoMad = !value;
                changed = true;
            }
        }
        private bool satsumaimoMad;
        public bool SatsumaimoMad
        {
            set
            {
                if (value == satsumaimoMad) return;
                satsumaimoMad = value;
                satsumaimoCrew = !value;
                changed = true;
            }
        }
        private bool partTimer;
        public bool PartTimer
        {
            set
            {
                if (value == partTimer) return;
                partTimer = value;
                changed = true;
            }
        }
        private bool medicalTechnologist;
        public bool MedicalTechnologist
        {
            set
            {
                if (value == medicalTechnologist) return;
                medicalTechnologist = value;
                changed = true;
            }
        }
        private bool bullet;
        public bool Bullet
        {
            set
            {
                if (value == bullet) return;
                bullet = value;
                changed = true;
            }
        }
        private bool cupid;
        public bool Cupid
        {
            set
            {
                if (value == cupid) return;
                cupid = value;
                changed = true;
            }
        }
        private bool jumbo;
        public bool Jumbo
        {
            set
            {
                if (value == jumbo) return;
                jumbo = value;
                changed = true;
            }
        }
        private StringBuilder sb = new();
        private string suffix = string.Empty;
        private bool bot;
        public string Suffix
        {
            get
            {
                if (bot) return string.Empty;
                if (!changed) return suffix;
                sb.Clear();
                if (quarreled) sb.Append(QuarreledSuffix);
                if (lovers || cupid) sb.Append(LoversSuffix);
                if (demon) sb.Append(DemonSuffix);
                if (arsonist) sb.Append(ArsonistSuffix);
                if (moira) sb.Append(MoiraSuffix);
                if (satsumaimoCrew) sb.Append(SatsumaimoCrewSuffix);
                if (satsumaimoMad) sb.Append(SatsumaimoMadSuffix);
                if (jumbo) sb.Append($"({(int)(RoleClass.Jumbo.JumboSize[playerid] * 15)})");
                if (partTimer) sb.Append(PartTimerSuffix);
                if (medicalTechnologist) sb.Append(Roles.Crewmate.MedicalTechnologist.ErythrocyteMark);
                if (bullet) sb.Append(BulletSuffix);
                suffix = sb.ToString();
                changed = false;
                return suffix;
            }
        }
        public PlayerSuffixBuilder(PlayerControl p)
        {
            if (p.IsBot()) this.bot = true;
            this.playerid = p.PlayerId;
        }
    }
}

//public class SetNameUpdate
//{
//    public static void Postfix(PlayerControl __instance)
//    {
//        //TODO: 本来なら毎フレームではなくRole変更やSabotage、タスク完了など、名前/RoleInfoの変更時のみ呼び出されるべき
//        SetNamesClass.InitNameTagsAndColors();
//        RoleId LocalRole = PlayerControl.LocalPlayer.GetRole();
//        bool CanSeeAllRole =
//            SetNamesClass.CanGhostSeeRoles() ||
//            Debugger.canSeeRole ||
//            (PlayerControl.LocalPlayer.GetRoleBase() is INameHandler nameHandler &&
//            nameHandler.AmAllRoleVisible);
//        if (CanSeeAllRole)
//        {
//            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
//            {
//                SetNamesClass.SetPlayerNameColors(player);
//                SetNamesClass.SetPlayerRoleInfo(player);
//            }
//        }
//        //TODO:神移行時にINameHandlerに移行する
//        else if (LocalRole == RoleId.God)
//        {
//            foreach (PlayerControl player in CachedPlayer.AllPlayers)
//            {
//                if (RoleClass.IsMeeting || player.IsAlive())
//                {
//                    SetNamesClass.SetPlayerNameColors(player);
//                    SetNamesClass.SetPlayerRoleInfo(player);
//                }
//            }
//        }
//        else
//        {
//            if (Madmate.CheckImpostor(PlayerControl.LocalPlayer) ||
//                LocalRole == RoleId.MadKiller ||
//                LocalRole == RoleId.Marlin ||
//                (RoleClass.Demon.IsCheckImpostor && LocalRole == RoleId.Demon) ||
//                (LocalRole == RoleId.Safecracker && Safecracker.CheckTask(__instance, Safecracker.CheckTasks.CheckImpostor)))
//            {
//                foreach (PlayerControl p in CachedPlayer.AllPlayers)
//                {
//                    if (p.IsImpostorAddedFake())
//                    {
//                        SetNamesClass.SetPlayerNameColor(p, RoleClass.ImpostorRed);
//                    }
//                }
//            }
//            switch (LocalRole)
//            {
//                case RoleId.Finder:
//                    if (RoleClass.Finder.IsCheck)
//                    {
//                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
//                        {
//                            if (player.IsMadRoles())
//                            {
//                                SetNamesClass.SetPlayerNameColor(player, Color.red);
//                            }
//                        }
//                    }
//                    break;
//                case RoleId.Dependents:
//                    foreach (PlayerControl p in RoleClass.Vampire.VampirePlayer)
//                    {
//                        SetNamesClass.SetPlayerNameColors(p);
//                    }
//                    break;
//                case RoleId.Vampire:
//                    foreach (PlayerControl p in RoleClass.Dependents.DependentsPlayer)
//                    {
//                        SetNamesClass.SetPlayerNameColors(p);
//                    }
//                    break;
//                case RoleId.PartTimer:
//                    if (RoleClass.PartTimer.IsLocalOn)
//                    {
//                        if (CustomOptionHolder.PartTimerIsCheckTargetRole.GetBool())
//                        {
//                            SetNamesClass.SetPlayerRoleInfo(RoleClass.PartTimer.CurrentTarget);
//                            SetNamesClass.SetPlayerNameColors(RoleClass.PartTimer.CurrentTarget);
//                        }
//                        else
//                        {
//                            SetNamesClass.PlayerNameSuffixes[RoleClass.PartTimer.CurrentTarget.PlayerId].PartTimer = true;
//                        }
//                    }
//                    break;
//                case RoleId.Fox:
//                case RoleId.FireFox:
//                    List<PlayerControl> foxs = new(RoleClass.Fox.FoxPlayer);
//                    foxs.AddRange(FireFox.FireFoxPlayer);
//                    foreach (PlayerControl p in foxs)
//                    {
//                        if (FireFox.FireFoxIsCheckFox.GetBool() || p.IsRole(PlayerControl.LocalPlayer.GetRole()))
//                        {
//                            SetNamesClass.SetPlayerRoleInfo(p);
//                            SetNamesClass.SetPlayerNameColors(p);
//                        }
//                    }
//                    break;
//                case RoleId.TheFirstLittlePig:
//                case RoleId.TheSecondLittlePig:
//                case RoleId.TheThirdLittlePig:
//                    foreach (var players in TheThreeLittlePigs.TheThreeLittlePigsPlayer)
//                    {
//                        if (!players.Contains(PlayerControl.LocalPlayer)) continue;
//                        foreach (PlayerControl p in players)
//                        {
//                            SetNamesClass.SetPlayerRoleInfo(p);
//                            SetNamesClass.SetPlayerNameColors(p);
//                        }
//                        break;
//                    }
//                    break;
//                case RoleId.OrientalShaman:
//                    foreach (var date in OrientalShaman.OrientalShamanCausative)
//                    {
//                        if (date.Key != PlayerControl.LocalPlayer.PlayerId) continue;
//                        SetNamesClass.SetPlayerRoleInfo(ModHelpers.PlayerById(date.Value));
//                        SetNamesClass.SetPlayerNameColors(ModHelpers.PlayerById(date.Value));
//                    }
//                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
//                    {
//                        if (OrientalShaman.IsKiller(player))
//                            SetNamesClass.SetPlayerNameColors(player);
//                    }
//                    break;
//                case RoleId.ShermansServant:
//                    foreach (var date in OrientalShaman.OrientalShamanCausative)
//                    {
//                        if (date.Value != PlayerControl.LocalPlayer.PlayerId) continue;
//                        SetNamesClass.SetPlayerRoleInfo(ModHelpers.PlayerById(date.Key));
//                        SetNamesClass.SetPlayerNameColors(ModHelpers.PlayerById(date.Key));
//                    }
//                    break;
//                case RoleId.Pokerface:
//                    Pokerface.PokerfaceTeam team = Pokerface.GetPokerfaceTeam(PlayerControl.LocalPlayer.PlayerId);
//                    if (team != null)
//                    {
//                        foreach (PlayerControl member in team.TeamPlayers)
//                        {
//                            SetNamesClass.SetPlayerNameColor(member, Pokerface.RoleData.color);
//                        }
//                    }
//                    break;
//            }
//            if (PlayerControl.LocalPlayer.IsImpostor())
//            {
//                foreach (PlayerControl p in RoleClass.SideKiller.MadKillerPlayer)
//                {
//                    SetNamesClass.SetPlayerNameColor(p, RoleClass.ImpostorRed);
//                }
//            }
//            if ((PlayerControl.LocalPlayer.IsJackalTeam() && !PlayerControl.LocalPlayer.IsFriendRoles()) ||
//                JackalFriends.CheckJackal(PlayerControl.LocalPlayer))
//            {
//                foreach (PlayerControl p in CachedPlayer.AllPlayers)
//                {
//                    if (p.PlayerId == PlayerControl.LocalPlayer.PlayerId)
//                        continue;
//                    if (!p.IsJackalTeam())
//                        continue;
//                    if (p.IsFriendRoles() && !p.IsRole(RoleId.Bullet))
//                        continue;
//                    SetNamesClass.SetPlayerRoleInfo(p);
//                    SetNamesClass.SetPlayerNameColors(p);
//                }
//            }
//            SetNamesClass.SetPlayerRoleInfo(PlayerControl.LocalPlayer);
//            SetNamesClass.SetPlayerNameColors(PlayerControl.LocalPlayer);
//        }
//        CustomRoles.NameHandler(CanSeeAllRole);
//        //Suffixes
//        Pavlovsdogs.SetNameUpdate();
//        SetNamesClass.ArsonistSet();
//        SetNamesClass.DemonSet();
//        SetNamesClass.CelebritySet();
//        SetNamesClass.QuarreledSet();
//        SetNamesClass.LoversSet();
//        SetNamesClass.MoiraSet();
//        SetNamesClass.SatsumaimoSet();
//        SetNamesClass.JumboSet();

//        if (RoleClass.PartTimer.Data.ContainsValue(CachedPlayer.LocalPlayer.PlayerId))
//        {
//            PlayerControl PartTimerTarget = RoleClass.PartTimer.Data.GetPCByValue(PlayerControl.LocalPlayer.PlayerId);
//            SetNamesClass.SetPlayerRoleInfo(PartTimerTarget);
//            SetNamesClass.SetPlayerNameColors(PartTimerTarget);
//        }
//        if (RoleClass.Stefinder.IsKill)
//        {
//            SetNamesClass.SetPlayerNameColor(PlayerControl.LocalPlayer, Color.red);
//        }
//        if (ModeHandler.IsMode(ModeId.Default))
//        {
//            if (Sabotage.SabotageManager.thisSabotage == Sabotage.SabotageManager.CustomSabotage.CognitiveDeficit)
//            {
//                foreach (PlayerControl p3 in CachedPlayer.AllPlayers)
//                {
//                    if (p3.IsAlive() && !Sabotage.CognitiveDeficit.Main.OKPlayers.IsCheckListPlayerControl(p3))
//                    {
//                        if (PlayerControl.LocalPlayer.IsImpostor())
//                        {
//                            if (!(p3.IsImpostor() || p3.IsRole(RoleId.MadKiller)))
//                            {
//                                SetNamesClass.SetPlayerNameColor(p3, new Color32(18, 112, 214, byte.MaxValue));
//                            }
//                        }
//                    }
//                }
//            }
//        }
//        SetNamesClass.ApplyAllPlayerNameTextAndColor();
//    }
//}