/*
 * DKIM.Net
 * 
 * Copyright (C) 2011 Damien McGivern, damien@mcgiv.com
 * 
 * 
 * 
 * */
using System;
using System.Collections.Generic;
using System.IO;


namespace DKIMCore
{

    /// <summary>
    /// Repersents an email message. Used during the signing process.
    /// </summary>
	public class Email
	{
        public Email()
        {
            Headers = new Dictionary<string, EmailHeader>();
        }

        public Dictionary<string, EmailHeader> Headers { get; set; }
		public string Body { get; set; }
		public string Raw { get; set; }
        public System.Text.Encoding HeaderEncoding { get; set; }
        public System.Text.Encoding BodyEncoding { get; set; }

    }
}
