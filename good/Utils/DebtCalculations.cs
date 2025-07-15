using System;
using System.Collections.Generic;
using System.Linq;
using good.Models;

namespace good.Utils
{
    public static class DebtCalculations
    {
        /// <summary>
        /// Calculates the payoff date for a debt given balance, interest rate, and minimum payment.
        /// Returns null if the debt cannot be paid off with the given payment.
        /// </summary>
        public static DateTime? CalculatePayoffDate(decimal balance, decimal interestRate, decimal minimumPayment)
        {
            if (balance <= 0 || minimumPayment <= 0) return null;
            var monthlyRate = interestRate / 100m / 12m;
            var monthlyInterest = balance * monthlyRate;
            if (minimumPayment <= monthlyInterest) return null;
            var months = (int)Math.Ceiling(-Math.Log(1 - (double)(balance * monthlyRate) / (double)minimumPayment) / Math.Log(1 + (double)monthlyRate));
            var payoffDate = DateTime.Now.AddMonths(months);
            return payoffDate;
        }

        /// <summary>
        /// Calculates the total interest paid over the life of a debt given balance, interest rate, and minimum payment.
        /// Returns decimal.MaxValue if the payment is too low to ever pay off the debt.
        /// </summary>
        public static decimal CalculateTotalInterest(decimal balance, decimal interestRate, decimal minimumPayment)
        {
            if (balance <= 0 || minimumPayment <= 0) return 0;
            var monthlyRate = interestRate / 100m / 12m;
            var monthlyInterest = balance * monthlyRate;
            if (minimumPayment <= monthlyInterest) return decimal.MaxValue;
            var remainingBalance = balance;
            decimal totalInterest = 0;
            int months = 0;
            while (remainingBalance > 0.01m && months < 600)
            {
                var interest = remainingBalance * monthlyRate;
                totalInterest += interest;
                var principalPayment = Math.Min(minimumPayment - interest, remainingBalance);
                remainingBalance -= principalPayment;
                months++;
            }
            return totalInterest;
        }

        /// <summary>
        /// Formats a payoff date as a human-readable string (e.g., "2 years, 3 months").
        /// </summary>
        public static string FormatTimeToPayoff(DateTime? date)
        {
            if (date == null) return "Never";
            var now = DateTime.Now;
            var diffTime = date.Value - now;
            var diffMonths = (int)Math.Ceiling(diffTime.TotalDays / 30.44);
            if (diffMonths <= 0) return "Paid off";
            if (diffMonths == 1) return "1 month";
            if (diffMonths < 12) return $"{diffMonths} months";
            var years = diffMonths / 12;
            var remainingMonths = diffMonths % 12;
            if (years == 1 && remainingMonths == 0) return "1 year";
            if (years == 1) return $"1 year, {remainingMonths} months";
            if (remainingMonths == 0) return $"{years} years";
            return $"{years} years, {remainingMonths} months";
        }

        /// <summary>
        /// Result of a payoff strategy simulation.
        /// </summary>
        public class PayoffStrategyResult
        {
            /// <summary>The projected payoff date.</summary>
            public DateTime? PayoffDate { get; set; }
            /// <summary>Total interest paid with minimum payments only.</summary>
            public decimal TotalInterest { get; set; }
            /// <summary>Total interest paid with extra payments.</summary>
            public decimal TotalInterestWithExtra { get; set; }
            /// <summary>Time saved (in ms) by making extra payments.</summary>
            public double? TimeSaved { get; set; }
            /// <summary>The order in which debts are paid off.</summary>
            public List<Debt> PayoffOrder { get; set; } = new();
        }

        /// <summary>
        /// Simulates a payoff strategy (avalanche or snowball) for a list of debts and extra payment amount.
        /// </summary>
        public static PayoffStrategyResult CalculatePayoffStrategy(List<Debt> debts, decimal extraPayment, string strategy)
        {
            var openDebts = debts.Where(d => d.Status == "open" && d.Balance > 0).ToList();
            if (openDebts.Count == 0)
            {
                return new PayoffStrategyResult
                {
                    PayoffDate = DateTime.Now,
                    TotalInterest = 0,
                    TotalInterestWithExtra = 0,
                    TimeSaved = null,
                    PayoffOrder = new List<Debt>()
                };
            }
            List<Debt> sortedDebts;
            if (strategy == "avalanche")
                sortedDebts = openDebts.OrderByDescending(d => d.InterestRate).ToList();
            else
                sortedDebts = openDebts.OrderBy(d => d.Balance).ToList();
            var baseResult = SimulatePayoff(openDebts.Select(d => d.Clone()).ToList(), 0);
            var extraResult = SimulatePayoff(sortedDebts.Select(d => d.Clone()).ToList(), extraPayment);
            return new PayoffStrategyResult
            {
                PayoffDate = extraResult.PayoffDate,
                TotalInterest = baseResult.TotalInterest,
                TotalInterestWithExtra = extraResult.TotalInterest,
                TimeSaved = (baseResult.PayoffDate.HasValue && extraResult.PayoffDate.HasValue)
                    ? (baseResult.PayoffDate.Value - extraResult.PayoffDate.Value).TotalMilliseconds
                    : null,
                PayoffOrder = sortedDebts
            };
        }

        private class SimResult
        {
            public DateTime? PayoffDate { get; set; }
            public decimal TotalInterest { get; set; }
            public int Months { get; set; }
        }

        private static SimResult SimulatePayoff(List<Debt> debts, decimal extraPayment)
        {
            var workingDebts = debts.Select(d => d.Clone()).ToList();
            decimal totalInterest = 0;
            int months = 0;
            const int maxMonths = 600;
            while (workingDebts.Any(d => d.Balance > 0.01m) && months < maxMonths)
            {
                foreach (var debt in workingDebts)
                {
                    if (debt.Balance > 0)
                    {
                        var monthlyInterest = (debt.Balance * debt.InterestRate / 100m) / 12m;
                        debt.Balance += monthlyInterest;
                        totalInterest += monthlyInterest;
                    }
                }
                foreach (var debt in workingDebts)
                {
                    if (debt.Balance > 0)
                    {
                        var payment = Math.Min(debt.MinimumPayment, debt.Balance);
                        debt.Balance -= payment;
                    }
                }
                var remainingExtra = extraPayment;
                foreach (var debt in workingDebts)
                {
                    if (debt.Balance > 0 && remainingExtra > 0)
                    {
                        var extraApplied = Math.Min(remainingExtra, debt.Balance);
                        debt.Balance -= extraApplied;
                        remainingExtra -= extraApplied;
                        break;
                    }
                }
                months++;
            }
            var payoffDate = months < maxMonths ? DateTime.Now.AddDays(months * 30.44) : (DateTime?)null;
            return new SimResult
            {
                PayoffDate = payoffDate,
                TotalInterest = totalInterest,
                Months = months
            };
        }

        /// <summary>
        /// Simulates payoff using actual payment history for a single debt.
        /// </summary>
        public static (DateTime? PayoffDate, decimal TotalInterest) SimulatePayoffWithHistory(Debt debt)
        {
            if (debt == null || debt.Balance <= 0 || debt.InterestRate < 0)
                return (null, 0);
            var payments = debt.Payments?.OrderBy(p => p.Date).ToList() ?? new List<Payment>();
            if (payments.Count == 0)
            {
                // Fallback to minimum payment simulation
                var fallbackPayoffDate = CalculatePayoffDate(debt.Balance, debt.InterestRate, debt.MinimumPayment);
                var fallbackTotalInterest = CalculateTotalInterest(debt.Balance, debt.InterestRate, debt.MinimumPayment);
                return (fallbackPayoffDate, fallbackTotalInterest);
            }
            decimal balance = debt.Balance;
            decimal totalInterest = 0;
            var monthlyRate = debt.InterestRate / 100m / 12m;
            DateTime currentDate = payments[0].Date.Date;
            int paymentIndex = 0;
            int maxMonths = 600;
            int months = 0;
            while (balance > 0.01m && months < maxMonths)
            {
                // Accrue interest for the month
                var interest = balance * monthlyRate;
                totalInterest += interest;
                balance += interest;
                // Apply all payments for this month
                var monthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1);
                while (paymentIndex < payments.Count && payments[paymentIndex].Date >= monthStart && payments[paymentIndex].Date < monthEnd)
                {
                    var payment = payments[paymentIndex];
                    balance -= payment.Amount;
                    paymentIndex++;
                    if (balance <= 0.01m) break;
                }
                if (balance <= 0.01m) break;
                // Advance to next month
                currentDate = monthEnd;
                months++;
            }
            // After all payments, if balance remains, continue with minimum payment
            while (balance > 0.01m && months < maxMonths)
            {
                var interest = balance * monthlyRate;
                totalInterest += interest;
                balance += interest;
                var principalPayment = Math.Min(debt.MinimumPayment - interest, balance);
                if (principalPayment < 0.01m) break; // Avoid infinite loop if payment is too low
                balance -= principalPayment;
                currentDate = currentDate.AddMonths(1);
                months++;
            }
            var payoffDate = (balance <= 0.01m) ? currentDate : (DateTime?)null;
            return (payoffDate, totalInterest);
        }

        /// <summary>
        /// Simulates payoff from current balance, interest rate, and minimum payment (ignores payment history).
        /// </summary>
        public static (DateTime? PayoffDate, decimal TotalInterest) SimulatePayoffFromCurrentBalance(decimal balance, decimal interestRate, decimal minimumPayment)
        {
            if (balance <= 0 || minimumPayment <= 0) return (null, 0);
            var monthlyRate = interestRate / 100m / 12m;
            int months = 0;
            decimal totalInterest = 0;
            const int maxMonths = 600;
            while (balance > 0.01m && months < maxMonths)
            {
                var interest = balance * monthlyRate;
                totalInterest += interest;
                balance += interest;
                var principalPayment = Math.Min(minimumPayment - interest, balance);
                if (principalPayment < 0.01m) break; // Avoid infinite loop if payment is too low
                balance -= principalPayment;
                months++;
            }
            var payoffDate = (balance <= 0.01m) ? DateTime.Now.AddMonths(months) : (DateTime?)null;
            return (payoffDate, balance > 0.01m ? decimal.MaxValue : totalInterest);
        }

        /// <summary>
        /// Simulates payoff from current balance, interest rate, minimum payment, and extra monthly payment.
        /// </summary>
        public static (DateTime? PayoffDate, decimal TotalInterest) SimulatePayoffWithExtraPayment(decimal balance, decimal interestRate, decimal minimumPayment, decimal extraPayment)
        {
            if (balance <= 0 || minimumPayment + extraPayment <= 0) return (null, 0);
            var monthlyRate = interestRate / 100m / 12m;
            int months = 0;
            decimal totalInterest = 0;
            const int maxMonths = 600;
            while (balance > 0.01m && months < maxMonths)
            {
                var interest = balance * monthlyRate;
                totalInterest += interest;
                balance += interest;
                var paymentThisMonth = minimumPayment + extraPayment;
                var principalPayment = Math.Min(paymentThisMonth - interest, balance);
                if (principalPayment < 0.01m) break; // Avoid infinite loop if payment is too low
                balance -= principalPayment;
                months++;
            }
            var payoffDate = (balance <= 0.01m) ? DateTime.Now.AddMonths(months) : (DateTime?)null;
            return (payoffDate, balance > 0.01m ? decimal.MaxValue : totalInterest);
        }

        /// <summary>
        /// Calculates the minimum payment required to pay off a debt in a given number of months.
        /// </summary>
        public static decimal CalculateMinimumPaymentForMonths(decimal balance, decimal interestRate, int months)
        {
            if (balance <= 0 || months <= 0) return 0;
            var monthlyRate = interestRate / 100m / 12m;
            if (monthlyRate == 0) return balance / months;
            // Amortization formula: P = (r*B) / (1 - (1 + r)^-n)
            var denominator = 1 - (decimal)Math.Pow(1 + (double)monthlyRate, -months);
            if (denominator == 0) return balance / months;
            var payment = (monthlyRate * balance) / denominator;
            return Math.Ceiling(payment * 100) / 100; // round up to nearest cent
        }

        /// <summary>
        /// Calculates the minimum payment required to pay off a debt in 25 years (300 months).
        /// </summary>
        public static decimal CalculateMinimumPaymentFor25Years(decimal balance, decimal interestRate)
        {
            return CalculateMinimumPaymentForMonths(balance, interestRate, 300);
        }

        /// <summary>
        /// Returns the greater of the user minimum or the 25-year minimum payment for a debt.
        /// </summary>
        public static decimal GetEffectiveMinimumPayment(Debt debt)
        {
            var min25 = CalculateMinimumPaymentFor25Years(debt.Balance, debt.InterestRate);
            return Math.Max(debt.MinimumPayment, min25);
        }

        /// <summary>
        /// Returns the sum of effective minimums (per-debt max of user minimum or 25-year minimum) for a list of debts.
        /// </summary>
        public static decimal GetTotalEffectiveMinimumPayment(IEnumerable<Debt> debts)
        {
            return debts.Where(d => d.Status == "open" && d.Balance > 0)
                        .Sum(d => GetEffectiveMinimumPayment(d));
        }
    }

    public static class DebtExtensions
    {
        /// <summary>
        /// Creates a deep copy of a Debt object.
        /// </summary>
        public static Debt Clone(this Debt debt)
        {
            return new Debt
            {
                Id = debt.Id,
                Name = debt.Name,
                Balance = debt.Balance,
                InterestRate = debt.InterestRate,
                MinimumPayment = debt.MinimumPayment,
                Status = debt.Status,
                Payments = debt.Payments != null ? new List<Payment>(debt.Payments) : new List<Payment>(),
                CreatedAt = debt.CreatedAt
            };
        }
    }
} 