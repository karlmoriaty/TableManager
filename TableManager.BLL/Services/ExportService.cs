using OfficeOpenXml;
using TableManager.DAL.Database;
using TableManager.DAL.Models;
using TableManager.DAL.Repositories;

namespace TableManager.BLL.Services;

public class ExportService
{
    private readonly ProductRepository _productRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly OrderRepository _orderRepository;
    private readonly StockRecordRepository _stockRecordRepository;



    public ExportService()
    {
        var dbContext = DatabaseManager.GetSharedContext();
        _productRepository = new ProductRepository(dbContext);
        _customerRepository = new CustomerRepository(dbContext);
        _orderRepository = new OrderRepository(dbContext);
        _stockRecordRepository = new StockRecordRepository(dbContext);
    }

    public ExportService(DatabaseContext dbContext)
    {
        _productRepository = new ProductRepository(dbContext);
        _customerRepository = new CustomerRepository(dbContext);
        _orderRepository = new OrderRepository(dbContext);
        _stockRecordRepository = new StockRecordRepository(dbContext);
    }

    public async Task<byte[]> ExportProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("商品列表");

        worksheet.Cells["A1"].Value = "ID";
        worksheet.Cells["B1"].Value = "商品名称";
        worksheet.Cells["C1"].Value = "商品编码";
        worksheet.Cells["D1"].Value = "分类";
        worksheet.Cells["E1"].Value = "价格";
        worksheet.Cells["F1"].Value = "库存数量";
        worksheet.Cells["G1"].Value = "描述";
        worksheet.Cells["H1"].Value = "创建时间";
        worksheet.Cells["I1"].Value = "更新时间";

        using (var range = worksheet.Cells["A1:I1"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
        }

        var row = 2;
        foreach (var product in products)
        {
            worksheet.Cells[row, 1].Value = product.Id;
            worksheet.Cells[row, 2].Value = product.Name;
            worksheet.Cells[row, 3].Value = product.Code;
            worksheet.Cells[row, 4].Value = product.Category;
            worksheet.Cells[row, 5].Value = product.Price;
            worksheet.Cells[row, 6].Value = product.StockQuantity;
            worksheet.Cells[row, 7].Value = product.Description;
            worksheet.Cells[row, 8].Value = product.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[row, 9].Value = product.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> ExportCustomersAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("客户列表");

        worksheet.Cells["A1"].Value = "ID";
        worksheet.Cells["B1"].Value = "客户名称";
        worksheet.Cells["C1"].Value = "电话";
        worksheet.Cells["D1"].Value = "邮箱";
        worksheet.Cells["E1"].Value = "地址";
        worksheet.Cells["F1"].Value = "公司";
        worksheet.Cells["G1"].Value = "备注";
        worksheet.Cells["H1"].Value = "创建时间";
        worksheet.Cells["I1"].Value = "更新时间";

        using (var range = worksheet.Cells["A1:I1"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
        }

        var row = 2;
        foreach (var customer in customers)
        {
            worksheet.Cells[row, 1].Value = customer.Id;
            worksheet.Cells[row, 2].Value = customer.Name;
            worksheet.Cells[row, 3].Value = customer.Phone;
            worksheet.Cells[row, 4].Value = customer.Email;
            worksheet.Cells[row, 5].Value = customer.Address;
            worksheet.Cells[row, 6].Value = customer.Company;
            worksheet.Cells[row, 7].Value = customer.Notes;
            worksheet.Cells[row, 8].Value = customer.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[row, 9].Value = customer.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> ExportOrdersAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("订单列表");

        worksheet.Cells["A1"].Value = "ID";
        worksheet.Cells["B1"].Value = "订单编号";
        worksheet.Cells["C1"].Value = "客户ID";
        worksheet.Cells["D1"].Value = "总金额";
        worksheet.Cells["E1"].Value = "状态";
        worksheet.Cells["F1"].Value = "订单日期";
        worksheet.Cells["G1"].Value = "备注";
        worksheet.Cells["H1"].Value = "创建时间";
        worksheet.Cells["I1"].Value = "更新时间";

        using (var range = worksheet.Cells["A1:I1"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
        }

        var row = 2;
        foreach (var order in orders)
        {
            worksheet.Cells[row, 1].Value = order.Id;
            worksheet.Cells[row, 2].Value = order.OrderNumber;
            worksheet.Cells[row, 3].Value = order.CustomerId;
            worksheet.Cells[row, 4].Value = order.TotalAmount;
            worksheet.Cells[row, 5].Value = order.Status;
            worksheet.Cells[row, 6].Value = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[row, 7].Value = order.Notes;
            worksheet.Cells[row, 8].Value = order.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[row, 9].Value = order.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> ExportStockRecordsAsync()
    {
        var stockRecords = await _stockRecordRepository.GetAllAsync();
        
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("库存记录");

        worksheet.Cells["A1"].Value = "ID";
        worksheet.Cells["B1"].Value = "商品ID";
        worksheet.Cells["C1"].Value = "类型";
        worksheet.Cells["D1"].Value = "数量";
        worksheet.Cells["E1"].Value = "变动前库存";
        worksheet.Cells["F1"].Value = "变动后库存";
        worksheet.Cells["G1"].Value = "备注";
        worksheet.Cells["H1"].Value = "创建时间";

        using (var range = worksheet.Cells["A1:H1"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightCoral);
        }

        var row = 2;
        foreach (var record in stockRecords)
        {
            worksheet.Cells[row, 1].Value = record.Id;
            worksheet.Cells[row, 2].Value = record.ProductId;
            worksheet.Cells[row, 3].Value = record.Type;
            worksheet.Cells[row, 4].Value = record.Quantity;
            worksheet.Cells[row, 5].Value = record.BeforeStock;
            worksheet.Cells[row, 6].Value = record.AfterStock;
            worksheet.Cells[row, 7].Value = record.Notes;
            worksheet.Cells[row, 8].Value = record.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }
}
