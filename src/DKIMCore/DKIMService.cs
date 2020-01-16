using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace DKIMCore
{
	internal class DKIMService : IDKIMService
	{
		public const string SIGNATURE_KEY = "DKIM-Signature";

		public DKIMService(DKIMSettings dKIMSettings,
			IPrivateKeySigner signer,
			IEmailSigner dkimSigner,
			IEmailMessageRawContentReader emailMessageRawContentReader)
		{
			this.DKIMSettings = dKIMSettings;
			this.PrivateKeySigner = signer;
			this.DkimSigner = dkimSigner;
			this.RawContentReader = emailMessageRawContentReader;
		}

		protected DKIMSettings DKIMSettings { get; }
		protected IPrivateKeySigner PrivateKeySigner { get; }
		protected IEmailSigner DkimSigner { get; }
		protected IEmailMessageRawContentReader RawContentReader { get; }

		public void KeySign(MailMessage mailMessage)
		{
			throw new NotImplementedException();
		}

		public void Sign(System.Net.Mail.MailMessage message)
		{
			// get email content and generate initial signature
			var rawMessageText = RawContentReader.GetRawContent(message);
			var email = Parse(rawMessageText);
			email.OriginalBody = message.Body;
			email.BodyEncoding = message.BodyEncoding;
			email.HeaderEncoding = message.HeadersEncoding;
			var dkimHeaderValue = DkimSigner.GenerateDkimHeaderValue(email);

			// signature value get formatted so add dummy signature value then remove it
			message.Headers.Add(SIGNATURE_KEY, dkimHeaderValue.ToString());
			var dkimRawMessageText = RawContentReader.GetRawContent(message);
			var emailWithDkim = Parse(dkimRawMessageText);
			emailWithDkim.BodyEncoding = message.BodyEncoding;
			emailWithDkim.HeaderEncoding = email.HeaderEncoding;

			// sign email
			var signature = DkimSigner.GenerateSignature(emailWithDkim);

			// Add Signature to dkimvalue header
			dkimHeaderValue.Signature = signature;
			message.Headers.Set(SIGNATURE_KEY, dkimHeaderValue.ToString());
		}

		public byte[] GetPublicKey()
		{
			const string pemprivheader = "-----BEGIN PUBLIC KEY-----";
			const string pemprivfooter = "-----END PUBLIC KEY-----";

			var sb = new StringBuilder(DKIMSettings.PublicKey);
			sb.Replace(pemprivheader, "");  
			sb.Replace(pemprivfooter, "");

			string pvkstr = sb.ToString().Trim();
			byte[] result = null;
			try
			{
				result = Convert.FromBase64String(pvkstr);
			}
			catch (FormatException e)
			{       //if can't b64 decode, it must be an encrypted private key
				throw new FormatException("Not an public key", e);
			}

			return result;
		}

		public Email Parse(string emailRawContent)
		{
			var headers = new Dictionary<string, EmailHeader>(StringComparer.InvariantCultureIgnoreCase);
			using (var reader = new StringReader(emailRawContent))
			{

				string line;
				string lastKey = null;

				// process headers
				while ((line = reader.ReadLine()) != null)
				{
					if (line == string.Empty)
					{
						var body = reader.ReadToEnd();
						// body = body.Replace(System.Environment.NewLine, string.Empty);
						// end of headers
						return new Email()
						{ 
							Headers = headers, 
							RawBody = body, 
							Raw = emailRawContent
						};
					}

					// check for folded value
					if (lastKey != null && line.Length > 0 && line[0].IsWhiteSpace())
					{
						var header = headers[lastKey];
						header.FoldedValue = true;
						header.Value += System.Environment.NewLine + line;

						continue;
					}

					// parse key & value 
					int sep = line.IndexOf(':');

					if (sep == -1)
					{
						throw new FormatException("Expected seperator not found in line." + line);
					}

					var key = line.Substring(0, sep);
					var value = line.Substring(sep + 1);
					lastKey = key.Trim().ToLower();

					headers.Add(lastKey, new EmailHeader { Key = key, Value = value });
				}
			}

			// email must have no body
			return new Email 
			{ 
				Headers = headers, 
				RawBody = string.Empty, 
				Raw = emailRawContent 
			};
		}

		public string GetCanocalizedMessageRaw(string emailRawContent)
		{
			var parsedEmail = Parse(emailRawContent);
			var messageBuilder = new StringBuilder();
			foreach (var headerKey in DKIMSettings.HeaderKeyList)
			{
				messageBuilder.AppendLine(parsedEmail.Headers[headerKey].ToString());
			}
			var dkimSignatureForCheck = parsedEmail.Headers["DKIM-Signature"].ToString();
			var dkimHeaderValue = new DkimHeaderValue(dkimSignatureForCheck);
			dkimHeaderValue.Signature = null;

			// Truncated first param before 70 caracters;
			var dkimHeader = dkimHeaderValue.ToTruncatedValue();

			messageBuilder.AppendLine(dkimHeader);

			var headerToCheck = messageBuilder.ToString().Trim();
			return headerToCheck;
		}
	}
}
