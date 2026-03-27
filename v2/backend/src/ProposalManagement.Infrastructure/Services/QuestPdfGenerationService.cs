using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ProposalManagement.Infrastructure.Services;

public class QuestPdfGenerationService : IPdfGenerationService
{
    private readonly IAppDbContext _db;

    public QuestPdfGenerationService(IAppDbContext db)
    {
        _db = db;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<PdfGenerationResult> GenerateApprovalOrderAsync(Guid proposalId, CancellationToken ct = default)
    {
        var proposal = await _db.Proposals
            .Include(p => p.Department)
            .Include(p => p.DeptWorkCategory)
            .Include(p => p.Zone)
            .Include(p => p.Prabhag)
            .Include(p => p.CreatedBy)
            .Include(p => p.Estimate)
            .Include(p => p.BudgetDetail).ThenInclude(b => b!.BudgetHead)
            .Include(p => p.Approvals.OrderBy(a => a.CreatedAt))
                .ThenInclude(a => a.Actor).ThenInclude(u => u.Designation)
            .Include(p => p.Palika)
            .FirstOrDefaultAsync(p => p.Id == proposalId, ct)
            ?? throw new InvalidOperationException($"Proposal {proposalId} not found");

        var palika = proposal.Palika;
        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Header
                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text(palika.Name_Mr ?? palika.Name_En).Bold().FontSize(16);
                    col.Item().AlignCenter().Text(palika.Name_En).FontSize(11);
                    col.Item().AlignCenter().Text("प्रशासकीय मान्यता आदेश / Administrative Approval Order").Bold().FontSize(13);
                    col.Item().PaddingTop(5).LineHorizontal(1);
                });

                // Content
                page.Content().PaddingVertical(10).Column(col =>
                {
                    // Proposal Info
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Proposal No: {proposal.ProposalNumber}").Bold();
                        row.RelativeItem().AlignRight().Text($"Date: {proposal.ProposalDate:dd/MM/yyyy}");
                    });
                    col.Item().PaddingTop(5).Text($"Department / विभाग: {proposal.Department?.Name_En} / {proposal.Department?.Name_Mr}");
                    col.Item().Text($"Work Category / कार्य प्रकार: {proposal.DeptWorkCategory?.Name_En} / {proposal.DeptWorkCategory?.Name_Mr}");
                    col.Item().Text($"Zone / क्षेत्र: {proposal.Zone?.Name_En} / {proposal.Zone?.Name_Mr}");
                    col.Item().Text($"Prabhag / प्रभाग: {proposal.Prabhag?.Name_En} / {proposal.Prabhag?.Name_Mr}");

                    col.Item().PaddingTop(8).Text("Subject / विषय:").Bold();
                    col.Item().Text($"{proposal.WorkTitle_En}");
                    if (!string.IsNullOrEmpty(proposal.WorkTitle_Mr))
                        col.Item().Text(proposal.WorkTitle_Mr);

                    col.Item().PaddingTop(5).Text("Description / वर्णन:").Bold();
                    col.Item().Text(proposal.WorkDescription_En ?? "—");
                    if (!string.IsNullOrEmpty(proposal.WorkDescription_Mr))
                        col.Item().Text(proposal.WorkDescription_Mr);

                    // Financial details
                    col.Item().PaddingTop(10).LineHorizontal(0.5f);
                    col.Item().PaddingTop(5).Text("Financial Details / आर्थिक तपशील:").Bold().FontSize(11);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                        });

                        void AddRow(string label, string value)
                        {
                            table.Cell().Border(0.5f).Padding(4).Text(label);
                            table.Cell().Border(0.5f).Padding(4).AlignRight().Text(value);
                        }

                        AddRow("Estimated Cost / अंदाजित खर्च", FormatCurrency(proposal.Estimate?.EstimatedCost));
                        AddRow("Budget Head / अर्थसंकल्प शीर्ष", proposal.BudgetDetail?.BudgetHead?.Name_En ?? "—");
                        AddRow("Allocated Fund / वाटपित निधी", FormatCurrency(proposal.BudgetDetail?.AllocatedFund));
                        AddRow("Available Fund / उपलब्ध निधी", FormatCurrency(proposal.BudgetDetail?.CurrentAvailableFund));
                        AddRow("Balance / शिल्लक", FormatCurrency(proposal.BudgetDetail?.BalanceAmount));
                    });

                    // Approval chain
                    col.Item().PaddingTop(10).LineHorizontal(0.5f);
                    col.Item().PaddingTop(5).Text("Approval Chain / मान्यता क्रम:").Bold().FontSize(11);

                    foreach (var approval in proposal.Approvals)
                    {
                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.ConstantItem(100).Text($"[{approval.Action}]").Bold();
                            row.RelativeItem().Column(ac =>
                            {
                                ac.Item().Text($"{approval.ActorName_En} ({approval.StageRole})");
                                if (!string.IsNullOrEmpty(approval.ActorName_Mr))
                                    ac.Item().Text(approval.ActorName_Mr).FontSize(9);
                                if (!string.IsNullOrEmpty(approval.Opinion_En))
                                    ac.Item().Text($"Opinion: {approval.Opinion_En}").FontSize(9).FontColor(Colors.Grey.Darken1);
                                ac.Item().Text($"{approval.CreatedAt:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                            });
                        });
                    }

                    // Final status
                    col.Item().PaddingTop(15).AlignCenter()
                        .Text($"Status: {proposal.CurrentStage}").Bold().FontSize(12);
                });

                // Footer
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Generated on ").FontSize(8);
                    t.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8);
                    t.Span(" | Page ").FontSize(8);
                    t.CurrentPageNumber().FontSize(8);
                    t.Span(" of ").FontSize(8);
                    t.TotalPages().FontSize(8);
                });
            });
        }).GeneratePdf();

        var title_En = $"Approval Order - {proposal.ProposalNumber}";
        var title_Mr = $"प्रशासकीय मान्यता आदेश - {proposal.ProposalNumber}";
        var fileName = $"ApprovalOrder_{proposal.ProposalNumber.Replace("/", "-")}_{DateTime.Now:yyyyMMddHHmm}.pdf";

        return new PdfGenerationResult(pdfBytes, fileName, title_En, title_Mr);
    }

    public async Task<PdfGenerationResult> GenerateFullProposalPdfAsync(Guid proposalId, CancellationToken ct = default)
    {
        var proposal = await _db.Proposals
            .Include(p => p.Department)
            .Include(p => p.DeptWorkCategory)
            .Include(p => p.Zone).Include(p => p.Prabhag)
            .Include(p => p.RequestSource).Include(p => p.CreatedBy)
            .Include(p => p.Estimate).ThenInclude(e => e!.PreparedBy)
            .Include(p => p.TechnicalSanction)
            .Include(p => p.PramaDetail).ThenInclude(pd => pd!.FundType)
            .Include(p => p.PramaDetail).ThenInclude(pd => pd!.BudgetHead)
            .Include(p => p.BudgetDetail).ThenInclude(b => b!.WorkExecutionMethod)
            .Include(p => p.BudgetDetail).ThenInclude(b => b!.BudgetHead)
            .Include(p => p.FieldVisits.OrderBy(fv => fv.VisitNumber)).ThenInclude(fv => fv.AssignedTo)
            .Include(p => p.Approvals.OrderBy(a => a.CreatedAt))
            .Include(p => p.Palika)
            .FirstOrDefaultAsync(p => p.Id == proposalId, ct)
            ?? throw new InvalidOperationException($"Proposal {proposalId} not found");

        var palika = proposal.Palika;
        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text(palika.Name_Mr ?? palika.Name_En).Bold().FontSize(14);
                    col.Item().AlignCenter().Text("कार्यालयीन टिपणी / Office Note").Bold().FontSize(12);
                    col.Item().PaddingTop(3).LineHorizontal(1);
                });

                page.Content().PaddingVertical(8).Column(col =>
                {
                    // Section 1: Basic Info
                    col.Item().Text("1. Proposal Information / प्रस्ताव माहिती").Bold().FontSize(11);
                    BilingualField(col, "Proposal No", proposal.ProposalNumber, null);
                    BilingualField(col, "Date", proposal.ProposalDate.ToString("dd/MM/yyyy"), null);
                    BilingualField(col, "Department / विभाग", proposal.Department?.Name_En, proposal.Department?.Name_Mr);
                    BilingualField(col, "Work Category / कार्य प्रकार", proposal.DeptWorkCategory?.Name_En, proposal.DeptWorkCategory?.Name_Mr);
                    BilingualField(col, "Zone / क्षेत्र", proposal.Zone?.Name_En, proposal.Zone?.Name_Mr);
                    BilingualField(col, "Prabhag / प्रभाग", proposal.Prabhag?.Name_En, proposal.Prabhag?.Name_Mr);
                    BilingualField(col, "Work Title / कामाचे शीर्षक", proposal.WorkTitle_En, proposal.WorkTitle_Mr);
                    BilingualField(col, "Description / वर्णन", proposal.WorkDescription_En, proposal.WorkDescription_Mr);
                    BilingualField(col, "Priority", proposal.Priority, null);
                    BilingualField(col, "Created By / तयार करणार", proposal.CreatedBy?.FullName_En, proposal.CreatedBy?.FullName_Mr);

                    // Section 2: Field Visits
                    col.Item().PaddingTop(8).Text("2. Field Visits / स्थळ भेट").Bold().FontSize(11);
                    if (proposal.FieldVisits.Any())
                    {
                        foreach (var fv in proposal.FieldVisits)
                        {
                            col.Item().PaddingTop(3).Text($"Visit #{fv.VisitNumber} — {fv.AssignedTo?.FullName_En ?? "—"} — Status: {fv.Status}");
                            if (!string.IsNullOrEmpty(fv.ProblemDescription_En))
                                BilingualField(col, "Problem", fv.ProblemDescription_En, fv.ProblemDescription_Mr);
                            if (!string.IsNullOrEmpty(fv.Recommendation_En))
                                BilingualField(col, "Recommendation", fv.Recommendation_En, fv.Recommendation_Mr);
                        }
                    }
                    else col.Item().Text("No field visits recorded.").FontColor(Colors.Grey.Medium);

                    // Section 3: Estimate
                    col.Item().PaddingTop(8).Text("3. Estimate / अंदाजपत्रक").Bold().FontSize(11);
                    if (proposal.Estimate is not null)
                    {
                        BilingualField(col, "Estimated Cost", FormatCurrency(proposal.Estimate.EstimatedCost), null);
                        BilingualField(col, "Status", proposal.Estimate.Status, null);
                        BilingualField(col, "Prepared By", proposal.Estimate.PreparedBy?.FullName_En, proposal.Estimate.PreparedBy?.FullName_Mr);
                    }
                    else col.Item().Text("No estimate recorded.").FontColor(Colors.Grey.Medium);

                    // Section 4: Technical Sanction
                    col.Item().PaddingTop(8).Text("4. Technical Sanction / तांत्रिक मान्यता").Bold().FontSize(11);
                    if (proposal.TechnicalSanction is not null)
                    {
                        var ts = proposal.TechnicalSanction;
                        BilingualField(col, "TS Number", ts.TsNumber, null);
                        BilingualField(col, "Amount", FormatCurrency(ts.TsAmount), null);
                        BilingualField(col, "Sanctioned By", ts.SanctionedByName, ts.SanctionedByName_Mr);
                    }
                    else col.Item().Text("No technical sanction recorded.").FontColor(Colors.Grey.Medium);

                    // Section 5: PRAMA
                    col.Item().PaddingTop(8).Text("5. PRAMA / प्रमा").Bold().FontSize(11);
                    if (proposal.PramaDetail is not null)
                    {
                        var pd = proposal.PramaDetail;
                        BilingualField(col, "Fund Type", pd.FundType?.Name_En, pd.FundType?.Name_Mr);
                        BilingualField(col, "Budget Head", pd.BudgetHead?.Name_En, pd.BudgetHead?.Name_Mr);
                        BilingualField(col, "Fund Year", pd.FundApprovalYear, null);
                    }
                    else col.Item().Text("No PRAMA recorded.").FontColor(Colors.Grey.Medium);

                    // Section 6: Budget
                    col.Item().PaddingTop(8).Text("6. Budget / अर्थसंकल्प").Bold().FontSize(11);
                    if (proposal.BudgetDetail is not null)
                    {
                        var bd = proposal.BudgetDetail;
                        BilingualField(col, "Work Method", bd.WorkExecutionMethod?.Name_En, bd.WorkExecutionMethod?.Name_Mr);
                        BilingualField(col, "Budget Head", bd.BudgetHead?.Name_En, bd.BudgetHead?.Name_Mr);
                        BilingualField(col, "Allocated", FormatCurrency(bd.AllocatedFund), null);
                        BilingualField(col, "Available", FormatCurrency(bd.CurrentAvailableFund), null);
                        BilingualField(col, "Balance", FormatCurrency(bd.BalanceAmount), null);
                        BilingualField(col, "Approval Slab", bd.DeterminedApprovalSlab, null);
                    }
                    else col.Item().Text("No budget recorded.").FontColor(Colors.Grey.Medium);

                    // Section 7: Approval Timeline
                    col.Item().PaddingTop(8).Text("7. Approval Timeline / मान्यता क्रम").Bold().FontSize(11);
                    foreach (var a in proposal.Approvals)
                    {
                        col.Item().PaddingTop(3).Text($"[{a.Action}] {a.ActorName_En} ({a.StageRole}) — {a.CreatedAt:dd/MM/yyyy HH:mm}");
                        if (!string.IsNullOrEmpty(a.Opinion_En))
                            col.Item().Text($"  Opinion: {a.Opinion_En}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        if (!string.IsNullOrEmpty(a.PushBackNote_En))
                            col.Item().Text($"  Push Back Note: {a.PushBackNote_En}").FontSize(8).FontColor(Colors.Red.Darken1);
                    }

                    col.Item().PaddingTop(15).AlignCenter()
                        .Text($"Current Status: {proposal.CurrentStage}").Bold().FontSize(12);
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Generated on ").FontSize(7);
                    t.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7);
                    t.Span(" | Page ").FontSize(7);
                    t.CurrentPageNumber().FontSize(7);
                    t.Span(" of ").FontSize(7);
                    t.TotalPages().FontSize(7);
                });
            });
        }).GeneratePdf();

        var title_En = $"Full Proposal - {proposal.ProposalNumber}";
        var title_Mr = $"संपूर्ण प्रस्ताव - {proposal.ProposalNumber}";
        var fileName = $"Proposal_{proposal.ProposalNumber.Replace("/", "-")}_{DateTime.Now:yyyyMMddHHmm}.pdf";

        return new PdfGenerationResult(pdfBytes, fileName, title_En, title_Mr);
    }

    private static void BilingualField(ColumnDescriptor col, string label, string? en, string? mr)
    {
        var text = en ?? "—";
        if (!string.IsNullOrEmpty(mr)) text += $" / {mr}";
        col.Item().PaddingLeft(10).Text($"{label}: {text}");
    }

    private static string FormatCurrency(decimal? amount)
        => amount.HasValue ? $"₹ {amount.Value:N2}" : "—";
}
