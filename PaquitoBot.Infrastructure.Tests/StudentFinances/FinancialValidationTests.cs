using PaquitoBot.Application.UseCases.Payments.Command;
using PaquitoBot.Application.UseCases.StudentFinances.Command;
using Xunit;

namespace PaquitoBot.Infrastructure.Tests.StudentFinances;

public sealed class FinancialValidationTests
{
    [Fact]
    public void RejectsPaymentsWithoutIdempotencyKey()
    {
        var command = new CreatePaymentCommand(
            Guid.NewGuid(), Guid.NewGuid(), 200m, FrappeExternalId: null);
        var result = new CreatePaymentCommandValidator().Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FrappeExternalId");
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-10")]
    [InlineData("10.123")]
    public void RejectsInvalidPaymentAmounts(string amountText)
    {
        var command = new CreatePaymentCommand(
            Guid.NewGuid(), Guid.NewGuid(), decimal.Parse(amountText,
                System.Globalization.CultureInfo.InvariantCulture), FrappeExternalId: "QA-KEY-001");
        var result = new CreatePaymentCommandValidator().Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount");
    }

    [Theory]
    [InlineData("Fees", true)]
    [InlineData("Sales Invoice", true)]
    [InlineData("Payment Entry", false)]
    [InlineData("", false)]
    public void OnlyKnownFinancialSourcesAreAccepted(string source, bool valid)
    {
        var command = new PreviewFrappeStudentFeesCommand(Guid.NewGuid(), source);
        var result = new PreviewFrappeStudentFeesCommandValidator().Validate(command);
        Assert.Equal(valid, result.IsValid);
    }
}
