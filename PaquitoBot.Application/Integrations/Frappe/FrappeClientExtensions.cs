namespace PaquitoBot.Application.Integrations.Frappe;

public static class FrappeClientExtensions
{
    private const int ListPageSize = 500;

    /// <summary>
    /// Reads every record of a Frappe document type by walking <see cref="IFrappeClient.GetPageAsync{T}"/> until
    /// Frappe reports that the scan is complete, so callers never depend on how Frappe caps a page.
    /// </summary>
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
