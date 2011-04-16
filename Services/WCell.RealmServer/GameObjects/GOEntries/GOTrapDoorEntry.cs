namespace WCell.RealmServer.GameObjects.GOEntries
{
	public class GOTrapDoorEntry : GOEntry
	{
	    public int WhenToPause
	    {
            get { return Fields[0]; }
	    }

	    public int StartOpen
	    {
            get { return Fields[1]; }
	    }

	    public int AutoClose
	    {
            get { return Fields[2]; }
	    }
	}
}