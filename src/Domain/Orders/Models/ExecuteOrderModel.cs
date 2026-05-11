using VibraHeka.Domain.Orders.Enums;

namespace VibraHeka.Domain.Orders.Models;

public class ExecuteOrderModel
{
    public String ProductID { get; set; } = string.Empty;
    public String UserID { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}
