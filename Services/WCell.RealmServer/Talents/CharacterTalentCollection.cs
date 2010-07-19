//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using WCell.RealmServer.Entities;

//namespace WCell.RealmServer.Talents
//{
//    public class CharacterTalentCollection : TalentCollection
//    {
//        public CharacterTalentCollection(Unit owner) : base(owner)
//        {

//        }

//        public override bool CanLearn(TalentEntry entry, int rank)
//        {
//            return entry.Tree.Class == Owner.Class && 
//                base.CanLearn(entry, rank);
//        } 
//    }
//}