namespace WCell.RealmServer.Interaction
{
	public interface IBaseRelation
	{
		/// <summary>
		/// The Character who created this Relation
		/// </summary>
		uint CharacterId
		{
			get;
			set;
		}

		/// <summary>
		/// The related Character with who this Relation is with
		/// </summary>
		uint RelatedCharacterId
		{
			get;
			set;
		}

		/// <summary>
		/// The relation type
		/// </summary>
		CharacterRelationType Type
		{
			get;
		}

		/// <summary>
		/// A note describing the relation
		/// </summary>
		string Note
		{
			get;
			set;
		}
	}
}