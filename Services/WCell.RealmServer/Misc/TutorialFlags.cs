using System;

namespace WCell.RealmServer.Misc
{
    public class TutorialFlags
    {
        /// <summary>
        /// Shares with the Character's record
        /// </summary>
        private byte[] m_flagData;

        internal TutorialFlags(byte[] flagData)
        {
            if (flagData.Length != 32)
            {
                throw new ArgumentOutOfRangeException("flagData", "byte array must be 32 bytes");
            }

            m_flagData = flagData;
        }

        internal byte[] FlagData
        {
            get
            {
                return m_flagData;
            }
        }

        public void SetFlag(uint flagIndex)
        {
            m_flagData[flagIndex / 8] |= (byte)(1 << ((int)flagIndex % 8));
        }

        public void ClearFlags()
        {
            for (int i = 0; i < 32; i++)
            {
                m_flagData[i] = 0xFF;
            }
        }

        public void ResetFlags()
        {
            for (int i = 0; i < 32; i++)
            {
                m_flagData[i] = 0;
            }
        }
    }
}