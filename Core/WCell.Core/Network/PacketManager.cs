using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using WCell.Core.Localization;
using WCell.Util.Logging;
using Cell.Core;
using WCell.Util;

namespace WCell.Core.Network
{

	/// <summary>
	/// Manages packet handlers and the execution of them.
	/// </summary>
	public abstract class PacketManager<C, P, A>
		where C : IClient
		where P : PacketIn
		where A : PacketHandlerAttribute
	{
		private readonly Logger s_log = LogManager.GetCurrentClassLogger();
		protected PacketHandler<C, P>[] m_handlers;

		public event Action<C, P> UnhandledPacket;

		public static Action<C, P> DefaultUnhandledPacketHandler = (client, packet) => {
			client.Server.Warning(client, Resources.UnhandledPacket, packet.PacketId, packet.PacketId.RawId, packet.Length);
		};

		protected PacketManager()
		{
			m_handlers = new PacketHandler<C, P>[MaxHandlers];
			UnhandledPacket += DefaultUnhandledPacketHandler;

		}

		public abstract uint MaxHandlers
		{
			get;
		}

		public PacketHandler<C, P>[] Handlers
		{
			get
			{
				return m_handlers;
			}
		}

		public PacketHandler<C, P> this[PacketId packetId]
		{
			get
			{
				return m_handlers[packetId.RawId];
			}
			set
			{
				m_handlers[packetId.RawId] = value;
			}
		}

		#region Register/Unregister handlers

		/// <summary>
		/// Registers a packet handler delegate for a specific packet.
		/// </summary>
		/// <param name="packetId">the PacketID of the packet to register the handler for</param>
		/// <param name="fn">the handler delegate to register for the specified packet type</param>
		public void Register(PacketId packetId, Action<C, P> fn, bool isGamePacket, bool requiresLogin)
		{
			if (m_handlers[packetId.RawId] != null)
			{
				s_log.Debug(string.Format(Resources.HandlerAlreadyRegistered, packetId, m_handlers[packetId.RawId].Handler, fn));
			}

			if (fn != null)
			{
				m_handlers[packetId.RawId] = new PacketHandler<C, P>(fn, isGamePacket, requiresLogin);
			}
			else
			{
				m_handlers[packetId.RawId] = null;
			}
		}

		public void Unregister(PacketId type)
		{
			m_handlers[type.RawId] = null;
		}
		#endregion

		#region Handle Packets

		/// <summary>
		/// Handles a packet that has no handler.
		/// </summary>
		/// <param name="client">the client the packet is from</param>
		/// <param name="packet">the unhandled packet</param>
		public virtual void HandleUnhandledPacket(C client, P packet)
		{
			var evt = UnhandledPacket;
			if (evt != null)
			{
				evt(client, packet);
			}
		}

		/// <summary>
		/// Attempts to handle an incoming packet.
		/// </summary>
		/// <param name="client">the client the packet is from</param>
		/// <param name="packet">the packet to be handled</param>
		/// <returns>true if the packet handler executed successfully; false otherwise</returns>
		public abstract bool HandlePacket(C client, P packet);

		#endregion

		#region Methods

		/// <summary>
		/// Registers all packet handlers defined in the given type.
		/// </summary>
		/// <param name="type">the type to search through for packet handlers</param>
		public void Register(Type type)
		{
			var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);

			foreach (var method in methods)
			{
				var attributes = method.GetCustomAttributes<A>();

				if (attributes.Length == 0)
					continue;

				try
				{
					var handlerDelegate = (Action<C, P>)Delegate.CreateDelegate(typeof(Action<C, P>), method);

					foreach (var attribute in attributes)
					{
						Register(attribute.Id, handlerDelegate, attribute.IsGamePacket, attribute.RequiresLogin);
					}
				}
				catch (Exception e)
				{
					var handlerStr = type.FullName + "." + method.Name;
					throw new Exception("Unable to register PacketHandler " + handlerStr + 
						".\n Make sure its arguments are: " + typeof(C).FullName + ", " + typeof(P).FullName +
						".\n" + e.Message);
				}
			}
		}

		/// <summary>
		/// Automatically detects and registers all PacketHandlers within the given Assembly
		/// </summary>
		public void RegisterAll(Assembly asm)
		{
			// Register all the packet handlers in the given assembly
			foreach (Type asmType in asm.GetTypes())
			{
				Register(asmType);
			}
		}

		//[Initialization(InitializationPass.Second, "Register packet handlers")]
		//public static void RegisterAction<C, P>s()
		//{
		//    AddHandlersOfAsm(Assembly.GetExecutingAssembly());

		//    s_log.Debug(Resources.RegisteredAllHandlers);
		//}

		#endregion
	}
}