using System.Text.RegularExpressions;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Application.Common.Text;

public record MessageSource(Guid DocumentId, string Title);

public static partial class MessageSources
{
    private const string Marker = "\n\nSources:\n";

    public static string Append(string answer, IReadOnlyList<KnowledgeSource> sources)
    {
        if (sources.Count == 0)
        {
            return answer;
        }

        var lines = sources.Select(source => $"- {source.Title} [{source.DocumentId}]");

        return answer + Marker + string.Join("\n", lines);
    }

    public static (string Answer, IReadOnlyList<MessageSource> Sources) Split(string content)
    {
        var index = content.LastIndexOf(Marker, StringComparison.Ordinal);

        if (index < 0)
        {
            return (content, []);
        }

        var sources = new List<MessageSource>();

        foreach (var line in content[(index + Marker.Length)..].Split('\n'))
        {
            var match = SourceLine().Match(line);

            if (match.Success)
            {
                sources.Add(new MessageSource(Guid.Parse(match.Groups[2].Value), match.Groups[1].Value));
            }
        }

        return (content[..index], sources);
    }

    [GeneratedRegex(@"^- (.+) \[([0-9a-fA-F\-]{36})\]$")]
    private static partial Regex SourceLine();
}
