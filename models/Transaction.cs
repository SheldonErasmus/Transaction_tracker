namespace transaction_tracker.models;

public class Transaction
{
    public required string Date { get; set; }
    public required string Description { get; set; }
    public required string Type { get; set; }
    public double Amount { get; set; }
}