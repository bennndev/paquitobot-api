using PaquitoBot.Application.Dtos.StudentFinances;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Services.Frappe;

public static class FrappeFeesPreviewRules
{
    public static FrappeFinancialPreviewItemDto Evaluate(
        FrappeFeesRow row, string expectedStudent, TuitionFee? local,
        bool requireFeeStructure = true)
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(row.Name))
            issues.Add("MISSING_EXTERNAL_ID");

        if (!string.Equals(row.Student?.Trim(), expectedStudent.Trim(), StringComparison.Ordinal))
            issues.Add("STUDENT_REFERENCE_MISMATCH");

        if (requireFeeStructure && string.IsNullOrWhiteSpace(row.FeeStructure))
            issues.Add("MISSING_FEE_STRUCTURE");

        if (row.Docstatus is not (0 or 1 or 2))
            issues.Add("INVALID_DOCSTATUS");

        var currency = row.Currency?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3 ||
            !currency.All(char.IsLetter))
        {
            issues.Add("INVALID_CURRENCY");
        }

        if (!row.GrandTotal.HasValue || row.GrandTotal.Value <= 0m)
            issues.Add("INVALID_GRAND_TOTAL");
        else if (decimal.Round(row.GrandTotal.Value, 2) != row.GrandTotal.Value)
            issues.Add("AMOUNT_PRECISION_UNSUPPORTED");

        if (!row.OutstandingAmount.HasValue ||
            row.OutstandingAmount.Value < 0m ||
            (row.GrandTotal.HasValue &&
             row.OutstandingAmount.Value > row.GrandTotal.Value))
        {
            issues.Add("INVALID_OUTSTANDING");
        }
        else if (decimal.Round(row.OutstandingAmount.Value, 2) != row.OutstandingAmount.Value)
        {
            issues.Add("OUTSTANDING_PRECISION_UNSUPPORTED");
        }

        if (!row.DueDate.HasValue)
            issues.Add("MISSING_DUE_DATE");

        var state = row.Docstatus switch
        {
            0 => "Draft",
            1 => "Submitted",
            2 => "Cancelled",
            _ => "Unknown"
        };

        if (local is not null && row.Docstatus == 1)
        {
            if (!string.Equals(local.CurrencyCode?.Trim(), currency,
                    StringComparison.OrdinalIgnoreCase))
                issues.Add("LOCAL_CURRENCY_MISMATCH");

            if (row.GrandTotal.HasValue && local.OriginalAmount != row.GrandTotal.Value)
                issues.Add("LOCAL_ORIGINAL_AMOUNT_MISMATCH");

            if (row.OutstandingAmount.HasValue &&
                local.OutstandingBalance.HasValue &&
                local.OutstandingBalance.Value != row.OutstandingAmount.Value)
                issues.Add("LOCAL_OUTSTANDING_MISMATCH");

            if (local.DueDate != row.DueDate)
                issues.Add("LOCAL_DUE_DATE_MISMATCH");
        }

        return new FrappeFinancialPreviewItemDto(
            row.Name?.Trim() ?? "",
            row.FeeStructure?.Trim(),
            row.FeeSchedule?.Trim(),
            currency,
            row.GrandTotal,
            row.OutstandingAmount,
            row.DueDate,
            state,
            local is not null,
            issues);
    }
}
