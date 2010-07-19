namespace WCell.RealmServer.GameObjects.GOEntries
{
	public class GOTrapDoorEntry : GOEntry
	{
	    public uint WhenToPause
	    {
            get { return Fields[0]; }
	    }

	    public uint StartOpen
	    {
            get { return Fields[1]; }
	    }

	    public uint AutoClose
	    {
            get { return Fields[2]; }
	    }
	}
}