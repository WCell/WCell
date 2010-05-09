using WCell.Constants;
using WCell.Constants.Login;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Server
{
    public interface IServerRules
    {
        RealmServerType ServerType { get; }

        bool CanCreateCharacter(IRealmClient client, RaceId chrRace, ClassId chrClass, out LoginErrorCode errorCode);
        bool CanAttackUnit(Unit attacker, Unit defender);
    }
}
