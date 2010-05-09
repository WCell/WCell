namespace WCell.RealmServer.Server
{
	public enum ServerRuleType
	{
		/// <summary>
		/// Anything that doesn't have its own RuleType
		/// </summary>
		Misc,

		/// <summary>
		/// Used unallowed colors etc in chat (can be abused for impersonating)
		/// </summary>
		ChatControlCodes,

		/// <summary>
		/// Tried to chat in language that he/she doesn't know
		/// </summary>
		ChatLanguage,
		
		
	}
}
