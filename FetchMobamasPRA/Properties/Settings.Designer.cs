﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace FetchMobamasPRA.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.6.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsUpgrade {
            get {
                return ((bool)(this["IsUpgrade"]));
            }
            set {
                this["IsUpgrade"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0, 0, 0, 0")]
        public global::System.Drawing.Rectangle MainBounds {
            get {
                return ((global::System.Drawing.Rectangle)(this["MainBounds"]));
            }
            set {
                this["MainBounds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Normal")]
        public global::System.Windows.Forms.FormWindowState MainWindowState {
            get {
                return ((global::System.Windows.Forms.FormWindowState)(this["MainWindowState"]));
            }
            set {
                this["MainWindowState"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("mail.server.jp")]
        public string SMTPServer {
            get {
                return ((string)(this["SMTPServer"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("587")]
        public int SMTPPort {
            get {
                return ((int)(this["SMTPPort"]));
            }
            set {
                this["SMTPPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SMTPSSL {
            get {
                return ((bool)(this["SMTPSSL"]));
            }
            set {
                this["SMTPSSL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("UserID")]
        public string SMTPUserID {
            get {
                return ((string)(this["SMTPUserID"]));
            }
            set {
                this["SMTPUserID"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("cGFzc3dvcmQ=")]
        public string SMTPPassword {
            get {
                return ((string)(this["SMTPPassword"]));
            }
            set {
                this["SMTPPassword"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("from@mail.server.jp")]
        public string SMTPFrom {
            get {
                return ((string)(this["SMTPFrom"]));
            }
            set {
                this["SMTPFrom"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("to@mail.server.jp")]
        public string SMTPTo {
            get {
                return ((string)(this["SMTPTo"]));
            }
            set {
                this["SMTPTo"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("差出人 表示名")]
        public string SMTPDisplay {
            get {
                return ((string)(this["SMTPDisplay"]));
            }
            set {
                this["SMTPDisplay"] = value;
            }
        }
    }
}
