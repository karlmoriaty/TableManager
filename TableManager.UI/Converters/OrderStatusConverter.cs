using System.Globalization;
using System.Windows.Data;
using TableManager.DAL.Models;

namespace TableManager.UI.Converters;

public class OrderStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string statusString && Enum.TryParse<OrderStatus>(statusString, out var status))
        {
            return GetStatusDisplayName(status);
        }
        if (value is OrderStatus statusEnum)
        {
            return GetStatusDisplayName(statusEnum);
        }
        return value ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private string GetStatusDisplayName(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "待处理",
            OrderStatus.Shipped => "已发货",
            OrderStatus.Completed => "已完成",
            OrderStatus.Cancelled => "已取消",
            _ => status.ToString()
        };
    }
}
