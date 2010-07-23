using System;
using System.Collections.Generic;
using System.Drawing;
using Palaso.Data;
using Palaso.DictionaryServices;
using Palaso.DictionaryServices.Model;
using Palaso.TestUtilities;
using NUnit.Framework;
using Palaso.WritingSystems;

namespace Palaso.DictionaryServices.Tests
{
	[TestFixture]
	public class LiftLexEntryRepositoryCachingTests
	{
		private TemporaryFolder _tempfolder;
		private LiftLexEntryRepository _repository;

		[SetUp]
		public void Setup()
		{
			_tempfolder = new TemporaryFolder();
			string persistedFilePath = _tempfolder.GetTemporaryFile();
			_repository = new LiftLexEntryRepository(persistedFilePath);
		}

		[TearDown]
		public void Teardown()
		{
			_repository.Dispose();
		}

		[Test]
		public void GetAllEntriesSortedByHeadWord_CreateItemAfterFirstCall_EntryIsReturnedAndSortedInResultSet()
		{
			LexEntry entryBeforeFirstQuery = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 1");

			_repository.GetAllEntriesSortedByHeadword(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			LexEntry entryAfterFirstQuery = _repository.CreateItem();

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByHeadword(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual(null, results[0]["Form"]);
			Assert.AreEqual("word 1", results[1]["Form"]);
		}

		private LexEntry CreateEntryWithLexicalFormBeforeFirstQuery(string writingSystem, string lexicalForm)
		{
			LexEntry entryBeforeFirstQuery = _repository.CreateItem();
			entryBeforeFirstQuery.LexicalForm.SetAlternative(writingSystem, lexicalForm);
			_repository.SaveItem(entryBeforeFirstQuery);
			return entryBeforeFirstQuery;
		}

		private static WritingSystemDefinition WritingSystemDefinitionForTest(string languageISO, Font font)
		{
			var retval = new WritingSystemDefinition();
			retval.ISO = languageISO;
			retval.DefaultFontName = font.Name;
			retval.DefaultFontSize = font.Size;
			return retval;
		}

		[Test]
		public void GetAllEntriesSortedByHeadWord_ModifyAndSaveAfterFirstCall_EntryIsModifiedAndSortedInResultSet()
		{
			LexEntry entryBeforeFirstQuery = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByHeadword(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			entryBeforeFirstQuery.LexicalForm.SetAlternative("de", "word 1");
			_repository.SaveItem(entryBeforeFirstQuery);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByHeadword(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("word 1", results[0]["Form"]);
		}

		[Test]
		public void GetAllEntriesSortedByHeadWord_ModifyAndSaveMultipleAfterFirstCall_EntriesModifiedAndSortedInResultSet()
		{
			List<LexEntry> entriesToModify = new List<LexEntry>();
			entriesToModify.Add(CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0"));
			entriesToModify.Add(CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 1"));

			_repository.GetAllEntriesSortedByHeadword(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			entriesToModify[0].LexicalForm["de"] = "word 3";
			entriesToModify[1].LexicalForm["de"] = "word 2";
			_repository.SaveItems(entriesToModify);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByHeadword(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual("word 2", results[0]["Form"]);
			Assert.AreEqual("word 3", results[1]["Form"]);

		}

		[Test]
		public void GetAllEntriesSortedByHeadWord_DeleteAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByHeadword(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			_repository.DeleteItem(entrytoBeDeleted);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByHeadword(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetAllEntriesSortedByHeadWord_DeleteByIdAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByHeadword(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			_repository.DeleteItem(_repository.GetId(entrytoBeDeleted));

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByHeadword(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetAllEntriesSortedByHeadWord_DeleteAllItemsAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByHeadword(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			_repository.DeleteAllItems();

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByHeadword(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetAllEntriesSortedByLexicalForm_CreateItemAfterFirstCall_EntryIsReturnedAndSortedInResultSet()
		{
			LexEntry entryBeforeFirstQuery = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 1");

			_repository.GetAllEntriesSortedByLexicalFormOrAlternative(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			LexEntry entryAfterFirstQuery = _repository.CreateItem();

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByLexicalFormOrAlternative(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual(null, results[0]["Form"]);
			Assert.AreEqual("word 1", results[1]["Form"]);
		}

		[Test]
		public void GetAllEntriesSortedByLexicalForm_ModifyAndSaveAfterFirstCall_EntryIsModifiedAndSortedInResultSet()
		{
			LexEntry entryBeforeFirstQuery = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByLexicalFormOrAlternative(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			entryBeforeFirstQuery.LexicalForm.SetAlternative("de", "word 1");
			_repository.SaveItem(entryBeforeFirstQuery);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByLexicalFormOrAlternative(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("word 1", results[0]["Form"]);
		}

		[Test]
		public void GetAllEntriesSortedByLexicalForm_ModifyAndSaveMultipleAfterFirstCall_EntriesModifiedAndSortedInResultSet()
		{
			List<LexEntry> entriesToModify = new List<LexEntry>();
			entriesToModify.Add(CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0"));
			entriesToModify.Add(CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 1"));

			_repository.GetAllEntriesSortedByLexicalFormOrAlternative(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			entriesToModify[0].LexicalForm["de"] = "word 3";
			entriesToModify[1].LexicalForm["de"] = "word 2";
			_repository.SaveItems(entriesToModify);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByLexicalFormOrAlternative(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual("word 2", results[0]["Form"]);
			Assert.AreEqual("word 3", results[1]["Form"]);

		}

		[Test]
		public void GetAllEntriesSortedByLexicalForm_DeleteAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByLexicalFormOrAlternative(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			_repository.DeleteItem(entrytoBeDeleted);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByLexicalFormOrAlternative(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetAllEntriesSortedByLexicalForm_DeleteByIdAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByLexicalFormOrAlternative(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			_repository.DeleteItem(_repository.GetId(entrytoBeDeleted));

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByLexicalFormOrAlternative(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetAllEntriesSortedByLexicalForm_DeleteAllItemsAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByLexicalFormOrAlternative(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			_repository.DeleteAllItems();

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByLexicalFormOrAlternative(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}



		[Test]
		public void NotifyThatLexEntryHasBeenUpdated_LexEntry_CachesAreUpdated()
		{
			LexEntry entryToUpdate = _repository.CreateItem();
			entryToUpdate.LexicalForm.SetAlternative("de", "word 0");
			_repository.SaveItem(entryToUpdate);
			CreateCaches();
			entryToUpdate.LexicalForm.SetAlternative("de", "word 1");
			_repository.NotifyThatLexEntryHasBeenUpdated(entryToUpdate);
			var writingSystemToMatch = WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont);
			ResultSet<LexEntry> headWordResults = _repository.GetAllEntriesSortedByHeadword(writingSystemToMatch);
			ResultSet<LexEntry> lexicalFormResults = _repository.GetAllEntriesSortedByLexicalFormOrAlternative(writingSystemToMatch);
			Assert.AreEqual("word 1", headWordResults[0]["Form"]);
			Assert.AreEqual("word 1", lexicalFormResults[0]["Form"]);
		}

		private void CreateCaches()
		{
			var writingSystemToMatch = WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont);
			_repository.GetAllEntriesSortedByHeadword(writingSystemToMatch);
			_repository.GetAllEntriesSortedByLexicalFormOrAlternative(writingSystemToMatch);
			//_repository.GetAllEntriesSortedByDefinitionOrGloss(writingSystemToMatch);
		}

		[Test]
		public void NotifyThatLexEntryHasBeenUpdated_Null_Throws()
		{
			Assert.Throws<ArgumentNullException>(() =>
				_repository.NotifyThatLexEntryHasBeenUpdated(null));
		}

		[Test]
		public void NotifyThatLexEntryHasBeenUpdated_LexEntryDoesNotExistInRepository_Throws()
		{
			var entryToUpdate = new LexEntry();
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				_repository.NotifyThatLexEntryHasBeenUpdated(entryToUpdate));
		}

		private LexEntry CreateEntryWithDefinitionBeforeFirstQuery(string writingSystem, string lexicalForm)
		{
			LexEntry entryBeforeFirstQuery = _repository.CreateItem();
			entryBeforeFirstQuery.Senses.Add(new LexSense());
			entryBeforeFirstQuery.Senses[0].Definition.SetAlternative(writingSystem, lexicalForm);
			_repository.SaveItem(entryBeforeFirstQuery);
			return entryBeforeFirstQuery;
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_CreateItemAfterFirstCall_EntryIsReturnedAndSortedInResultSet()
		{
			LexEntry entryBeforeFirstQuery = CreateEntryWithDefinitionBeforeFirstQuery("de", "word 1");

			_repository.GetAllEntriesSortedByDefinitionOrGloss(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			LexEntry entryAfterFirstQuery = CreateEntryWithDefinitionBeforeFirstQuery("de", "word 2");

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByDefinitionOrGloss(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual("word 1", results[0]["Form"]);
			Assert.AreEqual("word 2", results[1]["Form"]);
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_ModifyAndSaveAfterFirstCall_EntryIsModifiedAndSortedInResultSet()
		{
			LexEntry entryBeforeFirstQuery = CreateEntryWithDefinitionBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByDefinitionOrGloss(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			entryBeforeFirstQuery.Senses[0].Definition.SetAlternative("de", "word 1");
			_repository.SaveItem(entryBeforeFirstQuery);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByDefinitionOrGloss(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("word 1", results[0]["Form"]);
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_ModifyAndSaveMultipleAfterFirstCall_EntriesModifiedAndSortedInResultSet()
		{
			List<LexEntry> entriesToModify = new List<LexEntry>();
			entriesToModify.Add(CreateEntryWithDefinitionBeforeFirstQuery("de", "word 0"));
			entriesToModify.Add(CreateEntryWithDefinitionBeforeFirstQuery("de", "word 1"));

			_repository.GetAllEntriesSortedByDefinitionOrGloss(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			entriesToModify[0].Senses[0].Definition["de"] = "word 3";
			entriesToModify[1].Senses[0].Definition["de"] = "word 2";
			_repository.SaveItems(entriesToModify);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByDefinitionOrGloss(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual("word 2", results[0]["Form"]);
			Assert.AreEqual("word 3", results[1]["Form"]);
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DeleteAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithDefinitionBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByDefinitionOrGloss(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			_repository.DeleteItem(entrytoBeDeleted);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByDefinitionOrGloss(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DeleteByIdAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithDefinitionBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByDefinitionOrGloss(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			_repository.DeleteItem(_repository.GetId(entrytoBeDeleted));

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByDefinitionOrGloss(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DeleteAllItemsAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithDefinitionBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByDefinitionOrGloss(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));

			_repository.DeleteAllItems();

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByDefinitionOrGloss(WritingSystemDefinitionForTest("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}


	}
}