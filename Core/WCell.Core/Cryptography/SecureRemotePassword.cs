/*************************************************************************
 *
 *   file		: SecureRemotePassword.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-09-12 20:11:55 +0200 (lø, 12 sep 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1094 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace WCell.Core.Cryptography
{
    /// <summary>
    /// This is an implementation of the SRP algorithm documented here:
    /// 
    /// http://srp.stanford.edu/design.html
    /// 
    ///
    /// Example code (though usually data is copied over the wire):
    /// 	WCell.Cryptography.SRP server = new WCell.Cryptography.SRP(true, "USER", "PASSWORD");
    ///		WCell.Cryptography.SRP client = new WCell.Cryptography.SRP(false, "USER", "PASSWORD");
    ///
    ///		server.PublicEphemeralValueA = client.PublicEphemeralValueA;
    ///		client.Salt = server.Salt;
    ///		client.PublicEphemeralValueB = server.PublicEphemeralValueB;
    ///
    ///		Console.WriteLine("Server's session key = {0}", server.SessionKey.ToHexString());
    ///		Console.WriteLine("Client's session key = {0}", client.SessionKey.ToHexString());
    ///
    ///		Console.WriteLine("\nServer key == client key {0}", server.SessionKey == client.SessionKey);
    ///
    ///		Console.WriteLine("Client proof valid: {0}", client.ClientSessionKeyProof == server.ClientSessionKeyProof);
    ///		Console.WriteLine("Server proof valid: {0}", client.ServerSessionKeyProof == server.ServerSessionKeyProof);
    /// </summary>
    [Serializable]
    public partial class SecureRemotePassword
	{
		/// <summary>
		/// The required minimum length of a password
		/// </summary>
		public const int MinPassLength = 3;

		/// <summary>
		/// The required maximum length of a password
		/// </summary>
		public const int MaxPassLength = 16;

        private readonly bool m_isServer;
        private SRPParameters m_srpParams = SRPParameters.Defaults;
        private BigInteger m_credentialsHash;
        private BigInteger m_salt;
        private BigInteger m_sessionKey;
        private BigInteger m_publicEphemeralValueA;
        private readonly BigInteger m_secretEphemeralValueA = RandomNumber();
        private BigInteger m_publicEphemeralValueB;
        private BigInteger m_secretEphemeralValueB;
        private BigInteger verifier;

        public SecureRemotePassword(bool isServer, SRPParameters parameters)
        {
            m_srpParams = parameters;
            m_isServer = isServer;
        }

        public SecureRemotePassword(bool isServer)
            : this(isServer, SRPParameters.Defaults)
        {
        }

        public SecureRemotePassword(string username, BigInteger credentials, bool isServer, SRPParameters parameters)
        {
            if (parameters.CaseSensitive == false)
            {
                username = username.ToUpper();
            }

            m_srpParams = parameters;

            m_isServer = isServer;
            Username = username;
            Credentials = credentials;
        }

        public SecureRemotePassword(string username, BigInteger credentials, bool isServer)
            : this(username, credentials, isServer, SRPParameters.Defaults)
        {
        }

        /// <summary>
        /// Make an SRP for user authentication. You use something like this when your
        /// verifier and salt are stored in a database
        /// </summary>
        /// <param name="username"></param>
        /// <param name="verifier"></param>
        /// <param name="salt"></param>
        /// <param name="parameters"></param>
        public SecureRemotePassword(string username, BigInteger verifier, BigInteger salt, SRPParameters parameters)
        {
            if (parameters.CaseSensitive == false)
            {
                username = username.ToUpper();
            }

            m_srpParams = parameters;

            m_isServer = true;
            Username = username;
            Verifier = verifier;
            m_salt = salt;
        }

        public SecureRemotePassword(string username, BigInteger verifier, BigInteger salt)
            : this(username, verifier, salt, SRPParameters.Defaults)
        {
        }

        public SRPParameters Parameters
        {
            get { return m_srpParams; }
            set { m_srpParams = value; }
        }

        /// <summary>
        /// Are we the server? This should be set before calculation commences.
        /// </summary>
        public bool IsServer
        {
            get { return m_isServer; }
        }

        /// <summary>
        /// Correct username. This should be set before calculations happen.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Referred to as 'M' in the documentation. This is used for authentication.
        /// 
        /// The client sends this value to the server and the server calculates it locally to verify it.
        /// The same then happens with ServerSessionKeyProof. Note ClientSessionKeyProof should come first.
        /// </summary>
        public BigInteger ClientSessionKeyProof
        {
            get
            {
                return
                    Hash(Hash(Modulus) ^ Hash(Generator), Hash(Username), Salt, PublicEphemeralValueA,
                         PublicEphemeralValueB, SessionKey);
            }
        }

        /// <summary>
        /// The server sends this to the client as proof it has the same session key as the client.
        /// The client will calculate this locally to verify.
        /// </summary>
        public BigInteger ServerSessionKeyProof
        {
            get { return Hash(PublicEphemeralValueA, ClientSessionKeyProof, SessionKey); }
        }

        /// <summary>
        /// Generate a random number of maximal size
        /// </summary>
        /// <returns></returns>
        public static BigInteger RandomNumber()
        {
            return RandomNumber(SRPParameters.KeyLength);
        }

        public BigInteger Modulus
        {
            get { return Parameters.Modulus; }
        }

        public BigInteger Generator
        {
            get { return Parameters.Generator; }
        }

        /// <summary>
        /// 'k' in the spec. In SRP-6a k = H(N, g). Older versions have k = 3.
        /// </summary>
        public BigInteger Multiplier
        {
            get
            {
                if (Parameters.AlgorithmVersion == SRPParameters.SRPVersion.SRP6)
                    return (BigInteger) 3;

                return Hash(Modulus, Generator);
            }
        }

        public BigInteger Credentials { get; set; }

        /// <summary>
        /// Salt for credentials hash. You can bind this to the users'
        /// account or use the automatically generated random salt.
        /// </summary>
        public BigInteger Salt
        {
            set { m_salt = value; }
            get
            {
                if (m_salt == null)
                {
                    if (IsServer == false)
                    {
                        throw new Exception("Unknown salt! This should be set by the server.");
                    }

                    m_salt = RandomNumber();
                }

                return m_salt;
            }
        }

        /// <summary>
        /// 'x' in the spec. Note that this is slightly different - the spec says
        /// x = H(s, p) whereas here x = H(s, H(I, p)), which is the implementation in the demo.
        /// </summary>
        public BigInteger CredentialsHash
        {
            get
            {
                if (m_credentialsHash == null)
                {
                    m_credentialsHash = Hash(Salt, Credentials);
                }

                return m_credentialsHash;
            }
        }

        /// <summary>
        /// 'A' in the spec. A = g^a, generated by client and sent to the server
        /// </summary>
        public BigInteger PublicEphemeralValueA
        {
            get
            {
                // This guy is generated by the client and sent to the server
                if (IsServer == false && m_publicEphemeralValueA == null)
                {
                    m_publicEphemeralValueA = Generator.ModPow(m_secretEphemeralValueA, Modulus);
                }

                return m_publicEphemeralValueA;
            }
            set
            {
                if (IsServer == false)
                {
                    throw new Exception("Attempt by SRP client to set A. This is generated.");
                }

                m_publicEphemeralValueA = value;

                m_publicEphemeralValueA %= Modulus;

                if (m_publicEphemeralValueA < 0)
                {
                    m_publicEphemeralValueA += Modulus;
                }

                if (m_publicEphemeralValueA == 0)
                {
                    // Abort - we got hackers.
                    throw new InvalidDataException("A cannot be 0 mod N!");
                }
            }
        }

        /// <summary>
        /// 'B' in the spec. B = kv + g^b, generated by the server and sent to the client
        /// </summary>
        public BigInteger PublicEphemeralValueB
        {
            get
            {
                // This guy is generated by the server and sent to the client
                if (IsServer && m_publicEphemeralValueB == null)
                {
                    m_secretEphemeralValueB = RandomNumber();
                    m_publicEphemeralValueB = Multiplier*Verifier + Generator.ModPow(m_secretEphemeralValueB, Modulus);
                    m_publicEphemeralValueB %= Modulus;

                    if (m_publicEphemeralValueB < 0)
                    {
                        m_publicEphemeralValueB += Modulus;
                    }
                }

                return m_publicEphemeralValueB;
            }
            set
            {
                if (IsServer)
                {
                    throw new Exception("Attempt by SRP server to set B. This is generated.");
                }

                m_publicEphemeralValueB = value;
                m_publicEphemeralValueB %= Modulus;
                if (m_publicEphemeralValueB < 0)
                    m_publicEphemeralValueB += Modulus;

                if (m_publicEphemeralValueB == 0)
                {
                    // Abort - we got hackers.
                    throw new InvalidDataException("B cannot be 0 mod N!");
                }
            }
        }

        /// <summary>
        /// u in the spec. Generated by both server and client.
        /// </summary>
        public BigInteger ScramblingParameter
        {
            get { return Hash(PublicEphemeralValueA, PublicEphemeralValueB); }
        }

        /// <summary>
        /// This is the session key used for encryption later.
        /// 'K' in the spec. Note that this is different to 'k' (Multiplier)
        /// </summary>
        public BigInteger SessionKeyRaw
        {
            get
            {
                if (m_sessionKey == null)
                {
                    BigInteger S;
                    if (IsServer)
                    {
                        if (m_publicEphemeralValueA == null)
                            return null;

                        // S = (Av^u) ^ b (mod N)
                        S = Verifier.ModPow(ScramblingParameter, Modulus);
                        S = (S*PublicEphemeralValueA)%Modulus;
                        S = S.ModPow(m_secretEphemeralValueB, Modulus);
                    }
                    else
                    {
                        // S = (B - kg^x) ^ (a + ux) (mod N)
                        S = PublicEphemeralValueB - (Multiplier*Generator.ModPow(CredentialsHash, Modulus));
                        S = S.ModPow(m_secretEphemeralValueA + ScramblingParameter*CredentialsHash, Modulus);

                        // If S flips negative above the mod function preserves its sign.
                        // We want it positive.
                        if (S < 0)
                            S += Modulus;
                    }

                    m_sessionKey = S;
                }

                return m_sessionKey;
            }
        }

        /// <summary>
        /// V in the spec.
        /// v = g^x (mod N)
        /// 
        /// This only makes sense for servers.
        /// </summary>
        public BigInteger Verifier
        {
            get
            {
                if (verifier == null)
                {
                    verifier = Generator.ModPow(CredentialsHash, Modulus);
                }

                if (verifier < 0)
                {
                    verifier += Modulus;
                }

                return verifier;
            }
            set { verifier = value; }
        }

        public BigInteger SessionKey
        {
            get
            {
                byte[] data = SessionKeyRaw.GetBytes(32);

                // This is a strange beast.
                var temp = new byte[16];
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i] = data[2*i];
                }

                byte[] hash1 = Hash(temp).GetBytes(20);

                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i] = data[2*i + 1];
                }

                byte[] hash2 = Hash(temp).GetBytes(20);

                // its an interleaving of the two hashes..
                data = new byte[40];

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = i%2 == 0 ? hash1[i/2] : hash2[i/2];
                }

                return new BigInteger(data);
            }
        }

        public BigInteger Hash(params HashUtilities.HashDataBroker[] brokers)
        {
			return HashUtilities.HashToBigInteger(SRPParameters.Hash, brokers);
        }

        /// <summary>
        /// Generate a random number of a specified size
        /// </summary>
        /// <param name="size">Maximum size in bytes of the random number</param>
        /// <returns></returns>
        public static BigInteger RandomNumber(uint size)
        {
            var buffer = new byte[size];

            SRPParameters.RandomGenerator.GetBytes(buffer);

            // Must make sure the most significant byte is not zero
            if (buffer[0] == 0)
                buffer[0] = 1;

            return new BigInteger(buffer);
        }

        public bool IsClientProofValid(BigInteger client_proof)
        {
            return ClientSessionKeyProof == client_proof;
        }

        public bool IsServerProofValid(BigInteger server_proof)
        {
            return server_proof == ServerSessionKeyProof;
        }

        public string InternalsToString()
        {
            string type = IsServer ? "server" : "client";
            string str = string.Format("SRP {0} Internals:\n", type);

            str += string.Format("G      = {0}\n", Generator.ToHexString());
            str += string.Format("K      = {0}\n", Multiplier.ToHexString());
            str += string.Format("N      = {0}\n", Modulus.ToHexString());

            str += string.Format("I      = '{0}'\n", Credentials);
            str += string.Format("Hash(I)= {0}\n", Hash(Credentials).ToHexString());
            str += string.Format("X      = {0}\n", CredentialsHash.ToHexString());
            str += string.Format("V      = {0}\n", Verifier.ToHexString());
            if (m_salt != null)
                str += string.Format("Salt   = {0}\n", Salt.ToHexString());
            if (null != m_publicEphemeralValueA && null != m_publicEphemeralValueB)
            {
                str += string.Format("u      = {0}\n", ScramblingParameter.ToHexString());
                str += string.Format("h(A)   = {0}\n", Hash(PublicEphemeralValueA).ToHexString());
                str += string.Format("h(B)   = {0}\n", Hash(PublicEphemeralValueB.GetBytes()).ToHexString());
            }

            if (IsServer == false || PublicEphemeralValueA != null)
                str += string.Format("A      = {0}\n", PublicEphemeralValueA.ToHexString());
            if (IsServer || PublicEphemeralValueB != null)
            {
                str += string.Format("B      = {0}\n", PublicEphemeralValueB.ToHexString());
                BigInteger tmp = Multiplier*Generator.ModPow(CredentialsHash, Modulus);
                str += string.Format("kg^x   = {0}\n", tmp.ToHexString());
                tmp = PublicEphemeralValueB - tmp%Modulus;
                if (tmp < 0)
                    tmp += Modulus;
                str += string.Format("B-kg^x = {0}\n", tmp.ToHexString());
            }

            try
            {
                str += string.Format("S.key  = {0}\n", SessionKey.ToHexString());
            }
            catch
            {
                str += "S.key  = empty\n";
            }

            return str;
        }

        public static void Test()
        {
            var srpParams = new SRPParameters();

			BigInteger credentials = HashUtilities.HashToBigInteger(SRPParameters.Hash, "USER:PASSWORD");

            var server = new SecureRemotePassword("USER", credentials, true, SRPParameters.Defaults);
            var client = new SecureRemotePassword("USER", credentials, false, SRPParameters.Defaults);

            /* Typical communication works something like this:
             * 
             * client: I want to log in. Here is my username and here is my PublicEphemeralValueA.
             * server: Here is the Salt and here is my PublicEphemeralValueB.
             * 
             * Server looks up the username in the database and finds the associated password.
             * 
             * client: Here's proof I have the correct session key (hence correct password)
             *         (sends client.ClientSessionKeyProof)
             * server: Thats valid. Here's proof that *I* have the correct session key:
             *         (sends server.ServerSessionKeyProof)
             * 
             * client: Cheerio. *encrypts stuff using SessionKey*
             */
            Console.WriteLine("Client sending A = {0}", client.PublicEphemeralValueA.ToHexString());
            server.PublicEphemeralValueA = client.PublicEphemeralValueA;

            Console.WriteLine("Server sending salt = {0}", server.Salt.ToHexString());
            Console.WriteLine("Server sending B = {0}", server.PublicEphemeralValueB.ToHexString());
            client.Salt = server.Salt;
            client.PublicEphemeralValueB = server.PublicEphemeralValueB;

            /*
                Console.WriteLine("X = {0}", server.CredentialsHash.ToHexString());
                Console.WriteLine("a = {0}", client.secretEphemeralValueA.ToHexString());
                Console.WriteLine("b = {0}", server.secretEphemeralValueB.ToHexString());
                Console.WriteLine("v = {0}", server.Verifier.ToHexString());
                Console.WriteLine("U = {0}", server.ScramblingParameter.ToHexString());
                */

            // Note that session keys are never sent.
            Console.WriteLine("Server's session key = {0}", server.SessionKey.ToHexString());
            Console.WriteLine("Client's session key = {0}", client.SessionKey.ToHexString());

            // Are the session keys actually the same?
            Console.WriteLine("\nServer key == client key {0}", server.SessionKey == client.SessionKey);

            // This is how we can test it without sending actual session keys over the wire
            Console.WriteLine("Client proof valid: {0}", server.IsClientProofValid(client.ClientSessionKeyProof));
            Console.WriteLine("Server proof valid: {0}", client.IsServerProofValid(server.ServerSessionKeyProof));
        }

        #region Nested type: SRPParameters

        [Serializable]
        public class SRPParameters
        {
            #region SRPVersion enum

            /// <summary>
            /// Algorithm version. Consult specification for differences.
            /// </summary>
            public enum SRPVersion
            {
                SRP6,
                SRP6a,
            }

            #endregion

            /// <summary>
            /// Maximum length of crypto keys in bytes.
            /// </summary>
            /// <remarks>You might get unlucky and have much shorter keys - this should be checked and keys recalculated.</remarks>
            public const uint KeyLength = 32;

            /// <summary>
            /// Random number generator for this instance.
            /// </summary>
            public static RandomNumberGenerator RandomGenerator = new RNGCryptoServiceProvider();

            /// <summary>
            /// Version of this instance.
            /// </summary>
            public SRPVersion AlgorithmVersion = SRPVersion.SRP6;

            public bool CaseSensitive = false;

            /// <summary>
            /// Hashing function for the instance.
            /// </summary>
            /// <remarks>MD5 or other SHA hashes are usable, though SHA1 is more standard for SRP.</remarks>
            [NonSerialized] 
			public static readonly HashAlgorithm Hash = new SHA1Managed();

            /// <summary>
            /// 'g' in the spec. This number must be a generator in the finite field Modulus.
            /// </summary>
            private static readonly BigInteger s_generator = new BigInteger(7);

            private static readonly BigInteger s_modulus =
                new BigInteger("B79B3E2A87823CAB8F5EBFBF8EB10108535006298B5BADBD5B53E1895E644B89", 16);

            /// <summary>
            /// All operations are mod this number. It should be a large prime.
            /// </summary>
            /// <remarks>Referred to as 'N' in the spec.</remarks>
            /// <remarks>Defaults to 128 bits.</remarks>
            public BigInteger Modulus
            {
                get { return s_modulus; }
            }

            public BigInteger Generator
            {
                get { return s_generator; }
            }

            public static SRPParameters Defaults
            {
                get { return new SRPParameters(); }
            }
        }

        #endregion


		/// <summary>
		/// Generates a hash for an account's credentials (username:password) based on the SRP hashing method,
		/// </summary>
		/// <param name="username">the username</param>
		/// <param name="password">the password</param>
		/// <returns>a byte array of the resulting hash</returns>
		public static byte[] GenerateCredentialsHash(string username, string password)
		{
			// Account-names are always upper case
			var buf = SRPParameters.Hash.ComputeHash(
				Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username.ToUpper(), password.ToUpper())));

			if (buf.Length > 20)
			{
				// should never happen, since we're using SHA1, which always outputs a 20byte hash
			    throw new CryptographicException("SHA-1 hash too long - " + buf.Length + " bytes, should be 20!");
			}
			return buf;
		}
    }

    [GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
    public partial class SecureRemotePassword : object, ISerializable
    {
        public SecureRemotePassword(SerializationInfo info, StreamingContext context)
        {
            SerializationInfo = info;
        }

        public SerializationInfo SerializationInfo { get; set; }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if ((SerializationInfo == null))
            {
                return;
            }
            SerializationInfoEnumerator enumerator = SerializationInfo.GetEnumerator();
            for (
                ;
                enumerator.MoveNext();
                )
            {
                SerializationEntry entry = enumerator.Current;
                info.AddValue(entry.Name, entry.Value);
            }
        }

        #endregion
    }
}