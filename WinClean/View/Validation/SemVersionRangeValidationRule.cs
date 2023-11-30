using System.Globalization;
using System.Windows.Controls;

using Semver;

namespace Scover.WinClean.View.Validation;

public sealed class SemVersionRangeValidationRule : ValidationRule
{
    public int MaxLength { get; set; } = 2048;
    public SemVersionRangeOptions Options { get; set; }

    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
        => SemVersionRange.TryParse((string)value.NotNull(), Options, out _, MaxLength)
            ? ValidationResult.ValidResult
            : new(false, Resources.UI.ScriptView.InvalidVersionRange);
}