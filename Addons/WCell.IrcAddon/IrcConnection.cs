/* This addon was written by Mokrago using Domii's graceously provided irc lib and was often helped out by Domii.
 * So basically I wrote 45% of it, Domii did 50% of it, and the rest 5% was pure magic (hope those magic fixes are stable :P)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using IRCAddon.Commands;
using Squishy.Irc;
using Squishy.Irc.Commands;
using Squishy.Irc.Protocol;
using Squishy.Network;
using WCell.Constants.Login;
using WCell.Core.Initialization;
using WCell.RealmServer;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Global;
using WCell.Util.Commands;
using WCell.Util.NLog;
using WCell.Util;
using WCell.Util.Strings;
using WCellAddon.IRCAddon;


namespace IRCAddon
{
    public class IrcConnection : IrcClient
    {
        #region Fields

        #region Public Static Fields (used for configuration)

        public static bool AnnounceAuthToUser = true;
        public static bool AutoAuth = true;
        public static bool AutomaticTopicUpdating = true;
        public static bool HideChatting = false;
        public static bool HideIncomingIrcPackets = false;
        public static bool HideOutgoingIrcPackets = false;
        public static bool ReConnectOnDisconnect = true;
        public static int ReConnectWaitTime = 50;
        public static int ReConnectAttempts = 100; // If 0, attempts won't be limited
        public static bool ReJoinOnKick = true;
        public static bool ReplyOnUnknownCommandUsed = true;
        public static bool AuthAllUsersOnJoin = false;
        public static bool UpdateTopicOnFlagAdded = true;
        public static IrcCmdCallingRange IrcCmdCallingRange = IrcCmdCallingRange.LocalChannel;
        public static int ExceptionNotificationRank = 1000;
        public static bool ExceptionNotify = true;
        public static bool ExceptionChannelNotification = false;
        public static bool ExceptionNotifyStaffUsers = true;
        public static bool EchoBroadcasts = true;
        public static string[] IrcCmdPrefixes = new[] { "!" };

        public static int SendQueue
        {
            get { return ThrottledSendQueue.CharsPerSecond; }
            set { ThrottledSendQueue.CharsPerSecond = value; }
        }

        public override bool NotifyAuthedUsers
        {
            get { return AnnounceAuthToUser; }
        }

        public override bool AutoResolveAuth
        {
            get { return AutoAuth; }
        }

        #endregion

        #region Private Fields

        //Every channel the bot has joined
        private HashSet<string> _watchedChannels = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private Timer _maintainConnTimer;
        private int _reConnectAttempts = 0;
    	private string _duplicatedCmdPrefix = null;
        #endregion

        #endregion

        // Constructor
        public IrcConnection()
        {
            RealmServer.Instance.AuthClient.Disconnected += AuthClientDisconnected;
            RealmServer.Instance.AuthClient.Connected += AuthClientConnected;
            ProtocolHandler.PacketReceived += OnReceive;
            RealmServer.Shutdown += OnShutdown;
            RealmServer.Instance.StatusChanged += OnStatusNameChange;
            World.Broadcasted += OnBroadcast;
            _maintainConnTimer = new Timer(MaintainCallback);
            LogUtil.ExceptionRaised += LogUtilExceptionRaised;
            Client.Disconnected += OnDisconnect;

            IrcAddon.Instance.Connections.AddElement(this);

			// check if any prefix matches
			foreach (var cmd in WCellCmdTrigger.WCellCmdPrefixes)
			{
				var hasDuplicatedCmdPrefixes = IrcCmdPrefixes.Any(ircCmd => ircCmd == cmd);
				if (hasDuplicatedCmdPrefixes)
				{
					_duplicatedCmdPrefix = cmd;
					break;
				}
			}
        }


        /// <summary>
        /// The Main method. Connects and loads all necessary components. For reconnecting use Reconnect()
        /// </summary>
        [Initialization(InitializationPass.Last, "Initializing IrcAddon")]
        public static void InitIrc()
        {
            if (!File.Exists(AccountAssociationsList.FilePath))
            {
                var list = new AccountAssociationsList();
                list.SaveAs(AccountAssociationsList.FilePath);
            }

            AccountAssociationsList.LoadDictionary(AccountAssociationsList.AccountAssociationFileName);

            try
            {
                var client = new IrcConnection
                {
                    Nicks = IrcAddonConfig.Nicks,
                    UserName = IrcAddonConfig.UserName,
                    // the name that will appear in the hostmask before @ e.g. Mokbot@wcell.org
                    Info = IrcAddonConfig.Info // The info line: Mokbot@wcell.org : asd (<- this bit)
                };
                WCellUtil.Init(client);
                client.BeginConnect(IrcAddonConfig.Network, IrcAddonConfig.Port);
                client.CommandHandler.AddCmdsOfAsm(typeof(IrcConnection).Assembly);
            }
            catch (Exception e)
            {
                //Console.WriteLine("Exception: {0}", e);
                LogUtil.ErrorException(e, "[IRCAddon] Unable to connect to {0}", IrcAddonConfig.Network);
            }
        }

        public void TearDown()
        {
            VoteMgr.Votes = null;
        }

        /// <summary>
        /// Method to reconnect to the irc network
        /// </summary>
        public void Reconnect()
        {
            Client.BeginConnect(IrcAddonConfig.Network, IrcAddonConfig.Port);
        }

        /// <summary>
        /// Fires when the Client is fully logged on the network and the End of Motd is sent (raw 376).
        /// Initializes command and auth handler
        /// </summary>
        protected override void Perform()
        {
            //CommandHandler.Msg("Q@CServe.QuakeNet.org", "AUTH or whatever"););
            foreach (var channel in IrcAddonConfig.ChannelList)
            {
                CommandHandler.Join(channel.ChannelName, channel.Password);
            }
        }

        public void OnShutdown()
        {
            UpdateImportantChannels();
            Thread.Sleep(1000);
            Client.Disconnect();
        }

        private void OnStatusNameChange(RealmStatus status)
        {
            UpdateImportantChannels();
        }

        private void AuthClientDisconnected(object sender, EventArgs e)
        {
            UpdateImportantChannels();
        }

        private void AuthClientConnected(object sender, EventArgs e)
        {
            UpdateImportantChannels();
        }

        #region OnConnecting/OnDisconnected/Packets/OnIrcExceptionRaised

        protected override void OnConnecting()
        {
            Console.WriteLine("Connecting to {0}:{1} ...", Client.RemoteAddress, Client.RemotePort);
        }

        protected override void OnConnected()
        {
            Console.WriteLine("Connected to {0}:{1} ...", Client.RemoteAddress, Client.RemotePort);

            // If we achieve a connection, we don't want the timer to tick 
            _maintainConnTimer.Change(Timeout.Infinite, Timeout.Infinite);

            // We set the attempts back to 0 for the next time we lose connection.
            _reConnectAttempts = 0;
        }

        protected void OnReceive(IrcPacket packet)
        {
            if (HideIncomingIrcPackets != true)
            {
                Console.WriteLine("<-- " + packet);
            }
        }

        protected override void OnBeforeSend(string text)
        {
            if (HideOutgoingIrcPackets != true)
            {
                Console.WriteLine("--> " + text);
            }
        }

        protected override void OnConnectFail(Exception ex)
        {
            Console.WriteLine("Connection failed: " + ex);
            Console.WriteLine("Trying to reconnect in {0}", ReConnectWaitTime);

            StartReConnectTimer();
        }

        protected override void OnDisconnected(bool conLost)
        {
            Console.WriteLine("Disconnected" + (conLost ? " (Connection lost)" : ""));
        }

        protected override void OnExceptionRaised(Exception e)
        {
            Console.WriteLine(e);
        }

        /// <summary>
        /// Starts the reconnect timer. Checks whether the timer will be triggered
        /// </summary>
        private void StartReConnectTimer()
        {
            if (ReConnectAttempts != 0)
            {
                // We only want to try reconnect as many times the user defines
                if (_reConnectAttempts++ <= ReConnectAttempts)
                {
                    if (!Client.IsConnected && !LoggedIn && !Client.IsConnecting)
                        _maintainConnTimer.Change(ReConnectWaitTime * 1000, 0);
                }
                // Stop trying to reconnect if we've reached our user defined treshhold
                else
                    _maintainConnTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

                // If 0, there is no limit to reconnects
            else
            {
                if (!Client.IsConnected && !LoggedIn && !Client.IsConnecting)
                    _maintainConnTimer.Change(ReConnectWaitTime * 1000, 0);
            }
        }

        #endregion

        #region User related events/Methods

        /// <summary>
        /// Rejoin the channel the bot was kicked from.
        /// Only called when the kicked user's name matches the bot's name
        /// </summary>
        /// <param name="from"></param>
        /// <param name="chan"></param>
        /// <param name="target"></param>
        /// <param name="reason"></param>
        protected override void OnKick(IrcUser from, IrcChannel chan, IrcUser target, string reason)
        {
            if (target == Me && ReJoinOnKick)
            {
                try
                {
                    CommandHandler.Join(chan.Name);
                    CommandHandler.Msg(from, "Don't kick me!", Me.Nick);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        protected override void OnInvite(IrcUser user, string chan)
        {
            if (user != null && chan != null && user.IsAuthenticated)
            {
                CommandHandler.Join(chan);
            }
        }

        protected override void OnUserLeftChannel(IrcChannel chan, IrcUser user, string reason)
        {
            base.OnUserLeftChannel(chan, user, reason);
            Console.WriteLine("**{0} quit({1})", user.Nick, reason);
            if (user == Me)
            {
                _watchedChannels.Remove(chan.Name);
            }
        }

        protected override void OnJoin(IrcUser user, IrcChannel chan)
        {
            //If the bot joins a channel, it will add that chan to _watchedChannels
            if (user == Me)
            {
                _watchedChannels.Add(chan.Name);
            }

            //Try and auth the joined user
            try
            {
                if (AnnounceAuthToUser)
                {
                    user.Msg("Resolving User...".Colorize(IrcColorCode.Red));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnUsersAdded(IrcChannel chan, IrcUser[] users)
        {
            Console.WriteLine("Topic was set {0} by {1}", chan.TopicSetTime, chan.TopicSetter);
            Console.WriteLine("Topic is: {0}", chan.Topic);
            foreach (var channel in IrcAddonConfig.UpdatedChannelNames)
            {
                if (chan.Name == channel)
                {
                    UpdateTopic(chan, chan.Topic);
                }
            }
        }

        // Try and update the topic of the channel after the bot has been privileged
        // Useful when the bot joined and wasn't privileged to change the topic and if
        // afterwards a user/network service/bot opped the bot, it will try and update the topic
        // Bear in mind this will update the status of *any* channel he is given op or higher priv
        protected override void OnFlagAdded(IrcUser user, IrcChannel chan, Privilege priv, IrcUser target)
        {
            if (target == Me && priv >= IrcAddonConfig.RequiredStaffPriv && UpdateTopicOnFlagAdded)
            {
                Console.WriteLine("Updating topic...");
                UpdateTopic(chan);
            }
        }

        #endregion

        #region Topic/text

        protected override void OnTopic(IrcUser user, IrcChannel chan, string text, bool initial)
        {
            if (initial)
            {
                Console.WriteLine("The topic for channel {0} is {1}", chan.Name, chan.Topic);
            }
            else
            {
                Console.WriteLine("{0} changed topic in channel {1} to: {2}", user.Nick, chan.Name, text);
                if (user != Me)
                    UpdateTopic(chan, text);
            }
        }

        protected override void OnText(IrcUser user, IrcChannel chan, StringStream text)
        {
            CommandHandler.RemoteCommandPrefixes = IrcCmdPrefixes;		// no idea what this is good for
            if (!HideChatting)
            {
                Console.WriteLine("<{0}> {1}", user, text);				// no idea what this is good for
            }
			
			var textRecieved = text.CloneStream();
        	
			// first: Try to execute IRC command
			if (HasCommandPrefix(text, IrcCmdPrefixes))
			{
				var isIrcCommand = false;
				var trigger = new PrivmsgCmdTrigger(text, user, chan);
				var cmd = CommandHandler.GetCommand(trigger);
				if (cmd != null)
				{
					//check if we have the same prefix
					//for all commands
					if (!string.IsNullOrEmpty(_duplicatedCmdPrefix))
					{
						//we have, so now check if wcell also has this
						//command
						if (WCellUtil.CommandExists(trigger.Alias))
						{
							//it does, so now enforce case sensitivity on
							//the command alias
							if (trigger.Alias.ToUpper() != trigger.Alias)
							{
								trigger.Reply("Running irc command; to execute the WCell command use {0}{1} [args]", _duplicatedCmdPrefix, trigger.Alias.ToUpper());
								isIrcCommand = true;
							}
							else
							{
								trigger.Reply("Running WCell command; to execute the irc command use {0}{1} [args]", _duplicatedCmdPrefix, trigger.Alias.ToLower());
							}
						}
					}

					// IRC command exists
					if (isIrcCommand)
					{
						m_CommandHandler.Execute(trigger, cmd, false);
						return;
					}
				}
			}

			text = textRecieved;
			// IRC command does not exist -> Try WCell command
			if (!HasCommandPrefix(text, WCellCmdTrigger.WCellCmdPrefixes)) return;

			if (!user.IsAuthenticated)
			{
				// auth now
				AuthMgr.Authenticator.ResolveAuth(user, usr =>
				{
					if (usr.IsAuthenticated)
					{
						// auth succeeded -> execute command
						TryExecuteWCellCommand(user, chan, text.Remainder);
					}
					else
					{
						// User cannot use commands because he does not have a verified Account
						// maybe send him a link to register online
						user.Msg("You do not have sufficient rights");
					}
				});
			}
			else
			{
				// already auth'ed -> execute command
				TryExecuteWCellCommand(user, chan, text.Remainder);
			}
		}

		private static bool HasCommandPrefix(StringStream text, IEnumerable<string> prefixes)
		{
			// check if any prefix matches
			var hasPrefix = prefixes.Iterate(prefix =>
			{
				if (text.String.StartsWith(prefix,
					StringComparison.CurrentCultureIgnoreCase))
				{
					text.Skip(prefix.Length);
					return false;
				}
				return true;
			});
			return hasPrefix;
		}

    	private static void TryExecuteWCellCommand(IrcUser user, IrcChannel chan, string text)
        {
            var uArgs = user.Args as WCellArgs;
            if (uArgs != null && uArgs.CmdArgs != null)
            {
                WCellUtil.HandleCommand((WCellUser)uArgs.CmdArgs.User, user, chan, text);
            }
        }

        #endregion

        #region Command handling

        /// <summary>
        /// Return wether or not the given trigger may be processed.
        /// Auth command will always be processed.
        /// Other command triggers will be handled according to the user's priv levels
        /// </summary>
        public override bool MayTriggerCommand(CmdTrigger<IrcCmdArgs> trigger, Command<IrcCmdArgs> cmd)
        {
            var uArgs = trigger.Args.User.Args as WCellArgs;

            if (base.MayTriggerCommand(trigger, cmd))
            {
                // always ok
                return true;
            }

            if (cmd is AuthCommand)
            {
                // everyone may use the AuthCommand
                return true;
            }

            if (CheckIsStaff(trigger.Args.User))
            {
                // staff users may trigger 
                return true;
            }

            if (uArgs == null)
            {
                // Unauthorized user
                // Iterate over all the bot's channels
                return trigger.Args.User.Channels.Values.Any(userChan =>
                     _watchedChannels.Contains(userChan.Name) &&
                    userChan.IsUserAtLeast(trigger.Args.User, IrcAddonConfig.RequiredStaffPriv)
                );
            }
            return false;
        }

        /// <summary>
        /// Always returns false, because we have a custom command handling implementation
        /// </summary>
        public override bool TriggersCommand(IrcUser user, IrcChannel chan, StringStream input)
        {
            return false;
        }

        protected override void OnUnknownCommandUsed(CmdTrigger<IrcCmdArgs> trigger)
        {
            if (ReplyOnUnknownCommandUsed)
            {
                trigger.Reply("Command-Alias not found: " + trigger.Alias);
                base.OnUnknownCommandUsed(trigger);
            }
        }

        /// <summary>
        /// Method called when an Irc Exception is raised
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="ex"></param>
        protected override void OnCommandFail(CmdTrigger<IrcCmdArgs> trigger, Exception ex)
        {
            var cmd = trigger.Command;
            string[] lines = ex.ToString().Split(new[] { "\r\n|\n|\r" }, StringSplitOptions.RemoveEmptyEntries);

            trigger.Reply("Exception raised: " + lines[0]);
            for (int i = 1; i < lines.Length; i++)
            {
                // TODO: automatically detect lines before sending in Client-class
                trigger.Reply(lines[i]);
            }
            trigger.Reply("Unproper command use - " + cmd.Name + ": " + cmd.EnglishDescription);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Method called by the ExceptionRaised event
        /// </summary>
        /// <param name="text"></param>
        /// <param name="exception"></param>
        private void LogUtilExceptionRaised(string text, Exception exception)
        {
            if (ExceptionNotify)
            {
                if (ExceptionNotifyStaffUsers)
                {
                    foreach (IrcUser user in Users.Values)
                    {
                        if (user.IsAuthenticated)
                        {
                            var uArgs = user.Args as WCellArgs;
                            if (uArgs != null)
                            {
                                if (uArgs.Account.Role >= ExceptionNotificationRank && uArgs.AcceptExceptionEchos)
                                {
                                    user.Msg(text);
                                    foreach (var line in exception.GetAllMessages())
                                        user.Msg(line);
                                }
                            }
                        }
                    }
                }

                if (ExceptionChannelNotification)
                {
                    GetChannel(IrcAddonConfig.ExceptionChan).Msg(text);
                }
            }
        }

        private void MaintainCallback(object state)
        {
            // We don't want to connect multiple times
            if (!LoggedIn && !Client.IsConnecting)
                Reconnect();
        }

        /// <summary>
        /// A helper method to check whether or not the user is staff.
        /// Mostly used to maintain code readability.
        /// Did not use in places I really don't want anything to go wrong
        /// and also in places where I wanted the flow of code to be 
        /// centralized for readability.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool CheckIsStaff(IrcUser user)
        {
            var uArgs = user.Args as WCellArgs;
            if (uArgs != null)
            {
                if (uArgs.Account.Role.IsStaff)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Method to check IrcCmdCallingRange and return a boolean accordingly
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="chan"></param>
        /// <returns>Whether a given user can trigger a command on the target channel</returns>
        private bool CheckCmdCallingRange(CmdTrigger<IrcCmdArgs> trigger, IrcChannel chan, Command<IrcCmdArgs> cmd)
        {
            // TODO: The code is inconsistent and employs some weird heuristics (text.Contains("#") !?)
            var text = trigger.Text.Remainder;

            switch (IrcCmdCallingRange)
            {
                // Allow to call commands on any target channel
                case IrcCmdCallingRange.Everywhere:
                    return true;

                // Allow to call commands if the commands does not contain the channel argument
                case IrcCmdCallingRange.IsPrivilegedOnTrgt:
                    if (!text.Contains("#"))
                        return true;

                    // Join command should always be available
                    if (cmd is JoinCommand)
                        return true;

                    else
                    {
                        //Checks whether or not the triggerer has staff priv levels on the target channel
                        var user = trigger.Args.User;
                        var chanName = trigger.Text.CloneStream().NextWord(" ");
                        var targetChan = GetChannel(chanName);

                        if (targetChan.HasUser(user.Nick))
                        {
                            if (targetChan.IsUserAtLeast(user, IrcAddonConfig.RequiredStaffPriv))
                                return true;
                        }
                    }
                    break;

                default:
                    // Allow to call commands if the commands does not contain the channel argument
                    if (!text.Contains("#"))
                        return true;

                    else
                    {
                        // Join command should always be available
                        if (cmd is JoinCommand)
                            return true;

                        // IF target chan is the same as the triggerer's chan, allow command
                        string chanName = trigger.Text.CloneStream().NextWord(" ");

                        if (chan.Name == chanName)
                            return true;
                    }
                    break;
            }
            return false;
        }

        private void OnBroadcast(IChatter sender, string message)
        {
            if (EchoBroadcasts)
            {
                foreach (var chanInfo in IrcAddonConfig.UpdatedChannelNames)
                {
                    var chan = GetChannel(chanInfo);
                    if (chan == null) continue;
                    if (sender != null)
                    {
                        chan.Msg(sender.Name + ": " + ChatUtility.Strip(message));
                    }
                    else
                    {
                        chan.Msg(ChatUtility.Strip(message));
                    }
                }
            }
        }

        private void OnDisconnect(Connection con, bool connectionLost)
        {
            if (connectionLost && ReConnectOnDisconnect)
            {
                StartReConnectTimer();
            }
        }

        private void UpdateImportantChannels()
        {
            foreach (var chan in IrcAddonConfig.UpdatedChannelNames)
            {
                if (_watchedChannels.Contains(chan))
                {
                    var channel = GetChannel(chan);
                    UpdateTopic(channel, channel.Topic);
                }
            }
        }

        /// <summary>
        /// Formats the channel's topic and adds server status
        /// Only used in OnTopic
        /// </summary>
        /// <param name="chan"></param>The channel which topic is being updated
        /// <param name="text"></param>The channel's new topic
        private void UpdateTopic(IrcChannel chan, string text)
        {
            if (chan != null)
            {
                if (AutomaticTopicUpdating)
                {
                    if (text.Contains("Server status: "))
                    {
                        text = Regex.Replace(text, @"Server status\: [^$ ]+",
                                             "Server status: " + ServerStatus.StatusName);
                        chan.Topic = text;
                    }

                    else
                        chan.Topic = text.Trim() + " | Server status: " + ServerStatus.StatusName;
                }
            }
        }

        /// <summary>
        /// A static method to update and format the channel's topic
        /// </summary>
        /// <param name="chan">The target channel.</param>
        public static void UpdateTopic(IrcChannel chan)
        {
            if (chan == null) return;

            if (chan.Topic.Contains("Server status: "))
            {
                chan.Topic = Regex.Replace(chan.Topic, @"Server status\: [^$ ]+",
                                           "Server status: " + ServerStatus.StatusName);
            }

            else
            {
                chan.Topic = chan.Topic.Trim() + " | Server status: " + ServerStatus.StatusName;
            }
        }

        #endregion
    }

    public class PrivmsgCmdTrigger : IrcCmdTrigger
    {
        public PrivmsgCmdTrigger(string args, IrcUser user, IrcChannel chan)
            : base(args, user, chan)
        {
        }

        public PrivmsgCmdTrigger(StringStream args, IrcUser user, IrcChannel chan)
            : base(args, user, chan)
        {
        }

        public override void Reply(string text)
        {
            Args.Target.Msg(text);
        }
    }
}