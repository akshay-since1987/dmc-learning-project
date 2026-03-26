using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ProposalManagement.Infrastructure.Services;

public class ProposalPdfService : IPdfGenerationService
{
    private sealed class DocumentRenderItem
    {
        public ProposalDocument Document { get; init; } = null!;
        public byte[]? ImageBytes { get; init; }
        public byte[]? PdfBytes { get; init; }
        public bool IsImage { get; init; }
        public bool IsPdf { get; init; }
    }

    private readonly IRepository<Proposal> _proposalRepo;
    private readonly IFileStorage _fileStorage;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<ProposalPdfService> _logger;

    public ProposalPdfService(IRepository<Proposal> proposalRepo, IFileStorage fileStorage, IHostEnvironment hostEnvironment, ILogger<ProposalPdfService> logger)
    {
        _proposalRepo = proposalRepo;
        _fileStorage = fileStorage;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    public async Task<string> GenerateStageNoteAsync(Guid proposalId, CancellationToken cancellationToken = default)
    {
        var proposal = await _proposalRepo.Query()
            .Include(p => p.Department)
            .Include(p => p.SubmittedBy)
            .Include(p => p.SubmitterDesignation)
            .Include(p => p.FundType)
            .Include(p => p.AccountHead)
            .Include(p => p.Ward)
            .Include(p => p.ProcurementMethod)
            .Include(p => p.Documents)
            .Include(p => p.StageHistory.OrderBy(h => h.CreatedAt))
                .ThenInclude(h => h.ActionBy)
            .Include(p => p.Signatures)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == proposalId, cancellationToken)
            ?? throw new InvalidOperationException($"Proposal {proposalId} not found");

        var attachmentItems = await BuildAttachmentRenderItemsAsync(proposal.Documents.Where(d => !d.IsDeleted), cancellationToken);
        var mainPdfBytes = GeneratePdfBytes(proposal, attachmentItems);
        var pdfBytes = MergeWithAttachmentPdfs(mainPdfBytes, attachmentItems);

        var fileName = $"{proposal.ProposalNumber.Replace("/", "-")}_{proposal.CurrentStage}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        using var stream = new MemoryStream(pdfBytes);
        var storagePath = await _fileStorage.SaveAsync(stream, fileName, $"generated/{proposalId}", cancellationToken);

        _logger.LogInformation("Stage PDF generated for Proposal {ProposalNumber} at stage {Stage}", proposal.ProposalNumber, proposal.CurrentStage);
        return storagePath;
    }

    private byte[] GeneratePdfBytes(Proposal proposal, IReadOnlyList<DocumentRenderItem> attachmentItems)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginTop(1.5f, Unit.Centimetre);
                page.MarginBottom(1.5f, Unit.Centimetre);
                page.MarginHorizontal(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, proposal));
                page.Content().Element(c => ComposeContent(c, proposal, attachmentItems));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    // ── Header ──────────────────────────────────────────────
    private static void ComposeHeader(IContainer container, Proposal proposal)
    {
        container.Column(col =>
        {
            col.Item().AlignCenter().Text("धुळे महानगरपालिका").Bold().FontSize(16);
            col.Item().AlignCenter().Text("Dhule Municipal Corporation").Bold().FontSize(12);
            col.Item().AlignCenter().Text("प्रशासकीय मान्यतेसाठी कार्यालयीन टिपणी").FontSize(11);
            col.Item().AlignCenter().Text("(Administrative Approval Office Note)").FontSize(9).Italic();
            col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    // ── Content ─────────────────────────────────────────────
    private static void ComposeContent(IContainer container, Proposal proposal, IReadOnlyList<DocumentRenderItem> attachmentItems)
    {
        container.PaddingVertical(8).Column(col =>
        {
            col.Spacing(6);

            // ── Proposal Info Bar ──
            col.Item().Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.Span("Proposal No: ").Bold();
                    t.Span(proposal.ProposalNumber);
                });
                row.RelativeItem().AlignRight().Text(t =>
                {
                    t.Span("Date: ").Bold();
                    t.Span(proposal.Date.ToString("dd-MM-yyyy"));
                });
            });

            col.Item().Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.Span("Stage: ").Bold();
                    t.Span(proposal.CurrentStage.ToString());
                });
                row.RelativeItem().AlignRight().Text(t =>
                {
                    t.Span("Push-backs: ").Bold();
                    t.Span(proposal.PushBackCount.ToString());
                });
            });

            col.Item().PaddingVertical(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

            // ── Step 1: Basic Info ──
            col.Item().Text("1. Basic Information / मूलभूत माहिती").Bold().FontSize(11);
            ComposeFieldTable(col, new (string, string?)[]
            {
                ("Subject / विषय", $"{proposal.Subject_En}\n{proposal.Subject_Alt}"),
                ("Reason / कारण", $"{proposal.Reason_En ?? ""}\n{proposal.Reason_Alt ?? ""}"),
                ("Department / विभाग", proposal.Department?.Name_En ?? "—"),
                ("Proposer / प्रस्तावक", $"{proposal.SubmittedBy?.FullName_En ?? ""} ({proposal.SubmitterDesignation?.Name_En ?? ""})"),
                ("Brief / संक्षिप्त", $"{proposal.BriefInfo_En}\n{proposal.BriefInfo_Alt}"),
                ("Fund Type / निधी प्रकार", proposal.FundType?.Name_En ?? "—"),
                ("Fund Year / आर्थिक वर्ष", proposal.FundYear),
                ("Account Head / प्रकार खंड", proposal.AccountHead?.Name_En ?? "—"),
                ("Ward / प्रभाग", proposal.Ward?.Name_En ?? "—"),
                ("Estimated Cost / अंदाजपत्रक", $"₹ {proposal.EstimatedCost:N2}"),
                ("Reference No.", proposal.ReferenceNumber),
            });

            // ── Step 2: Field Visit ──
            col.Item().PaddingTop(6).Text("2. Field Visit & Inspection / क्षेत्रीय भेट").Bold().FontSize(11);
            ComposeFieldTable(col, new (string, string?)[]
            {
                ("Site Inspection Done / स्थळ तपासणी", proposal.SiteInspectionDone ? "Yes / हो" : "No / नाही"),
            });

            // ── Step 3: Technical Sanction ──
            col.Item().PaddingTop(6).Text("3. Technical Sanction / तांत्रिक मंजुरी").Bold().FontSize(11);
            ComposeFieldTable(col, new (string, string?)[]
            {
                ("TS Date / तां. मंजुरी दिनांक", proposal.TechnicalApprovalDate?.ToString("dd-MM-yyyy") ?? "—"),
                ("TS Number / क्र.", proposal.TechnicalApprovalNumber ?? "—"),
                ("TS Cost / तां. मंजुरी रक्कम", proposal.TechnicalApprovalCost?.ToString("N2") != null ? $"₹ {proposal.TechnicalApprovalCost:N2}" : "—"),
                ("Competent Authority TA / सक्षम प्राधिकरण", proposal.CompetentAuthorityTADone ? "Yes / हो" : "No / नाही"),
            });

            // ── Step 4: Publishing ──
            col.Item().PaddingTop(6).Text("4. Publishing / प्रसिद्धी").Bold().FontSize(11);
            ComposeFieldTable(col, new (string, string?)[]
            {
                ("Procurement Method / खरेदी पद्धत", proposal.ProcurementMethod?.Name_En ?? "—"),
                ("Publication Days / प्रसिद्धी दिवस", proposal.PublicationDays?.ToString() ?? "—"),
                ("Tender Period Verified / निविदा कालावधी सत्यापित", proposal.TenderPeriodVerified ? "Yes / हो" : "No / नाही"),
            });

            // ── Step 5: Accounting ──
            col.Item().PaddingTop(6).Text("5. Accounting / लेखा").Bold().FontSize(11);
            ComposeFieldTable(col, new (string, string?)[]
            {
                ("Home ID", proposal.HomeId ?? "—"),
                ("Accounting Number / लेखा क्र.", proposal.AccountingNumber ?? "—"),
                ("Has Previous Expenditure / पूर्वीचा खर्च", proposal.HasPreviousExpenditure ? $"Yes — ₹ {proposal.PreviousExpenditureAmount:N2}" : "No"),
                ("Approved Budget / मंजूर अंदाजपत्रक", $"₹ {proposal.ApprovedBudget:N2}"),
                ("Proposed Work Cost / प्रस्तावित कामाची किंमत", $"₹ {proposal.ProposedWorkCost:N2}"),
                ("Remaining Balance / शिल्लक रक्कम", $"₹ {proposal.RemainingBalance:N2}"),
                ("Accountant Willing / लेखापाल सहमत", proposal.AccountantWillingToProcess ? "Yes / हो" : "No / नाही"),
            });

            // ── Step 6: Work Place ──
            col.Item().PaddingTop(6).Text("6. Work Place Details / कार्यस्थळ तपशील").Bold().FontSize(11);
            ComposeFieldTable(col, new (string, string?)[]
            {
                ("Within Palika / महापालिका हद्दीत", proposal.WorkPlaceWithinPalika ? "Yes / हो" : "No / नाही"),
                ("Site Ownership Verified / मालकी सत्यापित", proposal.SiteOwnershipVerified ? "Yes / हो" : "No / नाही"),
                ("NOC Obtained / ना हरकत प्रमाणपत्र", proposal.NocObtained ? "Yes / हो" : "No / नाही"),
                ("Legal Survey Done / कायदेशीर सर्वेक्षण", proposal.LegalSurveyDone ? "Yes / हो" : "No / नाही"),
                ("Court Case Pending / न्यायालयीन प्रकरण", proposal.CourtCasePending ? $"Yes — {proposal.CourtCaseDetails_En}" : "No"),
            });

            // ── Step 7: Compliance ──
            col.Item().PaddingTop(6).Text("7. Duplication & Compliance / दुहेरीकरण आणि अनुपालन").Bold().FontSize(11);
            ComposeFieldTable(col, new (string, string?)[]
            {
                ("Duplicate Fund Check / दुहेरी निधी तपासणी", proposal.DuplicateFundCheckDone ? "Yes / हो" : "No / नाही"),
                ("Same Work Other Fund / समान काम इतर निधीत", proposal.SameWorkProposedInOtherFund ? "Yes / हो" : "No / नाही"),
                ("Vendor Tenure Completed / विक्रेता कार्यकाळ पूर्ण", proposal.VendorTenureCompleted ? "Yes / हो" : "No / नाही"),
                ("Legal Obstacle / कायदेशीर अडथळा", proposal.LegalObstacleExists ? "Yes" : "No / नाही"),
                ("Audit Objection / लेखापरीक्षण आक्षेप", proposal.AuditObjectionExists ? $"Yes — {proposal.AuditObjectionDetails_En}" : "No"),
                ("DLP Check / डी.एल.पी. तपासणी", proposal.DlpCheckDone ? "Yes / हो" : "No / नाही"),
                ("Overall Compliance / एकूण अनुपालन", proposal.OverallComplianceConfirmed ? "Yes / हो" : "No / नाही"),
            });

            // ── Submitter Declaration & Remarks ──
            col.Item().PaddingTop(6).Text("8. Submitter Declaration & Remarks / सादरकर्त्याची घोषणा आणि टिप्पणी").Bold().FontSize(11);
            ComposeFieldTable(col, new (string, string?)[]
            {
                ("Declaration Accepted / घोषणा मान्य", proposal.SubmitterDeclarationAccepted ? "Yes / हो" : "No / नाही"),
                ("Declaration (English)", proposal.SubmitterDeclarationText_En ?? "—"),
                ("घोषणा (मराठी)", proposal.SubmitterDeclarationText_Alt ?? "—"),
                ("Remarks (English)", string.IsNullOrWhiteSpace(proposal.SubmitterRemarks_En) ? "—" : proposal.SubmitterRemarks_En),
                ("टिप्पणी (मराठी)", string.IsNullOrWhiteSpace(proposal.SubmitterRemarks_Alt) ? "—" : proposal.SubmitterRemarks_Alt),
            });

            // ── Attached Documents ──
            col.Item().PaddingTop(8).Text("9. Attached Documents / जोडलेली कागदपत्रे").Bold().FontSize(11);
            if (attachmentItems.Count == 0)
            {
                col.Item().Text("No supporting documents uploaded.").FontSize(9).FontColor(Colors.Grey.Darken1);
            }
            else
            {
                foreach (var item in attachmentItems.OrderBy(d => d.Document.CreatedAt))
                {
                    var (titleEn, titleAlt) = GetDocumentTypeTitle(item.Document.DocumentType);
                    col.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(docCol =>
                    {
                        docCol.Spacing(2);
                        docCol.Item().Text(t =>
                        {
                            t.Span($"{titleEn} / {titleAlt}").Bold().FontSize(9);
                        });
                        docCol.Item().Text($"File: {item.Document.FileName}").FontSize(8);
                        docCol.Item().Text($"Type: {item.Document.ContentType}  |  Size: {FormatFileSize(item.Document.FileSize)}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        docCol.Item().Text($"Uploaded: {item.Document.CreatedAt:dd-MM-yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);

                        if (item.IsImage && item.ImageBytes is not null)
                        {
                            docCol.Item()
                                .PaddingTop(4)
                                .AlignCenter()
                                .MaxHeight(220)
                                .Image(item.ImageBytes)
                                .FitArea();
                        }
                        else if (item.IsPdf)
                        {
                            docCol.Item().PaddingTop(2).Text("Included PDF attachment reference for review.").FontSize(8).FontColor(Colors.Blue.Darken1);
                        }
                    });
                }
            }

            // ── Approval History / Timeline ──
            if (proposal.StageHistory.Count > 0)
            {
                col.Item().PaddingTop(10).Text("Approval History / मंजुरी इतिहास").Bold().FontSize(11);
                col.Item().PaddingVertical(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                foreach (var entry in proposal.StageHistory.OrderBy(h => h.CreatedAt))
                {
                    col.Item().PaddingBottom(6).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(entryCol =>
                    {
                        entryCol.Item().Row(row =>
                        {
                            row.RelativeItem().Text(t =>
                            {
                                t.Span($"{entry.Action}").Bold();
                                t.Span($" — {entry.ActionByName_En} ({entry.ActionByDesignation_En})");
                            });
                            row.ConstantItem(120).AlignRight().Text(entry.CreatedAt.ToString("dd-MM-yyyy HH:mm")).FontSize(8);
                        });

                        entryCol.Item().Text(t =>
                        {
                            t.Span($"{entry.FromStage} → {entry.ToStage}").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });

                        if (!string.IsNullOrWhiteSpace(entry.Opinion_En))
                        {
                            entryCol.Item().PaddingTop(2).Text(t =>
                            {
                                t.Span("Opinion: ").Bold().FontSize(9);
                                t.Span(entry.Opinion_En).FontSize(9);
                            });
                        }
                        if (!string.IsNullOrWhiteSpace(entry.Remarks_En))
                        {
                            entryCol.Item().PaddingTop(2).Text(t =>
                            {
                                t.Span("Remarks: ").Bold().FontSize(9);
                                t.Span(entry.Remarks_En).FontSize(9);
                            });
                        }
                        if (entry.Action == WorkflowAction.PushBack && !string.IsNullOrWhiteSpace(entry.Reason_En))
                        {
                            entryCol.Item().PaddingTop(2).Background(Colors.Red.Lighten5).Padding(3).Text(t =>
                            {
                                t.Span("Push-back Reason: ").Bold().FontSize(9).FontColor(Colors.Red.Medium);
                                t.Span(entry.Reason_En).FontSize(9);
                            });
                        }

                        // Show signature indication if signed
                        if (entry.DscSignedAt.HasValue)
                        {
                            entryCol.Item().PaddingTop(2).Text(t =>
                            {
                                t.Span("✓ Digitally Signed ").FontSize(8).FontColor(Colors.Green.Darken2);
                                t.Span($"on {entry.DscSignedAt.Value:dd-MM-yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                            });
                        }
                    });
                }
            }

            // ── Signature Placeholders ──
            col.Item().PaddingTop(20).Row(row =>
            {
                row.RelativeItem().Column(sc =>
                {
                    sc.Item().Text("Prepared By / तयार केलेले").FontSize(9);
                    sc.Item().PaddingTop(30).Text(proposal.SubmittedBy?.FullName_En ?? "").Bold();
                    sc.Item().Text(proposal.SubmitterDesignation?.Name_En ?? "").FontSize(9);
                });
                row.RelativeItem().AlignRight().Column(sc =>
                {
                    sc.Item().AlignRight().Text("Approved By / मंजूर").FontSize(9);
                    sc.Item().PaddingTop(30).AlignRight().Text("_______________").Bold();
                    sc.Item().AlignRight().Text("Commissioner / आयुक्त").FontSize(9);
                });
            });
        });
    }

    // ── Field Table Helper ──────────────────────────────────
    private static void ComposeFieldTable(ColumnDescriptor col, (string Label, string? Value)[] fields)
    {
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(2);
                c.RelativeColumn(3);
            });

            foreach (var (label, value) in fields)
            {
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                    .Text(label).FontSize(9).Bold();
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                    .Text(value ?? "—").FontSize(9);
            }
        });
    }

    private async Task<IReadOnlyList<DocumentRenderItem>> BuildAttachmentRenderItemsAsync(IEnumerable<ProposalDocument> documents, CancellationToken cancellationToken)
    {
        var items = new List<DocumentRenderItem>();
        foreach (var doc in documents)
        {
            var ext = Path.GetExtension(doc.FileName)?.ToLowerInvariant() ?? string.Empty;
            var isImage = doc.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                || ext is ".png" or ".jpg" or ".jpeg" or ".webp";
            var isPdf = doc.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
                || ext == ".pdf";

            byte[]? imageBytes = null;
            byte[]? pdfBytes = null;
            if (isImage)
            {
                try
                {
                    imageBytes = await ReadAttachmentBytesAsync(doc.StoragePath, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load image attachment {DocumentId} for proposal PDF generation", doc.Id);
                }
            }

            if (isPdf)
            {
                try
                {
                    pdfBytes = await ReadAttachmentBytesAsync(doc.StoragePath, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load PDF attachment {DocumentId} for proposal PDF generation", doc.Id);
                }
            }

            items.Add(new DocumentRenderItem
            {
                Document = doc,
                ImageBytes = imageBytes,
                PdfBytes = pdfBytes,
                IsImage = isImage,
                IsPdf = isPdf
            });
        }

        return items;
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024d:N1} KB";
        return $"{bytes / (1024d * 1024d):N2} MB";
    }

    private async Task<byte[]> ReadAttachmentBytesAsync(string storagePath, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await _fileStorage.GetAsync(storagePath, cancellationToken);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, cancellationToken);
            return ms.ToArray();
        }
        catch (FileNotFoundException)
        {
            // Backward compatibility: older document uploads were stored relative to ContentRootPath.
            var normalized = storagePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            var contentRootPath = Path.Combine(_hostEnvironment.ContentRootPath, normalized);
            if (!File.Exists(contentRootPath))
                throw;

            return await File.ReadAllBytesAsync(contentRootPath, cancellationToken);
        }
    }

    private byte[] MergeWithAttachmentPdfs(byte[] mainPdfBytes, IReadOnlyList<DocumentRenderItem> attachmentItems)
    {
        var pdfAttachments = attachmentItems
            .Where(x => x.IsPdf && x.PdfBytes is not null)
            .Select(x => x.PdfBytes!)
            .ToList();

        if (pdfAttachments.Count == 0)
            return mainPdfBytes;

        using var output = new PdfDocument();
        AppendPdf(output, mainPdfBytes);

        foreach (var attachmentPdf in pdfAttachments)
        {
            try
            {
                AppendPdf(output, attachmentPdf);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not merge attached PDF into generated stage note");
            }
        }

        using var stream = new MemoryStream();
        output.Save(stream, false);
        return stream.ToArray();
    }

    private static void AppendPdf(PdfDocument output, byte[] inputPdfBytes)
    {
        using var ms = new MemoryStream(inputPdfBytes);
        using var input = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
        for (var i = 0; i < input.PageCount; i++)
        {
            output.AddPage(input.Pages[i]);
        }
    }

    private static (string En, string Alt) GetDocumentTypeTitle(DocumentType type) => type switch
    {
        DocumentType.EstimateCopy => ("Estimate Copy", "अंदाजपत्रक प्रत"),
        DocumentType.InspectionReport => ("Inspection Report", "तपासणी अहवाल"),
        DocumentType.SitePhoto => ("Site Photo", "स्थळ छायाचित्र"),
        DocumentType.TechnicalApprovalOrder => ("Technical Approval Order", "तांत्रिक मंजुरी आदेश"),
        DocumentType.NocDocument => ("NOC Document", "ना हरकत प्रमाणपत्र"),
        DocumentType.LegalDocument => ("Legal Document", "कायदेशीर दस्तऐवज"),
        DocumentType.FieldVisitReport => ("Field Visit Report", "क्षेत्र भेट अहवाल"),
        DocumentType.GeoTaggedPhoto => ("Geo-tagged Photo", "जिओ-टॅग छायाचित्र"),
        DocumentType.TechnicalSanctionDoc => ("Technical Sanction Document", "तांत्रिक मंजुरी दस्तऐवज"),
        DocumentType.AccountingDoc => ("Accounting Document", "लेखा दस्तऐवज"),
        DocumentType.OwnershipDoc => ("Ownership Document", "मालकी हक्क दस्तऐवज"),
        DocumentType.CourtDoc => ("Court Document", "न्यायालयीन दस्तऐवज"),
        DocumentType.DuplicateFundDoc => ("Duplicate Fund Check Document", "दुहेरी निधी तपासणी दस्तऐवज"),
        DocumentType.OtherFundDoc => ("Other Fund Document", "इतर निधी दस्तऐवज"),
        _ => ("Other Document", "इतर दस्तऐवज")
    };

    // ── Footer ──────────────────────────────────────────────
    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(t =>
        {
            t.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
            t.Span("Page ");
            t.CurrentPageNumber();
            t.Span(" of ");
            t.TotalPages();
            t.Span("  |  Generated: ");
            t.Span(DateTime.Now.ToString("dd-MM-yyyy HH:mm"));
        });
    }
}
