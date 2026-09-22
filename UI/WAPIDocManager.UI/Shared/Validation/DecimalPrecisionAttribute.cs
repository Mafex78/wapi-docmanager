using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace WAPIDocManager.UI.Shared.Validation;

/// <summary>
/// Verifica precisione (cifre totali) e scala (cifre decimali) di un decimal,
/// equivalente allo ScalePrecision di FluentValidation usato da WAPIDocument
/// </summary>
/// <remarks>
/// <para>
/// Sorgente della regola: <c>WAPIDocument.Application/Validators/DocumentCreateUpdateRequestDocumentLineValidator.cs</c>
/// (Quantity <c>ScalePrecision(2, 9)</c>, UnitPrice <c>ScalePrecision(2, 12)</c>).
/// </para>
/// <para>
/// Regola applicata: decimali significativi ≤ <see cref="Scale"/> e cifre intere ≤ <see cref="Precision"/> - <see cref="Scale"/>
/// (es. precisione 9 e scala 2 → massimo 7 cifre intere). Gli zeri decimali finali non contano (1.50 è valido con scala 1).
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DecimalPrecisionAttribute : ValidationAttribute
{
    public DecimalPrecisionAttribute(int precision, int scale)
    {
        Precision = precision;
        Scale = scale;
    }

    /// <summary>Numero massimo di cifre totali.</summary>
    public int Precision { get; }

    /// <summary>Numero massimo di cifre decimali.</summary>
    public int Scale { get; }

    public override bool IsValid(object? value)
    {
        if (value is not decimal number)
        {
            return value is null;
        }

        // rimuove gli zeri decimali finali (1.50 -> 1.5)
        decimal normalized = number / 1.0000000000000000000000000000m;

        // la scala di un decimal è codificata nei bit 16-23 del quarto elemento di GetBits
        int actualScale = (decimal.GetBits(normalized)[3] >> 16) & 0xFF;

        decimal integerPart = Math.Truncate(Math.Abs(normalized));
        int integerDigits = integerPart == 0
            ? 0
            : integerPart.ToString(CultureInfo.InvariantCulture).Length;

        return actualScale <= Scale &&
               integerDigits <= Precision - Scale;
    }
}
