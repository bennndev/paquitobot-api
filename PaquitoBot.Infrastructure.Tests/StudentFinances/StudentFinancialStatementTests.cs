using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.UseCases.StudentFinances.Query;
using PaquitoBot.Domain.Models;
using Xunit;

namespace PaquitoBot.Infrastructure.Tests.StudentFinances;

public sealed class StudentFinancialStatementTests
{
    private static readonly DateOnly AsOf = new(2026, 11, 24);

    [Theory]
    [InlineData(0, 28, "Pending", 500, 0)]
    [InlineData(200, 28, "Partial", 300, 0)]
    [InlineData(500, 28, "Paid", 0, 0)]
    [InlineData(200, 20, "Overdue", 300, 300)]
    public async Task ComputesExpectedBalanceAndStatus(
        int paid, int dueDay, string status, int outstanding, int overdue)
    {
        var studentId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();
        var feeId = Guid.NewGuid();
        var fee = new TuitionFee
        {
            TuitionFeeId = feeId,
            StudentId = studentId,
            Description = "QA tuition",
            CurrencyCode = "PEN",
            OriginalAmount = 500m,
            OutstandingBalance = 500m - paid,
            DueDate = new DateOnly(2026, 11, dueDay),
            Status = "Pending"
        };
        var payments = paid > 0
            ? new[] { new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    StudentId = studentId,
                    TuitionFeeId = feeId,
                    Amount = paid,
                    PaidOn = new DateOnly(2026, 11, 21),
                    Status = "Paid",
                    CurrencyCode = "PEN"
                } }
            : Array.Empty<Payment>();

        var repo = new FakeFinanceReadRepository([fee], payments);
        var response = await new GetStudentFinancialStatementQueryHandler(repo).Handle(
            new GetStudentFinancialStatementQuery(studentId, requestingUserId, AsOf),
            CancellationToken.None);

        var obligation = Assert.Single(response.Obligations);
        var currency = Assert.Single(response.Currencies);
        Assert.Equal(status, obligation.CalculatedStatus);
        Assert.Equal((decimal)outstanding, obligation.OutstandingAmount);
        Assert.Equal((decimal)overdue, currency.TotalOverdue);
        Assert.Equal((decimal)paid, currency.TotalPaid);
        Assert.Equal(500m, currency.TotalScheduled);
        Assert.False(obligation.ReconciliationRequired);
        Assert.Equal(0, response.PaymentsNotCounted);
    }

    [Fact]
    public async Task CancelledFeeDoesNotCountAsOutstanding()
    {
        var studentId = Guid.NewGuid();
        var fee = new TuitionFee
        {
            TuitionFeeId = Guid.NewGuid(),
            StudentId = studentId,
            Description = "Cancelled",
            OriginalAmount = 500,
            CurrencyCode = "PEN",
            Status = "Cancelled",
            DueDate = new DateOnly(2026, 11, 20)
        };
        var result = await new GetStudentFinancialStatementQueryHandler(
            new FakeFinanceReadRepository([fee], [])).Handle(
                new GetStudentFinancialStatementQuery(studentId, Guid.NewGuid(), AsOf),
                CancellationToken.None);
        Assert.True(Assert.Single(result.Obligations).IsCancelled);
        Assert.Empty(result.Currencies);
    }

    [Fact]
    public async Task UnlinkedUserCannotReadFinancialStatement()
    {
        var repo = new FakeFinanceReadRepository([], []) { AllowAccess = false };
        var query = new GetStudentFinancialStatementQuery(Guid.NewGuid(), Guid.NewGuid(), AsOf);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new GetStudentFinancialStatementQueryHandler(repo)
                .Handle(query, CancellationToken.None));
    }

    private sealed class FakeFinanceReadRepository(
        IReadOnlyList<TuitionFee> fees,
        IReadOnlyList<Payment> payments) : IFinancialReadRepository
    {
        public bool AllowAccess { get; set; } = true;

        public Task<bool> StudentExistsAsync(Guid studentId,
            CancellationToken cancellationToken = default) => Task.FromResult(true);

        public Task<bool> CanUserAccessStudentAsync(Guid userId, Guid studentId,
            CancellationToken cancellationToken = default) => Task.FromResult(AllowAccess);

        public Task<IReadOnlyList<TuitionFee>> GetTuitionFeesAsync(Guid studentId,
            CancellationToken cancellationToken = default) => Task.FromResult(fees);

        public Task<IReadOnlyList<Payment>> GetPaymentsAsync(Guid studentId,
            CancellationToken cancellationToken = default) => Task.FromResult(payments);

        public Task<IReadOnlyList<Payment>> GetFilteredPaymentsAsync(
            Guid studentId, string? status, DateOnly? from, DateOnly? to,
            CancellationToken cancellationToken = default) => Task.FromResult(payments);

        public Task<IReadOnlyList<ProgramEnrollment>> GetProgramEnrollmentsAsync(
            Guid studentId, Guid? programId, Guid? academicPeriodId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ProgramEnrollment>>([]);

        public Task<IReadOnlyList<FeeStructure>> GetFeeStructuresForProgramsAsync(
            IReadOnlyCollection<Guid> programIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<FeeStructure>>([]);

        public Task<IReadOnlyList<FeeSchedule>> GetFeeSchedulesForStructuresAsync(
            IReadOnlyCollection<Guid> feeStructureIds, Guid? academicPeriodId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<FeeSchedule>>([]);
    }
}
