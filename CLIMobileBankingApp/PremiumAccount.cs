using CLIMobileBankingApp;
using System;

public class PremiumAccount : BankAccount
{
    public PremiumAccount(string name, string accNo, string pin, double deposit)
        : base(name, accNo, pin, deposit)
    {
        dailyLimit = 50000;
    }

    public override void Withdraw(double amount)
    {
        if (amount <= 0)
            throw new Exception("Invalid amount.");

        if (amount > balance)
            throw new Exception("Insufficient balance.");

        if (withdrawnToday + amount > dailyLimit)
            throw new Exception("Daily withdrawal limit exceeded.");

        balance -= amount;
        withdrawnToday += amount;

        Transactions.Add(new Transaction(TransactionType.Withdraw, amount, balance));
    }
}