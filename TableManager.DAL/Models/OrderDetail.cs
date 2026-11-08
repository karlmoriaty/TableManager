namespace TableManager.DAL.Models;

public class OrderDetail
{
    public Order Order { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public Customer? Customer { get; set; }
}
