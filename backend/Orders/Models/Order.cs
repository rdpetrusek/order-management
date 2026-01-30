public class Order
{
    public string SourceSystem {get; set;}
    public string OrderId {get; set;}
    public string CustomerName {get; set;}
    public DateTime OrderDate {get; set;}
    public decimal TotalAmount { get; set;}
    public OrderStatus Status {get; set;}
}