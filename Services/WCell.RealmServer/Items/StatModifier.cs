using WCell.Constants.Items;

namespace WCell.RealmServer.Items
{
    /// <summary>
    /// A modifier to an item's owner's stats and/or CombatRatings
    /// </summary>
    public struct StatModifier
    {
        public static readonly StatModifier[] EmptyArray = new StatModifier[0];

        public ItemModType Type;
        public int Value;

        public override string ToString()
        {
            return Value != 0 ? ("StatMod - " + Type + ": " + Value) : ("Empty StatMod");
        }
    }
}