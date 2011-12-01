using WCell.Constants;

namespace WCell.RealmServer.Lang
{
	public class TranslatableItem : Util.Lang.TranslatableItem<RealmLangKey>
	{
		public TranslatableItem(RealmLangKey key, params object[] args)
			: base(key, args)
		{
		}

		public /*virtual*/ string Translate(ClientLocale locale)
		{
			return RealmLocalizer.Instance.Translate(locale, this);
		}

		public string TranslateDefault()
		{
			return Translate(RealmServerConfiguration.DefaultLocale);
		}
	}

	//public class DefaultTranslatableItem : TranslatableItem
	//{
	//    private readonly string Text;

	//    public DefaultTranslatableItem(string defaultText, params object[] args)
	//        : base(RealmLangKey.None, args)
	//    {
	//        Text = defaultText;
	//    }

	//    public override string Translate(ClientLocale locale)
	//    {
	//        return string.Format(Text, Args);
	//    }
	//}
}
