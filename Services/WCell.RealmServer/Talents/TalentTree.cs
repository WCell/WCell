/*************************************************************************
 *
 *   file		: TalentTree.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 07:58:12 +0100 (lø, 07 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Talents;

namespace WCell.RealmServer.Talents
{
    public class TalentTree
    {
        public TalentTreeId Id;
        public string Name;

        // unnecessary
        //public uint Icon;						// 18

        public ClassId Class;					// 20

        public uint TabIndex;					// 22

        /// <summary>
        /// For Pet Talents
        /// </summary>
        public uint PetTabIndex;					// 21

        // unnecessary
        //public uint Icon2;					// 22

        /// <summary>
        /// All talents of this tree
        /// </summary>
        public readonly List<TalentEntry> Talents = new List<TalentEntry>(30);

        public TalentEntry[][] TalentTable = new TalentEntry[TalentMgr.MaxTalentRowCount][];

        /// <summary>
        /// Total amount of Talent ranks in this Tree
        /// </summary>
        public int TotalRankCount;

        /// <summary>
        /// Full name of this tree (includes the name of the Class this Tree belongs to)
        /// </summary>
        public string FullName
        {
            get
            {
                return Class + " " + Name;
            }
        }

        public IEnumerator<TalentEntry> GetEnumerator()
        {
            return Talents.GetEnumerator();
        }

        public override string ToString()
        {
            return FullName + " (Id: " + (int)Id + ")";
        }
    }
}
