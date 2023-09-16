// _Arch, _Path, _Output, _Version, _Description, _RepoUrl, _Publisher, _Copyright : emulated defines using the /D command line compiler option.

#include <idp.iss>
#include <idplang\French.iss>

#define Name WinClean
#define ExeName Name + ".exe"
#define SetupName Name + "-" + Version + "-" + _Arch

[Setup]
AppComments={#_Description}
AppCopyright={#_Copyright}
AppId={{F7168958-5DC1-4316-B05E-A5D6E7851C84}
AppName={#Name}
AppPublisher={#_Publisher}
AppReadmeFile={#_RepoUrl}#readme
AppSupportURL={#_RepoUrl}
AppUpdatesURL={#_RepoUrl}/releases
AppVerName={#Name} {#_Version}
AppVersion={#_Version}
DefaultDirName={autopf}\{#Name}
DisableProgramGroupPage=yes
LicenseFile=..\..\LICENSE
OutputBaseFilename={#SetupName}
OutputDir={#_Output}
PrivilegesRequiredOverridesAllowed = dialog
SetupIconFile=..\WinClean.ico
SignTool=signtool
SolidCompression=yes
Uninstallable=yes
UninstallDisplayIcon={app}\{#ExeName}
VersionInfoDescription={#_Description}
VersionInfoOriginalFileName={#SetupName}.exe
VersionInfoVersion={#_Version}

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

[UninstallDelete]
// Log files
Type: filesandordirs; Name: "{tmp}\{#Name}"

[Icons]
Name: "{autoprograms}\{#Name}"; Filename: "{app}\{#ExeName}"; WorkingDir: "{app}"; Tasks: startmenuicon
Name: "{autodesktop}\{#Name}"; Filename: "{app}\{#ExeName}"; WorkingDir: "{app}"; Tasks: desktopicon
Name: "{autoprograms}\{cm:UninstallProgram,{#Name}}"; Filename: "{uninstallexe}"; WorkingDir: "{app}"; IconFilename: "{app}\{#ExeName}"; Tasks: startmenuicon

[Run]
// shellexec because the app requires admin to run
Filename: "{app}\{#ExeName}"; Description: "{cm:LaunchProgram,{#stringChange(Name, '&', '&&')}}"; Flags: nowait postinstall skipifsilent shellexec;

[Messages]
UninstalledAll=%1 was successfully removed from your computer. However, .NET Dekstop Runtime was not uninstalled. You can manually uninstall it from the Add/Remove Programs Control Panel applet.
fr.UninstalledAll=%1 a été correctement désinstallé de cet ordinateur. Cependant, .NET Desktop Runtime n'a pas été désinstallé. Vous pouvez le désinstaller manuellement à partir de l'applet Ajout/Suppression de programmes du Panneau de configuration.

[CustomMessages]

InstallingDependencies=Installing dependencies
fr.InstallingDependencies=Installation des dépendances

InstallingDotNetRuntime=Installing .NET 6.0 Desktop Runtime.
fr.InstallingDotNetRuntime=.NET 6.0 Desktop Runtime est en cours d'installation.
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

type
    TVersion = record
        Product: string;
        Number: string;
    end;

function CompareVersionNumbers(V1: string; V2: string): integer; forward;
function ExecuteCommand(const Filename: string; const Arguments: string; var ExitCode: integer): boolean; forward;
function ExecuteWithOutput(const Filename: string; const Arguments: string; var ExitCode: integer; var Output: TArrayOfstring): boolean; forward;
function GetInstalledNetVersions(): array of TVersion; forward;
function InstallDotNetRuntime(): string; forward;
function IsDotNetInstalled(const Product: string; const MinVersionNumber: string; const MaxVersionNumber: string): boolean; forward;
function SplitString(Text: string; Separator: string): TArrayOfString; forward;

var
    g_requiresRestart: boolean;
    g_dotNetMissing: boolean;
    g_dotNetInstallerPath: string;

(* Event functions *)

procedure InitializeWizard();
var
    InstallerFilename: string; // Filename will be visible by user in IDP download page
    InstallerUrl: string;
begin
    
    // 6.0.2 is the minimum supported version (corresponds to the last PresentationFramework assembly version bump in 6.*.*)
    g_dotNetMissing := not IsDotNetInstalled('Microsoft.WindowsDesktop.App', '6.0.2', '7.0.0');

    if g_dotNetMissing then begin
        case '{#_Arch}' of
            'win-x86': begin
                InstallerFilename := 'windowsdesktop-runtime-6.0.18-win-x86.exe';
                InstallerUrl := 'https://download.visualstudio.microsoft.com/download/pr/68574b0b-3242-46f1-a406-9ef9aeeec3e5/d45d732e846f306889f41579104b1a33/windowsdesktop-runtime-6.0.18-win-x86.exe';
                end;
            'win-x64': begin
                InstallerFilename := 'windowsdesktop-runtime-6.0.18-win-x64.exe';
                InstallerUrl := 'https://download.visualstudio.microsoft.com/download/pr/f76bace5-6cf4-41d8-ab54-fb7a3766b673/1cbc047d4547dfa9ecd59d5a71402186/windowsdesktop-runtime-6.0.18-win-x64.exe';
                end;
        end;
        g_dotNetInstallerPath := ExpandConstant('{tmp}\') + InstallerFilename;
        idpAddFile(InstallerUrl, g_dotNetInstallerPath)
        idpDownloadAfter(wpReady);
    end;
end;

// Called after IDP has downloaded the files. Proceed with the installation.
function PrepareToInstall(var NeedsRestart: boolean): string;
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
        end;
    end;
end;

// Tell the setup program if the want to restart or not.
function NeedRestart(): boolean;
begin
    Result := g_requiresRestart;
end;

(* Misc functions *)

// Installs the appropriate .NET Windows Desktop Runtime version.
// Returns the error that occured while installing .NET runtime, or an empty string if the installation succeeded.
function InstallDotNetRuntime(): string;
var
    StatusText: string;
    ResultCode: integer;
begin
    StatusText := WizardForm.StatusLabel.Caption;
    WizardForm.StatusLabel.Caption := CustomMessage('InstallingDotNetRuntime');
    WizardForm.ProgressGauge.Style := npbstMarquee;
    try
        if not Exec(g_dotNetInstallerPath, '/passive /norestart /q', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
            Result := FmtMessage(CustomMessage('DotNetRuntimeFailedToLaunch'), [SysErrorMessage(resultCode)])
        else
            case resultCode of
                0: ;
                1602: Result := CustomMessage('DotNetRuntimeFailed1602');
                1603: Result := CustomMessage('DotNetRuntimeFailed1603');
                1641, 3010: g_requiresRestart := True;
                5100: Result := CustomMessage('DotNetRuntimeFailed5100');
            else Result :=
                FmtMessage(CustomMessage('DotNetRuntimeFailedOther'), [IntToStr(resultCode)]);
            end;
        // See https://docs.microsoft.com/en-us/dotnet/framework/deployment/deployment-guide-for-developers#return-codes
    finally
        WizardForm.StatusLabel.Caption := StatusText;
        WizardForm.ProgressGauge.Style := npbstNormal;
    end;
end;

// Checks if a specific .NET version is installed by running the .NET --list-runtimes command and scanning the output.
function GetInstalledNetVersions(): array of TVersion;
var
    ExitCode: integer;
    Output: TArrayOfstring;
    SplittedOutput: TArrayOfstring;
    i: integer;
begin
    if (ExecuteWithOutput('dotnet', '--list-runtimes', ExitCode, Output)) and (ExitCode = 0) then for i := 0 to GetArrayLength(Output) - 1 do begin
            SplittedOutput := SplitString(Output[i], ' ');
            SetArrayLength(Result, i + 1);
            with Result[i] do begin
                Product := SplittedOutput[0];
                Number := SplittedOutput[1];
                // SplittedOutput may contain more entries, the path, maybe splitted because it can contain spaces. Discard them.
            end;
        end
    else
        // If failed, then .NET isn't installed at all or an exceptional error occured. Return an empty array.
        SetArrayLength(Result, 0);

end;

function IsDotNetInstalled(
    const Product: string; // The .NET product name
    const MinVersionNumber: string; // The minimum version (inclusive)
    const MaxVersionNumber: string) // The maximum version (inclusive)
    : boolean; // Is a version in the range installed?
var
    InstalledVersions: array of TVersion;
    i: integer;
begin
    InstalledVersions := GetInstalledNetVersions;

    Result := False;

    for i := 0 to GetArrayLength(InstalledVersions) - 1 do begin
        Result := (InstalledVersions[i].Product = Product) and (CompareVersionNumbers(InstalledVersions[i].Number, MinVersionNumber) >= 0) and
            (CompareVersionNumbers(InstalledVersions[i].Number, MaxVersionNumber) <= 0);
        if Result then Exit;
    end;
end;

// Executes a filename and arguments with SW_HIDE and waits for exit.
// Logs in case of error.
function ExecuteCommand(
    const Filename: string; // Filename of the executable
    const Arguments: string; // Arguments
    var ExitCode: integer) // out: the exit code of the command
    : boolean; // Did execution succeed?
begin
    Result := Exec(Filename, Arguments, '', SW_HIDE, ewWaitUntilTerminated, ExitCode);
    if not Result then Log('Execution of command "' + Filename + ' ' + Arguments + '" failed with error message: ' + SysErrorMessage(ExitCode));
end;

// Executes a filename and arguments as a command and waits for exit.
// Output isn't altered if False is returned.
// Logs in case of error.
function ExecuteWithOutput(
    const Filename: string; // Filename of the executable
    const Arguments: string; // Arguments
    var ExitCode: integer; // out: the exit code of the command
    var Output: TArrayOfstring) // out: lines of the command's output
    : boolean; // Did execution succeed?.
var
    TempFilename: string;
begin
    TempFilename := ExpandConstant('{tmp}\ExecuteWithOutput.tmp');
    // Save output to a temp file.
    // Exec via cmd and redirect output to file.
    // Must use special string-behavior to work.
    Result := ExecuteCommand(ExpandConstant('{cmd}'), Format('/S /C ""%s" %s > "%s""', [Filename, Arguments, TempFilename]), ExitCode) and
        LoadstringsFromFile(TempFilename, Output);
    DeleteFile(TempFilename); // Delete the temp file.
end;

// Returns -1 if V1 is less than V2
// Returns 0 if V1 is equal to V2
// Returns 1 if V1 is greater than V2
function CompareVersionNumbers(V1: string; V2: string): integer;
var
    P, N1, N2: integer;
begin
    Result := 0;
    while (Result = 0) and ((V1 <> '') or (V2 <> '')) do begin
        P := Pos('.', V1);
        if P > 0 then begin
            N1 := StrToInt(Copy(V1, 1, P - 1));
            Delete(V1, 1, P);
        end
        else if V1 <> '' then begin
            N1 := StrToInt(V1);
            V1 := '';
        end
        else
            N1 := 0;

        P := Pos('.', V2);

        if P > 0 then begin
            N2 := StrToInt(Copy(V2, 1, P - 1));
            Delete(V2, 1, P);
        end
        else if V2 <> '' then begin
            N2 := StrToInt(V2);
            V2 := '';
        end
        else
            N2 := 0;
        if N1 < N2 then Result := -1
        else if N1 > N2 then Result := 1;
    end;
end;

function SplitString(Text: string; Separator: string): TArrayOfString;
var
    i, p: integer;
    Dest: TArrayOfString;
begin
    i := 0;
    repeat
        SetArrayLength(Dest, i + 1);
        p := Pos(Separator, Text);
        if p > 0 then begin
            Dest[i] := Copy(Text, 1, p - 1);
            Text := Copy(Text, p + Length(Separator), Length(Text));
            i := i + 1;
        end
        else begin
            Dest[i] := Text;
            Text := '';
        end;
    until Length(Text) = 0;
    Result := Dest;
end;