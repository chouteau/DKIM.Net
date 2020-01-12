/*
 * DKIM.Net
 * 
 * Copyright (C) 2011 Damien McGivern, damien@mcgiv.com
 * 
 * 
 * 
 * */

using System;
using System.Text;

namespace DKIMCore
{
	public interface IEmailSigner
	{
        // string SignEmail(string text);
        DkimHeaderValue GenerateDkimHeaderValue(Email email);
        string GenerateSignature(Email email);
    }
}
