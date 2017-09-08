
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.Archiving.Generic;
using SIL.Archiving.CMDI.Lists;
using SIL.Archiving.CMDI.Schema;
using SIL.Extensions;

namespace SIL.Archiving.CMDI
{
	/// <summary>Collects the data and produces an CMDI corpus to upload</summary>
	public class CMDIPackage : ArchivingPackage
	{
		/// <summary></summary>
		public MetaTranscript BaseCmdiFile { get; private set; }

		/// <summary>The file to import into Arbil</summary>
		public string MainExportFile { get; private set; }

		private readonly bool _corpus;
		private bool _creationStarted;
		private string _packagePath;

		/// ------------------------------------------------------------------------------------
		/// <summary>Constructor</summary>
		/// <param name="corpus">Indicates whether this is for an entire project corpus or a
		/// single session</param>
		/// <param name="packagePath"></param>
		/// ------------------------------------------------------------------------------------
		public CMDIPackage(bool corpus, string packagePath)
		{
			_corpus = corpus;
			PackagePath = packagePath;

			BaseCmdiFile = new MetaTranscript(MetatranscriptValueType.CORPUS);

			Sessions = new List<IArchivingSession>();
		}

#region Properties
		private ICMDIMajorObject BaseMajorObject
		{
			get { return (ICMDIMajorObject)BaseCmdiFile.Items[0]; }
		}

		/// <summary>The path where the corpus cmdi file and corpus directory will be created</summary>
		public string PackagePath
		{
			get { return _packagePath; }
			set
			{
				if (_creationStarted)
					throw new InvalidOperationException("Cannot change package path after package creation has already begun.");
				_packagePath = value;
			}
		}
		#endregion

		// **** Corpus Layout ****
		//
		// Test_Corpus (directory)
		// Test_Corpus.cmdi (corpus meta data file)
		// Test_Corpus\Test_Corpus_Catalog.cmdi (catalogue of information)
		// Test_Corpus\Test_Session (directory)
		// Test_Corpus\Test_Session.cmdi (session meta data file)
		// Test_Corpus\Test_Session\Contributors (directory - contains files pertaining to contributers/actors)
		// Test_Corpus\Test_Session\Files*.* (session files)
		// Test_Corpus\Contributors\Files*.* (contributor/actor files)

		/// <summary>Creates the corpus directory structure, meta data files, and copies content files</summary>
		/// <returns></returns>
		public bool CreateCMDIPackage()
		{
			_creationStarted = true;

			// list of session files for the corpus
			List<string> sessionFiles = new List<string>();

			// create the session directories
// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var session in Sessions)
			{
				var sessCmdi = new MetaTranscript { Items = new object[] { session }, Type = MetatranscriptValueType.SESSION };
				sessionFiles.Add(sessCmdi.WriteCmdiFile(PackagePath, Name));
			}

			if (!_corpus)
			{
				MainExportFile = Path.Combine(PackagePath, sessionFiles[0]);
				return true;
			}

			var corpus = (Corpus) BaseMajorObject;

			// add the session file links
			foreach (var fileName in sessionFiles)
				corpus.CorpusLink.Add(new CorpusLinkType { Value = fileName.Replace("\\", "/"), Name = string.Empty });

			// crate the catalogue
			corpus.CatalogueLink = CreateCorpusCatalogue();

			//  Create the corpus cmdi file
			MainExportFile = BaseCmdiFile.WriteCmdiFile(PackagePath, Name);

			return true;
		}

		private string CreateCorpusCatalogue()
		{
			// Create the package catalogue cmdi file
			var catalogue = new Catalogue
			{
				Name = Name + " Catalogue",
				Title = Title,
				Date = DateTime.Today.ToISO8601TimeFormatDateOnlyString(),
			};

			foreach (var language in MetadataIso3Languages)
			{
				var cmdiLanguage = LanguageList.Find(language).ToSimpleLanguageType();
				if (cmdiLanguage != null)
					catalogue.DocumentLanguages.Language.Add(cmdiLanguage);
			}


			foreach (var language in ContentIso3Languages)
			{
				var cmdiLanguage = LanguageList.Find(language).ToSubjectLanguageType();
				if (cmdiLanguage != null)
					catalogue.SubjectLanguages.Language.Add(cmdiLanguage);
			}

			// funding project
			if (FundingProject != null)
				catalogue.Project.Add(new Project(FundingProject));

			// location
			if (Location != null)
				catalogue.Location.Add(new LocationType(Location));

			// content type
			if (!string.IsNullOrEmpty(ContentType))
				catalogue.ContentType.Add(ContentType);

			// applications
			if (!string.IsNullOrEmpty(Applications))
				catalogue.Applications = Applications;

			// author
			if (!string.IsNullOrEmpty(Author))
				catalogue.Author.Add(new CommaSeparatedStringType { Value = Author });

			// publisher
			if (!string.IsNullOrEmpty(Publisher))
				catalogue.Publisher.Add(Publisher);

			// keys
			foreach (var kvp in _keys)
				catalogue.Keys.Key.Add(new KeyType { Name = kvp.Key, Value = kvp.Value });

			// access
			if (!string.IsNullOrEmpty(Access.DateAvailable))
				catalogue.Access.Date = Access.DateAvailable;

			if (!string.IsNullOrEmpty(Access.Owner))
				catalogue.Access.Owner = Access.Owner;

			// write the xml file
			var catCmdi = new MetaTranscript { Items = new object[] { catalogue }, Type = MetatranscriptValueType.CATALOGUE };
			return catCmdi.WriteCmdiFile(PackagePath, Name).Replace("\\", "/");
		}

		/// <summary>Add a description of the package/corpus</summary>
		/// <param name="description"></param>
		public new void AddDescription(LanguageString description)
		{
			// prevent duplicate description
			foreach (var itm in BaseMajorObject.Description)
			{
				if (itm.LanguageId == description.Iso3LanguageId)
					throw new InvalidOperationException(string.Format("A description for language {0} has already been set", itm.LanguageId));
			}

			BaseMajorObject.Description.Add(description);
		}

		/// <summary>Add a description of the package/corpus</summary>
		/// <param name="sessionId"></param>
		/// <param name="description"></param>
		public void AddDescription(string sessionId, LanguageString description)
		{
			// prevent duplicate description
			if (_corpus)
			{
				foreach (var sess in Sessions.Where(sess => sess.Name == sessionId))
				{
					sess.AddDescription(description);
				}
			}
			else
			{
				if (BaseMajorObject is Session)
				{
					if (Name == sessionId)
						AddDescription(description);
				}

			}
		}

		/// <summary></summary>
		public bool SetMissingInformation()
		{
			if (string.IsNullOrEmpty(BaseMajorObject.Name))
				BaseMajorObject.Name = Name;

			if (string.IsNullOrEmpty(BaseMajorObject.Title))
				BaseMajorObject.Title = Title;

			return true;
		}
	}
}
