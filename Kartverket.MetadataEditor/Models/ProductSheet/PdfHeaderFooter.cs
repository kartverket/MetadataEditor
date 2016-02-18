using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Kartverket.MetadataEditor.Models.ProductSheet
{
    public class PdfHeaderFooter : PdfPageEventHelper
    {
        private DateTime PrintTime = DateTime.Now;
        private BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.EMBEDDED);
        private PdfContentByte cb;

        // we will put the final number of pages in a template
        private PdfTemplate template;

        private string _imagePath;
        private ProductSheet _productsheet;
        private string _imagePathLogo;

        public PdfHeaderFooter(ProductSheet productsheet, string imagePath, string imagePathLogo)
        {
            this._productsheet = productsheet;
            this._imagePath = imagePath;
            this._imagePathLogo = imagePathLogo;

        }

        #region Properties

        public string Title { get; set; }

        public string HeaderLeft { get; set; }

        public string HeaderRight { get; set; }

        public Font HeaderFont { get; set; }

        public Font FooterFont { get; set; }

        #endregion

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            PrintTime = DateTime.Now;
            bf = BaseFont.CreateFont(@"C:\WINDOWS\Fonts\Arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            cb = writer.DirectContent;
            template = cb.CreateTemplate(50, 50);
        }

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            base.OnStartPage(writer, document);

        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

            //header

            Rectangle pageSize = document.PageSize;
            if (_imagePathLogo != "")
            {
                Image imageOrganization;
                imageOrganization = Image.GetInstance(_imagePathLogo);
                imageOrganization.Alt = "Etatens logo";
                imageOrganization.ScaleToFit(41f, 41f);

                imageOrganization.SetAbsolutePosition(pageSize.GetLeft(50), pageSize.GetTop(45));
                document.Add(imageOrganization);
            }

            var image_logo = Image.GetInstance(_imagePath + "logo_norgedigitalt.png");
            image_logo.Alt = "Norge Digitalt";
            image_logo.ScalePercent(50);
            image_logo.SetAbsolutePosition(pageSize.GetRight(50) - 35, pageSize.GetTop(40));
            document.Add(image_logo);

            cb.MoveTo(35, document.PageSize.Height - 50);
            cb.LineTo(document.PageSize.Width - 35, document.PageSize.Height - 50);
            cb.SetLineWidth(0.3f);
            cb.Stroke();

            cb.BeginText();
            cb.SetFontAndSize(bf, 8);
            if (_productsheet.Metadata.Title != null)
                cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, _productsheet.Metadata.Title, pageSize.GetRight(35), pageSize.GetTop(60), 0);
            cb.EndText();

            //footer
            pageSize = document.PageSize;

            cb.MoveTo(35, document.Bottom - 10);
            cb.LineTo(document.PageSize.Width - 35, document.Bottom - 10);
            cb.SetLineWidth(0.3f);
            cb.Stroke();

            DateTime dt = DateTime.Today;


            cb.BeginText();
            cb.SetFontAndSize(bf, 8);
            cb.SetTextMatrix(pageSize.GetLeft(36), pageSize.GetBottom(15));
            cb.ShowText(_productsheet.Metadata.ContactOwner.Organization + " - " + dt.ToString("dd.MM.yyyy"));
            cb.EndText();

        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

        }
    }
}