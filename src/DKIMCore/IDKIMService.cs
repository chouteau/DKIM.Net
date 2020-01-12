using System;
using System.Collections.Generic;
using System.Text;

namespace DKIMCore
{
	public interface IDKIMService
	{
		void Sign(System.Net.Mail.MailMessage mailMessage);
		void KeySign(System.Net.Mail.MailMessage mailMessage);
		Email Parse(string fullEmailMessageContent);
		byte[] GetPublicKey();
		string GetMailMessageRaw(System.Net.Mail.MailMessage message);
		string GetCanocalizedMessageRaw(string emailRawContent);
	}
}
