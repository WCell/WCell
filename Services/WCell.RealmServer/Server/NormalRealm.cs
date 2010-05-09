using WCell.Constants;
using WCell.Constants.Login;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Server
{
    [ServerType(RealmServerType.Normal)]
    public class NormalRules : IServerRules
    {
        public RealmServerType ServerType
        {
            get { return RealmServerType.Normal; }
        }

        public bool CanCreateCharacter(IRealmClient client, RaceId chrRace, ClassId chrClass, out LoginErrorCode errorCode)
        {
            errorCode = LoginErrorCode.RESPONSE_SUCCESS;

            // this is where we'd allow scripts to do custom rules
            return true;
        }

        public bool CanAttackUnit(Unit attacker, Unit defender)
        {
            return true;
        }
    }
}
