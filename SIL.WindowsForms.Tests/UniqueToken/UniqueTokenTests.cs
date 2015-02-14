﻿using NUnit.Framework;

namespace SIL.WindowsForms.Tests.UniqueToken
{
	[TestFixture, RequiresSTA]
	class UniqueTokenTests
	{
		[Test]
		public void AcquireTokenQuietly_SucceedsWhenTokenNotCurrentlyHeld()
		{
			const string uniqueIdentifier = "abc";

			bool tokenAcquired = global::SIL.WindowsForms.UniqueToken.UniqueToken.AcquireTokenQuietly(uniqueIdentifier);
			Assert.IsTrue(tokenAcquired);

			tokenAcquired = global::SIL.WindowsForms.UniqueToken.UniqueToken.AcquireTokenQuietly(uniqueIdentifier);
			Assert.IsFalse(tokenAcquired);

			global::SIL.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();

			tokenAcquired = global::SIL.WindowsForms.UniqueToken.UniqueToken.AcquireTokenQuietly(uniqueIdentifier);
			Assert.IsTrue(tokenAcquired);

			global::SIL.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();
		}

		[Test]
		public void AcquireTokenQuietlyTest_AcquireWithDifferentIdentifierAfterRelease()
		{
			const string uniqueIdentifier = "abc";
			const string uniqueIdentifier2 = "def";

			bool tokenAcquired = global::SIL.WindowsForms.UniqueToken.UniqueToken.AcquireTokenQuietly(uniqueIdentifier);
			Assert.IsTrue(tokenAcquired);

			global::SIL.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();

			tokenAcquired = global::SIL.WindowsForms.UniqueToken.UniqueToken.AcquireTokenQuietly(uniqueIdentifier2);
			Assert.IsTrue(tokenAcquired);

			global::SIL.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();
		}

		[Test]
		public void AcquireTokenQuietlyTest_AcquireTwiceNotAllowed()
		{
			const string uniqueIdentifier = "abc";
			const string uniqueIdentifier2 = "def";

			bool tokenAcquired = global::SIL.WindowsForms.UniqueToken.UniqueToken.AcquireTokenQuietly(uniqueIdentifier);
			Assert.IsTrue(tokenAcquired);

			tokenAcquired = global::SIL.WindowsForms.UniqueToken.UniqueToken.AcquireTokenQuietly(uniqueIdentifier2);
			Assert.IsFalse(tokenAcquired);

			global::SIL.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();
		}

		[Test]
		public void ReleaseTokenTest_ReleaseBeforeAcquireDoesNotThrow()
		{
			Assert.DoesNotThrow(global::SIL.WindowsForms.UniqueToken.UniqueToken.ReleaseToken);
		}

		[Test]
		public void AcquireToken_SucceedsWhenTokenNotCurrentlyHeld()
		{
			const string uniqueIdentifier = "abc";

			// No wait so test doesn't take a while
			bool tokenAcquired = global::SIL.WindowsForms.UniqueToken.UniqueToken.AcquireToken(uniqueIdentifier, null, 0);
			Assert.IsTrue(tokenAcquired);

			tokenAcquired = global::SIL.WindowsForms.UniqueToken.UniqueToken.AcquireToken(uniqueIdentifier, null, 0);
			Assert.IsFalse(tokenAcquired);

			global::SIL.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();

			tokenAcquired = global::SIL.WindowsForms.UniqueToken.UniqueToken.AcquireToken(uniqueIdentifier, null, 0);
			Assert.IsTrue(tokenAcquired);

			global::SIL.WindowsForms.UniqueToken.UniqueToken.ReleaseToken();
		}
	}
}
