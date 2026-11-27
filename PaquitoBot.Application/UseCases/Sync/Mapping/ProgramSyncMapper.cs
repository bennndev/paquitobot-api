using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Sync.Mapping;

internal static class ProgramSyncMapper
{
    public static void Apply(FrappeProgramDto source, Program program)
    {
        program.Code = TextNormalizer.Optional(source.Code);
        program.Name = TextNormalizer.Required(source.Name);
        program.Description = TextNormalizer.Optional(source.Description);

        if (!string.IsNullOrWhiteSpace(source.Status))
        {
            program.Status = TextNormalizer.OptionalLower(source.Status);
        }
    }
}
