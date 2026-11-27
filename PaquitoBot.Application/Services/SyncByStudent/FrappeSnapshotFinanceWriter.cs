using System.Globalization;
using System.Text.Json;
using PaquitoBot.Application.Common.Frappe;
using PaquitoBot.Application.Integrations.Frappe;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Application.UseCases.Sync.Mapping;

using static PaquitoBot.Application.Services.SyncByStudent.FrappeSnapshotJson;
using static PaquitoBot.Application.Services.SyncByStudent.FrappeSnapshotWrites;

namespace PaquitoBot.Application.Services.SyncByStudent;

internal sealed class FrappeSnapshotFinanceWriter(IUnitOfWork unitOfWork)
{
    public async Task<int> WriteAsync(JsonElement snapshot, Student student, string studentName,
        DateTime now, CancellationToken cancellationToken)
    {
        var processed = 0;
        var fees = new Dictionary<string, TuitionFee>();
        foreach (var row in Rows(snapshot, "invoices"))
        {
            EnsureStudent(row, studentName);
            var name = Required(row, "name");
            fees[name] = await Upsert(unitOfWork.Repository<TuitionFee>(), name, entity =>
            {
                entity.StudentId = student.StudentId;
                entity.Description = $"Factura {name}";
                entity.CurrencyCode = Optional(row, "currency") ?? "PEN";
                entity.OriginalAmount = Number(row, "grand_total") ?? 0;
                entity.OutstandingBalance = Number(row, "outstanding_amount");
                entity.IssuedOn = Date(row, "posting_date");
                entity.DueDate = Date(row, "due_date");
                entity.Status = Optional(row, "status");
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var paymentRows = new Dictionary<string, JsonElement>();
        foreach (var payment in Rows(snapshot, "payments"))
        {
            EnsureStudent(payment, studentName);
            paymentRows.Add(Required(payment, "name"), payment);
        }
        foreach (var row in Rows(snapshot, "payment_references"))
        {
            EnsureStudent(row, studentName);
            var paymentName = Required(row, "parent");
            var invoiceName = Required(row, "reference_name");
            if (!paymentRows.TryGetValue(paymentName, out var payment) ||
                !fees.TryGetValue(invoiceName, out var fee))
            {
                continue;
            }
            await Upsert(unitOfWork.Repository<Payment>(), $"{paymentName}:{invoiceName}", entity =>
            {
                entity.StudentId = student.StudentId;
                entity.TuitionFeeId = fee.TuitionFeeId;
                entity.CurrencyCode = fee.CurrencyCode;
                entity.Amount = Number(row, "allocated_amount") ?? 0;
                entity.PaidOn = Date(payment, "posting_date");
                entity.Reference = Optional(payment, "reference_no");
                entity.Status = Optional(payment, "status");
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }
        return processed;
    }
}
