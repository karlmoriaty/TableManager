using Microsoft.Data.Sqlite;
using TableManager.DAL.Database;
using TableManager.DAL.Models;
using TableManager.DAL.Repositories;

namespace TableManager.BLL.Services;

public class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly OrderItemRepository _orderItemRepository;
    private readonly ProductRepository _productRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly StockRecordRepository _stockRecordRepository;
    private readonly DatabaseContext _dbContext;

    public OrderService()
    {
        _dbContext = DatabaseManager.GetSharedContext();
        _orderRepository = new OrderRepository(_dbContext);
        _orderItemRepository = new OrderItemRepository(_dbContext);
        _productRepository = new ProductRepository(_dbContext);
        _customerRepository = new CustomerRepository(_dbContext);
        _stockRecordRepository = new StockRecordRepository(_dbContext);
    }

    public OrderService(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
        _orderRepository = new OrderRepository(_dbContext);
        _orderItemRepository = new OrderItemRepository(_dbContext);
        _productRepository = new ProductRepository(_dbContext);
        _customerRepository = new CustomerRepository(_dbContext);
        _stockRecordRepository = new StockRecordRepository(_dbContext);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await _orderRepository.GetAllAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _orderRepository.GetByIdAsync(id);
    }

    public async Task<OrderDetail?> GetOrderDetailByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return null;

        var items = await _orderItemRepository.GetByOrderIdAsync(id);
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);

        return new OrderDetail
        {
            Order = order,
            Items = items.ToList(),
            Customer = customer
        };
    }

    public async Task<string> GenerateOrderNumberAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        var today = DateTime.Today.ToString("yyyyMMdd");
        var todayOrders = orders.Where(o => o.OrderDate.Date == DateTime.Today).ToList();
        var sequence = todayOrders.Count + 1;
        return $"ORD{today}{sequence:D4}";
    }

    public async Task<OrderDetail> CreateOrderAsync(int customerId, List<(int productId, int quantity)> items, string? notes = null)
    {
        var transaction = await _dbContext.BeginTransactionAsync();
        try
        {
            var orderNumber = await GenerateOrderNumberAsync();
            var totalAmount = 0m;
            var orderItems = new List<OrderItem>();

            foreach (var (productId, quantity) in items)
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    throw new Exception($"商品 ID {productId} 不存在");
                }
                if (product.StockQuantity < quantity)
                {
                    throw new Exception($"商品 {product.Name} 库存不足，当前库存: {product.StockQuantity}");
                }

                var beforeStock = product.StockQuantity;
                product.StockQuantity -= quantity;
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateAsync(product, transaction);

                var stockRecord = new StockRecord
                {
                    ProductId = productId,
                    Type = "Out",
                    Quantity = quantity,
                    BeforeStock = beforeStock,
                    AfterStock = product.StockQuantity,
                    Notes = $"订单 {orderNumber} 出库",
                    CreatedAt = DateTime.UtcNow
                };
                await _stockRecordRepository.AddAsync(stockRecord, transaction);

                var subtotal = product.Price * quantity;
                totalAmount += subtotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    Subtotal = subtotal,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var order = new Order
            {
                OrderNumber = orderNumber,
                CustomerId = customerId,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending.ToString(),
                OrderDate = DateTime.UtcNow,
                Notes = notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _orderRepository.AddAsync(order, transaction);

            foreach (var item in orderItems)
            {
                item.OrderId = order.Id;
                await _orderItemRepository.AddAsync(item, transaction);
            }

            await _dbContext.CommitTransactionAsync(transaction);

            var customer = await _customerRepository.GetByIdAsync(customerId);
            return new OrderDetail
            {
                Order = order,
                Items = orderItems,
                Customer = customer
            };
        }
        catch
        {
            await _dbContext.RollbackTransactionAsync(transaction);
            throw;
        }
    }

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new Exception($"订单 ID {orderId} 不存在");
        }

        var currentStatus = Enum.Parse<OrderStatus>(order.Status);

        if (!IsValidStatusTransition(currentStatus, newStatus))
        {
            throw new Exception($"无法从 {currentStatus} 状态转换到 {newStatus} 状态");
        }

        order.Status = newStatus.ToString();
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);
    }

    private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return currentStatus switch
        {
            OrderStatus.Pending => newStatus is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => newStatus is OrderStatus.Completed or OrderStatus.Cancelled,
            OrderStatus.Completed => false,
            OrderStatus.Cancelled => false,
            _ => false
        };
    }

    public async Task UpdateOrderAsync(Order order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);
    }

    public async Task DeleteOrderAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            throw new Exception($"订单 ID {id} 不存在");
        }

        if (order.Status != OrderStatus.Pending.ToString() && order.Status != OrderStatus.Cancelled.ToString())
        {
            throw new Exception("只有待处理或已取消的订单可以删除");
        }

        await _orderRepository.DeleteAsync(id);
    }

    public async Task<List<OrderStatus>> GetAvailableStatusTransitionsAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return new List<OrderStatus>();

        var currentStatus = Enum.Parse<OrderStatus>(order.Status);
        var availableStatuses = new List<OrderStatus>();

        foreach (OrderStatus status in Enum.GetValues(typeof(OrderStatus)))
        {
            if (IsValidStatusTransition(currentStatus, status))
            {
                availableStatuses.Add(status);
            }
        }

        return availableStatuses;
    }
}
