using PaquitoBot.Domain.Models;

namespace PaquitoBot.Domain.Repositories;

public interface IPaymentPostingRepository
{
    Task<Payment> RecordAsync(Payment candidate, CancellationToken cancellationToken = default);
}
