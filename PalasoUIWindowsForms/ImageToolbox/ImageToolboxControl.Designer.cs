﻿namespace Palaso.UI.WindowsForms.ImageToolbox
{
	partial class ImageToolboxControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_toolImages.Dispose();
				if (components != null)
				{

					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("test", System.Windows.Forms.HorizontalAlignment.Left);
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("testing");
			this._toolListView = new System.Windows.Forms.ListView();
			this._panelForControls = new System.Windows.Forms.Panel();
			this.panel1 = new System.Windows.Forms.Panel();
			this._currentImageBox = new System.Windows.Forms.PictureBox();
			this.imageMetadataControl1 = new Palaso.UI.WindowsForms.ImageToolbox.ImageMetadataControl();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._currentImageBox)).BeginInit();
			this.SuspendLayout();
			//
			// _toolListView
			//
			this._toolListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			listViewGroup1.Header = "test";
			listViewGroup1.Name = "listViewGroup1";
			this._toolListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
			listViewGroup1});
			this._toolListView.HideSelection = false;
			listViewItem1.Group = listViewGroup1;
			this._toolListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
			listViewItem1});
			this._toolListView.Location = new System.Drawing.Point(0, 0);
			this._toolListView.Name = "_toolListView";
			this._toolListView.Size = new System.Drawing.Size(112, 499);
			this._toolListView.TabIndex = 0;
			this._toolListView.UseCompatibleStateImageBehavior = false;
			this._toolListView.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
			//
			// _panelForControls
			//
			this._panelForControls.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._panelForControls.BackColor = System.Drawing.SystemColors.Control;
			this._panelForControls.Location = new System.Drawing.Point(118, 0);
			this._panelForControls.Name = "_panelForControls";
			this._panelForControls.Size = new System.Drawing.Size(397, 499);
			this._panelForControls.TabIndex = 3;
			//
			// panel1
			//
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.imageMetadataControl1);
			this.panel1.Controls.Add(this._currentImageBox);
			this.panel1.Location = new System.Drawing.Point(521, 2);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(267, 500);
			this.panel1.TabIndex = 5;
			//
			// _currentImageBox
			//
			this._currentImageBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._currentImageBox.BackColor = System.Drawing.SystemColors.Control;
			this._currentImageBox.Location = new System.Drawing.Point(7, 0);
			this._currentImageBox.Name = "_currentImageBox";
			this._currentImageBox.Size = new System.Drawing.Size(251, 245);
			this._currentImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._currentImageBox.TabIndex = 5;
			this._currentImageBox.TabStop = false;
			//
			// imageMetadataControl1
			//
			this.imageMetadataControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.imageMetadataControl1.Location = new System.Drawing.Point(7, 251);
			this.imageMetadataControl1.Name = "imageMetadataControl1";
			this.imageMetadataControl1.Size = new System.Drawing.Size(251, 247);
			this.imageMetadataControl1.TabIndex = 6;
			//
			// ImageToolboxControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.panel1);
			this.Controls.Add(this._panelForControls);
			this.Controls.Add(this._toolListView);
			this.Name = "ImageToolboxControl";
			this.Size = new System.Drawing.Size(787, 502);
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._currentImageBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView _toolListView;
		private System.Windows.Forms.Panel _panelForControls;
		private System.Windows.Forms.Panel panel1;
		private ImageMetadataControl imageMetadataControl1;
		private System.Windows.Forms.PictureBox _currentImageBox;
	}
}