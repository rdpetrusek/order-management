public sealed class SystemBOrder
{
    public string orderID { get; set; } = default!;
    public string customer { get; set; } = default!;
    public DateTime orderDate { get; set; }
    public decimal totalAmount { get; set; }
    public string status { get; set; } = default!;
}