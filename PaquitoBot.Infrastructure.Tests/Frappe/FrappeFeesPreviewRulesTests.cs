using PaquitoBot.Application.Integrations.Frappe.Finance;
using PaquitoBot.Domain.Models;
using Xunit;

namespace PaquitoBot.Infrastructure.Tests.Frappe;

public sealed class FrappeFeesPreviewRulesTests
{
    private static FrappeFeesRow Row() => new()
    {
        Name = "EDU-FEE-0001",
        Student = "EDU-STU-0001",
        FeeStructure = "EDU-FST-0001",
        FeeSchedule = "EDU-FSH-0001",
        Currency = "PEN",
        GrandTotal = 500m,
        OutstandingAmount = 300m,
        DueDate = new DateOnly(2026, 11, 25),
        Docstatus = 1
    };

    [Fact]
    public void SubmittedValidFeesHasNoIssues()
    {
        var result = FrappeFeesPreviewRules.Evaluate(Row(), "EDU-STU-0001", null);
        Assert.Empty(result.Issues);
        Assert.Equal("Submitted", result.DocumentState);
        Assert.False(result.ExistsLocally);
        Assert.Equal(500m, result.OriginalAmount);
    }

    [Fact]
    public void RejectsOverOutstanding()
    {
        var item = FrappeFeesPreviewRules.Evaluate(
            Row() with { OutstandingAmount = 501m }, "EDU-STU-0001", null);
        Assert.Contains("INVALID_OUTSTANDING", item.Issues);
    }

    [Fact]
    public void RejectsDifferentStudent()
    {
        var item = FrappeFeesPreviewRules.Evaluate(
            Row() with { Student = "ANOTHER-STUDENT" }, "EDU-STU-0001", null);
        Assert.Contains("STUDENT_REFERENCE_MISMATCH", item.Issues);
    }

    [Fact]
    public void ReportsLocalInconsistencyWithoutEditing()
    {
        var local = new TuitionFee
        {
            TuitionFeeId = Guid.NewGuid(),
            StudentId = Guid.NewGuid(),
            FrappeExternalId = "EDU-FEE-0001",
            CurrencyCode = "PEN",
            OriginalAmount = 500,
            OutstandingBalance = 200,
            DueDate = new DateOnly(2026, 11, 25),
            Description = "Tuition"
        };
        var item = FrappeFeesPreviewRules.Evaluate(Row(), "EDU-STU-0001", local);
        Assert.True(item.ExistsLocally);
        Assert.Contains("LOCAL_OUTSTANDING_MISMATCH", item.Issues);
        Assert.Equal(200m, local.OutstandingBalance);
    }

    [Theory]
    [InlineData(0, "Draft")]
    [InlineData(1, "Submitted")]
    [InlineData(2, "Cancelled")]
    public void ClassifiesDocumentStatus(int status, string expected)
    {
        var item = FrappeFeesPreviewRules.Evaluate(
            Row() with { Docstatus = status }, "EDU-STU-0001", null);
        Assert.Equal(expected, item.DocumentState);
    }

    [Fact]
    public void RequiresPrecisionCompatibleWithLocalModel()
    {
        var item = FrappeFeesPreviewRules.Evaluate(
            Row() with { GrandTotal = 500.001m }, "EDU-STU-0001", null);
        Assert.Contains("AMOUNT_PRECISION_UNSUPPORTED", item.Issues);
    }

    [Fact]
    public void SalesInvoiceDoesNotRequireFeeStructureField()
    {
        var row = Row() with { FeeStructure = null };
        var item = FrappeFeesPreviewRules.Evaluate(
            row, "EDU-STU-0001", null, requireFeeStructure: false);
        Assert.DoesNotContain("MISSING_FEE_STRUCTURE", item.Issues);
    }

    [Fact]
    public void FeesMustIncludeFeeStructureField()
    {
        var row = Row() with { FeeStructure = null };
        var item = FrappeFeesPreviewRules.Evaluate(row, "EDU-STU-0001", null);
        Assert.Contains("MISSING_FEE_STRUCTURE", item.Issues);
    }
}
