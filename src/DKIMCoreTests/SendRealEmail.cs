using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

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
		public void Send()
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
