using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;

namespace PaquitoBot.Domain.Validations;

public static class RepositoryValidation
{
    public static async Task EnsureExistsAsync<T>(
        this IGenericRepository<T> repository,
        string entityName,
        Guid id,
        CancellationToken cancellationToken = default) where T : class
    {
        if (!await repository.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException(entityName, id);
        }
    }
}
