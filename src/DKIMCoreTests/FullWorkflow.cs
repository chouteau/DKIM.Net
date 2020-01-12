using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace DKIMCoreTests.Tests
{
	[TestClass]
	public class FullWorkflow
	{
		private static IWebHost m_WebHost;

		[TestInitialize]
		public void Initialize()
		{
			m_WebHost = (IWebHost)AppDomain.CurrentDomain.GetData("WebHhost");
			DKIMService = m_WebHost.Services.GetRequiredService<DKIMCore.IDKIMService>();
			DKIMSettings = m_WebHost.Services.GetRequiredService<DKIMCore.DKIMSettings>();
			Configuration = m_WebHost.Services.GetRequiredService<IConfiguration>();
			PrivateKeySigner = m_WebHost.Services.GetRequiredService<DKIMCore.IPrivateKeySigner>();
			DkimCanonicalizer = m_WebHost.Services.GetRequiredService<DKIMCore.IDKIMCanonicalizer>();
			EmailSigner = m_WebHost.Services.GetRequiredService<DKIMCore.IEmailSigner>();
		}

		protected IConfiguration Configuration { get; private set; }
		protected DKIMCore.IDKIMService DKIMService { get; private set; }
		protected DKIMCore.DKIMSettings DKIMSettings { get; private set; }
		protected DKIMCore.IPrivateKeySigner PrivateKeySigner { get; private set; }
		protected DKIMCore.IDKIMCanonicalizer DkimCanonicalizer { get; private set; }
		protected DKIMCore.IEmailSigner EmailSigner { get; private set; }

		[TestMethod]
		public void Send_Email_And_Check_It()
		{
			var message = new System.Net.Mail.MailMessage();
			var subject = message.Subject = "test dkim subject";
			var body = message.Body = "test dkim body";
			var fromSetting = Configuration.GetSection("TestSettings").GetValue<string>("From");
			var toSetting = Configuration.GetSection("TestSettings").GetValue<string>("To");
			var from = message.From = new System.Net.Mail.MailAddress(fromSetting, "testfrom");
			var to = new System.Net.Mail.MailAddress(toSetting, "testto");
			message.To.Add(to);

			DKIMService.Sign(message);
			var originalContent = DKIMService.GetMailMessageRaw(message);

			var smtp = new System.Net.Mail.SmtpClient();
			smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory;
			smtp.PickupDirectoryLocation = $"{AppDomain.CurrentDomain.GetData("TestDir")}";

			var existingEmails = from file in System.IO.Directory.GetFiles(smtp.PickupDirectoryLocation, "*.eml")
								 select file;

			foreach (var file in existingEmails)
			{
				System.IO.File.Delete(file);
			}

			smtp.Send(message);

			var emailTestFile = System.IO.Directory.GetFiles(smtp.PickupDirectoryLocation, "*.eml")
									.First();

			var emailContent = System.IO.File.ReadAllText(emailTestFile);
			var parsedEmail = DKIMService.Parse(emailContent);
			var signatureHeader = parsedEmail.Headers["DKIM-Signature"].Value;
			var dkimHeaderValue = new DKIMCore.DkimHeaderValue(signatureHeader);
			var signature = message.HeadersEncoding.GetBytes(dkimHeaderValue.Signature);
			var canonicalized = DKIMService.GetCanocalizedMessageRaw(emailContent);

			var pubKey = DKIMService.GetPublicKey();
			var rsaKey = PublicKeyFactory.CreateKey(pubKey) as RsaKeyParameters;
			ISigner sig = SignerUtilities.GetSigner("SHA256WithRSAEncryption");
			sig.Init(false, rsaKey);
			var input = Encoding.UTF8.GetBytes(canonicalized);
			sig.BlockUpdate(input, 0, input.Length);

			Check.That(sig.VerifySignature(signature)).IsTrue();

		}

	}
}
