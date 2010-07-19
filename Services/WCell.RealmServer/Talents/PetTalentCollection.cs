//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using WCell.Constants.Pets;
//using WCell.Constants.Talents;
//using WCell.RealmServer.Entities;

//namespace WCell.RealmServer.Talents
//{
//    public class PetTalentCollection : TalentCollection
//    {
//        public PetTalentCollection(NPC owner) : base(owner)
//        {
//        }

//        public override bool CanLearn(TalentEntry entry, int rank)
//        {
//            var tree = entry.Tree;
			
//            return base.CanLearn(entry, rank);
//        }
//    }
//}