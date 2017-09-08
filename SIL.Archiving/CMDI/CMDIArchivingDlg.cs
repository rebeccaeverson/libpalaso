using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;
using SIL.Windows.Forms.PortableSettingsProvider;

namespace SIL.Archiving.CMDI
{
	/// <summary />
	public class CMDIArchivingDlg : ArchivingDlg
	{
		private TableLayoutPanel _destinationFolderTable;
		private Label _destinationFolderLabel;
		private LinkLabel _browseDestinationFolder;

		private TableLayoutPanel _cmdiProgramTable;
		private LinkLabel _browseCMDIProgram;
		private ComboBox _selectCMDIPreset;

		/// <summary />
		public CMDIArchivingDlg(ArchivingDlgViewModel model, string localizationManagerId, Font programDialogFont, FormSettings settings) 
			: base(model, localizationManagerId, programDialogFont, settings)
		{
			// DO NOT SHOW THE LAUNCH OPTION AT THIS TIME
			model.PathToProgramToLaunch = null;

			InitializeNewControls();

			// get the saved CMDI program value
			GetSavedValues();

			// set control properties
			SetControlProperties();
		}

		private void InitializeNewControls()
		{
			AddDestinationFolder();
			AddCMDIProgram();
		}

		private void AddDestinationFolder()
		{
			_destinationFolderTable = new TableLayoutPanel
			{
				ColumnCount = 2,
				RowCount = 1,
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink
			};
			_destinationFolderTable.RowStyles.Add(new RowStyle { SizeType = SizeType.AutoSize });
			_destinationFolderTable.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.AutoSize });
			_destinationFolderTable.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 100 });

			// add the "Change Folder" link
			_browseDestinationFolder = new LinkLabel
			{
				Text = LocalizationManager.GetString("DialogBoxes.CMDIArchivingDlg.ChangeDestinationFolder",
					"Change Folder"),
				Anchor = AnchorStyles.Left,
				AutoSize = true,
				TextAlign = ContentAlignment.MiddleLeft,
			};

			_browseDestinationFolder.Click += _browseDestinationFolder_Click;
			_destinationFolderTable.Controls.Add(_browseDestinationFolder, 0, 0);

			// add the current folder label
			_destinationFolderLabel = new Label
			{
				Anchor = AnchorStyles.Left,
				AutoSize = true,
				TextAlign = ContentAlignment.MiddleLeft
			};
			SetDestinationLabelText();
			_destinationFolderTable.Controls.Add(_destinationFolderLabel, 1, 0);

			_flowLayoutExtra.Controls.Add(_destinationFolderTable);
		}

		protected override void DisableControlsDuringPackageCreation()
		{
			base.DisableControlsDuringPackageCreation();
			_destinationFolderTable.Visible = false;
		}

		void SetDestinationLabelText()
		{
			var labelText = ((CMDIArchivingDlgViewModel)_viewModel).OutputFolder;
			if (labelText.Length > 50)
				labelText = labelText.Substring(0, 3) + "..." + labelText.Substring(labelText.Length - 44);

			_destinationFolderLabel.Text = labelText;
		}

		void _browseDestinationFolder_Click(object sender, EventArgs e)
		{
			using (var chooseFolder = new FolderBrowserDialog())
			{
				var previousPath = ((CMDIArchivingDlgViewModel)_viewModel).OutputFolder;
				if (string.IsNullOrEmpty(previousPath))
					previousPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

				chooseFolder.Description = LocalizationManager.GetString(
					"DialogBoxes.CMDIArchivingDlg.ArchivingCMDILocationDescription",
					"Select a base folder where the CMDI directory structure should be created.");
				chooseFolder.ShowNewFolderButton = true;
				chooseFolder.SelectedPath = previousPath;
				if (chooseFolder.ShowDialog() == DialogResult.Cancel)
					return;

				((CMDIArchivingDlgViewModel)_viewModel).OutputFolder = chooseFolder.SelectedPath;

				SetDestinationLabelText();
				SetControlProperties();
			}
		}

		private void AddCMDIProgram()
		{
			_cmdiProgramTable = new TableLayoutPanel
			{
				ColumnCount = 2,
				RowCount = 1,
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink
			};
			_cmdiProgramTable.RowStyles.Add(new RowStyle { SizeType = SizeType.AutoSize });
			_cmdiProgramTable.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.AutoSize });
			_cmdiProgramTable.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 100 });

			// add the preset combo box
			_selectCMDIPreset = new ComboBox { Anchor = AnchorStyles.Left, DropDownStyle = ComboBoxStyle.DropDownList };
			_selectCMDIPreset.Items.AddRange(new object[] { "Arbil", "Other" });
			SizeComboBox(_selectCMDIPreset);
			_cmdiProgramTable.Controls.Add(_selectCMDIPreset, 0, 0);

			// add the "Change Program to Launch" link
			_browseCMDIProgram = new LinkLabel
			{
				Text = LocalizationManager.GetString("DialogBoxes.CMDIArchivingDlg.ChangeProgramToLaunch",
					"Change Program to Launch"),
				Anchor = AnchorStyles.Left,
				AutoSize = true,
				TextAlign = ContentAlignment.MiddleLeft
			};

			_browseCMDIProgram.Click += SelectCMDIProgramOnClick;
			_cmdiProgramTable.Controls.Add(_browseCMDIProgram, 1, 0);

			// DO NOT SHOW THE LAUNCH OPTION AT THIS TIME
			//_flowLayoutExtra.Controls.Add(_cmdiProgramTable);
		}

		private void SelectCMDIProgramOnClick(object sender, EventArgs eventArgs)
		{
			using (var chooseCMDIProgram = new OpenFileDialog())
			{
				chooseCMDIProgram.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
				chooseCMDIProgram.RestoreDirectory = true;
				chooseCMDIProgram.CheckFileExists = true;
				chooseCMDIProgram.CheckPathExists = true;
				chooseCMDIProgram.Filter = string.Format("{0} ({1})|{1}|{2} ({3})|{3}",
					LocalizationManager.GetString("DialogBoxes.ArchivingDlg.ProgramsFileTypeLabel", "Programs"),
					"*.exe;*.pif;*.com;*.bat;*.cmd",
					LocalizationManager.GetString("DialogBoxes.ArchivingDlg.AllFilesLabel", "All Files"),
					"*.*");
				chooseCMDIProgram.FilterIndex = 0;
				chooseCMDIProgram.Multiselect = false;
				chooseCMDIProgram.Title = LocalizationManager.GetString(
					"DialogBoxes.CMDIArchivingDlg.SelectCMDIProgram", "Select the program to launch after CMDI package is created");
				chooseCMDIProgram.ValidateNames = true;
				if (chooseCMDIProgram.ShowDialog() == DialogResult.OK && File.Exists(chooseCMDIProgram.FileName))
				{
					((CMDIArchivingDlgViewModel)_viewModel).OtherProgramPath = chooseCMDIProgram.FileName;
					SetControlProperties();
					
				}
			}
		}

		/// <summary>Resize a ComboBox to fit the width of the list items</summary>
		private static void SizeComboBox(ComboBox comboBox)
		{
			var maxWidth = 0;

// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var item in comboBox.Items)
			{
				var itmWidth = TextRenderer.MeasureText(item.ToString(), comboBox.Font).Width;
				if (itmWidth > maxWidth)
					maxWidth = itmWidth;
			}

			comboBox.Width = maxWidth + 30;
		}

		private void GetSavedValues()
		{
			SelectPreset(((CMDIArchivingDlgViewModel)_viewModel).ProgramPreset);
			_selectCMDIPreset.SelectedIndexChanged += SelectCMDIPresetOnSelectedIndexChanged;
		}

		private void SelectCMDIPresetOnSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			((CMDIArchivingDlgViewModel) _viewModel).ProgramPreset = _selectCMDIPreset.SelectedItem.ToString();
			SetControlProperties();
		}

		private void SelectPreset(string preset)
		{
			foreach (var item in _selectCMDIPreset.Items.Cast<object>().Where(item => item.ToString() == preset))
			{
				_selectCMDIPreset.SelectedItem = item;
				return;
			}

			// if you are here, the selected item was not found
			_selectCMDIPreset.SelectedIndex = 0;
		}

		private void SetControlProperties()
		{
			// DO NOT SHOW THE LAUNCH OPTION AT THIS TIME
			//_browseCMDIProgram.Visible = (_selectCMDIPreset.SelectedIndex == (_selectCMDIPreset.Items.Count - 1));
			//UpdateLaunchButtonText();
			_buttonLaunchRamp.Visible = false;
			_tableLayoutPanel.SetColumn(_buttonCreatePackage, 1);
			_buttonCancel.Text = LocalizationManager.GetString("DialogBoxes.CMDIArchivingDlg.CloseButtonLabel", "Close");
			_buttonCreatePackage.Text = LocalizationManager.GetString("DialogBoxes.CMDIArchivingDlg.CreatePackageButtonLabel", "Create Package");
			UpdateOverviewText();
		}

	}
}
