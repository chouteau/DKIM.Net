/*
 * DKIM.Net
 * 
 * Copyright (C) 2011 Damien McGivern, damien@mcgiv.com
 * 
 * 
 * 
 * */
using System;
using System.Diagnostics;
using System.Text;

namespace DKIMCore
{
	internal class DkimSigner : IEmailSigner
	{
		/// <summary>
		/// Header key used to add DKIM information to email.
		/// </summary>
		public DkimSigner(IPrivateKeySigner privateKeySigner,
				DKIMSettings settings,
				IDKIMCanonicalizer dkimCanonicalizer)
		{

			this.PrivateKeySigner = privateKeySigner;
            this.DKIMSettings = settings;
			this.DkimCanonicalizer = dkimCanonicalizer;
		}

		protected IPrivateKeySigner PrivateKeySigner { get; }
		protected DKIMSettings DKIMSettings { get; }
		protected IDKIMCanonicalizer DkimCanonicalizer { get; }

		/*
		 * see http://www.dkim.org/specs/rfc4871-dkimbase.html#dkim-sig-hdr
		 * 
		 * */
		public DkimHeaderValue GenerateDkimHeaderValue(Email email)
		{
			var result = new DkimHeaderValue(DKIMSettings);
			result.HashOfBody = SignBody(email);
			result.Signature = null;
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="email">The email to sign.</param>
		/// <returns></returns>
		public string GenerateSignature(Email email)
		{
		    var headers = DkimCanonicalizer.CanonicalizeHeaders(email.Headers, true);

			// assumes signature ends with "b="
			var headerBuffer = email.HeaderEncoding.GetBytes(headers);
			var headerSign = PrivateKeySigner.Sign(headerBuffer);
			return headerSign;
		}


		public string SignBody(Email email)
		{
			var cb = DkimCanonicalizer.CanonicalizeBody(email.RawBody);
			var canonalizedBodyBuffer = email.BodyEncoding.GetBytes(cb);
			var hash = PrivateKeySigner.Hash(canonalizedBodyBuffer);
			var result = Convert.ToBase64String(hash);
			return result;
		}
	}
}
