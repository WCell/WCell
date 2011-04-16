using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.GameObjects;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.Constants.Updates;
using WCell.Util.Variables;
using WCell.Util.Data;

namespace WCell.RealmServer.GameObjects.GOEntries
{
	public class GOCustomEntry : GOEntry
	{
		private GOUseHandler m_UseHandler;

		[NotPersistent]
		public GOUseHandler UseHandler
		{
			get { return m_UseHandler; }
			set
			{
				m_UseHandler = value;
				HandlerCreator = () => new CustomUseHandler(value);
			}
		}

		public GOCustomEntry()
		{
		}

		protected internal override void InitEntry()
		{
			if (Fields == null)
			{
				Fields = new int[GOConstants.EntryFieldCount];
			}
		}
	}

	internal class CustomUseHandler : GameObjectHandler
	{
		private readonly GOEntry.GOUseHandler Handler;

		public CustomUseHandler(GOEntry.GOUseHandler handler)
		{
			Handler = handler;
		}

		public override bool Use(Character user)
		{
			return Handler(m_go, user);
		}
	}

	/// <summary>
	/// Can be used to create custom GameObjects that will apply the given
	/// Spell to everyone in Radius.
	/// </summary>
	public class GOCustomAuraEntry : GOCustomEntry, ISpellParameters
	{
		[NotPersistent]
		public Spell Spell
		{
			get;
			set;
		}

		[NotPersistent]
		public int MaxCharges
		{
			get;
			set;
		}

		[NotPersistent]
		public int Amplitude
		{
			get;
			set;
		}

		[NotPersistent]
		public int StartDelay
		{
			get;
			set;
		}

		[NotPersistent]
		public int Radius
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Can be used to create custom GameObjects that will apply the given
	/// Spell to everyone in Radius.
	/// </summary>
	public class GOCustomAreaEffectEntry : GOCustomEntry
	{
		public delegate void GOInteractionHandler(GameObject go, Character chr);

		private GOInteractionHandler m_AreaEffectHandler;
		protected float m_Radius;
		protected float m_RadiusSq;

		public GOCustomAreaEffectEntry()
		{
			Radius = 5;
			UpdateDelayMillis = 500;
		}

		public float Radius
		{
			get { return m_Radius; }
			set
			{
				m_Radius = value;
				m_RadiusSq = m_Radius * m_Radius;
			}
		}

		public int UpdateDelayMillis
		{
			get;
			set;
		}

		/// <summary>
		/// The EffectHandler that will be applied to every Unit that comes into the Radius.
		/// When moving, removing or adding anything in this Method, enqueue a Message!
		/// </summary>
		public GOInteractionHandler AreaEffectHandler
		{
			get { return m_AreaEffectHandler; }
			set { m_AreaEffectHandler = value; }
		}

		protected internal override void InitGO(GameObject go)
		{
			go.SetUpdatePriority(UpdatePriority.VeryLowPriority);
			if (m_AreaEffectHandler != null)
			{
				go.CallPeriodically(UpdateDelayMillis, ApplyEffectsToArea);
			}
		}

		protected void ApplyEffectsToArea(WorldObject goObj)
		{
			var go = (GameObject)goObj;
			goObj.IterateEnvironment(Radius, obj => {
				if (obj is Character)
				{
					AreaEffectHandler(go, (Character)obj);
				}
				return true;
			});
		}
	}
}