using System.Collections.Generic;
using System.Linq;
using SIL.Archiving.Generic;

namespace SIL.Archiving.CMDI
{
	public class CMDIContributor : ArchivingActor
	{
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

		/// <summary>Add a file for this contributor</summary>
		/// <param name="file"></param>
		public void AddFile(CMDIFile file)
		{
			Files.Add(file);
		}

		public IEnumerable<CMDIFile> MediaFiles
		{
			get { return Files.Cast<CMDIFile>().Where(file => file.IsMediaFile).ToList(); }
		}

		public IEnumerable<CMDIFile> WrittenResources
		{
			get { return Files.Cast<CMDIFile>().Where(file => file.IsWrittenResource).ToList(); }
		}
	}
}
