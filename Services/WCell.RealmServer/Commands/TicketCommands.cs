using WCell.RealmServer.Chat;
using WCell.RealmServer.Help.Tickets;
using WCell.Util.Commands;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Commands
{
	public class TicketCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Ticket", "Tickets");
			EnglishDescription = "Provides all commands necessary to work with Tickets.";
		}

		#region Select
		public class SelectPlayerTicketCommand : TicketSubCmd
		{
			protected override void Initialize()
			{
				Init("Select", "Sel");
				EnglishParamInfo = "[<playername>]";
				EnglishDescription = "Selects the Ticket of the targeted Player or the Player with the given name.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var name = trigger.Text.NextWord();
				var chr = trigger.Args.GetCharArgumentOrTarget(trigger, name);
				var handler = trigger.Args.TicketHandler;

				if (chr != null && chr.IsInWorld)
				{
					var ticket = chr.Ticket;
					var oldHandler = ticket.Handler;
					if (oldHandler != null && oldHandler.Role > handler.Role)
					{
						trigger.Reply("Ticket is already being handled by: " + oldHandler.Name);
					}
					else
					{
						if (oldHandler != null)
						{
							trigger.Reply("Taking over Ticket from: " + oldHandler.Name);
							oldHandler.SendMessage("The Ticket you were handling by " + ticket.Owner + " is now handled by: " + handler);
						}
						ticket.Handler = handler;
					}
				}
				else
				{
					trigger.Reply("Selected player is offline or does not exist: " + name);
				}
			}
		}
		#endregion

		#region SelectNext
		public class SelectNextTicketCommand : TicketSubCmd
		{
			protected override void Initialize()
			{
				Init("SelectNext", "SelNext", "Next", "N");
				EnglishDescription = "Selects the next unhandled ticket.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var ticket = TicketMgr.Instance.HandleNextUnhandledTicket(trigger.Args.TicketHandler);
				if (ticket == null)
				{
					trigger.Reply("There are currently no unhandled Tickets.");
				}
				else
				{
					ticket.DisplayFormat(trigger, "Now Selected: ");
				}
			}
		}
		#endregion

		#region Unselect
		public class UnselectTicketCommand : TicketSubCmd
		{
			protected override void Initialize()
			{
				Init("Unselect", "U");
				EnglishDescription = "Unselects the current Ticket.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var ticket = trigger.Args.TicketHandler.HandlingTicket;
				ticket.Handler = null;
				trigger.Reply("Done.");
			}

			public override bool RequiresActiveTicket
			{
				get
				{
					return true;
				}
			}
		}
		#endregion

		#region Goto
		public class GotoTicketCommand : TicketSubCmd
		{
			protected override void Initialize()
			{
				Init("Goto", "Go", "Tele");
				EnglishDescription = "Teleports directly to the ticket issuer or his/her ticket posting location if offline.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var ticket = trigger.Args.TicketHandler.HandlingTicket;
				if (ticket.Owner == null)
				{
					trigger.Reply("The owner of this Ticket is offline.");
					trigger.Args.Target.TeleportTo(ticket.Region, ticket.Position);
				}
				else
				{
					trigger.Args.Target.TeleportTo(ticket.Owner);
				}
			}

			public override bool RequiresActiveTicket
			{
				get
				{
					return true;
				}
			}

			public override bool RequiresIngameTarget
			{
				get
				{
					return true;
				}
			}
		}
		#endregion

		#region List
		public class ListTicketsCommand : TicketSubCmd
		{
			protected override void Initialize()
			{
				Init("List", "L");
				EnglishDescription = "Shows all currently active tickets.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var tickets = TicketMgr.Instance.GetAllTickets();
				if (tickets.Length == 0)
				{
					trigger.Reply("There are no active tickets.");
				}
				else
				{
					foreach (var ticket in tickets)
					{
						trigger.Reply("{0} by {1}{2} (age: {3})", ticket.Type, ticket.OwnerName,
						              ticket.Owner != null ? "" : ChatUtility.Colorize(" (Offline)", Color.Red, true), ticket.Age);
					}
				}
			}

			public override bool RequiresTicketHandler
			{
				get
				{
					return false;
				}
			}
		}
		#endregion

		#region Current
		public class CurrentTicketCommand : TicketSubCmd
		{
			protected override void Initialize()
			{
				Init("Current", "Cur");
				EnglishDescription = "Shows the Ticket you are currently handling.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var ticket = trigger.Args.TicketHandler.HandlingTicket;
				ticket.DisplayFormat(trigger, "Now Selected: ");
			}

			public override bool RequiresActiveTicket
			{
				get
				{
					return true;
				}
			}
		}
		#endregion

		#region Show
		public class ShowTicketCommand : TicketSubCmd
		{
			protected override void Initialize()
			{
				Init("Show", "S");
				EnglishDescription = "Shows the Target's currently active Ticket.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var target = trigger.Args.Target as Character;
				if (target == null)
				{
					trigger.Reply("Only Characters can have Tickets.");
					return;
				}

				var ticket = target.Ticket;
				if (ticket != null)
				{
					ticket.DisplayFormat(trigger, "");
				}
				else
				{
					trigger.Reply("{0} has no active Ticket.", target.Name);
				}
			}

			public override bool RequiresIngameTarget
			{
				get
				{
					return true;
				}
			}
		}
		#endregion

		#region Delete
		public class DeleteTicketCommand : TicketSubCmd
		{
			protected override void Initialize()
			{
				Init("Delete", "Del", "D", "Remove", "R");
				EnglishDescription = "Deletes the current Ticket.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var ticket = trigger.Args.TicketHandler.HandlingTicket;
				trigger.Reply(ticket.OwnerName + "'s Ticket was deleted.");
				ticket.Delete();
			}

			public override bool RequiresActiveTicket
			{
				get
				{
					return true;
				}
			}
		}
		#endregion

		public override bool MayTrigger(CmdTrigger<RealmServerCmdArgs> trigger, BaseCommand<RealmServerCmdArgs> command, bool silent)
		{
			if (command is TicketSubCmd)
			{
				var cmd = (TicketSubCmd)command;
				if ((cmd.RequiresTicketHandler || cmd.RequiresActiveTicket) && trigger.Args.TicketHandler == null)
				{
					if (!silent)
					{
						trigger.Reply("Cannot use Command in this Context (TicketHandler required)");
						RealmCommandHandler.Instance.DisplayCmd(trigger, this, false, false);
					}
					return false;
				}
				if (cmd.RequiresActiveTicket && trigger.Args.TicketHandler.HandlingTicket == null)
				{
					if (!silent)
					{
						trigger.Reply("You do not have a Ticket selected. Use the \"Next\" command first:");
						var nextCmd = RealmCommandHandler.Instance.Get<SelectNextTicketCommand>();
						RealmCommandHandler.Instance.DisplayCmd(trigger, nextCmd, false, true);
					}
					return false;
				}
			}
			return true;
		}

		public abstract class TicketSubCmd : SubCommand
		{
			public virtual bool RequiresTicketHandler
			{
				get { return true; }
			}

			public virtual bool RequiresActiveTicket
			{
				get { return false; }
			}

			public virtual bool RequiresIngameTarget
			{
				get { return false; }
			}
		}
	}
}
