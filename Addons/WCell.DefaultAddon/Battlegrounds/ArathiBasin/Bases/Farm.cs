using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Addons.Default.Battlegrounds.ArathiBasin.Bases
{
    class Farm : ArathiBase
    {
        public Farm(ArathiBasin instance)
            : base(instance, null)
        {
        }

        public override string BaseName
        {
            get { return "Farm"; }
        }
    }
}
