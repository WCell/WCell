namespace WCell.Util.Serialization
{
	/// <summary>
	/// TODO: Fully implement required features
	/// </summary>
	public abstract class SerializerCollection
	{
		private VersionedSerializer[] Serializers;

		protected SerializerCollection(int currentVersion)
		{
			Serializers = new VersionedSerializer[currentVersion + 1];
		}
	}
}
