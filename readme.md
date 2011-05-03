DKIM.Net
===========
DomainKeys Identified Mail for .Net Framework


known issues
------------
using simple canonicalization for headers. (http://www.dkim.org/specs/rfc4871-dkimbase.html#canonicalization)


example
------------


	var privateKey = PrivateKeySigner.Create(@"-----BEGIN RSA PRIVATE KEY-----
	MIICXAIBAAKBgQDcruApwJruvr9GHYMnUlkOevmczah961FxiQXu7JwHiepKGkVf
	9f8DvzSiMprrqoR14f4puAi5PAG+MBxkvbAMI/kCc57E8nEN4ZGxKPRtuhiY6bsP
	SpxI7LXgHqlt/yOFrJNdTjSwGpAlVfNMd3BnP2RSlHgj58ZSwYYhG15OmQIDAQAB
	AoGAKOVbc0FXNOEybxrvAC15hX5ynYVbMSKXsDOVXuaIL7l2Ko9dxd+/h4E6jshT
	/1JVQ8dCo5aQP0uIgydFS8H/SpiAqCEAPKUOCObcBH9O81yBSxUOcZCkv0y2Qsah
	qZE0f4lYTxGYWZYC2GMZXt+cMHX528xEEg7UIHaR4U/hG/UCQQD3o8HcZcWR13rY
	DcNLwZ+G1TW/OTtfz/5bd8GeGBELUs4TKBV8dQqdxkpkcmCOLwCfETcnlBvyH6OT
	LxnpAWDnAkEA5CIk9r5JBRqz9tdknTyHmSok1ENsBM1uQ88b2TyEdODebe/mCZ6H
	MJnp/qQEnZ+/UlkZJW/cI6QLFxqj6+NkfwJBAN34szRTJRztAPfYnk2xaMT7KAoX
	yti/H0ftuGd1OxHjT0RskZXVc4aoztXqkBHin7P1QnL/l20YXw73EUqCKdECQEvf
	HzQArQBJlTivAgNZMi+6WG1Pzwj52YYrxzNEcTj94HvnoTXpx0Req/fITNCvZE3a
	3AYbYbdow1d3wLSe97kCQGfHreVl7MWOEk/5y0NASxaolY4+aFoXLwkGe1qIN2Vu
	xsjBBm9osDHsFVIuggd4fYKj05IWA6jX4z1LiRnLvVc=
	-----END RSA PRIVATE KEY-----");
	
	var dkim = new DKIMSigner(
	privateKey,
	"mydomain.com",
	"dkim",
	new string[] { "From", "To", "Subject" }
	);


	var msg = new MailMessage();

	msg.From = new MailAddress("me@mydomain.com", "Joe Bloggs");
	msg.To.Add(new MailAddress("check-auth@verifier.port25.com", "Port25"));
	msg.Subject = "Testing DKIM.Net";
	msg.Body = "Hello World";

	msg = dkim.SignMessage(msg);

	new SmtpClient().Send(msg);
 
