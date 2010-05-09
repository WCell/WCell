using System.Linq;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Server
{
    [ServerType(RealmServerType.PVP)]
    public class PvPRules : IServerRules
    {
        public RealmServerType ServerType
        {
            get { return RealmServerType.PVP; }
        }

        public bool CanCreateCharacter(IRealmClient client, RaceId chrRace, ClassId chrClass, out LoginErrorCode errorCode)
        {
			// Rule does not exist anymore:
			//errorCode = LoginErrorCode.CHAR_CREATE_PVP_TEAMS_VIOLATION;

			//if (client.Account.Characters.Count == 0)
			//{
			//    return true;
			//}

			//var firstChar = client.Account.Characters.First();
			//var clientFaction = FactionMgr.GetFactionGroup(firstChar.Race);

			//if (clientFaction == FactionMgr.GetFactionGroup(chrRace))
			//{
			//    return true;
			//}

			//// this is where we'd allow scripts to do custom rules
			//return false;
        	errorCode = LoginErrorCode.ACCOUNT_CREATE_FAILED;
			return true;
        }

        public bool CanAttackUnit(Unit attacker, Unit defender)
        {
            return true;
        }
    }
}
