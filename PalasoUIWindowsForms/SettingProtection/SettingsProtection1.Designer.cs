﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17626
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Palaso.UI.WindowsForms.SettingProtection {


	[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
	internal sealed partial class SettingsProtection1 : global::System.Configuration.ApplicationSettingsBase {

		private static SettingsProtection1 defaultInstance = ((SettingsProtection1)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new SettingsProtection1())));

		public static SettingsProtection1 Default {
			get {
				return defaultInstance;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("False")]
		public bool SettingRequirePassword {
			get {
				return ((bool)(this["SettingRequirePassword"]));
			}
			set {
				this["SettingRequirePassword"] = value;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("False")]
		public bool NormallyHidden {
			get {
				return ((bool)(this["NormallyHidden"]));
			}
			set {
				this["NormallyHidden"] = value;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("False")]
		public bool NeedUpgrade {
			get {
				return ((bool)(this["NeedUpgrade"]));
			}
			set {
				this["NeedUpgrade"] = value;
			}
		}
	}
}