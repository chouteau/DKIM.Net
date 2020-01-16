using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using DKIMCore;

namespace DKIMCoreTests
{
	[TestClass]
	public class SendRealEmail
	{
		private IWebHost m_WebHost;

		[TestInitialize]
		public void Initialize()
		{
			m_WebHost = (IWebHost)AppDomain.CurrentDomain.GetData("WebHhost");
			DKIMService = m_WebHost.Services.GetRequiredService<DKIMCore.IDKIMService>();
			Configuration = m_WebHost.Services.GetRequiredService<IConfiguration>();
			EmailMessageRawContentReader = m_WebHost.Services.GetRequiredService<DKIMCore.IEmailMessageRawContentReader>();
		}

		protected IConfiguration Configuration { get; private set; }
		protected DKIMCore.IDKIMService DKIMService { get; private set; }
		internal DKIMCore.IEmailMessageRawContentReader EmailMessageRawContentReader { get; private set; }

		[TestMethod]
		public void Send_With_Body_File()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			var message = new System.Net.Mail.MailMessage();

			var encoding = Encoding.UTF8;
			message.BodyEncoding = encoding;
			message.SubjectEncoding = encoding;
			message.HeadersEncoding = encoding;
			message.SubjectEncoding = encoding;

			message.Subject = $"Subject with accents éà" + '\u2190';
			var bodyFile = System.IO.Path.Combine(System.Environment.CurrentDirectory, "body.html");
			var body = System.IO.File.ReadAllText(bodyFile);
			message.Body = body.Trim();
			message.IsBodyHtml = true;
			var fromSetting = Configuration.GetSection("TestSettings").GetValue<string>("From");
			var toSetting = Configuration.GetSection("TestSettings").GetValue<string>("To");
			var from = message.From = new System.Net.Mail.MailAddress(fromSetting, "From with accent éà", encoding);
			var to = new System.Net.Mail.MailAddress("jdmbpqzidedhpj@dkimvalidator.com", "testto");
			message.To.Add(to);
			var dkimDomain = Configuration.GetSection("SmtpSettings").GetValue<string>("DKIMDomain");
			message.Headers.Add("Message-Id", $"<{Guid.NewGuid()}@{dkimDomain}>");

			var content = EmailMessageRawContentReader.GetRawContent(message);

			DKIMService.Sign(message);

			content = EmailMessageRawContentReader.GetRawContent(message);

			var smtpClient = new System.Net.Mail.SmtpClient();
			smtpClient.Host = Configuration.GetSection("SmtpSettings").GetValue<string>("Host");
			smtpClient.Port = Configuration.GetSection("SmtpSettings").GetValue<int>("Port");
			smtpClient.Credentials = new System.Net.NetworkCredential()
			{
				Password = Configuration.GetSection("SmtpSettings").GetValue<string>("Password"),
				UserName = Configuration.GetSection("SmtpSettings").GetValue<string>("UserName"),
			};
			smtpClient.EnableSsl = Configuration.GetSection("SmtpSettings").GetValue<bool>("EnableSsl");

			smtpClient.Send(message);
		}

		[TestMethod]
		public void Send_With_Body()
		{
			var message = new System.Net.Mail.MailMessage();
			message.Body = string.Empty;
			message.BodyEncoding = System.Text.Encoding.UTF8;
			message.SubjectEncoding = System.Text.Encoding.UTF8;
			message.HeadersEncoding = System.Text.Encoding.UTF8;
			var subject = message.Subject = $"[test?] [dkim] subject é à {DateTime.Now:HH:mm:ss} ?";
			var body = new StringBuilder();
			body.AppendLine("<html>");
			body.AppendLine(string.Empty);
			body.AppendLine(string.Empty);
			body.AppendLine(string.Empty);
			body.AppendLine(string.Empty);
			body.AppendLine("dkim body");
			body.AppendLine(string.Empty);
			body.AppendLine("<a href=\"https://www.test.com\">https://www.test.com</a>");
			body.AppendLine("\t");
			body.AppendLine("</html>");
			body.AppendLine(string.Empty);
			body.AppendLine(string.Empty);
			message.Body = body.ToString();
			message.IsBodyHtml = true;
			var fromSetting = Configuration.GetSection("TestSettings").GetValue<string>("From");
			var toSetting = Configuration.GetSection("TestSettings").GetValue<string>("To");
			var from = message.From = new System.Net.Mail.MailAddress(fromSetting, "Accent éà");
			var to = new System.Net.Mail.MailAddress("jdmbpqzidedhpj@dkimvalidator.com", "dkimvalidator");
			message.To.Add(to);
			var dkimDomain = Configuration.GetSection("SmtpSettings").GetValue<string>("DKIMDomain");
			message.Headers.Add("Message-Id", $"<{Guid.NewGuid()}@{dkimDomain}>");

			DKIMService.Sign(message);

			var smtpClient = new System.Net.Mail.SmtpClient();
			smtpClient.Host = Configuration.GetSection("SmtpSettings").GetValue<string>("Host");
			smtpClient.Port = Configuration.GetSection("SmtpSettings").GetValue<int>("Port");
			smtpClient.Credentials = new System.Net.NetworkCredential()
			{
				Password = Configuration.GetSection("SmtpSettings").GetValue<string>("Password"),
				UserName = Configuration.GetSection("SmtpSettings").GetValue<string>("UserName"),
			};
			smtpClient.EnableSsl = Configuration.GetSection("SmtpSettings").GetValue<bool>("EnableSsl");

			smtpClient.Send(message);
		}

	}
}
