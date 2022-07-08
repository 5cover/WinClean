namespace Scover.WinClean.BusinessLogic.ScriptExecution;

/// <summary>Factory methods for <see cref="ScriptHost"/> objects.</summary>
public static class ScriptHostFactory
{
    /// <summary>Creates a <see cref="ScriptHost"/> obejct of the specified display name.</summary>
    /// <returns>A new <see cref="ScriptHost"/> object.</returns>
    /// <exception cref="ArgumentException"><paramref name="displayName"/> is not the display name of any script host.</exception>
    public static ScriptHost FromDisplayName(string displayName)
        => new Cmd().DisplayName == displayName
            ? new Cmd()
            : new PowerShell().DisplayName == displayName
                ? new PowerShell()
                : new Regedit().DisplayName == displayName
                    ? new Regedit()
                    : throw new ArgumentException($"Localized name not found.", nameof(displayName));

    /// <summary>Creates the <see cref="ScriptHost"/> object of the specified name.</summary>
    /// <returns>A new <see cref="ScriptHost"/> object.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is not the name of any script host.</exception>
    public static ScriptHost FromName(string name)
        => new Cmd().Name == name
            ? new Cmd()
            : new PowerShell().Name == name
                ? new PowerShell()
                : new Regedit().Name == name
                    ? new Regedit()
                    : throw new ArgumentException($"Name not found.", nameof(name));
}