using Microsoft.AspNetCore.Hosting;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using ProposalManagement.Application.Common.Interfaces;

namespace ProposalManagement.Infrastructure.Services;

public class PdfSignatureStampService : IPdfSignatureStampService
{
    private const double OverlayScale = 1.5d;

    private readonly IFileStorage _fileStorage;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public PdfSignatureStampService(IFileStorage fileStorage, IWebHostEnvironment webHostEnvironment)
    {
        _fileStorage = fileStorage;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<string> StampSignatureAsync(
        string sourcePdfPath,
        string signatureImagePath,
        int pageNumber,
        decimal positionX,
        decimal positionY,
        decimal width,
        decimal height,
        decimal rotation,
        string outputFolder,
        string outputFileName,
        SignatureStampContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var pdfBytes = await ReadFileBytesAsync(sourcePdfPath, cancellationToken);
        var signatureFullPath = ResolveSignatureImagePath(signatureImagePath);

        using var sourcePdfStream = new MemoryStream(pdfBytes);
        using var document = PdfReader.Open(sourcePdfStream, PdfDocumentOpenMode.Modify);

        if (pageNumber <= 0 || pageNumber > document.PageCount)
            throw new InvalidOperationException($"Invalid page number {pageNumber} for PDF with {document.PageCount} page(s)");

        var page = document.Pages[pageNumber - 1];
        using (var graphics = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append))
        using (var image = XImage.FromFile(signatureFullPath))
        {
            var x = (double)positionX / OverlayScale;
            var y = (double)positionY / OverlayScale;
            var targetWidth = (double)width / OverlayScale;
            var targetHeight = (double)height / OverlayScale;
            var angle = (double)rotation;

            var centerX = x + (targetWidth / 2d);
            var centerY = y + (targetHeight / 2d);

            graphics.Save();
            graphics.TranslateTransform(centerX, centerY);
            if (Math.Abs(angle) > 0.01d)
                graphics.RotateTransform(angle);
            graphics.DrawImage(image, -targetWidth / 2d, -targetHeight / 2d, targetWidth, targetHeight);
            graphics.Restore();

            if (context is not null)
            {
                DrawSignerTextBlock(
                    graphics,
                    x,
                    y,
                    targetWidth,
                    targetHeight,
                    context);
            }
        }

        using var outputStream = new MemoryStream();
        document.Save(outputStream, false);
        outputStream.Position = 0;

        return await _fileStorage.SaveAsync(outputStream, outputFileName, outputFolder, cancellationToken);
    }

    private async Task<byte[]> ReadFileBytesAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = await _fileStorage.GetAsync(path, cancellationToken);
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        return memory.ToArray();
    }

    private string ResolveSignatureImagePath(string signatureImagePath)
    {
        var trimmed = signatureImagePath.TrimStart('/', '\\');
        var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, trimmed.Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Signature image not found", fullPath);

        return fullPath;
    }

    private static void DrawSignerTextBlock(
        XGraphics graphics,
        double x,
        double y,
        double signatureWidth,
        double signatureHeight,
        SignatureStampContext context)
    {
        var headerFont = new XFont("Arial", 8, XFontStyleEx.Bold);
        var bodyFont = new XFont("Arial", 7, XFontStyleEx.Regular);

        var lineHeight = 10d;
        var blockWidth = Math.Max(signatureWidth * 2.5d, 240d);
        var blockX = Math.Max(8d, x - 6d);
        var blockY = y + signatureHeight + 6d;

        var lines = new List<string>
        {
            $"Signed by: {context.SignerName} ({context.SignerRole})"
        };

        if (!string.IsNullOrWhiteSpace(context.Terms_En))
            lines.Add($"Terms: {context.Terms_En!.Trim()}");

        if (!string.IsNullOrWhiteSpace(context.Terms_Alt))
            lines.Add($"अटी: {context.Terms_Alt!.Trim()}");

        if (!string.IsNullOrWhiteSpace(context.Note_En))
            lines.Add($"Note: {context.Note_En!.Trim()}");

        if (!string.IsNullOrWhiteSpace(context.Note_Alt))
            lines.Add($"नोंद: {context.Note_Alt!.Trim()}");

        lines.Add($"Timestamp (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

        var wrapped = WrapLines(graphics, lines, bodyFont, blockWidth);
        var blockHeight = Math.Max(26d, wrapped.Count * lineHeight + 8d);

        var backgroundBrush = new XSolidBrush(XColor.FromArgb(235, 255, 255, 255));
        var borderPen = new XPen(XColor.FromArgb(120, 80, 80, 80), 0.6);
        graphics.DrawRoundedRectangle(borderPen, backgroundBrush, blockX, blockY, blockWidth, blockHeight, 4d, 4d);

        graphics.DrawString("Signer Agreement", headerFont, XBrushes.Black, new XRect(blockX + 4d, blockY + 3d, blockWidth - 8d, lineHeight), XStringFormats.TopLeft);

        var textY = blockY + 14d;
        foreach (var line in wrapped)
        {
            graphics.DrawString(line, bodyFont, XBrushes.Black, new XRect(blockX + 4d, textY, blockWidth - 8d, lineHeight), XStringFormats.TopLeft);
            textY += lineHeight;
        }
    }

    private static List<string> WrapLines(XGraphics graphics, IEnumerable<string> sourceLines, XFont font, double maxWidth)
    {
        var result = new List<string>();

        foreach (var originalLine in sourceLines)
        {
            if (string.IsNullOrWhiteSpace(originalLine))
                continue;

            var words = originalLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
                continue;

            var current = words[0];

            for (var i = 1; i < words.Length; i++)
            {
                var candidate = $"{current} {words[i]}";
                if (graphics.MeasureString(candidate, font).Width <= maxWidth - 8d)
                {
                    current = candidate;
                }
                else
                {
                    result.Add(current);
                    current = words[i];
                }
            }

            result.Add(current);
        }

        return result;
    }
}