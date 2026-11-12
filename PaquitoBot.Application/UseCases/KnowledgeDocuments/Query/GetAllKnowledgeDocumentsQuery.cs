using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.KnowledgeDocuments.Query;

public record GetAllKnowledgeDocumentsResponse(
    Guid DocumentId,
    Guid? CourseId,
    string Title,
    string? DocumentType,
    string? Source,
    string? SourceUrl,
    string? ContentHash,
    string Status,
    DateTime? IndexedAt,
    DateTime UpdatedAt);

public record GetAllKnowledgeDocumentsQuery(
    Guid? CourseId = null,
    string? Status = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IRequest<IReadOnlyList<GetAllKnowledgeDocumentsResponse>>;

public class GetAllKnowledgeDocumentsQueryValidator : AbstractValidator<GetAllKnowledgeDocumentsQuery>
{
    public GetAllKnowledgeDocumentsQueryValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(q => q.CourseId).NotEqual(Guid.Empty).When(q => q.CourseId.HasValue);
        RuleFor(q => q.Status).MaximumLength(40);
        RuleFor(q => q.Search).MaximumLength(255);
        RuleFor(q => q.Page).InclusiveBetween(1, 100_000);
        RuleFor(q => q.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetAllKnowledgeDocumentsQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllKnowledgeDocumentsQuery, IReadOnlyList<GetAllKnowledgeDocumentsResponse>>
{
    public async Task<IReadOnlyList<GetAllKnowledgeDocumentsResponse>> Handle(
        GetAllKnowledgeDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var status = TextNormalizer.Optional(request.Status);
        var search = TextNormalizer.Optional(request.Search);
        var skip = (request.Page - 1) * request.PageSize;

        var documents = await _unitOfWork.KnowledgeDocuments.SearchAsync(
            request.CourseId,
            status,
            search,
            skip,
            request.PageSize,
            cancellationToken);

        return documents
            .Select(d => new GetAllKnowledgeDocumentsResponse(
                d.DocumentId,
                d.CourseId,
                d.Title,
                d.DocumentType,
                d.Source,
                d.SourceUrl,
                d.ContentHash,
                d.Status,
                d.IndexedAt,
                d.UpdatedAt))
            .ToList();
    }
}
