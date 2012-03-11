namespace WCell.RealmServer.Commands
{
    public class GossipCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            Init("Gossip");
        }

        //public class GossipTextCommand : SubCommand
        //{
        //    protected override void Initialize()
        //    {
        //        Init("Text");
        //        EnglishParamInfo = "<id> [<new text>]";
        //        EnglishDescription = "Gets or sets the text of the first text of the gossip entry with the given id. " +
        //            "Keep in mind that changed text will only display to those who invalidate their client cache or who receive a forced update of the new entry.";
        //    }

        //    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        //    {
        //        var id = trigger.Text.NextUInt(0);
        //        var textEntry = GossipMgr.GetEntry(id);

        //        if (textEntry == null)
        //        {
        //            trigger.Reply("Invalid id: {0}", id);
        //            return;
        //        }

        //        if (trigger.Text.HasNext)
        //        {
        //            var str = trigger.Text.Remainder;
        //            textEntry.GossipTexts[0].TextMale = textEntry.GossipTexts[0].TextFemale = str;
        //            trigger.Reply("Changed text of entry {0} to: {1}", id, str);
        //            if (trigger.Args.Target is Character)
        //            {
        //                QueryHandler.SendNPCTextUpdate((Character)trigger.Args.Target, textEntry);
        //            }
        //        }
        //        else
        //        {
        //            trigger.Reply("Text of entry {0} is: {1}", id, textEntry.GossipTexts[0].TextMale);
        //        }
        //    }
        //}
    }
}
