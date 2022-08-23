---
name: Script request
about: Suggest a new script to ship with all WinClean installations
title: "[SCRIPT]"
labels: enhancement
assignees: 5cover

---

**Purpose**
A clear and concise description of the purpose of this script.

**Manual alternative?**
Steps for reproducing the action executed by this script manually. If not available, explain why.

**The script host**
The script host you chose and why. Try to favor PowerShell if possible.

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
  <!-- Choose one -->
  <Category>Maintenance/Debloat/Customization</Category>
  <!-- Choose one -->
  <Recommended>Yes/No/Limited</Recommended>
  <!-- Choose one -->
  <Host>Cmd/PowerShell/Regedit</Host>
  <!-- Choose one -->
  <Impact>Ergonomics/FreeStorageSpace/MemoryUsage/NetworkUsage/Privacy/Performance/ShutdownTime/StartupTime/StorageSpeed/Visuals</Impact>
  <Code>Code</Code>
</Script>
```
