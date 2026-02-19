using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public static class CertificateGenerator
{
    public static byte[] GenerateCertificate(string attendeeName, string eventTitle, DateTime eventDate, string certificateNumber)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12).FontColor(Colors.Black));

                // Decorative border
                page.Background()
                    .Border(2)
                    .BorderColor(Colors.Blue.Medium)
                    .Background(Colors.Grey.Lighten3);

                // Inner content
                page.Content()
                    .Padding(2, Unit.Centimetre)
                    .Column(col =>
                    {
                        // Spacer
                        col.Spacing(20);

                        // Title with line
                        col.Item().AlignCenter().Text("Certificate of Attendance")
                            .FontSize(32)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        col.Item().Height(2).Background(Colors.Blue.Medium);

                        col.Item().Height(30); // spacer

                        // Presented to
                        col.Item().AlignCenter().Text("This certifies that")
                            .FontSize(18)
                            .Italic();

                        col.Item().AlignCenter().Text(attendeeName)
                            .FontSize(36)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        col.Item().AlignCenter().Text("has attended the event")
                            .FontSize(18)
                            .Italic();

                        col.Item().AlignCenter().Text(eventTitle)
                            .FontSize(28)
                            .Bold();

                        col.Item().AlignCenter().Text($"on {eventDate:MMMM dd, yyyy}")
                            .FontSize(16);

                        col.Item().Height(30); // spacer

                        // Certificate number and seal
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().AlignLeft().Text($"Certificate No: {certificateNumber}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken2);

                            // Optional seal/logo placeholder
                            row.RelativeItem().AlignRight().Text("EventFlow")
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.Blue.Medium);
                        });

                        col.Item().Height(20);

                        // Footer
                        col.Item().AlignCenter().Text("This certificate is issued by EventFlow.")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Medium);
                    });
            });
        });
        return document.GeneratePdf();
    }
}