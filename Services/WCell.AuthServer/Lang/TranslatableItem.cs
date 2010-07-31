namespace WCell.AuthServer.Lang
{
	public class TranslatableItem : Util.Lang.TranslatableItem<AuthLangKey>
	{
		public TranslatableItem(AuthLangKey key, params object[] args)
			: base(key, args)
		{
		}
	}
}
