namespace good.Models;

public class Debt
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal InterestRate { get; set; }
    public decimal MinimumPayment { get; set; }
    public string Status { get; set; } = "open"; // "open" or "closed"
    public List<Payment> Payments { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}