using System;
using System.Collections.Generic;
using System.Text;

namespace DKIMCore
{
	public interface IDKIMService
	{
		void Sign(System.Net.Mail.MailMessage mailMessage);
	}
}
