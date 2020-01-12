using System;
using System.Collections.Generic;
using System.Text;

namespace DKIMCore
{
	public class DkimHeaderValue  
	{
		internal const string VERSION = "v=";
		internal const string ALGORITHM = "a=";
		internal const string CANONICALIZATION = "c=";
		internal const string DOMAIN = "d=";
		internal const string HEADER_TO_BE_SIGNED = "h=";
		internal const string PUBLIC_KEY_LOCATION = "q=";
		internal const string SELECTOR = "s=";
		internal const string TIME_SENT = "t=";
		internal const string HASH_OF_BODY = "bh=";
		internal const string SIGNATURE = "b=";

		public DkimHeaderValue()
		{
			TimeSpan t = DateTime.Now.ToUniversalTime() -
			 DateTime.SpecifyKind(DateTime.Parse("00:00:00 January 1, 1970"), DateTimeKind.Utc);
			TimeSent = (int)t.TotalSeconds;
		}

		public DkimHeaderValue(DKIMSettings settings) : this()
		{
			Algorithm = settings.GetAlgorithmName();
			Canonialization = $"{settings.HeaderCanonalization}/{settings.BodyCanonalization}".ToLower();
			Domain = settings.Domain;
			HeaderToBySigned = string.Join(':', settings.HeaderKeyList);
			PublicKeyLocation = "dns/txt";
			Selector = settings.Selector;
		}

		public DkimHeaderValue(string header)
		{
			header = (header ?? string.Empty).Trim();
			Version = "1";
			Algorithm = GetValue(header,ALGORITHM);
			Canonialization = GetValue(header, CANONICALIZATION);
			Domain = GetValue(header, DOMAIN);
			HeaderToBySigned = GetValue(header, HEADER_TO_BE_SIGNED);
			PublicKeyLocation = "dns/txt";
			Selector = GetValue(header, SELECTOR);
			var timeToSent = GetValue(header, TIME_SENT);
			if (timeToSent != null)
			{
				TimeSent = Convert.ToInt64(timeToSent);
			}
			HashOfBody = GetValue(header, HASH_OF_BODY);
			Signature = GetSignatureValue(header, SIGNATURE);
		}

		public string Version { get; set; } = "1";
		public string Algorithm { get; set; }
		public string Canonialization { get; set; }
		public string Domain { get; set; }
		public string HeaderToBySigned { get; set; }
		public string PublicKeyLocation { get; set; }
		public string Selector { get; set; }
		public long TimeSent { get; set; }
		public string HashOfBody { get; set; }
		public string Signature { get; set; }

		private string GetValue(string input, string parameterName)
		{
			var match = System.Text.RegularExpressions.Regex.Match(input, $@"{parameterName}\s*(?<value>[^;]*);");
			if (match.Success)
			{
				return match.Groups["value"].Value;
			}
			return null;
		}

		private string GetSignatureValue(string input, string parameterName)
		{
			var match = System.Text.RegularExpressions.Regex.Match(input, $@"{parameterName}\s*(?<value>.*)$");
			if (match.Success)
			{
				return match.Groups["value"].Value;
			}
			return null;
		}


		public override string ToString()
		{
			var raw = new StringBuilder();
			raw.Append($"{VERSION}{Version};");
			raw.Append($"{ALGORITHM} {Algorithm};");
			raw.Append($"{CANONICALIZATION} {Canonialization};");
			raw.Append($"{DOMAIN} {Domain};");
			raw.Append($"{HEADER_TO_BE_SIGNED} {HeaderToBySigned};");
			raw.Append($"{PUBLIC_KEY_LOCATION} {PublicKeyLocation};");
			raw.Append($"{TIME_SENT} {TimeSent};");
			raw.Append($"{SELECTOR} {Selector};");
			raw.Append($"{HASH_OF_BODY} {HashOfBody};");
			raw.Append($"{SIGNATURE} {Signature}".Trim());
			var result = raw.ToString();
			return result;
		}

		public string ToTruncatedValue()
		{
			var result = new StringBuilder();
			var line = $"{DKIMService.SIGNATURE_KEY}: {this.ToString()}";
			var ident = string.Empty;
			while(true)
			{
				var trunc = line.Substring(0, Math.Min(line.Length, 70));
				string appendLine = null;
				int lastSpaceIndex = 0;
				if (line.Length < 70)
				{
					appendLine = $"{ident}{trunc}";
					result.Append(appendLine);
					break;
				}
				else
				{
					if (!trunc.EndsWith("="))
					{
						lastSpaceIndex = trunc.LastIndexOf(' ');
					}
					else
					{
						lastSpaceIndex = trunc.Length;
					}
					appendLine = $"{ident}{trunc.Substring(0, lastSpaceIndex)}\r";
				}
				result.Append(appendLine);
				line = line.Substring(lastSpaceIndex + 1);
				ident = " ";
			}
			var truncatedResult = result.ToString();
			return truncatedResult;
		}
	}
}
