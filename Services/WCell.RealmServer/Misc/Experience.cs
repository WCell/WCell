using WCell.RealmServer.Formulas;

namespace WCell.RealmServer.Misc
{
	/// <summary>
	/// Helper struct for experience-calculation
	/// </summary>
	public struct Experience
	{
		public int Xp;
		public int Level;

		public Experience(int totalXp) : this()
		{
			TotalXp = totalXp;
		}

		public Experience(int level, int xp)
		{
			Xp = xp;
			Level = level;
		}

		public int TotalXp
		{
			get
			{
				var xp = Xp;
				for (var i = 2; i <= Level; i++)
				{
					xp += XpGenerator.GetXpForlevel(i);
				}
				return xp;
			}
			set
			{
				int levelXp;
				var level = 1;
				while (value >= (levelXp = XpGenerator.GetXpForlevel(level+1)))
				{
					level++;
					value -= levelXp;
				}

				Xp = value;
				Level = level;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is Experience)
			{
				var xp2 = (Experience) obj;
				return Level == xp2.Level && Xp == xp2.Xp;
			}

			if (obj is uint)
			{
				return TotalXp == (uint)obj;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return TotalXp;
		}

		public static Experience operator +(Experience exp, Experience exp2)
		{
			return new Experience(exp.TotalXp + exp2.TotalXp);
		}

		public static Experience operator -(Experience exp, Experience exp2)
		{
			return new Experience(exp.TotalXp - exp2.TotalXp);
		}

		public static Experience operator +(Experience exp, int exp2)
		{
			return new Experience(exp.TotalXp + exp2);
		}

		public static Experience operator -(Experience exp, int exp2)
		{
			return new Experience(exp.TotalXp - exp2);
		}

		public override string ToString()
		{
			return "Level: " + Level + ", Xp: " + Xp;
		}
	}
}
