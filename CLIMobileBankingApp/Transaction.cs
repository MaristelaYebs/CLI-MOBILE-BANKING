using CLIMobileBankingApp;
using System;

public class Transaction
{
    public TransactionType Type { get; }
    public double Amount { get; }
    public DateTime Date { get; }
    public double BalanceAfterTransaction { get; }

    public Transaction(TransactionType type, double amount, double balanceAfter)
    {
        Type = type;
        Amount = amount;
        Date = DateTime.Now;
        BalanceAfterTransaction = balanceAfter;
    }
}