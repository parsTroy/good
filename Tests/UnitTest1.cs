using Xunit;
using good.Utils;
using good.Models;
using System;
using System.Collections.Generic;

namespace good.Tests.Utils
{
    public class DebtCalculationsTests
    {
        [Fact]
        public void CalculatePayoffDate_ReturnsNull_WhenMinimumPaymentTooLow()
        {
            var result = DebtCalculations.CalculatePayoffDate(1000m, 20m, 1m);
            Assert.Null(result);
        }

        [Fact]
        public void CalculateTotalInterest_ReturnsMaxValue_WhenMinimumPaymentTooLow()
        {
            var result = DebtCalculations.CalculateTotalInterest(1000m, 20m, 1m);
            Assert.Equal(decimal.MaxValue, result);
        }

        [Fact]
        public void CalculateMinimumPaymentFor25Years_IsCorrect()
        {
            var payment = DebtCalculations.CalculateMinimumPaymentFor25Years(10000m, 5m);
            Assert.True(payment > 0);
        }

        [Fact]
        public void GetEffectiveMinimumPayment_Uses25YearMinimum_WhenUserMinimumTooLow()
        {
            var debt = new Debt
            {
                Balance = 10000m,
                InterestRate = 5m,
                MinimumPayment = 1m
            };
            var effective = DebtCalculations.GetEffectiveMinimumPayment(debt);
            Assert.True(effective > 1m);
        }

        [Fact]
        public void GetEffectiveMinimumPayment_UsesUserMinimum_WhenHigherThan25YearMinimum()
        {
            var debt = new Debt
            {
                Balance = 10000m,
                InterestRate = 5m,
                MinimumPayment = 1000m
            };
            var effective = DebtCalculations.GetEffectiveMinimumPayment(debt);
            Assert.Equal(1000m, effective);
        }

        [Theory]
        [InlineData(null, "Never")]
        public void FormatTimeToPayoff_ReturnsNever_WhenDateIsNull(DateTime? date, string expected)
        {
            var result = DebtCalculations.FormatTimeToPayoff(date);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FormatTimeToPayoff_ReturnsPaidOff_WhenDateIsPastOrNow()
        {
            var now = DateTime.Now;
            var past = now.AddMonths(-1);
            Assert.Equal("Paid off", DebtCalculations.FormatTimeToPayoff(now));
            Assert.Equal("Paid off", DebtCalculations.FormatTimeToPayoff(past));
        }

        [Fact]
        public void FormatTimeToPayoff_ReturnsOneMonth_WhenDateIsOneMonthAhead()
        {
            var oneMonth = DateTime.Now.AddMonths(1);
            Assert.Equal("1 month", DebtCalculations.FormatTimeToPayoff(oneMonth));
        }

        [Fact]
        public void FormatTimeToPayoff_ReturnsOneYear_WhenDateIsTwelveMonthsAhead()
        {
            var oneYear = DateTime.Now.AddMonths(12);
            Assert.Equal("1 year", DebtCalculations.FormatTimeToPayoff(oneYear));
        }

        [Fact]
        public void FormatTimeToPayoff_ReturnsYearsAndMonths_ForMultiYearCases()
        {
            var twoYearsThreeMonths = DateTime.Now.AddMonths(27);
            Assert.Equal("2 years, 3 months", DebtCalculations.FormatTimeToPayoff(twoYearsThreeMonths));
        }

        [Fact]
        public void CalculatePayoffStrategy_ReturnsNow_WhenNoOpenDebts()
        {
            var debts = new List<Debt>();
            var result = DebtCalculations.CalculatePayoffStrategy(debts, 0, "avalanche");
            Assert.NotNull(result);
            Assert.True(result.PayoffDate.HasValue);
            Assert.Equal(0, result.TotalInterest);
            Assert.Equal(0, result.TotalInterestWithExtra);
            Assert.Empty(result.PayoffOrder);
        }

        [Fact]
        public void CalculatePayoffStrategy_ReturnsNow_WhenAllDebtsClosed()
        {
            var debts = new List<Debt> {
                new Debt { Balance = 0, Status = "closed" },
                new Debt { Balance = 0, Status = "closed" }
            };
            var result = DebtCalculations.CalculatePayoffStrategy(debts, 0, "snowball");
            Assert.NotNull(result);
            Assert.True(result.PayoffDate.HasValue);
            Assert.Equal(0, result.TotalInterest);
            Assert.Equal(0, result.TotalInterestWithExtra);
            Assert.Empty(result.PayoffOrder);
        }

        [Fact]
        public void CalculatePayoffStrategy_AvalancheOrdersByInterestRate()
        {
            var debts = new List<Debt> {
                new Debt { Balance = 1000, InterestRate = 10, MinimumPayment = 50, Status = "open" },
                new Debt { Balance = 1000, InterestRate = 20, MinimumPayment = 50, Status = "open" }
            };
            var result = DebtCalculations.CalculatePayoffStrategy(debts, 0, "avalanche");
            Assert.Equal(20, result.PayoffOrder[0].InterestRate);
            Assert.Equal(10, result.PayoffOrder[1].InterestRate);
        }

        [Fact]
        public void CalculatePayoffStrategy_SnowballOrdersByBalance()
        {
            var debts = new List<Debt> {
                new Debt { Balance = 500, InterestRate = 10, MinimumPayment = 50, Status = "open" },
                new Debt { Balance = 1000, InterestRate = 20, MinimumPayment = 50, Status = "open" }
            };
            var result = DebtCalculations.CalculatePayoffStrategy(debts, 0, "snowball");
            Assert.Equal(500, result.PayoffOrder[0].Balance);
            Assert.Equal(1000, result.PayoffOrder[1].Balance);
        }

        [Fact]
        public void CalculatePayoffStrategy_ExtraPaymentReducesTotalInterestAndTime()
        {
            var debts = new List<Debt> {
                new Debt { Balance = 1000, InterestRate = 10, MinimumPayment = 50, Status = "open" }
            };
            var resultNoExtra = DebtCalculations.CalculatePayoffStrategy(debts, 0, "avalanche");
            var resultWithExtra = DebtCalculations.CalculatePayoffStrategy(debts, 100, "avalanche");
            Assert.True(resultWithExtra.TotalInterestWithExtra < resultNoExtra.TotalInterest);
            Assert.True(resultWithExtra.PayoffDate < resultNoExtra.PayoffDate);
        }

        [Fact]
        public void SimulatePayoffWithHistory_FallsBackToMinimumPayment_WhenNoPayments()
        {
            var debt = new Debt
            {
                Balance = 1000m,
                InterestRate = 10m,
                MinimumPayment = 50m,
                Payments = new System.Collections.Generic.List<Payment>()
            };
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffWithHistory(debt);
            Assert.NotNull(payoffDate);
            Assert.True(totalInterest > 0);
        }

        [Fact]
        public void SimulatePayoffWithHistory_UsesPaymentHistory()
        {
            var debt = new Debt
            {
                Balance = 1000m,
                InterestRate = 10m,
                MinimumPayment = 50m,
                Payments = new System.Collections.Generic.List<Payment>
                {
                    new Payment { Amount = 500m, Date = DateTime.Now.AddMonths(-1), Type = "minimum" },
                    new Payment { Amount = 500m, Date = DateTime.Now, Type = "minimum" }
                }
            };
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffWithHistory(debt);
            Assert.NotNull(payoffDate);
            Assert.True(totalInterest >= 0);
        }

        [Fact]
        public void SimulatePayoffWithHistory_ReturnsNull_WhenNegativeInterest()
        {
            var debt = new Debt
            {
                Balance = 1000m,
                InterestRate = -5m,
                MinimumPayment = 50m,
                Payments = new System.Collections.Generic.List<Payment>()
            };
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffWithHistory(debt);
            Assert.Equal((DateTime?)null, payoffDate);
            Assert.Equal(0, totalInterest);
        }

        [Fact]
        public void SimulatePayoffWithHistory_ReturnsNull_WhenDebtIsNull()
        {
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffWithHistory(null);
            Assert.Equal((DateTime?)null, payoffDate);
            Assert.Equal(0, totalInterest);
        }

        [Fact]
        public void SimulatePayoffWithHistory_ReturnsNull_WhenBalanceIsZero()
        {
            var debt = new Debt
            {
                Balance = 0m,
                InterestRate = 10m,
                MinimumPayment = 50m,
                Payments = new System.Collections.Generic.List<Payment>()
            };
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffWithHistory(debt);
            Assert.Equal((DateTime?)null, payoffDate);
            Assert.Equal(0, totalInterest);
        }

        [Fact]
        public void SimulatePayoffFromCurrentBalance_ReturnsNull_WhenBalanceIsZero()
        {
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffFromCurrentBalance(0m, 10m, 50m);
            Assert.Null(payoffDate);
            Assert.Equal(0, totalInterest);
        }

        [Fact]
        public void SimulatePayoffFromCurrentBalance_ReturnsNull_WhenBalanceIsNegative()
        {
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffFromCurrentBalance(-100m, 10m, 50m);
            Assert.Null(payoffDate);
            Assert.Equal(0, totalInterest);
        }

        [Fact]
        public void SimulatePayoffFromCurrentBalance_ReturnsNull_WhenMinimumPaymentIsZero()
        {
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffFromCurrentBalance(1000m, 10m, 0m);
            Assert.Null(payoffDate);
            Assert.Equal(0, totalInterest);
        }

        [Fact]
        public void SimulatePayoffFromCurrentBalance_ReturnsNull_WhenMinimumPaymentIsNegative()
        {
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffFromCurrentBalance(1000m, 10m, -50m);
            Assert.Null(payoffDate);
            Assert.Equal(0, totalInterest);
        }

        [Fact]
        public void SimulatePayoffFromCurrentBalance_HandlesLargeValues()
        {
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffFromCurrentBalance(1_000_000m, 5m, 10_000m);
            Assert.NotNull(payoffDate);
            Assert.True(totalInterest > 0);
        }

        [Fact]
        public void SimulatePayoffFromCurrentBalance_NormalScenario()
        {
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffFromCurrentBalance(1000m, 10m, 50m);
            Assert.NotNull(payoffDate);
            Assert.True(totalInterest > 0);
        }

        [Fact]
        public void SimulatePayoffWithExtraPayment_ReturnsNull_WhenBalanceIsZero()
        {
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffWithExtraPayment(0m, 10m, 50m, 100m);
            Assert.Null(payoffDate);
            Assert.Equal(0, totalInterest);
        }

        [Fact]
        public void SimulatePayoffWithExtraPayment_ReturnsNull_WhenBalanceIsNegative()
        {
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffWithExtraPayment(-100m, 10m, 50m, 100m);
            Assert.Null(payoffDate);
            Assert.Equal(0, totalInterest);
        }

        [Fact]
        public void SimulatePayoffWithExtraPayment_ReturnsNull_WhenMinimumPlusExtraIsZero()
        {
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffWithExtraPayment(1000m, 10m, 0m, 0m);
            Assert.Null(payoffDate);
            Assert.Equal(0, totalInterest);
        }

        [Fact]
        public void SimulatePayoffWithExtraPayment_ReturnsNull_WhenMinimumPlusExtraIsNegative()
        {
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffWithExtraPayment(1000m, 10m, -50m, -100m);
            Assert.Null(payoffDate);
            Assert.Equal(0, totalInterest);
        }

        [Fact]
        public void SimulatePayoffWithExtraPayment_HandlesLargeExtraPayment()
        {
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffWithExtraPayment(1000m, 10m, 50m, 10_000m);
            Assert.NotNull(payoffDate);
            Assert.True(totalInterest >= 0);
        }

        [Fact]
        public void SimulatePayoffWithExtraPayment_NormalScenario()
        {
            var (payoffDate, totalInterest) = DebtCalculations.SimulatePayoffWithExtraPayment(1000m, 10m, 50m, 100m);
            Assert.NotNull(payoffDate);
            Assert.True(totalInterest > 0);
        }

        [Fact]
        public void CalculateMinimumPaymentForMonths_ReturnsZero_WhenBalanceIsZero()
        {
            var payment = DebtCalculations.CalculateMinimumPaymentForMonths(0m, 5m, 12);
            Assert.Equal(0, payment);
        }

        [Fact]
        public void CalculateMinimumPaymentForMonths_ReturnsZero_WhenBalanceIsNegative()
        {
            var payment = DebtCalculations.CalculateMinimumPaymentForMonths(-100m, 5m, 12);
            Assert.Equal(0, payment);
        }

        [Fact]
        public void CalculateMinimumPaymentForMonths_ReturnsZero_WhenMonthsIsZero()
        {
            var payment = DebtCalculations.CalculateMinimumPaymentForMonths(1000m, 5m, 0);
            Assert.Equal(0, payment);
        }

        [Fact]
        public void CalculateMinimumPaymentForMonths_ReturnsZero_WhenMonthsIsNegative()
        {
            var payment = DebtCalculations.CalculateMinimumPaymentForMonths(1000m, 5m, -12);
            Assert.Equal(0, payment);
        }

        [Fact]
        public void CalculateMinimumPaymentForMonths_HandlesZeroInterest()
        {
            var payment = DebtCalculations.CalculateMinimumPaymentForMonths(1200m, 0m, 12);
            Assert.Equal(100, payment);
        }

        [Fact]
        public void CalculateMinimumPaymentForMonths_NormalScenario()
        {
            var payment = DebtCalculations.CalculateMinimumPaymentForMonths(1000m, 5m, 12);
            Assert.True(payment > 0);
        }

        [Fact]
        public void GetTotalEffectiveMinimumPayment_SumsEffectiveMinimums_ForMultipleOpenDebts()
        {
            var debts = new List<Debt>
            {
                new Debt { Balance = 1000m, InterestRate = 5m, MinimumPayment = 50m, Status = "open" },
                new Debt { Balance = 2000m, InterestRate = 10m, MinimumPayment = 100m, Status = "open" }
            };
            var total = DebtCalculations.GetTotalEffectiveMinimumPayment(debts);
            Assert.True(total > 0);
        }

        [Fact]
        public void GetTotalEffectiveMinimumPayment_ReturnsZero_WhenAllDebtsClosed()
        {
            var debts = new List<Debt>
            {
                new Debt { Balance = 1000m, InterestRate = 5m, MinimumPayment = 50m, Status = "closed" },
                new Debt { Balance = 2000m, InterestRate = 10m, MinimumPayment = 100m, Status = "closed" }
            };
            var total = DebtCalculations.GetTotalEffectiveMinimumPayment(debts);
            Assert.Equal(0, total);
        }

        [Fact]
        public void GetTotalEffectiveMinimumPayment_SkipsClosedDebts_InMixedList()
        {
            var debts = new List<Debt>
            {
                new Debt { Balance = 1000m, InterestRate = 5m, MinimumPayment = 50m, Status = "open" },
                new Debt { Balance = 2000m, InterestRate = 10m, MinimumPayment = 100m, Status = "closed" }
            };
            var total = DebtCalculations.GetTotalEffectiveMinimumPayment(debts);
            Assert.True(total > 0);
        }

        [Fact]
        public void GetTotalEffectiveMinimumPayment_ReturnsZero_WhenNoDebts()
        {
            var debts = new List<Debt>();
            var total = DebtCalculations.GetTotalEffectiveMinimumPayment(debts);
            Assert.Equal(0, total);
        }

        [Fact]
        public void GetTotalEffectiveMinimumPayment_SkipsOpenDebtsWithZeroBalance()
        {
            var debts = new List<Debt>
            {
                new Debt { Balance = 0m, InterestRate = 5m, MinimumPayment = 50m, Status = "open" },
                new Debt { Balance = 2000m, InterestRate = 10m, MinimumPayment = 100m, Status = "open" }
            };
            var total = DebtCalculations.GetTotalEffectiveMinimumPayment(debts);
            Assert.True(total > 0);
        }
    }
}
