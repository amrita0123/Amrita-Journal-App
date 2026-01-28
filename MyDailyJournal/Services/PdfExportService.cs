using Markdig;
using MyDailyJournal.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MyDailyJournal.Services
{
    public class PdfExportService
    {
        private readonly JournalDbContext _db;

        public PdfExportService(JournalDbContext db)
        {
            _db = db;
        }

        public async Task<byte[]> ExportEntriesToPdf(DateTime startDate, DateTime endDate)
        {
            // Get entries in the date range
            var entries = await _db.JournalEntries
                .Include(e => e.PrimaryMood)
                .Include(e => e.EntryMoods)
                    .ThenInclude(em => em.Mood)
                .Include(e => e.EntryTags)
                    .ThenInclude(et => et.Tag)
                .Include(e => e.Category)
                .Where(e => e.EntryDate.Date >= startDate.Date && e.EntryDate.Date <= endDate.Date)
                .OrderBy(e => e.EntryDate)
                .ToListAsync();

            // Generate PDF
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text($"Journal Entries: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")
                        .SemiBold().FontSize(16).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .Column(col =>
                        {
                            foreach (var entry in entries)
                            {
                                col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Column(c =>
                                {
                                    c.Item().Text($"{entry.EntryDate:dddd, MMM dd, yyyy} - {entry.Title}").Bold();
                                    c.Item().Text($"Mood: {entry.PrimaryMood?.Emoji} {entry.PrimaryMood?.Name}");
                                    c.Item().Text($"Category: {entry.Category?.Name ?? "None"}");
                                    
                                    // Tags
                                    if (entry.EntryTags?.Any() == true)
                                    {
                                        var tagString = string.Join(", ", entry.EntryTags.Select(t => t.Tag.Name));
                                        c.Item().Text($"Tags: {tagString}");
                                    }

                                    // Content (Markdown -> Plain text for now)
                                    var plainText = Markdown.ToPlainText(entry.Content); // You may need a Markdown parser
                                    c.Item().Text(plainText).FontSize(11);
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x => x.Span("Generated on " + DateTime.Now.ToString("MMM dd, yyyy HH:mm")));
                });
            }).GeneratePdf();

            return pdfBytes;
        }
    }
}
