namespace Katino.Domain.Helpers;

public static class SortHelper
{
    public static string NormalizeSizeName(string sizeName)
    {
        return (sizeName ?? string.Empty).Trim().ToUpperInvariant();
    }

    public static int GetSizeSortGroup(string sizeName)
    {
        var normalized = NormalizeSizeName(sizeName);

        if (int.TryParse(normalized, out _))
        {
            return 1;
        }

        if (TryGetLetterSizeOrder(normalized, out _))
        {
            return 2;
        }

        return 3;
    }

    public static int GetSizeSortValue(string sizeName)
    {
        var normalized = NormalizeSizeName(sizeName);

        if (int.TryParse(normalized, out var numericValue))
        {
            return numericValue;
        }

        if (TryGetLetterSizeOrder(normalized, out var letterOrder))
        {
            return letterOrder;
        }

        return int.MaxValue;
    }

    public static bool TryGetLetterSizeOrder(string normalizedSize, out int order)
    {
        order = normalizedSize switch
        {
            "XS" => 1,
            "S" => 2,
            "M" => 3,
            "L" => 4,
            "XL" => 5,
            "XXL" => 6,
            "XXXL" => 7,
            _ => 0
        };

        return order != 0;
    }
}
