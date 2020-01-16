using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Linq;

namespace DKIMCore
{
	internal class LocalTempSmtpWriterContentReader : IEmailMessageRawContentReader
	{
		public string GetRawContent(MailMessage message)
		{
			var smtp = new System.Net.Mail.SmtpClient();
			smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory;
			smtp.PickupDirectoryLocation = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dkimcontent");
			if (!System.IO.Directory.Exists(smtp.PickupDirectoryLocation))
			{
				System.IO.Directory.CreateDirectory(smtp.PickupDirectoryLocation);
			}

			var existingEmails = from file in System.IO.Directory.GetFiles(smtp.PickupDirectoryLocation, "*.eml")
								 select file;

			foreach (var file in existingEmails)
			{
				System.IO.File.Delete(file);
			}
			smtp.Send(message);

			var emailFile = System.IO.Directory.GetFiles(smtp.PickupDirectoryLocation, "*.eml")
									.First();

			var emailContent = System.IO.File.ReadAllText(emailFile);

			try
			{
				System.IO.File.Delete(emailFile);
			}
			catch { }

			return emailContent;
		}
	}
}
