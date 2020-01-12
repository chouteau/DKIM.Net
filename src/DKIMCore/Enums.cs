using System;
using System.Collections.Generic;
using System.Text;

namespace DKIMCore
{
	public enum DkimCanonicalizationAlgorithm
	{
		Simple,
		Relaxed
	}

	public enum DomainKeyCanonicalizationAlgorithm
	{
		Simple,

		/// <summary>
		/// No Folding White Space
		/// </summary>
		Nofws
	}

	/// <summary>
	/// The algorithms supported
	/// </summary>
	// ReSharper disable InconsistentNaming
	public enum SigningAlgorithm
	{
		/// <summary>
		/// Supported by DKIM and DomainKeys
		/// </summary>
		RSASha1,


		/// <summary>
		/// Supported by DKIM
		/// </summary>
		RSASha256

	}
}
