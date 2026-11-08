namespace TableManager.DAL.Models;

public class StockRecord
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int BeforeStock { get; set; }
    public int AfterStock { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
