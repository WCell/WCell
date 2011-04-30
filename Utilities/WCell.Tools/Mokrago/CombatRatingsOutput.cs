using System;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.RealmServer.Entities;
using System.IO;
using WCell.Util.Toolshed;

namespace WCell.Tools.Mokrago
{
    public static class CombatRatingsOutput
    {
        public static readonly string DumpFile = ToolConfig.OutputDir + "CombatRatings.txt";

        private delegate float ReturnTableValue(int level, ClassId id);

        [Tool]
        public static void WriteCombatRatingTables()
        {
            Tools.StartRealm();
            GameTables.LoadGtDBCs();

            if (!GameTables.Loaded)
            {
                Console.WriteLine("GameTables are not loaded, aborting....");
                return;
            }

            Console.Write("Writing combat rating tables to " + DumpFile);

            var names = Enum.GetNames(typeof(CombatRating));
            using (var writer = new StreamWriter(DumpFile))
            {
                for (int i = 0; i < names.Length; i++ )
                {
                    writer.WriteLine(names[i]);
                    WriteTable((CombatRating)i + 1, writer);
                }

                WriteTableByClass(GameTables.BaseMeleeCritChance, writer, "BaseMeleeCritChance");
                WriteTableByClass(GameTables.BaseSpellCritChance, writer, "BaseSpellCritChance");
                WriteTableByClassLevel(GameTables.GetUnModifiedClassMeleeCritChanceValue, writer, "ClassMeleeCritChance");
                WriteTableByClassLevel(GameTables.GetUnmodifiedClassSpellCritChanceValue, writer, "ClassSpellCritChance");
                //WriteTableByClassLevel(GameTables.OCTRegenHP, writer, "OCTRegenHealth");
                WriteTableByClassLevel(GameTables.OCTRegenMP, writer, "OCTRegenMana");
                WriteTableByClassLevel(GameTables.OCTHpPerStamina, writer, "OCTRegenHealthPerStamina");
                WriteTableByClassLevel(GameTables.RegenMPPerSpirit, writer, "RegenManaPerSpirit");
            }

            Console.Write("\nSuccess!");
        }

        private static void WriteTable(CombatRating rating, StreamWriter writer)
        {
            var table = GameTables.GetCRTable(rating);
            WriteTable(table, writer);
        }

        private static void WriteTable(float[] table, StreamWriter writer)
        {
            writer.WriteLine(" ################################### ");
            writer.WriteLine();

            for (int i = 0; i < table.Length; ++i)
            {
                writer.WriteLine("Level {0}: {1}", (i + 1), table[i]);
            }
            writer.WriteLine();
        }

        private static void WriteTableByClass(float[] table, StreamWriter writer, string tableName)
        {
            writer.WriteLine(tableName);
            writer.WriteLine(" ################################### ");
            writer.WriteLine();

            for (int i = 0; i < table.Length; i++)
            {
                writer.WriteLine("{0}: {1}", Enum.GetName(typeof(ClassId), i + 1), table[i]);
            }
            writer.WriteLine();
        }

        private static void WriteTableByClassLevel(float[] table, StreamWriter writer, string tableName)
        {
            writer.WriteLine(tableName);
            writer.WriteLine(" ################################### ");
            writer.WriteLine();

            for (int i = 1; i < 12; i++)
            {
                writer.WriteLine("{0} - {1}:", tableName, Enum.GetName(typeof(ClassId), i));

                for (int k = 1; k < 101; k++)
                {
                    writer.WriteLine("Level {0}: {1}", (k), table[100*i+k-101]);
                }
                writer.WriteLine();
            }
        }

        private static void WriteTableByClassLevel(ReturnTableValue funct, StreamWriter writer, string tableName)
        {
            writer.WriteLine(tableName);
            writer.WriteLine(" ################################### ");
            writer.WriteLine();

            for (int i = 1; i < 12; i++)
            {
                writer.WriteLine("{0} - {1}:", tableName, Enum.GetName(typeof(ClassId), i));

                for (int k = 1; k < 101; k++)
                {
                    writer.WriteLine("Level {0}: {1}", (k), funct(k, (ClassId)i));
                }
                writer.WriteLine();
            }
        }
    }
}