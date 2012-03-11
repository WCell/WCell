namespace WCell.RealmServer.Help.Tickets
{
    public partial class Ticket
    {
        public delegate void TicketHandlerChangedHandler(Ticket ticket, ITicketHandler oldHandler);

        public event TicketHandlerChangedHandler TicketHandlerChanged;
    }
}