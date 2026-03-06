using CLIMobileBankingApp;
using System;
using System.Collections.Generic;

public abstract class BankAccount
{
    public string Name { get; }
    public string AccountNumber { get; }

    protected double balance;
    private string pin;

    private int loginAttempts = 0;
    public bool IsLocked { get; private set; }

    protected double dailyLimit;
    protected double withdrawnToday = 0;
    private DateTime lastWithdrawalDate;

    public List<Transaction> Transactions { get; }

    protected BankAccount(string name, string accNo, string pin, double initialDeposit)
    {
        Name = name;
        AccountNumber = accNo;
        this.pin = pin;
        balance = initialDeposit;
        Transactions = new List<Transaction>();
        lastWithdrawalDate = DateTime.Today;
        withdrawnToday = 0;
    }

    public bool ValidatePIN(string inputPin)
    {
        if (IsLocked) return false;

        if (pin == inputPin)
        {
            loginAttempts = 0;
            return true;
        }

        loginAttempts++;
        if (loginAttempts >= 3)
            IsLocked = true;

        return false;
    }

    public void ChangePIN(string newPin)
    {
        if (string.IsNullOrWhiteSpace(newPin) || newPin.Length != 4)
            throw new Exception("Invalid PIN format.");

        pin = newPin;
        loginAttempts = 0;   // reset attempts
        IsLocked = false;    // unlock account after successful change
    }

    public void Deposit(double amount)
    {
        if (amount <= 0 || amount > 50000)
            throw new Exception("Invalid deposit amount.");

        balance += amount;
        Transactions.Add(new Transaction(TransactionType.Deposit, amount, balance));
    }

    public abstract void Withdraw(double amount);

    public double ShowBalance()
    {
        return balance;
    }

    /// <summary>Check if daily limit needs to be reset (new day)</summary>
    protected void CheckAndResetDailyLimit()
    {
        if (lastWithdrawalDate.Date != DateTime.Today.Date)
        {
            withdrawnToday = 0;
            lastWithdrawalDate = DateTime.Today;
        }
    }

    /// <summary>True if the user has already reached or exceeded the daily withdrawal/transfer limit.</summary>
    public bool IsDailyWithdrawalLimitReached()
    {
        CheckAndResetDailyLimit();
        return withdrawnToday >= dailyLimit;
    }

    /// <summary>True if the user has already fully used the daily limit (no remaining amount).</summary>
    public bool IsDailyWithdrawalLimitFullyUsed()
    {
        CheckAndResetDailyLimit();
        return withdrawnToday >= dailyLimit;
    }

    /// <summary>Get the daily withdrawal limit amount.</summary>
    public double GetDailyWithdrawalLimit()
    {
        return dailyLimit;
    }

    /// <summary>Get the remaining amount that can be withdrawn today.</summary>
    public double GetRemainingDailyWithdrawalLimit()
    {
        CheckAndResetDailyLimit();
        double remaining = dailyLimit - withdrawnToday;
        return remaining > 0 ? remaining : 0;
    }

    /// <summary>Update withdrawn amount after successful withdrawal/transfer.</summary>
    protected void UpdateWithdrawnToday(double amount)
    {
        CheckAndResetDailyLimit();
        withdrawnToday += amount;
    }
}