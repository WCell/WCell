using WCell.Constants.Misc;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOTextEntry : GOEntry
    {
        /// <summary>
        /// The Id of a PageText object that is associated with this object.
        /// </summary>
        public override uint PageId
        {
            get { return (uint)Fields[0]; }
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
        public int PageTextMaterialId;

        protected internal override void InitEntry()
        {
            AllowMounted = Fields[3] > 0;
        }
    }
}