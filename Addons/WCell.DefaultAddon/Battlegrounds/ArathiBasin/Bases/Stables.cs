using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Addons.Default.Battlegrounds.ArathiBasin
{
    class Stables : ArathiBase
    {
        public Stables(ArathiBasin instance)
            : base(instance, null)
        {
        }

        public override string BaseName
        {
            get { return "Stables"; }
        }
    }
}