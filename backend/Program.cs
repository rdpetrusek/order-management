var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IOrderRepository, OrderRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/health", async () =>
{
    // Add any dependencies that may need time to startup
    // here and ensure they are ready.
    return Results.Ok;
});

app.MapGet("/api/orders", async (IOrderRepository orderRepository) => 
{ 
    var orders = await orderRepository.GetAllOrders();
    return Results.Ok(orders);
});

app.MapGet("/api/orders/search", async (string? status, IOrderRepository orderRepository) =>
{
    if (string.IsNullOrWhiteSpace(status))
    {
        return Results.BadRequest(new { error = "Status parameter is required" });
    }

    if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var orderStatus))
    {
        return Results.BadRequest(new { error = $"Invalid status value. Valid values are: {string.Join(", ", Enum.GetNames<OrderStatus>())}" });
    }
    
    var filteredOrders = await orderRepository.SearchOrders(orderStatus);
    return Results.Ok(filteredOrders);
});

app.MapGet("/api/orders/{orderId}", async (string orderId, IOrderRepository orderRepository) =>
{
    var order = await orderRepository.GetOrderById(orderId);
    if (order == null) {
        return Results.NotFound();
    }
    return Results.Ok(order);
});


app.Run();
