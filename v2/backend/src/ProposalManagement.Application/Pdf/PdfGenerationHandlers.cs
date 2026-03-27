using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Pdf;

public record GeneratePdfCommand(Guid ProposalId, string PdfType) : IRequest<Result<GeneratePdfResult>>;

public record GeneratePdfResult(string StoragePath, string FileName, string Title_En, string? Title_Mr);

public class GeneratePdfHandler(
    IAppDbContext db,
    ICurrentUser user,
    IFileStorageService fileStorage,
    IPdfGenerationService pdfService,
    ILogger<GeneratePdfHandler> logger)
    : IRequestHandler<GeneratePdfCommand, Result<GeneratePdfResult>>
{
    public async Task<Result<GeneratePdfResult>> Handle(GeneratePdfCommand request, CancellationToken ct)
    {
        var proposal = await db.Proposals.FindAsync(new object[] { request.ProposalId }, ct);
        if (proposal is null) return Result<GeneratePdfResult>.NotFound("Proposal not found");

        PdfGenerationResult pdf = request.PdfType switch
        {
            "ApprovalOrder" => await pdfService.GenerateApprovalOrderAsync(request.ProposalId, ct),
            "FullProposal" => await pdfService.GenerateFullProposalPdfAsync(request.ProposalId, ct),
            _ => throw new ArgumentException($"Unknown PDF type: {request.PdfType}")
        };

        // Save to storage
        var storagePath = await fileStorage.SaveAsync(
            $"generated-pdfs/{request.ProposalId}", pdf.FileName, pdf.Content, ct);

        // Record in DB
        db.GeneratedPdfs.Add(new GeneratedPdf
        {
            Id = Guid.NewGuid(),
            ProposalId = request.ProposalId,
            PdfType = request.PdfType,
            Title_En = pdf.Title_En,
            Title_Mr = pdf.Title_Mr,
            StoragePath = storagePath,
            GeneratedById = user.UserId!.Value,
            FileSize = pdf.Content.Length,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);

        logger.LogInformation("PDF {PdfType} generated for Proposal {ProposalId}", request.PdfType, request.ProposalId);
        return Result<GeneratePdfResult>.Success(new GeneratePdfResult(storagePath, pdf.FileName, pdf.Title_En, pdf.Title_Mr));
    }
}

// ── Query: list generated PDFs ──
public record GetGeneratedPdfsQuery(Guid ProposalId) : IRequest<Result<List<GeneratedPdfDto>>>;

public record GeneratedPdfDto(Guid Id, string PdfType, string Title_En, string? Title_Mr, string StoragePath, long FileSize, DateTime CreatedAt);

public class GetGeneratedPdfsHandler(IAppDbContext db) : IRequestHandler<GetGeneratedPdfsQuery, Result<List<GeneratedPdfDto>>>
{
    public async Task<Result<List<GeneratedPdfDto>>> Handle(GetGeneratedPdfsQuery request, CancellationToken ct)
    {
        var items = await db.GeneratedPdfs
            .Where(g => g.ProposalId == request.ProposalId)
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new GeneratedPdfDto(g.Id, g.PdfType, g.Title_En, g.Title_Mr, g.StoragePath, g.FileSize ?? 0, g.CreatedAt))
            .ToListAsync(ct);

        return Result<List<GeneratedPdfDto>>.Success(items);
    }
}
