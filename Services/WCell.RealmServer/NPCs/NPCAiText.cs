using System;
using System.Linq;
using WCell.Constants;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs
{
    /// <summary>
    /// Texts yelled by mobs
    /// </summary>
    [DataHolder]
    public  class NPCAiText : IDataHolder
    {
        #region Fields
        public int Id;
        public int Sound;
        public int Type;
        public int Language;
        public int Emote;
        /// <summary>
        /// Mob's ID or string like "Common Kobold Text"
        /// </summary>
        public string Comment;
        /// <summary>
        /// Texts on all languages : 0 - eng, ... , 7 - rus, requied for .Localize()
        /// </summary>
        [Persistent((int)ClientLocale.End)]
        public string[] Texts = new string[(int) ClientLocale.End];
        #endregion;


        public string Text
        {
            get
            {
                return Texts[(int) RealmServerConfiguration.DefaultLocale];
            }
            set
            {
                Texts[(int) RealmServerConfiguration.DefaultLocale] = value;
            }
        }
        
        public int GetMobId()
        {
            int mobId;
            return Int32.TryParse(Comment, out mobId) ? mobId : 0;
        }

        /// <summary>
        /// Is called to initialize the object; usually after a set of other operations have been performed or if
        /// the right time has come and other required steps have been performed.
        /// </summary>
        public void FinalizeDataHolder()
        {
            Texts = Texts.Select(x => x ?? "").ToArray();
            NPCAiTextMgr.Entries.Add(Id, this);
        }
    }
}