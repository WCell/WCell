using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.Addons.Default.Battlegrounds.WarsongGulch
{
    /// <summary>
    /// Handles the FlagDrop type (WarsongFlag, SilverwingFlag in WSG)
    /// </summary>
    public class WarsongFlagsHandler : AuraEffectHandler
    {
        private WarsongGulch Instance;

        /// <summary>
        /// Ensure that the constraints are correct for the Flag aura to be applied
        /// </summary>
        protected override void CheckInitialize(ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
        {
			if (!(target is Character))
			{
				failReason = SpellFailedReason.BadTargets;
			}
			else
			{
				Instance = target.Region as WarsongGulch;
				if (Instance == null)
				{
					failReason = SpellFailedReason.IncorrectArea;
				}
			}
        }

        /// <summary>
        /// Override to trigger the flag drop and recently dropped spells.
        /// </summary>
        /// <param name="cancelled"></param>
        protected override void Remove(bool cancelled)
        {
			if (Instance.IsDisposed)
			{
				// already disposed -> Don't do anything anymore
				return;
			}

        	if (Instance.Silverwing.IsFlagCap || Instance.WarsongClan.IsFlagCap)
			{
				// Aura is removed due to Flag capture - Don't drop a new flag
                return;
            }

            var unit = m_aura.Auras.Owner;
            var chr = (Character)unit;
        	var team = chr.Battlegrounds.Team;

			if (team != null)
			{
				var faction = Instance.GetFaction(team.Side).Opponent;
				faction.DropFlag();
			}

        }
    }
}