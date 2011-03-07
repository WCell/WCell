/*************************************************************************
 *
 *   file		: Authenticator.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-04 09:19:17 +0100 (on, 04 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 779 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Security.Cryptography;
using System.Text;
using WCell.Core.Network;
using WCell.Intercommunication.DataTypes;

namespace WCell.Core.Cryptography
{
	/// <summary>
	/// Handles writing the server's authentication proof and validating the client's proof.
	/// </summary>
	public class Authenticator
	{
		private readonly SecureRemotePassword m_srp;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="srp">the SRP instance for our current session</param>
		public Authenticator(SecureRemotePassword srp)
		{
			m_srp = srp;
		}

		/// <summary>
		/// The SRP instance we're using
		/// </summary>
		public SecureRemotePassword SRP
		{
			get { return m_srp; }
		}

        public byte[] ReconnectProof
        {
            get;
            set;
        }

		/// <summary>
		/// Writes the server's challenge.
		/// </summary>
		/// <param name="packet">the packet to write to</param>
		public void WriteServerChallenge(PrimitiveWriter packet)
		{
			packet.WriteBigInt(m_srp.PublicEphemeralValueB, 32);
			packet.WriteBigIntLength(m_srp.Generator, 1);

			// We will pad this out to 32 bytes.
			packet.WriteBigIntLength(m_srp.Modulus, 32);
			packet.WriteBigInt(m_srp.Salt);
		}

		/// <summary>
		/// Checks if the client's proof matches our proof.
		/// </summary>
		/// <param name="packet">the packet to read from</param>
		/// <returns>true if the client proof matches; false otherwise</returns>
        public bool IsClientProofValid(PacketIn packet)
		{
		    m_srp.PublicEphemeralValueA = packet.ReadBigInteger(32);

		    BigInteger proof = packet.ReadBigInteger(20);

            // SHA1 of PublicEphemeralValueA and the 16 random bytes sent in 
            // AUTH_LOGON_CHALLENGE from the server
		    byte[] arr = packet.ReadBytes(20);

		    byte keyCount = packet.ReadByte();
		    for (int i = 0; i < keyCount; i++)
		    {
		        ushort keyUnk1 = packet.ReadUInt16();
		        uint keyUnk2 = packet.ReadUInt32();
		        byte[] keyUnkArray = packet.ReadBytes(4);
                // sha of the SRP's PublicEphemeralValueA, PublicEphemeralValueB, 
                // and 20 unknown bytes
		        byte[] keyUnkSha = packet.ReadBytes(20);
		    }

		    byte securityFlags = packet.ReadByte();

		    if ((securityFlags & 1) != 0)
		    {
		        // PIN
		        byte[] pinRandom = packet.ReadBytes(16);
		        byte[] pinSha = packet.ReadBytes(20);
		    }
		    if ((securityFlags & 2) != 0)
		    {
		        byte[] security2Buf = packet.ReadBytes(20);
		    }
		    if ((securityFlags & 4) != 0)
		    {
		        byte arrLen = packet.ReadByte();
		        byte[] security4Buf = packet.ReadBytes(arrLen);
		    }

		    return m_srp.IsClientProofValid(proof);
		}

	    /// <summary>
		/// Writes the server's proof. 
		/// </summary>
		/// <param name="packet">the packet to write to</param>
		public void WriteServerProof(PrimitiveWriter packet)
		{
			packet.WriteBigInt(m_srp.ServerSessionKeyProof, 20);
		}


        public void WriteReconnectChallenge(PrimitiveWriter packet)
        {
            ReconnectProof = new byte[16];
            new System.Random(System.Environment.TickCount).NextBytes(ReconnectProof);

            packet.Write(ReconnectProof);
            // 16 bytes of 0's
            packet.Write(0L);
            packet.Write(0L);
        }

        public bool IsReconnectProofValid(PacketIn packet, AuthenticationInfo authInfo)
        {
            //md5 hash of account name and secure random
            byte[] md5Hash = packet.ReadBytes(16);

            //byte[20] sha1 hash of accountname, md5 from above, reconnectProof, byte[40] sessionkey
            byte[] shaHash1 = packet.ReadBytes(20);
            //byte[20] sha1 hash of md5 from above and byte[20] (all zero)
            byte[] shaHash2 = packet.ReadBytes(20);

			byte[] username = WCellConstants.DefaultEncoding.GetBytes(m_srp.Username);

            var sha = new SHA1Managed();
            sha.TransformBlock(username, 0, username.Length, username, 0);
            sha.TransformBlock(md5Hash, 0, md5Hash.Length, md5Hash, 0);
            sha.TransformBlock(ReconnectProof, 0, ReconnectProof.Length, ReconnectProof, 0);
            sha.TransformBlock(authInfo.SessionKey, 0, authInfo.SessionKey.Length, authInfo.SessionKey, 0);
            byte[] hash = sha.TransformFinalBlock(new byte[0], 0, 0);

            for (int i = 0; i < 20; i++)
            {
                if (shaHash1[i] != hash[i])
                    return false;
            }
            return true;
        }
	}
}