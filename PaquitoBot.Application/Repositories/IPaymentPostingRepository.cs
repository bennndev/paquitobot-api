using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Repositories;

// Finance-specific write port. Infrastructure owns transactions and concurrency control.
public interface IPaymentPostingRepository
{
    Task<Payment> RecordAsync(Payment candidate, CancellationToken cancellationToken = default);
}
