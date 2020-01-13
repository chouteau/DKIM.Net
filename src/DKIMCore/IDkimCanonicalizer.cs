/*
 * DKIM.Net
 * 
 * Copyright (C) 2011 Damien McGivern, damien@mcgiv.com
 * 
 * 
 * 
 * */
using System.Collections.Generic;

namespace DKIMCore
{
    internal interface IDKIMCanonicalizer
	{
		string CanonicalizeBody(string body);
		string CanonicalizeHeaders(Dictionary<string, EmailHeader> headers, bool includeSignatureHeader);
	}
}