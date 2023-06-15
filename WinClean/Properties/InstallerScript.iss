// _Arch, _Path, _Output : emulated #define using the /D command line compiler option.

#include <idp.iss>
#include <idplang\French.iss>

#define Name "WinClean"
#define Version "1.3.0"
#define RepoUrl "https://github.com/5cover/WinClean"
#define ExeName "WinClean.exe"
#define Description "Windows optimization and debloating utility."
#define SetupName Name + "-" + Version + "-" + _Arch

[Setup]
AppId={{F7168958-5DC1-4316-B05E-A5D6E7851C84}
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
VersionInfoVersion={#Version}
DefaultDirName={code:GetProgramFiles}\{#Name}
DisableProgramGroupPage=yes
LicenseFile=..\..\LICENSE
OutputDir={#_Output}
OutputBaseFilename={#SetupName}
SetupIconFile=..\WinClean.ico
SignTool=signtool
SolidCompression=yes

Uninstallable=yes
UninstallDisplayIcon={app}\{#ExeName}

[CustomMessages]
CreateStartMenuIcon=Create a &Start menu icon
fr.CreateStartMenuIcon=Créer une &icône dans le menu Démarrer

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "fr"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}
Name: startmenuicon; Description: {cm:CreateStartMenuIcon}; GroupDescription: {cm:AdditionalIcons}

[Files]
Source: "{#_Path}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

// Remove old default scripts prior to 1.2.0
[InstallDelete]
Type: files; Name: "{userappdata}\{#Name}\Scripts\*"

[UninstallDelete]
// Log files
Type: filesandordirs; Name: "{tmp}\{#Name}"

[Icons]
Name: "{autoprograms}\{#Name}"; Filename: "{app}\{#ExeName}"; WorkingDir: "{app}"; Tasks: startmenuicon
Name: "{autodesktop}\{#Name}"; Filename: "{app}\{#ExeName}"; WorkingDir: "{app}"; Tasks: desktopicon
Name: "{autoprograms}\{cm:UninstallProgram,{#Name}}"; Filename: "{uninstallexe}"; WorkingDir: "{app}"; IconFilename: "{app}\{#ExeName}"; Tasks: startmenuicon

[Run]
// shellexec because the app requires admin to run
Filename: "{app}\{#ExeName}"; Description: "{cm:LaunchProgram,{#StringChange(Name, '&', '&&')}}"; Flags: nowait postinstall skipifsilent shellexec;

[Messages]
UninstalledAll=%1 was successfully removed from your computer. However, .NET Dekstop Runtime was not uninstalled. You can manually uninstall it from the Add/Remove Programs Control Panel applet.
fr.UninstalledAll=%1 a été correctement désinstallé de cet ordinateur. Cependant, .NET Desktop Runtime n'a pas été désinstallé. Vous pouvez le désinstaller manuellement à partir de l'applet Ajout/Suppression de programmes du Panneau de configuration.

[CustomMessages]

InstallingDependencies=Installing dependencies
fr.InstallingDependencies=Installation des dépendances

InstallingDotNetRuntime=Installing .NET 6 Desktop Runtime.
fr.InstallingDotNetRuntime=.NET 6 Desktop Runtime est en cours d'installation.
DotNetRuntimeFailedToLaunch=Failed to launch .NET Desktop Runtime Installer with error "%1". Please fix the error then run this installer again.
fr.DotNetRuntimeFailedToLaunch=Le lancement de .NET Desktop Runtime Installer a échoué avec l'erreur "%1". Corrigez l'erreur puis redémarrez l'installation.

DotNetRuntimeFailed1602=.NET Desktop Runtime installation was cancelled. This installation can continue, but be aware that this application may not run unless the .NET Desktop Runtime installation is completed successfully.
fr.DotNetRuntimeFailed1602=L'installation de .NET Desktop Runtime a été annulée. L'installation peut continuer, mais cette application ne fonctionnera pas correctement tant que .NET Desktop Runtime ne sera pas installé.

DotNetRuntimeFailed1603=A fatal error occurred while installing the .NET Desktop Runtime. Please fix the error, then run the installer again.
fr.DotNetRuntimeFailed1603=Une erreur critique s'est produite pendant l'installation de .NET Desktop Runtime. Corrigez l'erreur puis redémarrez l'installation.

DotNetRuntimeFailed5100=Your computer does not meet the requirements of the .NET Desktop Runtime.
fr.DotNetRuntimeFailed5100=Votre ordinateur ne correspond pas à la configuration requise de .NET Desktop Runtime. 

DotNetRuntimeFailedOther=The .NET Desktop Runtime installer exited with an unexpected status code "%1". Please review any other messages shown by the installer to determine whether the installation completed successfully, and abort this installation and fix the problem if it did not.
fr.DotNetRuntimeFailedOther=L'installeur de .NET Desktop Runtime a renvoyé un code d'erreur innatendu "%1". Veuillez consulter tous les autres messages affichés par le programme d'installation pour déterminer si l'installation s'est terminée avec succès, et abandonner cette installation et résoudre le problème si ce n'est pas le cas.

[Code]

(* PROTOTYPES *)

function InstallDotNetRuntime(): String; forward;
function IsDotNetInstalled(const DotNetName: string): Boolean; forward;
function GetProgramFiles(const Param: String): String; forward;

var
    g_requiresRestart: Boolean;
    g_dotNetMissing: Boolean;
    g_dotNetInstallerPath: String;

(* Event functions *)

procedure InitializeWizard();
begin
    g_dotNetInstallerPath := ExpandConstant('{tmp}\NetDesktopRuntime6.0.18_Installer.exe');
    g_dotNetMissing := not IsDotNetInstalled('Microsoft.WindowsDesktop.App 6.0.14');
    
    if g_dotNetMissing then begin
        case '{#_Arch}' of
            'win-x86': idpAddFile('https://download.visualstudio.microsoft.com/download/pr/68574b0b-3242-46f1-a406-9ef9aeeec3e5/d45d732e846f306889f41579104b1a33/windowsdesktop-runtime-6.0.18-win-x86.exe', g_dotNetInstallerPath);
            'win-x64': idpAddFile('https://download.visualstudio.microsoft.com/download/pr/f76bace5-6cf4-41d8-ab54-fb7a3766b673/1cbc047d4547dfa9ecd59d5a71402186/windowsdesktop-runtime-6.0.18-win-x64.exe', g_dotNetInstallerPath);
        end
        idpDownloadAfter(wpReady);
    end
end;

// This is run after IDP has downloaded the files. Proceed with the installation.
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
    progressPage: TOutputMarqueeProgressWizardPage;
begin
    if g_dotNetMissing then begin
        progressPage := CreateOutputMarqueeProgressPage(CustomMessage('InstallingDependencies'), CustomMessage('InstallingDotNetRuntime'));
        progressPage.Show;
        progressPage.Animate;
        try
            // Not using 'NeedRestart' as it only has an effect if a non-empty string is returned, thus aborting the installation with an error message.
            Result := InstallDotNetRuntime;
            DeleteFile(g_dotNetInstallerPath);
        finally
            progressPage.Hide;
        end
    end
end;

// Tell the setup program if the want to restart or not.
function NeedRestart(): Boolean;
begin
    Result := g_requiresRestart
end;

(* Misc functions *)

// Get the appropriate program files dir (based on the architecture)
function GetProgramFiles(const Param: String): String;
begin
    if IsWin64 then
        Result := ExpandConstant('{commonpf64}')
    else
        Result := ExpandConstant('{commonpf32}')
end;

// Returns the error that occured while installing .NET runtime, or an empty string if the installation succeeded.
function InstallDotNetRuntime(): String;
var
    StatusText: string;
    ResultCode: Integer;
begin
    StatusText := WizardForm.StatusLabel.Caption;
    WizardForm.StatusLabel.Caption := CustomMessage('InstallingDotNetRuntime');
    WizardForm.ProgressGauge.Style := npbstMarquee;
    try
        if not Exec(g_dotNetInstallerPath, '/passive /norestart /q', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
            Result := FmtMessage(CustomMessage('DotNetRuntimeFailedToLaunch'), [SysErrorMessage(resultCode)])
        else begin
            // See https://docs.microsoft.com/en-us/dotnet/framework/deployment/deployment-guide-for-developers#return-codes
            case resultCode of
                0:;
                1602: Result := CustomMessage('DotNetRuntimeFailed1602');
                1603: Result := CustomMessage('DotNetRuntimeFailed1603');
                1641, 3010: g_requiresRestart := True;
                5100: Result := CustomMessage('DotNetRuntimeFailed5100');
                else Result := FmtMessage(CustomMessage('DotNetRuntimeFailedOther'), [IntToStr(resultCode)]);
            end
        end;
    finally
        WizardForm.StatusLabel.Caption := StatusText;
        WizardForm.ProgressGauge.Style := npbstNormal;
    end
end;

// Checks if a specific .NET version is installed by running the .NET --list-runtimes command and scanning the output.
function IsDotNetInstalled(const DotNetName: string): Boolean;
var
    Cmd, Args: string;
    FileName: string;
    Output: AnsiString;
    Command: string;
    ResultCode: Integer;
begin
    FileName := ExpandConstant('{tmp}\dotnet.txt');
    Cmd := ExpandConstant('{cmd}');
    Command := 'dotnet --list-runtimes';
    Args := '/C ' + Command + ' > "' + FileName + '" 2>&1';
    if Exec(Cmd, Args, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0) then begin
        if LoadStringFromFile(FileName, Output) then begin
            if Pos(DotNetName, Output) > 0 then begin
                Log('"' + DotNetName + '" found in output of "' + Command + '"');
                Result := True;
            end
            else begin
                Log('"' + DotNetName + '" not found in output of "' + Command + '"');
                Result := False;
            end;
        end
        else begin
            Log('Failed to read output of "' + Command + '"');
        end;
    end
    else begin
        Log('Failed to execute "' + Command + '"');
        Result := False;
    end;
    DeleteFile(FileName);
end;