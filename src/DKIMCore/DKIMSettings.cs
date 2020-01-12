using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DKIMCore
{
	public class DKIMSettings
	{
		private AsymmetricCipherKeyPair m_AsymmetricCipherKeyPair;
		private byte[] m_PrivateKeyBuffer;

		public DKIMSettings()
		{
			SigningAlgorithm = SigningAlgorithm.RSASha256;
			HeaderCanonalization = DkimCanonicalizationAlgorithm.Relaxed;
			BodyCanonalization = DkimCanonicalizationAlgorithm.Relaxed;
			// Encoding = Encoding.UTF8;
			HeaderKeyList = new List<string>()
			{
				"From",
				"To",
				"Subject"
			};
		}
		public string PrivateKey { get; set; }
		public string PublicKey { get; set; }
		public string Selector { get; set; }
		public string Domain { get; set; }
		public IList<string> HeaderKeyList { get; set; }
		public SigningAlgorithm SigningAlgorithm { get; set; }
		public DkimCanonicalizationAlgorithm HeaderCanonalization { get; set; }
		public DkimCanonicalizationAlgorithm BodyCanonalization { get; set; }
		// public Encoding Encoding { get; set; }

		internal string GetAlgorithmName()
		{
			switch (this.SigningAlgorithm)
			{
				case SigningAlgorithm.RSASha1:
					{
						return "rsa-sha1";
					}
				case SigningAlgorithm.RSASha256:
					{
						return "rsa-sha256";
					}
				default:
					{
						throw new InvalidOperationException("Invalid SigningAlgorithm");
					}
			}
		}
		internal string GetHashName()
		{
			switch (SigningAlgorithm)
			{
				case SigningAlgorithm.RSASha1:
					{
						return "SHA1WithRSAEncryption";
					}
				case SigningAlgorithm.RSASha256:
					{
						return "SHA256WithRSAEncryption";
					}
				default:
					{
						throw new ArgumentException("Invalid SigningAlgorithm value", "algorithm");
					}
			}
		}

		internal byte[] GetBase64PrivateKeyBuffer()
		{
			if (m_PrivateKeyBuffer != null)
			{
				return m_PrivateKeyBuffer;
			}
			var privateKey = PrivateKey
					.Replace("-----BEGIN RSA PRIVATE KEY-----", string.Empty)
					.Replace("-----END RSA PRIVATE KEY-----", string.Empty)
					.Trim();

			m_PrivateKeyBuffer = Convert.FromBase64String(privateKey);
			return m_PrivateKeyBuffer;
		}

		internal AsymmetricCipherKeyPair GetAsymmetricCipherKeyPair()
		{
			if (m_AsymmetricCipherKeyPair != null)
			{
				return m_AsymmetricCipherKeyPair;
			}
			var buffer = GetBase64PrivateKeyBuffer();
			var reader = new StringReader(PrivateKey);
			var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(reader);
			m_AsymmetricCipherKeyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
			return m_AsymmetricCipherKeyPair;
		}
	}
}
