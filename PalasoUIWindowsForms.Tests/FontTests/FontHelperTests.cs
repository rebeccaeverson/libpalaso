﻿using System;
using System.Drawing;
using NUnit.Framework;
using Palaso.UI.WindowsForms;

namespace PalasoUIWindowsForms.Tests.FontTests
{
	[TestFixture]
	public class FontHelperTests
	{

		[SetUp]
		public void SetUp()
		{
			// setup code goes here
		}

		[TearDown]
		public void TearDown()
		{
			// tear down code goes here
		}

		[Test]
		public void MakeFont_FontName_ValidFont()
		{
			Font sourceFont = SystemFonts.DefaultFont;
			Font returnFont = FontHelper.MakeFont(sourceFont.FontFamily.Name);
			Assert.AreEqual(sourceFont.FontFamily.Name, returnFont.FontFamily.Name);
		}

	}
}
