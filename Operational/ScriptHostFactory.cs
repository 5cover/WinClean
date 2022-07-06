namespace Scover.WinClean.Operational;

/// <summary>Factory methods for <see cref="ScriptHost"/> objects.</summary>
public static class ScriptHostFactory
{
    /// <summary>Creates a <see cref="ScriptHost"/> obejct of the specified localized name.</summary>
    /// <returns>A new <see cref="ScriptHost"/> object.</returns>
    /// <exception cref="ArgumentException"><paramref name="localizedName"/> is not the localized name of any script host.</exception>
    public static ScriptHost FromLocalizedName(string localizedName)
        => new Cmd().LocalizedName == localizedName
            ? new Cmd()
            : new PowerShell().LocalizedName == localizedName
                ? new PowerShell()
                : new Regedit().LocalizedName == localizedName
                    ? new Regedit()
                    : throw new ArgumentException($"Localized name not found.", nameof(localizedName));

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