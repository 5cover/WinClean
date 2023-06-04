Param
(
	[Parameter(Mandatory=$false)]
    [Switch]
	$SkipBuild = $false

)

$thumbprint = "1069E1903B197B71ADFCF6D2151BAF5920706A21"

$name = "WinClean"

$win86 = "..\bin\publish\win-x86"
$win64 = "..\bin\publish\win-x64"
$portableWin86 = "..\bin\publish\portable-win-x86"
$portableWin64 = "..\bin\publish\portable-win-x64"
$outputDir = "..\bin\setup"

# Publish
if (-Not $SkipBuild)
{
	dotnet publish .. /p:PublishProfile=win-x86 -o $win86
	dotnet publish .. /p:PublishProfile=win-x64 -o $win64
	dotnet publish .. /p:PublishProfile=portable-win-x86 -o $portableWin86
	dotnet publish .. /p:PublishProfile=portable-win-x64 -o $portableWin64
}


# Configure signing if available

if (Test-Path Cert:\CurrentUser\My\$thumbprint)
{
    $signFile = "signtool sign /fd SHA256 /sha1 $thumbprint /t http://timestamp.digicert.com"
}
else
{
    # no-op dummy exe
    $signFile = "systray"
}

# Compile scripts

$ISCC = "D:\Programmes\Inno Setup 6\ISCC.exe"

if (Test-Path $ISCC)
{
    & $ISCC /Ssigntool=`"$signFile `$f`" /D_Output=$outputDir /D_Arch=win-x86 /D_Path=$win86 InstallerScript.iss
    & $ISCC /Ssigntool=`"$signFile `$f`" /D_Output=$outputDir /D_Arch=win-x64 /D_Path=$win64 InstallerScript.iss
}

# Copy portable binaries

if (Test-Path "$portableWin86\$name.exe")
{
    Copy-Item "$portableWin86\$name.exe" "$outputDir\$name-x86.exe"
}
if (Test-Path "$PortableWin64\$name.exe")
{
    Copy-Item "$PortableWin64\$name.exe" "$outputDir\$name-x64.exe"
}