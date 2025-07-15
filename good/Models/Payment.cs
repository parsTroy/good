namespace good.Models;

public class Payment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string Type { get; set; } = "minimum"; // "minimum" or "extra"
}