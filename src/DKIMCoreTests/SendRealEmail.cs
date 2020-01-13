using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
		}

		protected IConfiguration Configuration { get; private set; }
		protected DKIMCore.IDKIMService DKIMService { get; private set; }

		[TestMethod]
		public void Send_With_Body_File()
		{
			var message = new System.Net.Mail.MailMessage();
			message.Body = string.Empty;
			message.BodyEncoding = System.Text.Encoding.UTF8;
			message.SubjectEncoding = System.Text.Encoding.UTF8;
			message.HeadersEncoding = System.Text.Encoding.UTF8;
			var subject = message.Subject = $"test dkim subject {DateTime.Now:HH:mm:ss}";
			var bodyFile = System.IO.Path.Combine(System.Environment.CurrentDirectory, "body.html");
			var body = System.IO.File.ReadLines(bodyFile);
			var truncateBody = string.Join(System.Environment.NewLine, body.Take(40));
			message.Body = truncateBody;
			message.IsBodyHtml = true;
			var fromSetting = Configuration.GetSection("TestSettings").GetValue<string>("From");
			var toSetting = Configuration.GetSection("TestSettings").GetValue<string>("To");
			var from = message.From = new System.Net.Mail.MailAddress(fromSetting, "testfrom");
			var to = new System.Net.Mail.MailAddress(toSetting, "testto");
			// var to = new System.Net.Mail.MailAddress("jdmbpqzidedhpj@dkimvalidator.com", "testto");
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

		[TestMethod]
		public void Send_With_Body()
		{
			var message = new System.Net.Mail.MailMessage();
			message.Body = string.Empty;
			// message.BodyEncoding = System.Text.Encoding.UTF8;
			message.SubjectEncoding = System.Text.Encoding.UTF8;
			message.HeadersEncoding = System.Text.Encoding.UTF8;
			var subject = message.Subject = $"test dkim subject {DateTime.Now:HH:mm:ss}";
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
			var from = message.From = new System.Net.Mail.MailAddress(fromSetting, "testfrom");
			var to = new System.Net.Mail.MailAddress("jdmbpqzidedhpj@dkimvalidator.com", "testto");
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
