using System;
using System.Collections.Generic;
using System.Text;

namespace DKIMCore
{
	internal interface IEmailMessageRawContentReader
	{
		string GetRawContent(System.Net.Mail.MailMessage message);
	}
}
