using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
public interface INameHandler
{
    /// <summary>
    /// 全員の役職を見れるか
    /// </summary>
    public bool AmAllRoleVisible => false;
    /// <summary>
    /// 死亡後に全員の役職が見られるか, 役職別で設定を行う。
    /// </summary>
    /// <value>true : 見る事ができる, false : 見る事ができない</value>
    public bool CanGhostSeeRole => true;
    /// <summary>
    /// 自分視点で他の人の名前や役職名を表示等する時に
    /// </summary>
    public void OnHandleName() { }
    /// <summary>
    /// 自分が全員の役職を見られる場合に実行するやつ
    /// </summary>
    public void OnHandleDeadPlayer() { }
    /// <summary>
    /// 全員視点で見える名前を処理するやつ
    /// </summary>
    public void OnHandleAllPlayer() { }
}