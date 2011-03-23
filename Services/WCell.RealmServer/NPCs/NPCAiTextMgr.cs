using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WCell.Util.Logging;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.Util.Variables;

namespace WCell.RealmServer.NPCs
{
    public static class NPCAiTextMgr
    {
        #region Global Containers & Get Methods
        [NotVariable]
        /// <summary>
        /// All NPCEntries by their Entry-Id
        /// </summary>
        internal static readonly Dictionary<int, NPCAiText> Entries = new Dictionary<int, NPCAiText>();

        public static IEnumerable<NPCAiText> GetAllEntries()
        {
            return new EntryIterator();
        }
        #endregion

        #region Select
        /// <summary>
        /// Select entries by mob's ID
        /// </summary>
        /// <param name="id">Mob's ID</param>
        public static NPCAiText[] GetEntry(uint id)
        {
            return Entries.Where(entry => entry.Value.GetMobId() == id).Select(entry => entry.Value).ToArray();
        }
        /// <summary>
        /// Select entries by preposition of yelled text (on any localization)
        /// </summary>
        /// <param name="str">String preposition</param>
        public static NPCAiText[] GetEntry(string str)
        {
            return
                Entries.Where(entry => entry.Value.Texts.Any(text => text.StartsWith(str))).Select(entry => entry.Value)
                    .ToArray();
        }
        #endregion

        #region Fixing
        /// <summary>
        /// It may be useful... sometime
        /// </summary>
        /// <param name="cb">Action</param>
        /// <param name="texts">Texts</param>
        public static void Apply(this Action<NPCAiText> cb, params NPCAiText[] texts)
        {
            foreach (var text in texts.Where(text => text != null))
            {
                cb(text);
            }
        }

        #endregion

        #region Loading 
        private static bool _entriesLoaded;

        public static bool Loaded
        {
            get { return _entriesLoaded; }
            private set
            {
                _entriesLoaded = value;
                CheckLoaded();
            }
        }

        private static void CheckLoaded()
        {
            if (_entriesLoaded)
            {
                //RealmServer.InitMgr.SignalGlobalMgrReady(typeof(NPCAiTextMgr));
            }
        }

        public static void LoadEntries()
        {
            if (!Loaded)
                ContentHandler.Load<NPCAiText>();
            Loaded = true;
        }
        #endregion

        #region Iterator
        class EntryIterator : IEnumerable<NPCAiText>
        {
            public IEnumerator<NPCAiText> GetEnumerator()
            {
                return new EntryEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        class EntryEnumerator : IEnumerator<NPCAiText>
        {
            private IEnumerator currentIterator;

            public EntryEnumerator()
            {
                Reset();
            }

            public bool MoveNext()
            {
                return currentIterator.MoveNext();
            }

            public void Reset()
            {
                currentIterator = Entries.Values.GetEnumerator();
            }

            public void Dispose()
            {
                if (currentIterator is IEnumerator<NPCAiText>)
                {
                    ((IEnumerator<NPCAiText>)currentIterator).Dispose();
                }
            }

            public NPCAiText Current
            {
                get { return (NPCAiText)currentIterator.Current; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
        #endregion
    }
}