using System;
using System.Collections.Generic;
using System.Text;

namespace DKIMCore
{
	public interface IPrivateKeySigner
	{
		string Sign(byte[] data);

		byte[] Hash(byte[] data);
	}
}
