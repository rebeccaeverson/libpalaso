using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class LookupISOCodeDialog : Form
	{
		public LookupISOCodeDialog()
		{
			InitializeComponent();
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			_lookupISOControl.StopTimer();
			base.OnClosing(e);
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Close();
		}

//        public Iso639LanguageCode ISOCodeAndName
//        {
//            get
//            {
//				if( DialogResult != DialogResult.OK)
//					return null;
//	            return new Iso639LanguageCode(_lookupISOControl.LanguageInfo.Code, _lookupISOControl.LanguageInfo.Names[0],
//	                                          _lookupISOControl.LanguageInfo.Code);//review: it's not clear which codes these are supposed to be. As is, they are 639-1 if it exists, else 639-3
//            }
//        }

		public LanguageInfo SelectedLanguage
		{
			get
			{
				if( DialogResult != DialogResult.OK)
					return null;
				return _lookupISOControl.LanguageInfo;
			}
		}


		private void OnChooserDoubleClicked(object sender, EventArgs e)
		{
			_okButton_Click(sender, e);
		}

		private void _lookupISOControl_Changed(object sender, EventArgs e)
		{
			_okButton.Enabled = _lookupISOControl.LanguageInfo != null;
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}