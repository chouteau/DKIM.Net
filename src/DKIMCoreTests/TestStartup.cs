using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

using DKIMCore;

namespace DKIMCoreTests
{
	public class TestStartup
	{
		public TestStartup()
		{
		}

		public void ConfigureServices(IServiceCollection services)
		{
			var outputPath = $"{AppDomain.CurrentDomain.GetData("DataDirectory")}";

			services.AddLogging(builder =>
			{
				builder.SetMinimumLevel(LogLevel.Debug);
				builder.AddConsole();
				builder.AddDebug();
			});

			var configuration = new ConfigurationBuilder()
								.SetBasePath(outputPath)
								.AddJsonFile("localconfig/appsettings.json",
									 optional: false,
									 reloadOnChange: true)
								.Build();

			services.AddSingleton<IConfiguration>(configuration);

			var sectionName = "SmtpSettings";
			var host = configuration.GetSection(sectionName).GetValue<string>("Host");
			var privateKeyFileName = configuration.GetSection(sectionName).GetValue<string>("DKIMPrivateKeyFileName");
			var privateKey = System.IO.File.ReadAllText(privateKeyFileName);
			var publicKeyFileName = configuration.GetSection(sectionName).GetValue<string>("DKIMPublicKeyFileName");
			var publicKey = System.IO.File.ReadAllText(publicKeyFileName);
			var dkimDomain = configuration.GetSection(sectionName).GetValue<string>("DKIMDomain");
			var dkimSelector = configuration.GetSection(sectionName).GetValue<string>("DKIMSelector");
			var dkimHeaders = configuration.GetSection(sectionName).GetValue<string>("DKIMHeaders");


			services.AddDkimService(config =>
			{
				config.Domain = dkimDomain;
				config.Selector = dkimSelector;
				config.HeaderKeyList = dkimHeaders.Split(",").ToList();
				config.PublicKey = publicKey;
				config.PrivateKey = privateKey;
				config.HeaderCanonalization = DkimCanonicalizationAlgorithm.Relaxed;
				config.BodyCanonalization = DkimCanonicalizationAlgorithm.Simple;
			});
		}

		public void Configure(IApplicationBuilder builder)
		{
		}
	}
}
