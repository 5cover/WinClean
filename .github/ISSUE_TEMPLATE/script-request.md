---
name: Script request
about: Suggest a new script to embed in all WinClean installations
title: "[SCRIPT]"
labels: enhancement
assignees: 5cover

---

**Purpose**
A clear and concise description of the purpose of this script.

**Manual alternative?**
Steps for reproducing the action executed by this script manually. If not available, explain why.

**Host**
The script host you chose and why.

**Additional context**
Add any other context or screenshots about the script.

**Script code**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Script>
  <Name>EnglishName</Name>
  <Name xml:lang="fr">FrenchName</Name>
  <Description>EnglishDescription</Description>
  <Description xml:lang="fr">FrenchDescription</Description>
  <Category>Maintenance/Debloating/Customization</Category>
  <Recommended>Safe/Dangerous/Limited</Recommended>
  <Impact>Ergonomics/Free storage space/Memory usage/Network usage/Privacy/Performance/Shutdown time/Startup time/Storage speed/Visuals</Impact>
  <Versions>>=10.0.0</Versions> <!-- Supported Windows version range in SemVer 2.0.0 standard range syntax (optional). If unspecified, the script considered to support all Windows versions. -->
  <Code>
    <Execute Host="Cmd/PowerShell/Regedit">code</Execute> <!-- (optional) -->
    <Enable Host="Cmd/PowerShell/Regedit">code</Enable> <!-- (optional) -->
    <Disable Host="Cmd/PowerShell/Regedit">code</Disable> <!-- (optional) -->
    <Detect Host="Cmd/PowerShell/Regedit">code</Detect> <!-- (optional) -->
  </Code>
</Script>
```
