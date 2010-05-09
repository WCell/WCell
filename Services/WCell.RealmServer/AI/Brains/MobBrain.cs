using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.AI.Actions;
using System.Collections.Generic;
using WCell.Util;

namespace WCell.RealmServer.AI.Brains
{

    #region Constants
    public enum NPCEmoteType
    {
        /// <summary>
        /// Npc yells( Red text, big area )
        /// </summary>
        NPC_Yell = 1,
        /// <summary>
        /// Npc says( Orange text, small area )
        /// </summary>
        NPC_Say = 2,
        /// <summary>
        /// Does creature emote.
        /// </summary>
        NPC_Emote = 3,
        /// <summary>
        /// Wispers in-range players.
        /// </summary>
        NPC_Whisper = 4
    }
    public enum NPCBrainEvents
    {
        /// <summary>
        /// Called when AI holder enters combat.
        /// </summary>
        OnEnterCombat = 1,
        /// <summary>
        /// Called when AI holder leaves combat.
        /// </summary>
        OnLeaveCombat = 2,
        /// <summary>
        /// Called on AI holders death.
        /// </summary>
        OnDeath = 3,
        /// <summary>
        /// Called on AI holders main target death.
        /// </summary>
        OnTargetDied = 4,
        /// <summary>
        /// Called on Update Event.
        /// </summary>
        OnUpdateEvent = 5,

        End = 6
    }

    public struct EmoteData
    {
        public EmoteData(string pText, NPCBrainEvents pEvent, NPCEmoteType pType, uint pSoundId)
        {
            mText = pText;
            mEvent = pEvent;
            mType = pType;
            mSoundId = pSoundId;
        }

        public string mText;
        public NPCBrainEvents mEvent;
        public NPCEmoteType mType;
        public uint mSoundId;
    }

    #endregion

	/// <summary>
	/// TODO: Consider visibility of targets - Don't pursue target nor remove it from Threat list if not visible
	/// </summary>
    /// 
	public class MobBrain : BaseBrain
    {
        #region Private Members
        private List<EmoteData>[] m_emoteData;
        #endregion
        #region Constructors

        public MobBrain(NPC owner)
			: base(owner)
		{
            
		}

		public MobBrain(NPC owner, BrainState defaultState)
			: base(owner, defaultState)
		{

		}

		public MobBrain(NPC owner, IAIActionCollection actions) : this(owner, actions, BrainState.Idle)
		{
		}

		public MobBrain(NPC owner, IAIActionCollection actions, BrainState defaultState) :
			base(owner, actions, defaultState)
		{

		}

		#endregion

        #region Creature Functions

        #region Emote Functions
        

        /// <summary>
        /// Makes npc Yell text.
        /// </summary>
        /// <param name="pText">Wanted text.</param>
        public void DoEmote(string pText)
        {
            NPC.Yell(pText);
        }

        /// <summary>
        /// Makes npc emote text.
        /// </summary>
        /// <param name="pText">Wanted Text.</param>
        /// <param name="pType">Type of emote, Yell, Say, etc.</param>
        public void DoEmote(string pText, NPCEmoteType pType)
        {
            switch (pType)
            {
                case NPCEmoteType.NPC_Say:
                    NPC.Say(pText);
                    break;
                case NPCEmoteType.NPC_Yell:
                    NPC.Yell(pText);
                    break;
                case NPCEmoteType.NPC_Emote:
                    NPC.Emote(pText);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Makes npc emote text. And play wanted sound.
        /// </summary>
        /// <param name="pText">Wanted Text,</param>
        /// <param name="pType">Type of emote, Yell, Say, etc.</param>
        /// <param name="pSoundId">Id of sound(Found in DBC).</param>
        public void DoEmote(string pText, NPCEmoteType pType, uint pSoundId)
        {
            if( pSoundId != 0 )
                NPC.PlaySound(pSoundId);

            DoEmote(pText, pType);
        }

        private void DoEmote(EmoteData emoteData)
        {
            DoEmote(emoteData.mText, emoteData.mType, emoteData.mSoundId);
        }

        private void DoEmoteForEvent(NPCBrainEvents pNPCBrainEvents)
        {
            var evtId = (uint)pNPCBrainEvents;
            if (m_emoteData != null)
            {
                // Select random
            	var evt = m_emoteData[evtId];
                var count = evt.Count;
                if (count < 1)
                    return;

                DoEmote(m_emoteData[evtId][count == 1 ? 0 : Utility.Random(0, count - 1)]);
            }
        }

        public void AddEmote(string pText, NPCBrainEvents pEvent, NPCEmoteType pType, uint pSoundId)
        {
            var newEmoteData = new EmoteData(pText, pEvent, pType, pSoundId);
            AddEmote(newEmoteData);
        }

        public void AddEmote(string pText, NPCBrainEvents pEvent, NPCEmoteType pType)
        {
            var newEmoteData = new EmoteData(pText, pEvent, pType, 0);
            AddEmote(newEmoteData);
        }

        public void AddEmote(EmoteData pEmoteData)
        {
            if (m_emoteData == null)
                m_emoteData = new List<EmoteData>[(int)NPCBrainEvents.End];

            uint Event = (uint)pEmoteData.mEvent;
            if( m_emoteData[Event] == null ) 
                m_emoteData[Event] = new List<EmoteData>();

            m_emoteData[Event].Add(pEmoteData);
        }

        #endregion

        #region Spell Functions
        // todo!!!
        #endregion 

        #endregion

        public override void OnHeal(Unit healer, Unit healed, int amtHealed)
		{
			if (m_owner is NPC && m_owner.IsInCombat && m_owner.CanBeAggroedBy(healer))
			{
				((NPC) m_owner).ThreatCollection[healer] += amtHealed/2;
			}
		}

		#region Event Handlers

        public override void OnEnterCombat()
        {
            // Do assigned emotes
            DoEmoteForEvent(NPCBrainEvents.OnEnterCombat);
        }

		public override void OnLeaveCombat()
		{
            // Do assigned emotes
            DoEmoteForEvent(NPCBrainEvents.OnLeaveCombat);

			if (m_owner is NPC)
			{
				((NPC)m_owner).ThreatCollection.Clear();
			}
		}

        public override void OnKilled(Unit killerUnit, Unit victimUnit)
        {
            if (victimUnit != Owner && killerUnit == Owner)
            {
                // Do assigned emotes
                DoEmoteForEvent(NPCBrainEvents.OnTargetDied);
            }
        }

        public override void OnDeath()
        {
            // Do assigned emotes
            DoEmoteForEvent(NPCBrainEvents.OnDeath);

            base.OnDeath();
        }

		/// <summary>
		/// Called when owner received a debuff by the given caster
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="cast"></param>
		/// <param name="debuff"></param>
		public override void OnDebuff(Unit caster, SpellCast cast, Aura debuff)
		{
			if (!m_IsRunning || caster == null)
				return;

			// TODO: How much threat do debuffs cause?
			if (m_owner is NPC && m_owner.CanBeAggroedBy(caster))
			{
				((NPC)m_owner).ThreatCollection[caster] += 1;
			}
		}

		/// <summary>
		/// Called whenever someone performs a harmful action on this Mob.
		/// </summary>
		/// <param name="action"></param>
		public override void OnDamageReceived(IDamageAction action)
		{
			if (!m_IsRunning)
				return;

			if (action.Attacker == null)
			{
				return;
			}

			if (m_owner is NPC && m_owner.CanBeAggroedBy(action.Attacker))
			{
				((NPC)m_owner).ThreatCollection[action.Attacker] += action.Attacker.GetGeneratedThreat(action);
			}
		}

		public override void OnCombatTargetOutOfRange()
		{

		}

		#endregion
	}
}