using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.KnowledgeDocuments.Query;

public record GetKnowledgeDocumentByIdResponse(
    Guid DocumentId,
    Guid? CourseId,
    string Title,
    string? DocumentType,
    string? Source,
    string? SourceUrl,
    string? Content,
    string? ContentHash,
    string Status,
    DateTime? IndexedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record GetKnowledgeDocumentByIdUseCaseQuery(Guid DocumentId) : IRequest<GetKnowledgeDocumentByIdResponse>;

public class GetKnowledgeDocumentByIdUseCaseQueryValidator : AbstractValidator<GetKnowledgeDocumentByIdUseCaseQuery>
{
    public GetKnowledgeDocumentByIdUseCaseQueryValidator()
    {
        RuleFor(q => q.DocumentId).NotEmpty();
    }
}

public class GetKnowledgeDocumentByIdUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetKnowledgeDocumentByIdUseCaseQuery, GetKnowledgeDocumentByIdResponse>
{
    public async Task<GetKnowledgeDocumentByIdResponse> Handle(
        GetKnowledgeDocumentByIdUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.Repository<KnowledgeDocument>().GetByIdAsync(request.DocumentId, cancellationToken)
            ?? throw new NotFoundException("KnowledgeDocument", request.DocumentId);

        return new GetKnowledgeDocumentByIdResponse(
            document.DocumentId,
            document.CourseId,
            document.Title,
            document.DocumentType,
            document.Source,
            document.SourceUrl,
            document.Content,
            document.ContentHash,
            document.Status,
            document.IndexedAt,
            document.CreatedAt,
            document.UpdatedAt);
    }
}
