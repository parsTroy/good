using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using good.Models;
using good.Utils;
using Microsoft.AspNetCore.Components;

namespace good.Components.Payoff
{
    public partial class PayoffSummary : ComponentBase
    {
        /// <summary>
        /// The list of debts to analyze.
        /// </summary>
        [Parameter] public List<good.Models.Debt> Debts { get; set; } = new();
        /// <summary>
        /// The extra monthly payment amount.
        /// </summary>
        [Parameter] public decimal ExtraPayment { get; set; }
        /// <summary>
        /// Event callback when the extra payment changes.
        /// </summary>
        [Parameter] public EventCallback<decimal> OnExtraPaymentChange { get; set; }
        /// <summary>
        /// The payoff strategy ("avalanche" or "snowball").
        /// </summary>
        [Parameter] public string Strategy { get; set; } = "avalanche";
        /// <summary>
        /// Event callback when the strategy changes.
        /// </summary>
        [Parameter] public EventCallback<string> OnStrategyChange { get; set; }

        /// <summary>
        /// The total minimum payment required to pay off all debts in 25 years.
        /// </summary>
        private decimal BaselinePayment => DebtCalculations.GetTotalEffectiveMinimumPayment(Debts);

        /// <summary>
        /// The list of debts with their effective minimum payments.
        /// </summary>
        private List<good.Models.Debt> EffectiveDebts => Debts.Select(d => new good.Models.Debt {
            Id = d.Id,
            Name = d.Name,
            Balance = d.Balance,
            InterestRate = d.InterestRate,
            MinimumPayment = DebtCalculations.GetEffectiveMinimumPayment(d),
            Status = d.Status,
            Payments = d.Payments,
            CreatedAt = d.CreatedAt
        }).ToList();

        // Memoization cache for payoff strategy results
        private (string key, DebtCalculations.PayoffStrategyResult result)? _lastBaselineResult;
        private (string key, DebtCalculations.PayoffStrategyResult result)? _lastExtraResult;

        private string GetCacheKey(List<good.Models.Debt> debts, decimal extra, string strategy)
        {
            var debtsKey = string.Join("|", debts.Select(d => $"{d.Id}:{d.Balance}:{d.InterestRate}:{d.MinimumPayment}:{d.Status}"));
            return $"{debtsKey}|{extra}|{strategy}";
        }

        private DebtCalculations.PayoffStrategyResult GetBaselineResult()
        {
            var key = GetCacheKey(EffectiveDebts, 0, Strategy);
            if (_lastBaselineResult?.key == key)
                return _lastBaselineResult.Value.result;
            var result = DebtCalculations.CalculatePayoffStrategy(EffectiveDebts, 0, Strategy);
            _lastBaselineResult = (key, result);
            return result;
        }

        private DebtCalculations.PayoffStrategyResult GetExtraResult()
        {
            var key = GetCacheKey(EffectiveDebts, ExtraPayment, Strategy);
            if (_lastExtraResult?.key == key)
                return _lastExtraResult.Value.result;
            var result = DebtCalculations.CalculatePayoffStrategy(EffectiveDebts, ExtraPayment, Strategy);
            _lastExtraResult = (key, result);
            return result;
        }

        /// <summary>
        /// Optionally override ShouldRender for future render optimization.
        /// </summary>
        protected override bool ShouldRender()
        {
            // Always render for now; customize if needed for performance
            return true;
        }

        /// <summary>
        /// Handles the slider change event for extra payment.
        /// </summary>
        protected async Task OnSliderChanged(ChangeEventArgs e)
        {
            if (decimal.TryParse(e.Value?.ToString(), out var value))
            {
                await OnExtraPaymentChange.InvokeAsync(value);
            }
        }

        /// <summary>
        /// Formats the time saved as a human-readable string.
        /// </summary>
        protected string FormatTimeSaved(double? ms)
        {
            if (ms == null || ms <= 0) return "0 months";
            var months = (int)Math.Round(ms.Value / (1000 * 60 * 60 * 24 * 30.44));
            if (months < 1) return "<1 month";
            if (months == 1) return "1 month";
            if (months < 12) return $"{months} months";
            var years = months / 12;
            var rem = months % 12;
            if (years == 1 && rem == 0) return "1 year";
            if (years == 1) return $"1 year, {rem} months";
            if (rem == 0) return $"{years} years";
            return $"{years} years, {rem} months";
        }
    }
} 