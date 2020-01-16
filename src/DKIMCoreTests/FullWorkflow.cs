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
			DKIMService = (DKIMCore.DKIMService) m_WebHost.Services.GetRequiredService<DKIMCore.IDKIMService>();
			DKIMSettings = m_WebHost.Services.GetRequiredService<DKIMCore.DKIMSettings>();
			Configuration = m_WebHost.Services.GetRequiredService<IConfiguration>();
			PrivateKeySigner = m_WebHost.Services.GetRequiredService<DKIMCore.IPrivateKeySigner>();
			EmailSigner = (DKIMCore.DkimSigner) m_WebHost.Services.GetRequiredService<DKIMCore.IEmailSigner>();
			EmailMessageRawContentReader = m_WebHost.Services.GetRequiredService<DKIMCore.IEmailMessageRawContentReader>();
		}

		protected IConfiguration Configuration { get; private set; }
		internal DKIMCore.DKIMService DKIMService { get; private set; }
		protected DKIMCore.DKIMSettings DKIMSettings { get; private set; }
		protected DKIMCore.IPrivateKeySigner PrivateKeySigner { get; private set; }
		internal DKIMCore.DkimSigner EmailSigner { get; private set; }
		internal DKIMCore.IEmailMessageRawContentReader EmailMessageRawContentReader { get; private set; }

		[TestMethod]
		public void Canonicanize_Body()
		{
			var message = new System.Net.Mail.MailMessage();
			var subject = message.Subject = "test dkim subject";
			var body = new StringBuilder();
			body.AppendLine("<html>");
			body.AppendLine(string.Empty);
			body.AppendLine(string.Empty);
			body.AppendLine(string.Empty);
			body.AppendLine(string.Empty);
			body.AppendLine("dkim body");
			body.AppendLine(string.Empty);
			body.AppendLine("</html>");
			body.AppendLine(string.Empty);
			body.AppendLine(string.Empty);
			message.Body = body.ToString();
			message.IsBodyHtml = true;
			var fromSetting = Configuration.GetSection("TestSettings").GetValue<string>("From");
			var toSetting = Configuration.GetSection("TestSettings").GetValue<string>("To");
			var from = message.From = new System.Net.Mail.MailAddress(fromSetting, "testfrom");
			var to = new System.Net.Mail.MailAddress(toSetting, "testto");
			message.To.Add(to);

			DKIMService.Sign(message);
			var originalContent = EmailMessageRawContentReader.GetRawContent(message);

		}

		[TestMethod]
		public void Send_Email_And_Check_It()
		{
			var message = new System.Net.Mail.MailMessage();
			var encoding = Encoding.UTF8;
			message.BodyEncoding = encoding;
			message.SubjectEncoding = encoding;
			message.HeadersEncoding = encoding;
			message.SubjectEncoding = encoding;

			var subject = message.Subject = "test dkim é subject";
			var body = new StringBuilder();
			body.AppendLine("<html>");
			body.AppendLine("	<body>");
			body.AppendLine("		Test <b>DKIM</b><br/>");
			for (int i = 0; i < 50; i++)
			{
				body.AppendLine($"		<span>big line big line big line big line big line big line big line big line big line big line big line big line big line big line{i}</span>");
			}
			body.AppendLine("	</body>");
			body.AppendLine("</html>");

			message.Body = body.ToString();
			message.IsBodyHtml = true;
			var fromSetting = Configuration.GetSection("TestSettings").GetValue<string>("From");
			var toSetting = Configuration.GetSection("TestSettings").GetValue<string>("To");
			var from = message.From = new System.Net.Mail.MailAddress(fromSetting, "test é from");
			var to = new System.Net.Mail.MailAddress(toSetting, "testto");
			message.To.Add(to);

			DKIMService.Sign(message);
			var originalContent = EmailMessageRawContentReader.GetRawContent(message);

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
			var generatedBuffer = sig.GenerateSignature();
			var generated = Convert.ToBase64String(generatedBuffer);

			Check.That(sig.VerifySignature(signature)).IsTrue();

		}

	}
}
