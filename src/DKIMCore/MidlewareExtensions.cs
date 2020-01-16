using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DKIMCoreTests", AllInternalsVisible = true)]

namespace DKIMCore
{
	public static class MidlewareExtensions
	{
		public static IServiceCollection AddDkimService(this IServiceCollection services, Action<DKIMSettings> config)
		{
			var settings = new DKIMSettings();
			config.Invoke(settings);
			services.AddSingleton<DKIMSettings>(settings);

			services.AddTransient<IEmailSigner, DkimSigner>();
			services.AddTransient<IPrivateKeySigner, PrivateKeySigner>();
			services.AddTransient<IDKIMService, DKIMService>();
			services.AddTransient<IDKIMCanonicalizer, DKIMCanonicalizer>();
			services.AddSingleton<IEmailMessageRawContentReader, LocalTempSmtpWriterContentReader>();
			return services;
		}
	}
}
