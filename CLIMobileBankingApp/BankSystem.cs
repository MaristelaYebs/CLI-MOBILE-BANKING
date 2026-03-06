using CLIMobileBankingApp;
using System;
using System.Collections.Generic;
using System.Linq;

public class BankSystem
{
    private List<BankAccount> accounts = new List<BankAccount>();
    private readonly Random random = new Random();

    public void AddAccount(BankAccount account)
    {
        accounts.Add(account);
    }

    public BankAccount FindAccount(string accNo)
    {
        return accounts.FirstOrDefault(a => a.AccountNumber == accNo);
    }

    // Generates a unique numeric account number
    public string GenerateUniqueAccountNumber()
    {
        string accNo;
        do
        {
            accNo = random.Next(100000000, 999999999).ToString(); // 9 digits
        } while (FindAccount(accNo) != null);

        return accNo;
    }

    public void Transfer(BankAccount from, BankAccount to, double amount)
    {
        if (from == to)
            throw new Exception("Cannot transfer to self.");

        if (amount <= 0)
            throw new Exception("Transfer amount must be greater than 0.");

        from.Withdraw(amount);
        to.Deposit(amount);

        from.Transactions.Add(new Transaction(TransactionType.Transfer, amount, from.ShowBalance()));
        to.Transactions.Add(new Transaction(TransactionType.Transfer, amount, to.ShowBalance()));
    }
}