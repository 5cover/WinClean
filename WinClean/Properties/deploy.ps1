Param
(
	[Parameter(Mandatory=$false)]
    [Switch]
	$SkipBuild = $false

)

$thumbprint = '1069E1903B197B71ADFCF6D2151BAF5920706A21'

$name = 'WinClean'

$outputDir = '..\bin\setup'

$portableWin64 = '..\bin\publish\portable-win-x64'
$portableWin86 = '..\bin\publish\portable-win-x86'
$win64 = '..\bin\publish\win-x64'
$win86 = '..\bin\publish\win-x86'


# Publish
if (-Not $SkipBuild)
{
	dotnet publish .. /p:PublishProfile=portable-win-x64 -o $portableWin64
	dotnet publish .. /p:PublishProfile=portable-win-x86 -o $portableWin86
	dotnet publish .. /p:PublishProfile=win-x64 -o $win64
	dotnet publish .. /p:PublishProfile=win-x86 -o $win86
}


# Sign if available

if (Test-Path Cert:\CurrentUser\My\$thumbprint)
{
    $signExe = 'signtool'
    $signArgs = @('sign', '/fd', 'SHA256', '/sha1', $thumbprint, '/t', 'http://timestamp.digicert.com')

    & $signExe @signArgs "$portableWin64\$name.exe"
    & $signExe @signArgs "$portableWin86\$name.exe"
    & $signExe @signArgs "$win64\$name.dll"
    & $signExe @signArgs "$win64\$name.exe"
    & $signExe @signArgs "$win86\$name.dll"
    & $signExe @signArgs "$win86\$name.exe"

}
else
{
    # no-op dummy exe
    $signFile = "systray"
    Write-Error "Cannot sign: certificate not found"
}

# Compile scripts

$ISCC = 'D:\Programmes\Inno Setup 6\ISCC.exe'

& $ISCC @("/Ssigntool=$signExe $signArgs `$f", "/D_Output=$outputDir", '/D_Arch=win-x64', "/D_Path=$win64", 'InstallerScript.iss')
& $ISCC @("/Ssigntool=$signExe $signArgs `$f", "/D_Output=$outputDir", '/D_Arch=win-x86', "/D_Path=$win86", 'InstallerScript.iss')

# Copy portable binaries

Copy-Item "$portableWin64\$name.exe" "$outputDir\$name-x64.exe"
Copy-Item "$portableWin86\$name.exe" "$outputDir\$name-x86.exe"