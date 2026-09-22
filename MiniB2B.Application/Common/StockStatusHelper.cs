namespace MiniB2B.Application.Common;

public static class StockStatusHelper
{
    public const string Available = "Available";
    public const string Critical = "Critical";
    public const string OutOfStock = "OutOfStock";

    public static string GetStockStatus(int stockQuantity, int criticalStockLevel)
    {
        if (stockQuantity > criticalStockLevel)
        {
            return Available;
        }

        return stockQuantity > 0 ? Critical : OutOfStock;
    }
}
