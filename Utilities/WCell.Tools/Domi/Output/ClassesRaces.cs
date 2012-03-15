//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;
//using System.Xml;
//using WCell.RealmServer;
//using WCell.RealmServer.Classes;
//using WCell.RealmServer.Races;
//using WCell.RealmServer.World;
//using WCell.Core;

//using System.Collections;

//namespace WCell.Tools.Domi.Output
//{
//    public static class ClassesRaces
//    {
//        public class ClassComparer : IEqualityComparer<BaseClass>
//        {
//            #region IEqualityComparer<BaseClass> Members

//            public bool Equals(BaseClass x, BaseClass y)
//            {
//                return x.ClassID == y.ClassID;
//            }

//            public int GetHashCode(BaseClass obj)
//            {
//                return (int)obj.ClassID;
//            }

//            #endregion
//        }

//        public static void WriteClassesRaces(string dir, string raceFile, string classFile, string comboFile)
//        {
//            XmlDocument raceDoc = new XmlDocument();
//            var raceNode = raceDoc.Add("Races");

//            XmlDocument clssDoc = new XmlDocument();
//            var clssNode = clssDoc.Add("Classes");

//            XmlDocument comboDoc = new XmlDocument();
//            var comboNode = comboDoc.Add("Archetypes");

//            HashSet<BaseClass> classes = new HashSet<BaseClass>(new ClassComparer());

//            foreach (var race in WorldMgr.Races)
//            {
//                if (race != null)
//                {
//                    raceNode.AppendRace(race);
//                    foreach (var clss in race.m_archetypes)
//                    {
//                        if (clss != null)
//                        {
//                            classes.Add(clss);
//                        }
//                    }
//                    comboNode.AppendArchetypes(race);
//                }
//            }

//            foreach (var clss in classes)
//            {
//                clssNode.AppendClass(clss);
//            }

//            using (var stream = new FileStream(Path.Combine(dir, classFile), FileMode.Create, FileAccess.WriteIndent, FileShare.None))
//            {
//                clssDoc.Save(stream);
//            }

//            using (var stream = new FileStream(Path.Combine(dir, raceFile), FileMode.Create, FileAccess.WriteIndent, FileShare.None))
//            {
//                raceDoc.Save(stream);
//            }

//            using (var stream = new FileStream(Path.Combine(dir, comboFile), FileMode.Create, FileAccess.WriteIndent, FileShare.None))
//            {
//                comboDoc.Save(stream);
//            }
//        }

//        public static void AppendArchetypes(this XmlNode root, BaseRace race)
//        {
//            var node = root.Add(race.Type.ToString());

//            foreach (var clss in race.m_archetypes)
//            {
//                if (clss != null)
//                {
//                    node.AppendArchetype(clss);
//                }
//            }
//        }

//        public static void AppendArchetype(this XmlNode root, BaseClass archeType)
//        {
//            var node = root.Add(archeType.ClassID.ToString());

//            node.Add("BaseHealth", archeType.BaseHealth);
//            node.Add("BasePower", archeType.BasePower);
//        }

//        public static void AppendClass(this XmlNode root, BaseClass clss)
//        {
//            var node = root.Add(clss.ClassID.ToString());

//            var stats = node.Add("StatBoni");
//            stats.Add("Agility", clss.AgilityBonus);
//            stats.Add("Intellect", clss.IntellectBonus);
//            stats.Add("Spirit", clss.SpiritBonus);
//            stats.Add("Stamina", clss.StaminaBonus);
//            stats.Add("Strength", clss.StrengthBonus);

//            node.Add("Languages", ChatLanguage.Common);

//            node.Add("AttackTime", clss.AttackTime);
//            node.Add("MaxDamage", clss.MaxDamage);
//            node.Add("MinDamage", clss.MinDamage);
//            node.Add("PowerType", clss.PowerType);

//            //node.Add("Spells", "1100");

//            //var skills = node.Add("Skills");
//            //skills.Add("Alchemy", 100);

//            //var items = node.Add("Items");
//            //var item = items.Add("Item", 25);
//            //item.AddAttr("Amount", 1);
//        }

//        public static void AppendRace(this XmlNode root, BaseRace race)
//        {
//            var node = root.Add(race.Type.ToString());

//            var stats = node.Add("Stats");
//            stats.Add("Agility", race.BaseAgility);
//            stats.Add("Intellect", race.BaseIntellect);
//            stats.Add("Spirit", race.BaseSpirit);
//            stats.Add("Stamina", race.BaseStamina);
//            stats.Add("Strength", race.BaseStrength);

//            node.Add("StartMap", race.StartMap);
//            node.Add("StartZone", race.StartZone);
//            node.Add("StartPos", race.StartPosition);
//            node.Add("Scale", race.Scale);
//            node.Add("Intro", race.IntroductionMovie);
//            node.Add("ModelOffset", race.ModelOffset);
//        }
//    }
//}