﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.42000
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Scover.WinClean.Resources.UI {
    using System;
    
    
    /// <summary>
    ///   Une classe de ressource fortement typée destinée, entre autres, à la consultation des chaînes localisées.
    /// </summary>
    // Cette classe a été générée automatiquement par la classe StronglyTypedResourceBuilder
    // à l'aide d'un outil, tel que ResGen ou Visual Studio.
    // Pour ajouter ou supprimer un membre, modifiez votre fichier .ResX, puis réexécutez ResGen
    // avec l'option /str ou régénérez votre projet VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Dialogs {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Dialogs() {
        }
        
        /// <summary>
        ///   Retourne l'instance ResourceManager mise en cache utilisée par cette classe.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Scover.WinClean.Resources.UI.Dialogs", typeof(Dialogs).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Remplace la propriété CurrentUICulture du thread actuel pour toutes
        ///   les recherches de ressources à l'aide de cette classe de ressource fortement typée.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Aborting the operation may make the system unstable. Are you sure you want to continue?.
        /// </summary>
        public static string ConfirmAbortOperationContent {
            get {
                return ResourceManager.GetString("ConfirmAbortOperationContent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Exit {0}?.
        /// </summary>
        public static string ConfirmProgramExitContent {
            get {
                return ResourceManager.GetString("ConfirmProgramExitContent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Are you sure you want to delete this script? This action is irreversible..
        /// </summary>
        public static string ConfirmScriptDeletionContent {
            get {
                return ResourceManager.GetString("ConfirmScriptDeletionContent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Are you sure you want to overwrite this script? This action is irreversible..
        /// </summary>
        public static string ConfirmScriptOverwriteContent {
            get {
                return ResourceManager.GetString("ConfirmScriptOverwriteContent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Cannot {0} {1} &quot;{2}&quot;. {3}.
        /// </summary>
        public static string FSErrorContent {
            get {
                return ResourceManager.GetString("FSErrorContent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Script &quot;{0}&quot; has been running for {1} and is probably hung..
        /// </summary>
        public static string HungScriptDialogContent {
            get {
                return ResourceManager.GetString("HungScriptDialogContent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à To select scripts, check their associated check box..
        /// </summary>
        public static string NoScriptsSelectedContent {
            get {
                return ResourceManager.GetString("NoScriptsSelectedContent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Can&apos;t execute scripts because no scripts are selected..
        /// </summary>
        public static string NoScriptsSelectedMainInstruction {
            get {
                return ResourceManager.GetString("NoScriptsSelectedMainInstruction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à An unhandled exception occured: {0}
        ///&lt;A HREF=&quot;https://github.com/5cover/WinClean/issues/new&quot;&gt;Report this issue...&lt;/A&gt;.
        /// </summary>
        public static string UnhandledExceptionDialogContent {
            get {
                return ResourceManager.GetString("UnhandledExceptionDialogContent", resourceCulture);
            }
        }
    }
}
