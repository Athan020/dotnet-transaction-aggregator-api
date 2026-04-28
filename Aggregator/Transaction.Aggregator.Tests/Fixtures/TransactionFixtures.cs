using System;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Tests.Fixtures;

public static class TransactionFixtures
{

    public static TransactionItem Salary() =>
        CreateTransactionItem(
            id: "1",
            amount: 5000.00m,
            fromAccountId: 1234567890,
            date: DateTime.UtcNow.AddDays(-10),
            description: "Monthly Salary",
            source: "Payroll");

    public static TransactionItem Grocery() =>
        CreateTransactionItem(
            id: "2",
            amount: -150.75m,
            fromAccountId: 1234567890,
            date: DateTime.UtcNow.AddDays(-5),
            description: "Grocery Store",
            source: "Card");
    
    public static TransactionItem OnlineSubscription() =>
        CreateTransactionItem(
            id: "3",
            amount: -200.00m,
            fromAccountId: 1234567890,
            date: DateTime.UtcNow.AddDays(-2),
            description: "Online Subscription - Netflix",
            source: "Card");
    
    public static TransactionItem Lotto() =>
        CreateTransactionItem(
            id: "4",
            amount: -75.00m,
            fromAccountId: 1234567890,
            date: DateTime.UtcNow.AddDays(-1),
            description: "Daily Lotto *Ithuba",
            source: "Prepaid");
    
    public static TransactionItem VodacomTopup() =>
        CreateTransactionItem(
            id: "5",
            amount: -120.00m,
            fromAccountId: 1234567890,
            date: DateTime.UtcNow.AddDays(-1),
            description: "Vodacom *Topup",
            source: "Prepaid");

    
    public static TransactionItem CreateTransactionItem(
        string id = "1",
        decimal? amount = 100.00m,
        long fromAccountId = 1234567890,
        DateTime? date = null,
        string description = "Test Transaction",
        string category = "Test Category",
        string source = "Test Source")
    {
        return new TransactionItem
        {
            Id = id,
            Amount = amount,
            FromAccountId = fromAccountId,
            Date = date ?? DateTime.UtcNow,
            Description = description,
            Category = category,
            Source = source
        };
    }
}
