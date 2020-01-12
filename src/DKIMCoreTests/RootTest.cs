using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DKIMCoreTests
{
	[TestClass]
	public class RootTest
	{
		[AssemblyInitialize]
		public static void AssemblyInit(TestContext ctx)
		{
			AppDomain.CurrentDomain.SetData(
				"DataDirectory",
				System.IO.Path.Combine(ctx.TestDeploymentDir, string.Empty));

			AppDomain.CurrentDomain.SetData(
				"TestDir", ctx.TestDir);

			AppDomain.CurrentDomain.SetData("Test", true);

			var webHost = WebHost.CreateDefaultBuilder()
				.UseEnvironment("Test")
				.UseStartup<TestStartup>()
				.Build();

			AppDomain.CurrentDomain.SetData("WebHhost", webHost);

		}

	}
}
