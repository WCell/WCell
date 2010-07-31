using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Addons.Default.Battlegrounds.ArathiBasin.Bases
{
    class GoldMine : ArathiBase
    {
        public GoldMine(ArathiBasin instance)
            : base(instance, null)
        {
        }

        public override string BaseName
        {
            get { return "Gold Mine"; }
        }
    }
}