using PaquitoBot.Application.Dtos.KnowledgeDocuments;
using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.KnowledgeDocuments.Queries;


public record GetKnowledgeDocumentByIdQuery(Guid DocumentId) : IRequest<GetKnowledgeDocumentByIdResponse>;

public class GetKnowledgeDocumentByIdQueryValidator : AbstractValidator<GetKnowledgeDocumentByIdQuery>
{
    public GetKnowledgeDocumentByIdQueryValidator()
    {
        RuleFor(q => q.DocumentId).NotEmpty();
    }
}

internal sealed class GetKnowledgeDocumentByIdQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetKnowledgeDocumentByIdQuery, GetKnowledgeDocumentByIdResponse>
{
    public async Task<GetKnowledgeDocumentByIdResponse> Handle(
        GetKnowledgeDocumentByIdQuery request,
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
