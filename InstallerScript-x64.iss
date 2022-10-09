// Requires Inno Download Plugin
// See at https://mitrichsoftware.wordpress.com/inno-setup-tools/inno-download-plugin/
#include <idp.iss>            
#include <idplang\french.iss>

#define Name "WinClean"
#define Version "1.2.0"
#define RepoUrl "https://github.com/5cover/WinClean"
#define ExeName "WinClean.exe"
#define SetupName "WinClean-Installer-x64"
#define Description "Windows optimization and debloating utility."

[Setup]
AppId={{F7168958-5DC1-4316-B05E-A5D6E7851C84}
ArchitecturesAllowed=x64
AppName={#Name}
AppComments={#Description}
AppVersion={#Version}
AppVerName={#Name} {#Version}
AppPublisher=Scover
AppSupportURL={#RepoUrl}
AppReadmeFile={#RepoUrl}#readme
AppUpdatesURL={#RepoUrl}/releases
AppCopyright=© 2022 Scover

VersionInfoDescription={#Description}
VersionInfoOriginalFileName={#SetupName}.exe
VersionInfoVersion={#Version}.0

DefaultDirName={autopf64}\{#Name}
DisableProgramGroupPage=yes
LicenseFile=.\LICENSE
OutputDir=.\bin\Setup
OutputBaseFilename={#SetupName}
SetupIconFile=.\Resources\WinClean.ico
SolidCompression=yes

// IDP is not compatible with modern style
WizardStyle=classic

Uninstallable=yes
UninstallDisplayIcon={app}\{#ExeName}

[CustomMessages]
CreateStartMenuIcon=Create a &Start menu icon
fr.CreateStartMenuIcon=Créer une &icône dans le menu Démarrer

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "fr"; MessagesFile: "compiler:Languages\french.isl"

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}
Name: startmenuicon; Description: {cm:CreateStartMenuIcon}; GroupDescription: {cm:AdditionalIcons}

[Files]
Source: ".\bin\publish\x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: ".\Scripts\*"; DestDir: "{userappdata}\{#Name}\Scripts"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#Name}"; Filename: "{app}\{#ExeName}"; WorkingDir: "{app}"; Tasks: startmenuicon
Name: "{autodesktop}\{#Name}"; Filename: "{app}\{#ExeName}"; WorkingDir: "{app}"; Tasks: desktopicon
Name: "{group}\{cm:UninstallProgram,{#Name}}"; Filename: "{uninstallexe}"; WorkingDir: "{app}"; IconFilename: "{app}\{#ExeName}"; Tasks: startmenuicon;

[Run]
// shellexec because the app requires admin to run
Filename: "{app}\{#ExeName}"; Description: "{cm:LaunchProgram,{#StringChange(Name, '&', '&&')}}"; Flags: nowait postinstall skipifsilent shellexec

[Messages]
UninstalledAll=%1 was successfully removed from your computer. However, .NET Dekstop Runtime was not uninstalled. You can manually uninstall it from the Add/Remove Programs Control Panel applet.
fr.UninstalledAll=%1 a été correctement désinstallé de cet ordinateur. Cependant, .NET Desktop Runtime n'a pas été désinstallé. Vous pouvez le désinstaller manuellement à partir de l'applet Ajout/Suppression de programmes du Panneau de configuration.

[CustomMessages]

InstallingDependencies=Installing dependencies
fr.InstallingDependencies=Installation des dépendances

InstallingDotNetRuntime=Installing .NET 6 Desktop Runtime.
fr.InstallingDotNetRuntime=.NET 6 Desktop Runtime est en cours d'installation.

DotNetRuntimeFailedToLaunch=Failed to launch .NET Desktop Runtime Installer with error "%1". Please fix the error then run this installer again.
fr.DotNetRuntimeFailedToLaunch=Le lancement de .NET Desktop Runtime Installer a échoué avec l'erreur "%1". Corrigez l'erreur puis redémarrez l'installeur.

DotNetRuntimeFailed1602=.NET Desktop Runtime installation was cancelled. This installation can continue, but be aware that this application may not run unless the .NET Desktop Runtime installation is completed successfully.
fr.DotNetRuntimeFailed1602=L'installation de .NET Desktop Runtime a été annulée. L'installation peut continuer, mais cette application ne fonctionnera pas correctement tant que .NET Desktop Runtime ne sera pas installé.

DotNetRuntimeFailed1603=A fatal error occurred while installing the .NET Desktop Runtime. Please fix the error, then run the installer again.
fr.DotNetRuntimeFailed1603=Une erreur critique s'est produite pendant l'installation de .NET Desktop Runtime. Corrigez l'erreur puis redémarrez l'installeur.

DotNetRuntimeFailed5100=Your computer does not meet the requirements of the .NET Desktop Runtime. Please consult the documentation.
fr.DotNetRuntimeFailed5100=Vote ordinateur ne correspond pas à la configuration requise de .NET Desktop Runtime. Consultez la documentation. 

DotNetRuntimeFailedOther=The .NET Desktop Runtime installer exited with an unexpected status code "%1". Please review any other messages shown by the installer to determine whether the installation completed successfully, and abort this installation and fix the problem if it did not.
fr.DotNetRuntimeFailedOther=L'installeur de .NET Desktop Runtime a renvoyé un code d'erreur innatendu "%1". Veuillez consulter tous les autres messages affichés par le programme d'installation pour déterminer si l'installation s'est terminée avec succès, et abandonner cette installation et résoudre le problème si ce n'est pas le cas.


// Adpated from https://engy.us/blog/2021/02/28/installing-net-5-runtime-automatically-with-inno-setup/
[Code]

var
    g_requiresRestart: Boolean;
    g_dotNetMissing : Boolean;

function RegExMatch(const match, pattern: String): Boolean;
var
    RegExp: variant;
begin
    try
        RegExp := createOleObject('VBScript.RegExp')
    except
        raiseException('VBScript RegExp is required to complete the post-installation process.'#13#10#13#10'(Error: ' + GetExceptionMessage)
    end
    RegExp.Pattern := pattern;
    Result := RegExp.Test(match) 
end;

// Exec with output stored in result.    
// ResultString will only be altered if True is returned.
function ExecWithResult(const Filename, Params, WorkingDir: String;
                        const ShowCmd: Integer;
                        out ResultCode: Integer;
                        out ResultString: AnsiString
                        ): Boolean;
var
    TempFilename: String;
    Command: String;
begin
    TempFilename := ExpandConstant('{tmp}\~execwithresult.txt');
    // Exec via cmd and redirect output to file. Must use special string-behavior to work.
    Command := Format('"%s" /S /C ""%s" %s > "%s""', [ExpandConstant('{cmd}'), Filename, Params, TempFilename]);
    Result := Exec(ExpandConstant('{cmd}'), Command, WorkingDir, ShowCmd, ewWaitUntilTerminated, ResultCode);
    if not Result then
        Exit;
    LoadStringFromFile(TempFilename, ResultString); // Cannot fail
    DeleteFile(TempFilename);
    // Remove new-line at the end
    if (Length(ResultString) >= 2) and
       (ResultString[Length(ResultString) - 1] = #13) and
       (ResultString[Length(ResultString)] = #10) then
        Delete(ResultString, Length(ResultString) - 1, 2)
end;

function NetRuntimeIsMissing(): Boolean;
var
    installed: Boolean;
    returnCode: Integer;
    output: AnsiString;  
begin
    // dotnet command unvailable OR result doesnt match 'Microsoft.NETCore.App 6.0.x'
    installed := ExecWithResult('dotnet', '--list-runtimes', '', SW_HIDE, returnCode, output)
    Result := not installed or not RegExMatch(output, 'Microsoft\.NETCore\.App 6\.0\.\d+') 
end;

function InstallDotNetRuntime(): String;
var
    StatusText: string;
    ResultCode: Integer;
begin
    StatusText := WizardForm.StatusLabel.Caption;
    WizardForm.StatusLabel.Caption := CustomMessage('InstallingDotNetRuntime');
    WizardForm.ProgressGauge.Style := npbstMarquee;
    try
        if not Exec(ExpandConstant('{tmp}\NetRuntimeInstaller.exe'), '/passive /norestart /q', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
            Result := FmtMessage(CustomMessage('DotNetRuntimeFailedToLaunch'), [SysErrorMessage(resultCode)])
    else
    begin
        // See https://docs.microsoft.com/en-us/dotnet/framework/deployment/deployment-guide-for-developers#return-codes
        case resultCode of
            0: ;
            1602: MsgBox(CustomMessage('DotNetRuntimeFailed1602'), mbInformation, MB_OK);
            1603: Result := CustomMessage('DotNetRuntimeFailed1603');
            1641, 3010: g_requiresRestart := True;
            5100: Result := CustomMessage('DotNetRuntimeFailed5100');
            else MsgBox(FmtMessage(CustomMessage('DotNetRuntimeFailedOther'), [IntToStr(resultCode)]), mbError, MB_OK)
        end
    end;
    finally
        WizardForm.StatusLabel.Caption := StatusText;
        WizardForm.ProgressGauge.Style := npbstNormal;
    
        DeleteFile(ExpandConstant('{tmp}\NetRuntimeInstaller.exe'))
    end
end;
 
{ EVENT FUNCTIONS } 

// Firstly, download the .NET Runtime Installer
procedure InitializeWizard;
begin
    // Save the result in a variable so we only need to call NetRuntimeIsMissing() once
    g_dotNetMissing := NetRuntimeIsMissing();
    if g_dotNetMissing then
    begin
        // URL of the latest dotnet runtime installer
        idpAddFile('https://download.visualstudio.microsoft.com/download/pr/fe8415d4-8a35-4af9-80a5-51306a96282d/05f9b2a1b4884238e69468e49b3a5453/windowsdesktop-runtime-6.0.9-win-x64.exe', ExpandConstant('{tmp}\NetRuntimeInstaller.exe'));
        idpDownloadAfter(wpReady)
    end
end;

// Then, run it.
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
    progressPage : TOutputMarqueeProgressWizardPage;
begin
    // 'NeedsRestart' only has an effect if we return a non-empty string, thus aborting the installation.
    // If the installers indicate that they want a restart, this should be done at the end of installation.
    // Therefore we set the global 'restartRequired' if a restart is needed, and return this from NeedRestart()

    if g_dotNetMissing then
    begin
        progressPage := CreateOutputMarqueeProgressPage(CustomMessage('InstallingDependencies'), CustomMessage('InstallingDotNetRuntime'));
        progressPage.Show();
        progressPage.Animate();
        try
            Result := InstallDotNetRuntime()
        finally
            progressPage.Hide()
        end
    end
end;

// Tell the setup program if the want to restart or not.
function NeedRestart(): Boolean;
begin
    Result := g_requiresRestart
end;