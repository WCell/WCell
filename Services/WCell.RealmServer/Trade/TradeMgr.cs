using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Trade
{
    public class TradeMgr
    {
        public const int MaxSlotCount = 7;
        public const int NontradeSlot = 6;
        public const int TradeSlotCount = 6;

        public static float MaxTradeRadius = 10;

        /// <summary>
        /// Makes initChr propose trading to targetChr
        /// Call CheckRequirements first
        /// </summary>
        /// <param name="initChr">initiator of trading</param>
        /// <param name="targetChr">traget of trading</param>
        public static void Propose(Character initChr, Character targetChr)
        {
            initChr.TradeWindow = new TradeWindow(initChr);
            targetChr.TradeWindow = new TradeWindow(targetChr);

            initChr.TradeWindow.m_otherWindow = targetChr.TradeWindow;
            targetChr.TradeWindow.m_otherWindow = initChr.TradeWindow;

            TradeHandler.SendTradeProposal(targetChr.Client, initChr);
        }

        /// <summary>
        /// Checks requirements for trading between two characters
        /// </summary>
        /// <param name="initChr">possible initiator of trading</param>
        /// <param name="targetChr">possible target of trading</param>
        public static bool MayProposeTrade(Character initChr, Character targetChr)
        {
            TradeStatus tradeStatus;

            if (targetChr == null || !targetChr.IsInContext)
            {
                tradeStatus = TradeStatus.PlayerNotFound;
                return false;
            }
            else if (initChr.IsLoggingOut || targetChr.IsLoggingOut)
            {
                tradeStatus = TradeStatus.LoggingOut;
            }
            else if (!initChr.IsAlive)
            {
                tradeStatus = TradeStatus.PlayerDead;
            }
            else if (!targetChr.IsInRadius(initChr, MaxTradeRadius))
            {
                tradeStatus = TradeStatus.TooFarAway;
            }
            else if (!targetChr.IsAlive)
            {
                tradeStatus = TradeStatus.TargetDead;
            }
            else if (targetChr.IsStunned)
            {
                tradeStatus = TradeStatus.TargetStunned;
            }
            else if (targetChr.IsIgnoring(initChr))
            {
                tradeStatus = TradeStatus.PlayerIgnored;
            }
            else if (targetChr.TradeWindow != null)
            {
                tradeStatus = TradeStatus.AlreadyTrading;
            }
            else if (targetChr.Faction.Group != initChr.Faction.Group && !initChr.Role.IsStaff)
            {
                tradeStatus = TradeStatus.WrongFaction;
            }
            else if (targetChr.IsLoggingOut)
            {
                tradeStatus = TradeStatus.TargetLoggingOut;
            }
            else
            {
                tradeStatus = TradeStatus.Proposed;
                return true;
            }

            TradeHandler.SendTradeStatus(initChr, tradeStatus);
            return false;
        }
    }
}
