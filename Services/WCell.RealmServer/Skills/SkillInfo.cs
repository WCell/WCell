using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Skills
{
	/// <summary>
	/// 
	/// </summary>
	public class SkillInfo
	{
		public SkillLine Skill;
		public uint Value;
		public uint MaxValue;

		public override string ToString()
		{
			return string.Format("{0} - {1} / {2}", Skill, Value, MaxValue);
		}

		public override bool Equals(object obj)
		{
			return obj is SkillInfo && ((SkillInfo)obj).Skill.Id == Skill.Id;
		}

		public override int GetHashCode()
		{
			return (int)Skill.Id;
		}
	}
}
