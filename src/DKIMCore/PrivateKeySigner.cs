using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DKIMCore
{
	internal class PrivateKeySigner : IPrivateKeySigner
	{
		public PrivateKeySigner(DKIMSettings dKIMSettings)
		{
			this.DKIMSettings = dKIMSettings;
		}

		protected DKIMSettings DKIMSettings { get; }

		public string Sign(byte[] data)
		{
			var kp = DKIMSettings.GetAsymmetricCipherKeyPair();

			var hashName = DKIMSettings.GetHashName();

			var sig = SignerUtilities.GetSigner(hashName);
			sig.Init(true, kp.Private);
			sig.BlockUpdate(data, 0, data.Length);
			var signature = sig.GenerateSignature();

			var result = Convert.ToBase64String(signature);
			return result;
		}

		public byte[] Hash(byte[] data)
		{
			var hash = new SHA256Managed();
			return hash.ComputeHash(data);
		}
	}
}
