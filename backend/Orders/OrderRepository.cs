using System.Globalization;
using CsvHelper;
using System.Text.Json;

public interface IOrderRepository
{
    public Task<List<Order>> GetAllOrders();
    Task<Order?> GetOrderById(string orderId);
    Task<List<Order>> SearchOrders(OrderStatus status);
}

public sealed class OrderRepository : IOrderRepository
{
    private readonly string _rootPath;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(IWebHostEnvironment env, ILogger<OrderRepository> logger)
    {
        _rootPath = env.ContentRootPath;
        _logger = logger;
    }
    public async Task<List<Order>> GetAllOrders()
    {
        return (await GetSystemAOrders().ToListAsync())
            .Concat(await GetSystemBOrders().ToListAsync())
            .OrderBy(o => o.OrderDate)
            .ToList();
    }

    private string GetDataPath(string fileName)
    {
        var filePath = Path.Combine(_rootPath, "..", "data", fileName);
        if (!File.Exists(filePath))
        {
            _logger.LogError("No file at {filePath}", filePath);
            return null;
        }
        return filePath;
    }

    private async IAsyncEnumerable<Order> GetSystemAOrders()
    {
        var jsonPath = GetDataPath("system_a_orders.json");
        if (jsonPath == null)
        {
            yield break;
        }

        await using var stream = File.OpenRead(jsonPath);
        await foreach (var jsonOrder in JsonSerializer.DeserializeAsyncEnumerable<SystemBOrder>(
                           stream,
                           new JsonSerializerOptions
                           {
                               PropertyNameCaseInsensitive = true
                           }))
        {
            if (jsonOrder is null)
                continue;

            if (GetOrderStatusFromSystemAStatus(jsonOrder.status) is not OrderStatus orderStatus)
            {
                _logger.LogError("Invalid order status.");
                continue;
            }

            yield return new Order
            {
                SourceSystem = "SystemA",
                OrderId = jsonOrder.orderID,
                CustomerName = jsonOrder.customer,
                OrderDate = jsonOrder.orderDate,
                TotalAmount = jsonOrder.totalAmount,
                Status = orderStatus
            };
        }
    }

    private static OrderStatus? GetOrderStatusFromSystemAStatus(string status)
    {
        return status?.ToUpperInvariant() switch
        {
            "PEND" => OrderStatus.Pending,
            "PROC" => OrderStatus.Processing,
            "SHIP" => OrderStatus.Shipped,
            "COMP" => OrderStatus.Completed,
            "CANC" => OrderStatus.Cancelled,
            _ => null
        };
    }

    private async IAsyncEnumerable<Order> GetSystemBOrders()
    {
        var csvPath = GetDataPath("system_b_orders.csv");
        if (csvPath == null)
        {
            yield break;
        }
        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        await foreach (var csvOrder in csv.GetRecordsAsync<SystemAOrder>())
        {
            if (!Enum.IsDefined(typeof(OrderStatus), csvOrder.order_status))
            {
                _logger.LogError("Invalid order status");
                continue;
            }
            if (!DateTime.TryParseExact(
                csvOrder.date_placed,
                "MM/dd/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var orderDate))
            {
                _logger.LogError("Invalid date");
                continue;
            }
            if (!decimal.TryParse(
                    csvOrder.total,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var orderTotal))
            {
                _logger.LogError("Invalid total");
                continue;
            }
            yield return new Order
            {
                SourceSystem = "SystemB",
                OrderId = csvOrder.order_num,
                Status = (OrderStatus)csvOrder.order_status,
                CustomerName = csvOrder.client_name,
                OrderDate = orderDate,
                TotalAmount = orderTotal
            };
        }
    }

    public async Task<Order?> GetOrderById(string orderId)
    {
        var orders = await GetAllOrders();
        return orders.FirstOrDefault(o => o.OrderId == orderId);
    }

    public async Task<List<Order>> SearchOrders(OrderStatus status)
    {
        var orders = await GetAllOrders();
        return orders.Where(o => o.Status == status).ToList();
    }
}