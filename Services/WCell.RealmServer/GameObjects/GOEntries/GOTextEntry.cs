using NLog;
using WCell.Constants.Misc;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOTextEntry : GOEntry
    {
		private static readonly Logger sLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The Id of a PageText object that is associated with this object.
        /// </summary>
        public uint PageId
        {
            get { return Fields[0]; }
        }

        /// <summary>
        /// The LanguageId from Languages.dbc
        /// </summary>
        public ChatLanguage Language
        {
            get { return (ChatLanguage)Fields[1]; }
        }

		/// <summary>
		/// The PageTextMaterialId from PageTextMaterial.dbc
		/// </summary>
		public uint PageTextMaterialId;

		protected internal override void InitEntry()
		{
			AllowMounted = Fields[3] > 0;
		}
	}
}
