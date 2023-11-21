using DinkToPdf;
using DinkToPdf.Contracts;

namespace Kalbe.App.InternshipLogbookLogbook.Api.Services
{
    public interface IPDFGenerator
    {
        byte[] GeneratePDF(string htmlContent);
    } 
    public class PDFGenerator : IPDFGenerator
    {
        private readonly IConverter _converter;

        public PDFGenerator(IConverter converter)
        {
            _converter = converter;
        }

        public byte[] GeneratePDF(string htmlContent)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                DocumentTitle = "Generated PDF"
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8" }
            };

            var document = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            return _converter.Convert(document);
        }
    }
}
