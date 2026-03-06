using System;
using System.Linq;

class Program
{
    static BankSystem bank = new BankSystem();
    static BankAccount currentUser;

    // Format amount 
    static string FormatAmount(double amount)
    {
        return amount.ToString("N2");
    }

    // Allow user to cancel or exit on any prompt
    static bool HandleCancelExit(string input, out bool exitRequested)
    {
        exitRequested = false;
        if (input == null) return false;

        var trimmed = input.Trim().ToUpperInvariant();
        if (trimmed == "C")
        {
            return true; 
        }
        if (trimmed == "X")
        {
            exitRequested = true; 
            return true;
        }

        return false;
    }

    
    static int ReadIntInRange(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (int.TryParse(input, out int value) && value >= min && value <= max)
            {
                return value;
            }
            Console.WriteLine($"Please enter a number between {min} and {max}.");
        }
    }

    static double ReadPositiveDouble(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (!double.TryParse(input, out double value))
            {
                Console.WriteLine("Amount must be a valid number.");
                continue;
            }

            if (value <= 0)
            {
                Console.WriteLine("Amount must be greater than 0.");
                continue;
            }

            return value;
        }
    }
    static string ReadNonEmptyString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                return input.Trim();
            }

            Console.WriteLine("This field cannot be empty.");
        }
    }

    static void Main()
    {
        // Pre-register two accounts with fixed account numbers
        string accNo1 = "10001";
        var acc1 = new SavingsAccount("Maristela Yebra", accNo1, "1234", 1000);
        bank.AddAccount(acc1);

        string accNo2 = "10002";
        var acc2 = new PremiumAccount("Jay Carlo", accNo2, "5678", 2000);
        bank.AddAccount(acc2);

        Console.WriteLine("Pre-registered accounts (for testing):");
        Console.WriteLine($"1. {acc1.Name} – Account No: {accNo1} (PIN: 1234)");
        Console.WriteLine($"2. {acc2.Name} – Account No: {accNo2} (PIN: 5678)");
        Console.WriteLine("\nPress any key to continue to the main menu...");
        Console.ReadKey();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== CLI MOBILE BANKING ===");
            Console.WriteLine("1. Create Account");
            Console.WriteLine("2. Login");
            Console.WriteLine("3. Exit");
            Console.Write("Choose: ");

            var input = Console.ReadLine();
            if (HandleCancelExit(input, out bool exitRequested))
            {
                if (exitRequested) Environment.Exit(0);
                continue;
            }
            if (!int.TryParse(input, out int mainChoice) || mainChoice < 1 || mainChoice > 3)
            {
                continue;
            }
            switch (mainChoice)
            {
                case 1: CreateAccount(); break;
                case 2: Login(); break;
                case 3: return;
            }
        }
    }

    static void CreateAccount()
    {
        try
        {
            string name;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Account Creation ===");
                Console.WriteLine("C - Cancel | X - Exit\n");
                Console.WriteLine("Full Name: (e.g., Maristela Yebra or Maristela L. Yebra)");
                Console.WriteLine("Letters, spaces, and periods only (no numbers)");
                Console.Write("> ");

                string input = Console.ReadLine()?.Trim();
                if (HandleCancelExit(input, out bool exitRequested))
                {
                    if (exitRequested) Environment.Exit(0);
                    return;
                }

                if (IsValidFullName(input))
                {
                    name = input;
                    break;
                }

                Console.WriteLine("\nInvalid name. Please use only letters, spaces, and periods.");
                Console.WriteLine("A full name must include at least first and last name.");
                Console.WriteLine("\nPress any key to try again...");
                Console.ReadKey(true);
            }

            string email;
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Full Name: {name}\n");
                Console.WriteLine("C - Cancel | X - Exit\n");
                Console.WriteLine("Email Address (must end with @gmail.com)");
                Console.Write("> ");

                string input = Console.ReadLine()?.Trim();
                if (HandleCancelExit(input, out bool exitRequested))
                {
                    if (exitRequested) Environment.Exit(0);
                    return;
                }

                if (IsValidEmail(input))
                {
                    if (input != input.ToLowerInvariant())
                    {
                        Console.Clear();
                        Console.WriteLine("Email must be all lowercase. Please re-enter.\n");
                        continue;
                    }
                    email = input.ToLowerInvariant();
                    break;
                }

                Console.Clear();
                Console.WriteLine("Invalid email. It must end with '@gmail.com'.\n");
            }

            // ----- PIN creation (digits 0-9 only, 4 digits) -----
            string pin;
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Full Name: {name}");
                Console.WriteLine($"Email    : {email}\n");
                Console.WriteLine("C - Cancel | X - Exit\n");
                Console.WriteLine("Create 4-digit PIN (digits 0-9 only)");
                Console.Write("> ");

                var firstPinInput = Console.ReadLine()?.Trim();
                if (HandleCancelExit(firstPinInput, out bool exitRequested))
                {
                    if (exitRequested) Environment.Exit(0);
                    return;
                }

                if (!IsValidPin(firstPinInput))
                {
                    Console.WriteLine("\nPIN must be exactly 4 digits, each from 0 to 9.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                Console.Write("\nRe-enter PIN: ");
                var confirmPinInput = Console.ReadLine()?.Trim();
                if (HandleCancelExit(confirmPinInput, out exitRequested))
                {
                    if (exitRequested) Environment.Exit(0);
                    return;
                }

                if (firstPinInput != confirmPinInput)
                {
                    Console.WriteLine("\nPINs do not match.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                pin = firstPinInput;
                break;
            }

            // ----- OTP generation and validation (with resend option) -----
            var rnd = new Random();
            bool otpValid = false;

            while (!otpValid)
            {
                // Generate new OTP
                string otp = rnd.Next(100000, 999999).ToString();

                int attempts = 0;
                const int maxAttempts = 3;

                while (attempts < maxAttempts)
                {
                    Console.Clear();
                    Console.WriteLine($"Full Name: {name}");
                    Console.WriteLine($"Email    : {email}\n");
                    Console.WriteLine("C - Cancel | X - Exit\n");
                    Console.WriteLine($"Your OTP is: {otp}");
                    Console.WriteLine($"\nAttempt {attempts + 1} of {maxAttempts}");
                    Console.Write("Enter OTP > ");

                    var otpInput = Console.ReadLine()?.Trim();
                    if (HandleCancelExit(otpInput, out bool exitRequested))
                    {
                        if (exitRequested) Environment.Exit(0);
                        return;
                    }

                    if (otpInput == otp)
                    {
                        otpValid = true;
                        break;
                    }

                    attempts++;

                    if (attempts < maxAttempts)
                    {
                        Console.WriteLine("\nInvalid OTP.");
                        Console.WriteLine("Press any key to try again...");
                        Console.ReadKey(true);
                    }
                }

                if (otpValid) break;

                // After max attempts, ask if user wants to resend
                Console.Clear();
                Console.WriteLine($"Full Name: {name}");
                Console.WriteLine($"Email    : {email}\n");
                Console.WriteLine("C - Cancel | X - Exit\n");
                Console.WriteLine("Maximum attempts reached.");
                Console.Write("Would you like to resend a new OTP? (Y/N): ");

                string resendChoice = Console.ReadLine()?.Trim().ToUpper();
                if (HandleCancelExit(resendChoice, out bool exitReq))
                {
                    if (exitReq) Environment.Exit(0);
                    return;
                }

                if (resendChoice != "Y" && resendChoice != "YES")
                {
                    Console.WriteLine("\nOTP validation failed. Account creation cancelled.");
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey(true);
                    return;
                }
            }

            // ----- Initial deposit validation -----
            double deposit;
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Full Name: {name}");
                Console.WriteLine($"Email    : {email}\n");
                Console.WriteLine("C - Cancel | X - Exit\n");
                Console.WriteLine("Initial Deposit (minimum: 500)");
                Console.Write("> ");

                string input = Console.ReadLine()?.Trim();
                if (HandleCancelExit(input, out bool exitRequested))
                {
                    if (exitRequested) Environment.Exit(0);
                    return;
                }

                if (double.TryParse(input, out deposit) && deposit >= 500)
                    break;

                Console.WriteLine("\nInvalid amount. Minimum deposit is 500.");
                Console.WriteLine("\nPress any key to try again...");
                Console.ReadKey(true);
            }

            // ----- Account type selection -----
            int type;
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Full Name: {name}");
                Console.WriteLine($"Email    : {email}");
                Console.WriteLine($"Deposit  : {FormatAmount(deposit)}\n");
                Console.WriteLine("C - Cancel | X - Exit\n");
                Console.WriteLine("Select Account Type:");
                Console.WriteLine("  1 - Savings Account");
                Console.WriteLine("  2 - Premium Account");
                Console.Write("> ");

                string input = Console.ReadLine()?.Trim();
                if (HandleCancelExit(input, out bool exitRequested))
                {
                    if (exitRequested) Environment.Exit(0);
                    return;
                }

                if (int.TryParse(input, out type) && (type == 1 || type == 2))
                    break;

                Console.WriteLine("\nPlease enter 1 for Savings or 2 for Premium.");
                Console.WriteLine("\nPress any key to try again...");
                Console.ReadKey(true);
            }

            // ----- Generate unique account number and create account -----
            string accNo = bank.GenerateUniqueAccountNumber();

            BankAccount account;
            if (type == 1)
                account = new SavingsAccount(name, accNo, pin, deposit);
            else
                account = new PremiumAccount(name, accNo, pin, deposit);

            bank.AddAccount(account);

            // ----- Final success notice -----
            Console.Clear();
            Console.WriteLine("ACCOUNT CREATED SUCCESSFULLY\n");
            Console.WriteLine($"Full Name      : {name}");
            Console.WriteLine($"Email          : {email}");
            Console.WriteLine($"Account Number : {accNo}");
            Console.WriteLine($"Initial Deposit: {FormatAmount(deposit)}");
            Console.WriteLine($"Account Type   : {(type == 1 ? "Savings" : "Premium")}");
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"ERROR: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    static bool IsValidFullName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        string[] parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            return false;

        foreach (char c in name)
        {
            if (!char.IsLetter(c) && c != ' ' && c != '.')
                return false;
        }
        foreach (char c in name)
        {
            if (char.IsDigit(c))
                return false;
        }

        return true;
    }

    static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Convert to lowercase for validation
        email = email.ToLower();

        // Check if it ends with @gmail.com
        if (!email.EndsWith("@gmail.com"))
            return false;

        // Check for exactly one @ symbol
        if (email.Count(c => c == '@') != 1)
            return false;

        // Split into local and domain parts
        string[] parts = email.Split('@');
        string localPart = parts[0];
        string domainPart = parts[1];

        // Check domain is exactly gmail.com
        if (domainPart != "gmail.com")
            return false;

        // Validate local part characters (letters, numbers, dots, underscores, hyphens)
        foreach (char c in localPart)
        {
            if (!char.IsLetterOrDigit(c) && c != '.' && c != '_' && c != '-')
                return false;
        }

        // Ensure local part is not empty and doesn't start/end with special characters
        if (string.IsNullOrEmpty(localPart) ||
            localPart.StartsWith(".") ||
            localPart.EndsWith(".") ||
            localPart.StartsWith("_") ||
            localPart.EndsWith("_") ||
            localPart.StartsWith("-") ||
            localPart.EndsWith("-"))
            return false;

        // Prevent consecutive dots
        if (localPart.Contains(".."))
            return false;

        return true;
    }

    static bool IsValidPin(string pin)
    {
        if (string.IsNullOrEmpty(pin) || pin.Length != 4)
            return false;

        foreach (char c in pin)
        {
            if (!char.IsDigit(c)) // Allow digits 0-9
                return false;
        }
        return true;
    }

    static void Login()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Login ===");
            Console.WriteLine("C - Cancel | X - Exit\n");
            Console.Write("Account Number: ");

            var inputAcc = Console.ReadLine();
            if (HandleCancelExit(inputAcc, out bool exitRequested))
            {
                if (exitRequested) Environment.Exit(0);
                return;
            }

            if (string.IsNullOrWhiteSpace(inputAcc))
            {
                Console.Clear();
                Console.WriteLine("Account number is required.\n");
                Console.WriteLine("Press any key to try again...");
                Console.ReadKey(true);
                continue;
            }

            // Validate account number format (digits only)
            bool isValidFormat = true;
            foreach (char c in inputAcc.Trim())
            {
                if (!char.IsDigit(c))
                {
                    isValidFormat = false;
                    break;
                }
            }

            if (!isValidFormat)
            {
                Console.Clear();
                Console.WriteLine("Invalid account number. Account number must contain only digits.\n");
                Console.WriteLine("Press any key to try again...");
                Console.ReadKey(true);
                continue;
            }

            string accNo = inputAcc.Trim();
            var account = bank.FindAccount(accNo);
            if (account == null)
            {
                Console.Clear();
                Console.WriteLine($"Account '{accNo}' not found.\n");
                Console.WriteLine("Press any key to try again...");
                Console.ReadKey(true);
                continue;
            }

            for (int i = 0; i < 3; i++)
            {
                Console.Clear();
                Console.WriteLine("=== Login ===");
                Console.WriteLine("C - Cancel | X - Exit\n");
                Console.WriteLine($"Account Number: {accNo}");
                Console.Write($"Enter PIN (attempt {i + 1} of 3): ");

                var pinInput = Console.ReadLine();
                if (HandleCancelExit(pinInput, out exitRequested))
                {
                    if (exitRequested) Environment.Exit(0);
                    return;
                }

                // Validate PIN format (digits only)
                bool isPinValidFormat = true;
                if (pinInput != null)
                {
                    foreach (char c in pinInput.Trim())
                    {
                        if (!char.IsDigit(c))
                        {
                            isPinValidFormat = false;
                            break;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(pinInput) || pinInput.Trim().Length != 4 || !isPinValidFormat)
                {
                    Console.WriteLine("\nInvalid PIN. PIN must be exactly 4 digits (0-9).");
                    if (i < 2)
                    {
                        Console.WriteLine("Press any key to try again...");
                        Console.ReadKey(true);
                    }
                    continue;
                }

                if (account.ValidatePIN(pinInput))
                {
                    currentUser = account;
                    Dashboard();
                    return;
                }

                Console.WriteLine("\nIncorrect PIN.");
                if (i < 2)
                {
                    Console.WriteLine("Press any key to try again...");
                    Console.ReadKey(true);
                }
            }

            Console.Clear();
            Console.WriteLine("Account locked. Too many failed login attempts.\n");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            return;
        }
    }

    static void Dashboard()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"Welcome {currentUser.Name}");
            Console.WriteLine($"Current Balance: {FormatAmount(currentUser.ShowBalance())}\n");
            Console.WriteLine("C - Cancel (Logout) | X - Exit");
            Console.WriteLine("1. Deposit");
            Console.WriteLine("2. Withdraw");
            Console.WriteLine("3. Transfer");
            Console.WriteLine("4. Transactions");
            Console.WriteLine("5. Change PIN");
            Console.WriteLine("6. Logout");
            Console.Write("Choose: ");

            var input = Console.ReadLine();
            if (HandleCancelExit(input, out bool exitRequested))
            {
                if (exitRequested) Environment.Exit(0);
                return;
            }
            if (!int.TryParse(input, out int choice) || choice < 1 || choice > 6)
            {
                continue;
            }
            switch (choice)
            {
                case 1: Deposit(); break;
                case 2: Withdraw(); break;
                case 3: Transfer(); break;
                case 4: ShowTransactions(); break;
                case 5: ChangePIN(); break;
                case 6: return;
            }
        }
    }

    static void Deposit()
    {
        try
        {
            double amount;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Deposit ===");
                Console.WriteLine($"Current Balance: {FormatAmount(currentUser.ShowBalance())}\n");
                Console.WriteLine("C - Cancel | X - Exit\n");
                Console.WriteLine("Enter amount to deposit (minimum: 1.00, maximum: 50,000.00)");
                Console.Write("> ");

                string input = Console.ReadLine()?.Trim();

                if (HandleCancelExit(input, out bool exitRequested))
                {
                    if (exitRequested) Environment.Exit(0);
                    return;
                }

                if (!double.TryParse(input, out amount))
                {
                    Console.WriteLine("\nInvalid amount. Please enter a valid number.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                if (amount <= 0)
                {
                    Console.WriteLine("\nInvalid amount. Amount must be greater than zero.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                // Validate minimum deposit
                if (amount < 1.00)
                {
                    Console.WriteLine("\nInvalid amount. Minimum deposit amount is 1.00.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                // Validate maximum deposit
                if (amount > 50000)
                {
                    Console.WriteLine("\nInvalid amount. Maximum deposit amount is 50,000.00 per transaction.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                break;
            }

            double oldBalance = currentUser.ShowBalance();
            currentUser.Deposit(amount);

            // Generate simple reference number
            string reference = $"DEP-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";

            //notice
            Console.Clear();
            Console.WriteLine("=== Deposit Successful ===\n");
            Console.WriteLine($"Amount Deposited: {FormatAmount(amount)}");
            Console.WriteLine($"Previous Balance: {FormatAmount(oldBalance)}");
            Console.WriteLine($"New Balance     : {FormatAmount(currentUser.ShowBalance())}");
            Console.WriteLine($"Reference No.   : {reference}\n");
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine("=== Deposit Error ===\n");
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    static void Withdraw()
    {
        try
        {
            // Check if daily limit is already fully used
            if (currentUser.IsDailyWithdrawalLimitFullyUsed())
            {
                Console.Clear();
                Console.WriteLine("=== Withdraw ===\n");
                Console.WriteLine("You have already reached your daily withdrawal limit of " +
                                $"{FormatAmount(currentUser.GetDailyWithdrawalLimit())}.");
                Console.WriteLine("Please try again tomorrow.");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
                return;
            }

            double amount;
            double remainingLimit = currentUser.GetRemainingDailyWithdrawalLimit();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Withdraw ===");
                Console.WriteLine($"Current Balance: {FormatAmount(currentUser.ShowBalance())}");
                Console.WriteLine($"Daily Limit    : {FormatAmount(currentUser.GetDailyWithdrawalLimit())}");
                Console.WriteLine($"Remaining Today: {FormatAmount(remainingLimit)}\n");
                Console.WriteLine("C - Cancel | X - Exit\n");
                Console.WriteLine($"Enter amount to withdraw (minimum: 1.00, maximum: {FormatAmount(remainingLimit)})");
                Console.Write("> ");

                string input = Console.ReadLine()?.Trim();

                if (HandleCancelExit(input, out bool exitRequested))
                {
                    if (exitRequested) Environment.Exit(0);
                    return;
                }

                if (!double.TryParse(input, out amount))
                {
                    Console.WriteLine("\nInvalid amount. Please enter a valid number.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                if (amount <= 0)
                {
                    Console.WriteLine("\nInvalid amount. Amount must be greater than zero.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                if (amount < 1.00)
                {
                    Console.WriteLine("\nInvalid amount. Minimum withdrawal amount is 1.00.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                if (amount > currentUser.ShowBalance())
                {
                    Console.WriteLine($"\nInsufficient balance. Your current balance is {FormatAmount(currentUser.ShowBalance())}.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                if (amount > remainingLimit)
                {
                    Console.WriteLine($"\nAmount exceeds your remaining daily limit of {FormatAmount(remainingLimit)}.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                break;
            }

            double oldBalance = currentUser.ShowBalance();
            currentUser.Withdraw(amount);

            string reference = $"WDR-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";

            Console.Clear();
            Console.WriteLine("=== Withdrawal Successful ===\n");
            Console.WriteLine($"Amount Withdrawn: {FormatAmount(amount)}");
            Console.WriteLine($"Previous Balance: {FormatAmount(oldBalance)}");
            Console.WriteLine($"New Balance     : {FormatAmount(currentUser.ShowBalance())}");
            Console.WriteLine($"Reference No.   : {reference}\n");

            // Check if daily limit is now reached
            if (currentUser.IsDailyWithdrawalLimitFullyUsed())
            {
                Console.WriteLine("Note: You have reached your daily withdrawal limit.");
            }
            else
            {
                double newRemaining = currentUser.GetRemainingDailyWithdrawalLimit();
                Console.WriteLine($"Remaining daily limit: {FormatAmount(newRemaining)}");
            }

            if (currentUser.ShowBalance() < 500)
            {
                Console.WriteLine("Warning: Your balance is below 500.");
            }
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine("=== Withdrawal Error ===\n");
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    static void Transfer()
    {
        try
        {
            // Check if daily limit is already fully used
            if (currentUser.IsDailyWithdrawalLimitFullyUsed())
            {
                Console.Clear();
                Console.WriteLine("=== Transfer ===\n");
                Console.WriteLine("You have already reached your daily transfer/withdrawal limit of " +
                                $"{FormatAmount(currentUser.GetDailyWithdrawalLimit())}.");
                Console.WriteLine("Please try again tomorrow.");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
                return;
            }

            BankAccount target = null;
            string targetAccNo = null;
            double amount = 0;
            double remainingLimit = currentUser.GetRemainingDailyWithdrawalLimit();

            // Recipient account number input loop
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Transfer ===");
                Console.WriteLine($"Current Balance: {FormatAmount(currentUser.ShowBalance())}");
                Console.WriteLine($"Daily Limit    : {FormatAmount(currentUser.GetDailyWithdrawalLimit())}");
                Console.WriteLine($"Remaining Today: {FormatAmount(remainingLimit)}\n");
                Console.WriteLine("C - Cancel | X - Exit\n");
                Console.Write("Enter recipient account number: ");

                var input = Console.ReadLine();
                if (HandleCancelExit(input, out bool exitRequested))
                {
                    if (exitRequested) Environment.Exit(0);
                    return;
                }

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("\nInvalid input. Account number cannot be empty.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                if (!long.TryParse(input.Trim(), out _))
                {
                    Console.WriteLine("\nInvalid account number. Account number must contain only digits.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                targetAccNo = input.Trim();
                target = bank.FindAccount(targetAccNo);
                if (target == null)
                {
                    Console.WriteLine($"\nAccount not found. Account number '{targetAccNo}' does not exist.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                if (target == currentUser)
                {
                    Console.WriteLine("\nInvalid transfer. Cannot transfer to your own account.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                break;
            }

            // Amount input loop
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Transfer ===");
                Console.WriteLine($"Recipient Account: {targetAccNo}");
                Console.WriteLine($"Current Balance  : {FormatAmount(currentUser.ShowBalance())}");
                Console.WriteLine($"Daily Limit      : {FormatAmount(currentUser.GetDailyWithdrawalLimit())}");
                Console.WriteLine($"Remaining Today  : {FormatAmount(remainingLimit)}\n");
                Console.WriteLine("C - Cancel | X - Exit\n");
                Console.WriteLine($"Enter amount to transfer (minimum: 1.00, maximum: {FormatAmount(remainingLimit)})");
                Console.Write("> ");

                var input = Console.ReadLine();
                if (HandleCancelExit(input, out bool exitRequested))
                {
                    if (exitRequested) Environment.Exit(0);
                    return;
                }

                if (!double.TryParse(input, out amount))
                {
                    Console.WriteLine("\nInvalid amount. Please enter a valid number.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                if (amount <= 0)
                {
                    Console.WriteLine("\nInvalid amount. Amount must be greater than zero.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                if (amount > currentUser.ShowBalance())
                {
                    Console.WriteLine($"\nInsufficient balance. Your current balance is {FormatAmount(currentUser.ShowBalance())}.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                if (amount > remainingLimit)
                {
                    Console.WriteLine($"\nAmount exceeds your remaining daily limit of {FormatAmount(remainingLimit)}.");
                    Console.WriteLine("\nPress any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                break;
            }

            bank.Transfer(currentUser, target, amount);

            string reference = $"TRF-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";

            Console.Clear();
            Console.WriteLine("=== Transfer Successful ===\n");
            Console.WriteLine($"Reference No.     : {reference}");
            Console.WriteLine($"Transferred Amount: {FormatAmount(amount)}");
            Console.WriteLine($"Recipient Account : {targetAccNo}");
            Console.WriteLine($"Remaining Balance : {FormatAmount(currentUser.ShowBalance())}\n");

            // Check if daily limit is now reached
            if (currentUser.IsDailyWithdrawalLimitFullyUsed())
            {
                Console.WriteLine("Note: You have reached your daily transfer/withdrawal limit.");
            }
            else
            {
                double newRemaining = currentUser.GetRemainingDailyWithdrawalLimit();
                Console.WriteLine($"Remaining daily limit: {FormatAmount(newRemaining)}");
            }
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine("=== Transfer Error ===\n");
            Console.WriteLine($"Error: {ex.Message}");
        }
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    static void ChangePIN()
    {
        try
        {
            // STEP 1: Verify current PIN
            for (int i = 0; i < 3; i++)
            {
                Console.Clear();
                Console.WriteLine("=== Change PIN ===");
                Console.WriteLine("C - Cancel | X - Exit\n");
                Console.Write("Enter current PIN: ");

                string oldPin = Console.ReadLine()?.Trim();
                if (HandleCancelExit(oldPin, out bool exitRequested))
                {
                    if (exitRequested) Environment.Exit(0);
                    return;
                }

                if (!IsValidPin(oldPin))
                {
                    Console.WriteLine("\nInvalid PIN format.");
                    Console.WriteLine("Press any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                if (!currentUser.ValidatePIN(oldPin))
                {
                    Console.WriteLine("\nIncorrect current PIN.");
                    Console.WriteLine("Press any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                break;
            }

            // STEP 2: OTP Verification
            var rnd = new Random();
            bool otpVerified = false;

            while (!otpVerified)
            {
                string otp = rnd.Next(100000, 999999).ToString();
                int attempts = 0;

                while (attempts < 3)
                {
                    Console.Clear();
                    Console.WriteLine("=== OTP Verification ===");
                    Console.WriteLine("C - Cancel | X - Exit\n");
                    Console.WriteLine($"Your OTP is: {otp}");
                    Console.WriteLine($"Attempt {attempts + 1} of 3");
                    Console.Write("Enter OTP: ");

                    string input = Console.ReadLine()?.Trim();
                    if (HandleCancelExit(input, out bool exitRequested))
                    {
                        if (exitRequested) Environment.Exit(0);
                        return;
                    }

                    if (input == otp)
                    {
                        otpVerified = true;
                        break;
                    }

                    attempts++;
                    Console.WriteLine("\nInvalid OTP.");
                    Console.WriteLine("Press any key to try again...");
                    Console.ReadKey(true);
                }

                if (otpVerified) break;

                Console.Clear();
                Console.WriteLine("OTP attempts exceeded.");
                Console.Write("Resend new OTP? (Y/N): ");
                string resend = Console.ReadLine()?.Trim().ToUpper();

                if (HandleCancelExit(resend, out bool exitReq))
                {
                    if (exitReq) Environment.Exit(0);
                    return;
                }

                if (resend != "Y" && resend != "YES")
                {
                    Console.WriteLine("\nPIN change cancelled.");
                    Console.ReadKey(true);
                    return;
                }
            }

            // STEP 3: Enter new PIN
            string newPin;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Create New PIN ===");
                Console.WriteLine("C - Cancel | X - Exit\n");
                Console.Write("Enter new 4-digit PIN: ");

                string first = Console.ReadLine()?.Trim();
                if (HandleCancelExit(first, out bool exitRequested))
                {
                    if (exitRequested) Environment.Exit(0);
                    return;
                }

                if (!IsValidPin(first))
                {
                    Console.WriteLine("\nPIN must be exactly 4 digits.");
                    Console.WriteLine("Press any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                Console.Write("\nRe-enter new PIN: ");
                string second = Console.ReadLine()?.Trim();
                if (HandleCancelExit(second, out exitRequested))
                {
                    if (exitRequested) Environment.Exit(0);
                    return;
                }

                if (first != second)
                {
                    Console.WriteLine("\nPINs do not match.");
                    Console.WriteLine("Press any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                newPin = first;
                break;
            }

            // STEP 4: Update PIN
            currentUser.ChangePIN(newPin);

            Console.Clear();
            Console.WriteLine("=== PIN Changed Successfully ===\n");
            Console.WriteLine("Your PIN has been updated securely.");
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine("=== Error ===\n");
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    static void ShowTransactions()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Transaction ===\n");
            Console.WriteLine("C - Cancel (Back) | X - Exit\n");

            if (currentUser.Transactions.Count == 0)
            {
                Console.WriteLine("No transactions yet.");
            }
            else
            {
                string header = string.Format("{0,-10} | {1,-22} | {2,-10} | {3,15} | {4,18}",
                    "Ref. No.", "Date", "Type", "Amount", "Balance After");
                Console.WriteLine(header);
                Console.WriteLine(new string('-', header.Length));
                int refNo = 1;
                foreach (var t in currentUser.Transactions)
                {
                    Console.WriteLine(string.Format("{0,-10} | {1,-22} | {2,-10} | {3,15} | {4,18}",
                        $"TXN-{refNo}",
                        t.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                        t.Type.ToString(),
                        FormatAmount(t.Amount),
                        FormatAmount(t.BalanceAfterTransaction)));
                    refNo++;
                }
            }

            Console.Write("\nPress Enter to go back, or type C - Cancel, X - Exit: ");
            var line = Console.ReadLine();
            if (HandleCancelExit(line, out bool exitRequested))
            {
                if (exitRequested) Environment.Exit(0);
                return;
            }
            return;
        }
    }
}