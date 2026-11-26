using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Application.Services;
namespace PaquitoBot.Infrastructure.Services.Frappe;

public static class FrappeClientExtensions
{
    private const int ListPageSize = 500;

    public static async Task<IReadOnlyList<T>> GetListAsync<T>(
        this IFrappeClient client,
        string doctype,
        IReadOnlyList<string> fields,
        CancellationToken cancellationToken = default)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(client);

        var items = new List<T>();
        var query = new FrappeQuery(doctype, fields, PageSize: ListPageSize);

        while (true)
        {
            var page = await client.GetPageAsync<T>(query, cancellationToken);
            items.AddRange(page.Items);

            if (page.NextOffset is not { } nextOffset)
            {
                return items;
            }

            query = query with { Offset = nextOffset };
        }
    }
}
