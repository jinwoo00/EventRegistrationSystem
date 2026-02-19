//using QuestPDF.Fluent;
//using QuestPDF.Helpers;
//using QuestPDF.Infrastructure;

//public static class CertificateGenerator
//{
//    public static byte[] GenerateCertificate(string attendeeName, string eventTitle, DateTime eventDate, string certificateNumber)
//    {
//        var document = Document.Create(container =>
//        {
//            container.Page(page =>
//            {
//                page.Size(PageSizes.A4);
//                page.Margin(2, Unit.Centimetre);
//                page.DefaultTextStyle(x => x.FontSize(14));

//                page.Header()
//                    .AlignCenter()
//                    .Text("Certificate of Attendance")
//                    .SemiBold().FontSize(28).FontColor(Colors.Blue.Medium);

//                page.Content()
//                    .PaddingVertical(2, Unit.Centimetre)
//                    .Column(col =>
//                    {
//                        col.Spacing(20);
//                        col.Item().AlignCenter().Text("This certifies that").FontSize(16);
//                        col.Item().AlignCenter().Text(attendeeName).Bold().FontSize(24);
//                        col.Item().AlignCenter().Text("has attended the event").FontSize(16);
//                        col.Item().AlignCenter().Text(eventTitle).Bold().FontSize(22);
//                        col.Item().AlignCenter().Text($"on {eventDate:MMMM dd, yyyy}").FontSize(16);
//                        col.Item().AlignCenter().PaddingTop(20).Text($"Certificate Number: {certificateNumber}").FontSize(10).FontColor(Colors.Grey.Medium);
//                    });

//                page.Footer()
//                    .AlignCenter()
//                    .Text("EventFlow – All rights reserved")
//                    .FontSize(10);
//            });
//        });
//        return document.GeneratePdf();
//    }
//}