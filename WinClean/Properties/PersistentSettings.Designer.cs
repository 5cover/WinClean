﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Scover.WinClean.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.5.0.0")]
    public sealed partial class PersistentSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static PersistentSettings defaultInstance = ((PersistentSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new PersistentSettings())));
        
        public static PersistentSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ScriptExecutionTimes {
            get {
                return ((string)(this["ScriptExecutionTimes"]));
            }
            set {
                this["ScriptExecutionTimes"] = value;
            }
        }
    }
}
