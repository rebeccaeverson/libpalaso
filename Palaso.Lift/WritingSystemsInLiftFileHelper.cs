﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Palaso.IO;
using Palaso.Reporting;
using Palaso.WritingSystems;
using Palaso.Xml;

namespace Palaso.Lift
{
	public class WritingSystemsInLiftFileHelper
	{
		private readonly string _liftFilePath;
		private readonly IWritingSystemRepository _writingSystemRepository;

		public WritingSystemsInLiftFileHelper(IWritingSystemRepository writingSystemRepository, string liftFilePath)
		{
			_writingSystemRepository = writingSystemRepository;
			_liftFilePath = liftFilePath;
		}

		public IEnumerable<string> WritingSystemsInUse
		{
			get
			{
				var uniqueIds = new List<string>();
				using (var reader = XmlReader.Create(_liftFilePath))
				{
					while (reader.Read())
					{
						if (reader.MoveToAttribute("lang"))
						{
							if (!uniqueIds.Contains(reader.Value))
							{
								uniqueIds.Add(reader.Value);
							}
						}
					}
				}
				return uniqueIds;
			}
		}

		public void ReplaceWritingSystemId(string oldId, string newId)
		{
			var fileToBeWrittenTo = new IO.TempFile();
			var reader = XmlReader.Create(_liftFilePath, Xml.CanonicalXmlSettings.CreateXmlReaderSettings());
			var writer = XmlWriter.Create(fileToBeWrittenTo.Path, Xml.CanonicalXmlSettings.CreateXmlWriterSettings());
			var liftCopier = new LiftCopyStateMachine(reader, writer);
			liftCopier.CopyLiftReplacingWritingSystems(oldId, newId);
			reader.Close();
			writer.Close();
			File.Delete(_liftFilePath);
			fileToBeWrittenTo.MoveTo(_liftFilePath);
		}

		public void CreateNonExistentWritingSystemsFoundInFile()
		{
			WritingSystemOrphanFinder.FindOrphans(WritingSystemsInUse, ReplaceWritingSystemId, _writingSystemRepository);
		}

		private class LiftCopyStateMachine
		{
			private class FormOrGlossBlockCopyStatemachine
			{
				private readonly XmlReader _reader;
				private readonly XmlWriter _writer;
				private States _state;
				List<string> _formValues = new List<string>();

				public FormOrGlossBlockCopyStatemachine(XmlReader reader, XmlWriter writer)
				{
					_reader = reader;
					_writer = writer;
				}

				public States State
				{
					get { return _state; }
				}

				public void CopyGlossReplacingWritingSystem(string oldId, string newId)
				{
					_state = States.FoundGlossElement;
					while (_state != States.FoundGlossEndElement)
					{
						//_writer.Flush();
						switch (_state)
						{
							case States.FoundGlossElement:
								{
									if (_reader.GetAttribute("lang") == oldId || _reader.GetAttribute("lang") == newId)
									{
										_writer.WriteStartElement(_reader.Name);
										_writer.WriteAttributeString("lang", newId);
										_state = States.LogValue;
									}
									else
									{
										_writer.WriteNodeShallow(_reader);
										_state = States.CopyToEndOfGloss;
									}
									break;
								}
							case States.LogValue:
								if (_reader.NodeType == XmlNodeType.Text)
								{
									if (_formValues.Contains(_reader.Value))
									{
										_writer.WriteValue("");
									}
									else
									{
										_formValues.Add(_reader.Value);
										_writer.WriteNodeShallow(_reader);
									}
									_state = States.CopyToEndOfGloss;
								}
								else if (_reader.NodeType == XmlNodeType.EndElement && _reader.Name == "gloss")
								{
									_writer.WriteNodeShallow(_reader);
									_state = States.FoundGlossEndElement;
									return;
								}
								else
								{
									_writer.WriteNodeShallow(_reader);
								}
								break;
							case States.CopyToEndOfGloss:
								if (_reader.NodeType == XmlNodeType.EndElement && _reader.Name == "gloss")
								{
									_writer.WriteNodeShallow(_reader);
									_state = States.FoundGlossEndElement;
									return;
								}
								else
								{
									_writer.WriteNodeShallow(_reader);
								}
								break;
						}
						//_writer.Flush();
						_reader.Read();
					}
				}

				public void CopyFormReplacingWritingSystems(string oldId, string newId)
				{
					_state = States.FoundFormElement;
					var liftCopier = new LiftCopyStateMachine(_reader, _writer);


					while (_state != States.EndOfFormBlock)
					{
						switch (_state)
						{
							case States.FoundFormElement:
								if (_reader.GetAttribute("lang") == oldId || _reader.GetAttribute("lang") == newId)
								{
									_writer.WriteStartElement(_reader.Name);
									_writer.WriteAttributeString("lang", newId);
									_state = States.LogValue;
								}
								else
								{
									_writer.WriteNodeShallow(_reader);
									_state = States.SearchForFormOrGlossBlock;
									liftCopier = new LiftCopyStateMachine(_reader, _writer);
									liftCopier.CopyLiftReplacingWritingSystems(oldId, newId);
									_state = liftCopier.State;
									return;
								}
								break;
							case States.LogValue:
								if (_reader.NodeType == XmlNodeType.Text)
								{
									if (_formValues.Contains(_reader.Value))
									{
										_writer.WriteValue("");
									}
									else
									{
										_formValues.Add(_reader.Value);
										_writer.WriteNodeShallow(_reader);
									}
									_state = States.SearchForFormOrGlossBlock;
									liftCopier = new LiftCopyStateMachine(_reader, _writer);
									liftCopier.CopyLiftReplacingWritingSystems(oldId, newId);
									_state = liftCopier.State;
									return;
								}
								else if(_reader.NodeType == XmlNodeType.EndElement && _reader.Name == "text")
								{
									_writer.WriteNodeShallow(_reader);
									_state = States.SearchForFormOrGlossBlock;
									liftCopier = new LiftCopyStateMachine(_reader, _writer);
									liftCopier.CopyLiftReplacingWritingSystems(oldId, newId);
									_state = liftCopier.State;
									return;
								}
								else
								{
									_writer.WriteNodeShallow(_reader);
								}
								break;
						}
						//_writer.Flush();
						_reader.Read();
					}
				}
			}

			private readonly XmlReader _reader;
			private readonly XmlWriter _writer;

			public LiftCopyStateMachine(XmlReader reader, XmlWriter writer)
			{
				_reader = reader;
				_writer = writer;
			}

			protected enum States
			{
				SearchForFormOrGlossBlock,
				FoundFormElement,
				FoundFormEndElement,
				LogValue,
				EndOfFormBlock,
				FoundGlossElement,
				FoundGlossEndElement,
				CopyToEndOfGloss
			}

			private States _state = States.SearchForFormOrGlossBlock;

			protected States State
			{
				get { return _state; }
			}

			public void CopyLiftReplacingWritingSystems(string oldId, string newId)
			{
				var formBlockCopier = new FormOrGlossBlockCopyStatemachine(_reader, _writer);
				while(_reader.Read())
				{
					//_writer.Flush();
					switch(State)
					{
						case States.SearchForFormOrGlossBlock:
							//found form block, process it
							if (_reader.Name == "form" && _reader.NodeType == XmlNodeType.Element)
							{
								_state = States.FoundFormElement;
								formBlockCopier = new FormOrGlossBlockCopyStatemachine(_reader, _writer);
								formBlockCopier.CopyFormReplacingWritingSystems(oldId, newId);
								_state = formBlockCopier.State;
							}
							//couldn't find new form block inside current form block
							else if (_reader.Name == "form" && _reader.NodeType == XmlNodeType.EndElement)
							{
								_writer.WriteNodeShallow(_reader);
								_state = States.FoundFormEndElement;
								return;
							}
							//found gloss block, process it
							else if (_reader.Name == "gloss" && _reader.NodeType == XmlNodeType.Element)
							{
								_state = States.FoundGlossElement;
								formBlockCopier = new FormOrGlossBlockCopyStatemachine(_reader, _writer);
								formBlockCopier.CopyGlossReplacingWritingSystem(oldId, newId);
								_state = formBlockCopier.State;
							}
							//copy anything else
							else
							{
								_writer.WriteNodeShallow(_reader);
							}
							break;
						case States.FoundFormEndElement:
							if (_reader.Name == "form" && _reader.NodeType == XmlNodeType.Element)
							{
								_state = States.FoundFormElement;
								formBlockCopier.CopyFormReplacingWritingSystems(oldId, newId);
								_state = formBlockCopier.State;
							}
							else
							{
								_writer.WriteNodeShallow(_reader);
								_state = States.SearchForFormOrGlossBlock;
							}
							break;
						case States.FoundGlossEndElement:
							if (_reader.Name == "gloss" && _reader.NodeType == XmlNodeType.Element)
							{
								_state = States.FoundGlossElement;
								formBlockCopier.CopyGlossReplacingWritingSystem(oldId, newId);
								_state = formBlockCopier.State;
							}
							else
							{
								_writer.WriteNodeShallow(_reader);
								_state = States.SearchForFormOrGlossBlock;
							}
							break;
					}
				}
			}
		}
	}
}