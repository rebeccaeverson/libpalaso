using System;
using NUnit.Framework;
using Palaso.CommandLineProcessing;
using Palaso.Progress;


namespace Palaso.Tests.Progress
{
	[TestFixture]
	public class CommandLineRunnerTests
	{

		[Test]
		public void CommandWith10Line_NoCallbackOption_Get10LinesSynchronously()
		{
			var app = "PalasoUIWindowsForms.TestApp.exe";// FileLocator.GetFileDistributedWithApplication("PalasoUIWindowsForms.TestApp.exe");
			var progress = new StringBuilderProgress();
			int linesReceivedAsynchronously = 0;
			var result = CommandLineRunner.Run(app, "CommandLineRunnerTest", null, string.Empty, 100, progress, null);
			Assert.IsTrue(result.StandardOutput.Contains("0"));
			Assert.IsTrue(result.StandardOutput.Contains("9"));
		}

		[Test]
		public void CommandWith10Line_CallbackOption_Get10LinesAsynchronously()
		{
			var app = "PalasoUIWindowsForms.TestApp.exe";// FileLocator.GetFileDistributedWithApplication("PalasoUIWindowsForms.TestApp.exe");
			var progress = new StringBuilderProgress();
			int linesReceivedAsynchronously = 0;
			CommandLineRunner.Run(app, "CommandLineRunnerTest", null, string.Empty, 100, progress, s => { ++linesReceivedAsynchronously; });
			Assert.AreEqual(10, linesReceivedAsynchronously);
		}
	}
}