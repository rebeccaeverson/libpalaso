using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SIL.Code;

namespace SIL.WritingSystems
{
	/// <summary>
	/// This class forms the bases for managing collections of WritingSystemDefinitions. WritingSystemDefinitions
	/// can be registered and then retrieved and deleted by ID. The preferred use when editting a WritingSystemDefinition stored
	/// in the WritingSystemRepository is to Get the WritingSystemDefinition in question and then to clone it either via the
	/// Clone method on WritingSystemDefinition or via the MakeDuplicate method on the WritingSystemRepository. This allows
	/// changes made to a WritingSystemDefinition to be registered back with the WritingSystemRepository via the Set method,
	/// or to be discarded by simply discarding the object.
	/// Internally the WritingSystemRepository uses the WritingSystemDefinition's StoreId property to establish the identity of
	/// a WritingSystemDefinition. This allows the user to change the IETF language tag components and thereby the ID of a
	/// WritingSystemDefinition and the WritingSystemRepository to update itself and the underlying store correctly.
	/// </summary>
	abstract public class WritingSystemRepositoryBase : IWritingSystemRepository
	{

		private readonly Dictionary<string, WritingSystemDefinition> _writingSystems;
		private readonly Dictionary<string, DateTime> _writingSystemsToIgnore;

		private readonly Dictionary<string, string> _idChangeMap;

		public event EventHandler<WritingSystemIdChangedEventArgs> WritingSystemIdChanged;
		public event EventHandler<WritingSystemDeletedEventArgs> WritingSystemDeleted;
		public event EventHandler<WritingSystemConflatedEventArgs> WritingSystemConflated;

		/// <summary>
		/// Indicates that writing systems are merging.
		/// </summary>
		protected bool Conflating { get; private set; }

		/// <summary>
		/// </summary>
		protected WritingSystemRepositoryBase()
		{
			_writingSystems = new Dictionary<string, WritingSystemDefinition>(StringComparer.OrdinalIgnoreCase);
			_writingSystemsToIgnore = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
			_idChangeMap = new Dictionary<string, string>();
			//_sharedStore = LdmlSharedWritingSystemRepository.Singleton;
		}

		/// <summary>
		/// Gets the writing systems to ignore.
		/// </summary>
		protected IDictionary<string, DateTime> WritingSystemsToIgnore
		{
			get
			{
				return _writingSystemsToIgnore;
			}
		}

		/// <summary>
		/// Gets the changed IDs mapping.
		/// </summary>
		protected IDictionary<string, string> ChangedIDs
		{
			get { return _idChangeMap; }
		}

		virtual public WritingSystemDefinition CreateNew()
		{
			return new WritingSystemDefinition();
		}

		virtual public void Conflate(string wsToConflate, string wsToConflateWith)
		{
			Conflating = true;
			if (WritingSystemConflated != null)
			{
				WritingSystemConflated(this, new WritingSystemConflatedEventArgs(wsToConflate, wsToConflateWith));
			}
			Remove(wsToConflate);
			Conflating = false;
		}

		/// <summary>
		/// Remove the specified WritingSystemDefinition.
		/// </summary>
		/// <param name="identifier">the StoreID of the WritingSystemDefinition</param>
		/// <remarks>
		/// Note that ws.StoreID may differ from ws.Id.  The former is the key into the
		/// dictionary, but the latter is what gets persisted to disk (and shown to the
		/// user).
		/// </remarks>
		virtual public void Remove(string identifier)
		{
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (!_writingSystems.ContainsKey(identifier))
			{
				throw new ArgumentOutOfRangeException("identifier");
			}
			// Remove() uses the StoreID field, but file storage and UI use the Id field.
			string realId = _writingSystems[identifier].ID;
			// Delete from us
			//??? Do we really delete or just mark for deletion?

			_writingSystems.Remove(identifier);
			if (_writingSystemsToIgnore.ContainsKey(identifier))
				_writingSystemsToIgnore.Remove(identifier);
			if (_writingSystemsToIgnore.ContainsKey(realId))
				_writingSystemsToIgnore.Remove(realId);
			if (!Conflating && WritingSystemDeleted != null)
			{
				WritingSystemDeleted(this, new WritingSystemDeletedEventArgs(realId));
			}
			//TODO: Could call the shared store to advise that one has been removed.
			//TODO: This may be useful if writing systems were reference counted.
		}

		abstract public string WritingSystemIDHasChangedTo(string id);

		virtual public void LastChecked(string identifier, DateTime dateModified)
		{
			if (_writingSystemsToIgnore.ContainsKey(identifier))
			{
				_writingSystemsToIgnore[identifier] = dateModified;
			}
			else
			{
				_writingSystemsToIgnore.Add(identifier, dateModified);
			}
		}

		/// <summary>
		/// Removes all writing systems.
		/// </summary>
		protected void Clear()
		{
			_writingSystems.Clear();
		}

		public WritingSystemDefinition MakeDuplicate(WritingSystemDefinition definition)
		{
			if (definition == null)
			{
				throw new ArgumentNullException("definition");
			}
			return definition.Clone();
		}

		public abstract bool WritingSystemIDHasChanged(string id);

		public bool Contains(string identifier)
		{
			// identifier should not be null, but some unit tests never define StoreID
			// on their temporary WritingSystemDefinition objects.
			return identifier != null && _writingSystems.ContainsKey(identifier);
		}

		public bool CanSet(WritingSystemDefinition ws)
		{
			if (ws == null)
			{
				return false;
			}
			return !(_writingSystems.Keys.Any(id => id.Equals(ws.ID, StringComparison.OrdinalIgnoreCase)) &&
				ws.StoreID != _writingSystems[ws.ID].StoreID);
		}

		public virtual void Set(WritingSystemDefinition ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}

			//Check if this is a new writing system with a conflicting id
			if (!CanSet(ws))
			{
				throw new ArgumentException(String.Format("Unable to set writing system '{0}' because this id already exists. Please change this writing system id before setting it.", ws.ID));
			}
			string oldId = _writingSystems.Where(kvp => kvp.Value.StoreID == ws.StoreID).Select(kvp => kvp.Key).FirstOrDefault();
			//??? How do we update
			//??? Is it sufficient to just set it, or can we not change the reference in case someone else has it too
			//??? i.e. Do we need a ws.Copy(WritingSystemDefinition)?
			if (!String.IsNullOrEmpty(oldId) && _writingSystems.ContainsKey(oldId))
			{
				_writingSystems.Remove(oldId);
			}
			_writingSystems[ws.ID] = ws;

			if (!String.IsNullOrEmpty(oldId) && (oldId != ws.ID))
			{
				UpdateChangedIDs(oldId, ws.ID);
				if (WritingSystemIdChanged != null)
				{
					WritingSystemIdChanged(this, new WritingSystemIdChangedEventArgs(oldId, ws.ID));
				}
			}

			ws.StoreID = ws.ID;
		}

		/// <summary>
		/// Updates the changed IDs mapping.
		/// </summary>
		protected void UpdateChangedIDs(string oldId, string newId)
		{
			if (_idChangeMap.ContainsValue(oldId))
			{
				// if the oldid is in the value of key/value, then we can update the cooresponding key with the newId
				string keyToChange = _idChangeMap.First(pair => pair.Value == oldId).Key;
				_idChangeMap[keyToChange] = newId;
			}
			else if (_idChangeMap.ContainsKey(oldId))
			{
				// if oldId is already in the dictionary, set the result to be newId
				_idChangeMap[oldId] = newId;
			}
		}

		/// <summary>
		/// Loads the changed IDs mapping from the existing writing systems.
		/// </summary>
		protected void LoadChangedIDsFromExistingWritingSystems()
		{
			_idChangeMap.Clear();
			foreach (var pair in _writingSystems)
			{
				_idChangeMap[pair.Key] = pair.Key;
			}
		}

		public string GetNewStoreIDWhenSet(WritingSystemDefinition ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			return String.IsNullOrEmpty(ws.StoreID) ? ws.ID : ws.StoreID;
		}

		public WritingSystemDefinition Get(string identifier)
		{
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (!_writingSystems.ContainsKey(identifier))
			{
				throw new ArgumentOutOfRangeException("identifier", String.Format("Writing system id '{0}' does not exist.", identifier));
			}
			return _writingSystems[identifier];
		}

		public int Count
		{
			get
			{
				return _writingSystems.Count;
			}
		}

		virtual public void Save()
		{
		}

		virtual protected void OnChangeNotifySharedStore(WritingSystemDefinition ws)
		{
			DateTime lastDateModified;
			if (_writingSystemsToIgnore.TryGetValue(ws.ID, out lastDateModified) && ws.DateModified > lastDateModified)
				_writingSystemsToIgnore.Remove(ws.ID);
		}

		virtual protected void OnRemoveNotifySharedStore()
		{
		}

		virtual public IEnumerable<WritingSystemDefinition> WritingSystemsNewerIn(IEnumerable<WritingSystemDefinition> rhs)
		{
			if (rhs == null)
			{
				throw new ArgumentNullException("rhs");
			}
			var newerWritingSystems = new List<WritingSystemDefinition>();
			foreach (WritingSystemDefinition ws in rhs)
			{
				Guard.AgainstNull(ws, "ws in rhs");
				if (_writingSystems.ContainsKey(ws.IetfLanguageTag))
				{
					DateTime lastDateModified;
					if ((!_writingSystemsToIgnore.TryGetValue(ws.IetfLanguageTag, out lastDateModified) || ws.DateModified > lastDateModified)
						&& (ws.DateModified > _writingSystems[ws.IetfLanguageTag].DateModified))
					{
						newerWritingSystems.Add(ws.Clone());
					}
				}
			}
			return newerWritingSystems;
		}

		public IEnumerable<WritingSystemDefinition> AllWritingSystems
		{
			get
			{
				return _writingSystems.Values;
			}
		}

		public IEnumerable<WritingSystemDefinition> TextWritingSystems
		{
			get { return _writingSystems.Values.Where(ws => !ws.IsVoice); }
		}

		public IEnumerable<WritingSystemDefinition> VoiceWritingSystems
		{
			get { return _writingSystems.Values.Where(ws => ws.IsVoice); }
		}

		public virtual void OnWritingSystemIDChange(WritingSystemDefinition ws, string oldId)
		{
			_writingSystems[ws.ID] = ws;
			_writingSystems.Remove(oldId);
		}

		/// <summary>
		/// filters the list down to those that are texts (not audio), while preserving their order
		/// </summary>
		/// <param name="idsToFilter"></param>
		/// <returns></returns>
		public IEnumerable<string> FilterForTextIDs(IEnumerable<string> idsToFilter)
		{
			var textIds = TextWritingSystems.Select(ws => ws.ID);
			return idsToFilter.Where(id => textIds.Contains(id));
		}

		/// <summary>
		/// Get the writing system that is most probably intended by the user, when input language changes to the specified layout and cultureInfo,
		/// given the indicated candidates, and that wsCurrent is the preferred result if it is a possible WS for the specified culture.
		/// wsCurrent is also returned if none of the candidates is found to match the specified inputs.
		/// See interface comment for intended usage information.
		/// Enhance JohnT: it may be helpful, if no WS has an exact match, to look for one where the culture prefix (before hyphen) matches,
		/// thus finding a WS that has a keyboard for the same language as the one the user selected.
		/// Could similarly match against WS ID's language ID, for WS's with no RawLocalKeyboard.
		/// Could use LocalKeyboard instead of RawLocalKeyboard, thus allowing us to find keyboards for writing systems where the
		/// local keyboard has not yet been determined. However, this would potentially establish a particular local keyboard for
		/// a user who has never typed in that writing system or configured a keyboard for it, nor even selected any text in it.
		/// In the expected usage of this library, there will be a RawLocalKeyboard for every writing system in which the user has
		/// ever typed or selected text. That should have a high probability of catching anything actually useful.
		/// </summary>
		/// <param name="layoutName"></param>
		/// <param name="cultureInfo"></param>
		/// <param name="wsCurrent"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public WritingSystemDefinition GetWsForInputLanguage(string layoutName, CultureInfo cultureInfo, WritingSystemDefinition wsCurrent,
			WritingSystemDefinition[] options)
		{
			// See if the default is suitable.
			if (WsMatchesLayout(layoutName, wsCurrent) && WsMatchesCulture(cultureInfo, wsCurrent))
				return wsCurrent;
			WritingSystemDefinition layoutMatch = null;
			WritingSystemDefinition cultureMatch = null;
			foreach (WritingSystemDefinition ws in options)
			{
				bool matchesCulture = WsMatchesCulture(cultureInfo, ws);
				if (WsMatchesLayout(layoutName, ws))
				{
					if (matchesCulture)
						return ws;
					if (layoutMatch == null || ws.Equals(wsCurrent))
						layoutMatch = ws;
				}
				if (matchesCulture && (cultureMatch == null || ws.Equals(wsCurrent)))
					cultureMatch = ws;
			}
			return layoutMatch ?? cultureMatch ?? wsCurrent;
		}

		private bool WsMatchesLayout(string layoutName, WritingSystemDefinition ws)
		{
			return ws != null && ws.RawLocalKeyboard != null && ws.RawLocalKeyboard.Layout == layoutName;
		}

		private bool WsMatchesCulture(CultureInfo cultureInfo, WritingSystemDefinition ws)
		{
			return ws != null && ws.RawLocalKeyboard != null && ws.RawLocalKeyboard.Locale == cultureInfo.Name;
		}
	}
}