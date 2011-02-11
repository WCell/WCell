using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
    public class Inebriate : SpellEffectHandler
    {
        public Inebriate(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override void Apply()
        {
            var target = Cast.CasterObject as Character;

            if (target != null)
            {
                target.DrunkState += (byte)Effect.BasePoints;
            }
        }
    }
}