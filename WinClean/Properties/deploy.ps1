Param (
	[Parameter(Mandatory=$false)]
    [Switch]
	$SkipBuild = $false,
    [Parameter(Mandatory=$false)]
    [Switch]
    $SkipSign = $false
)

$thumbprint = '1069E1903B197B71ADFCF6D2151BAF5920706A21'
$name = 'WinClean'
$outputDir = '..\bin\setup'

$portableWin64 = '..\bin\publish\portable-win-x64'
$portableWin86 = '..\bin\publish\portable-win-x86'
$win64 = '..\bin\publish\win-x64'
$win86 = '..\bin\publish\win-x86'

# Get app info
$csprojXml = [Xml] (Get-Content '..\WinClean.csproj')
$version = $csprojXml.Project.PropertyGroup.Version[0].Trim()
$description = $csprojXml.Project.PropertyGroup.Description[0].Trim()
$repoUrl = $csprojXml.Project.PropertyGroup.RepositoryUrl[0].Trim()
$authors = $csprojXml.Project.PropertyGroup.Authors[0].Trim()
$copyright = $csprojXml.Project.PropertyGroup.Copyright[0].Trim()

# Publish
if (-Not $SkipBuild) {
	dotnet publish .. /p:PublishProfile=portable-win-x64 -o $portableWin64
	dotnet publish .. /p:PublishProfile=portable-win-x86 -o $portableWin86
	dotnet publish .. /p:PublishProfile=win-x64 -o $win64
	dotnet publish .. /p:PublishProfile=win-x86 -o $win86
}

# Sign if available
$signFile = 'systray' # no-op dummy exe
$isccArgs = @(
    "/D_Copyright=$copyright",
    "/D_Description=$description",
    "/D_Output=$outputDir",
    "/D_Publisher=$author",
    "/D_RepoUrl=$repoUrl",
    "/D_Version=$version",
    'InstallerScript.iss')

if ((-Not $SkipSign) -And (Test-Path Cert:\CurrentUser\My\$thumbprint)) {
    $signExe = 'signtool'
    $signArgs = @('sign', '/fd', 'SHA256', '/sha1', $thumbprint, '/t', 'http://timestamp.digicert.com')
    $innoSigntoolName = 'signtool'
    $isccArgs += @("/S$innoSigntoolName=$signExe $signArgs `$f", "/D_SignTool=$innoSigntoolName")

    & $signExe $signArgs "$portableWin64\$name.exe"
    & $signExe $signArgs "$portableWin86\$name.exe"
    & $signExe $signArgs "$win64\$name.dll"
    & $signExe $signArgs "$win64\$name.exe"
    & $signExe $signArgs "$win86\$name.dll"
    & $signExe $signArgs "$win86\$name.exe"
} elseif (-Not $SkipSign) {
    Write-Error 'Cannot sign: certificate not found'
}

# Compile scripts
$ISCC = 'D:\Programmes\Inno Setup 6\ISCC.exe'

& $ISCC $($isccArgs + @('/D_Arch=win-x64', "/D_Path=$win64"))
& $ISCC $($isccArgs + @('/D_Arch=win-x86', "/D_Path=$win86"))

# Copy portable binaries
Copy-Item "$portableWin64\$name.exe" "$outputDir\$name-x64.exe"
Copy-Item "$portableWin86\$name.exe" "$outputDir\$name-x86.exe"