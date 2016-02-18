using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.events;
using System.Text.RegularExpressions;


namespace Kartverket.MetadataEditor.Models.ProductSheet
{
    public class PdfGenerator
    {

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        ProductSheet productsheet;
        string imagePath;
        string logoPath;
        Document doc;
        BaseFont bf;
        Font font1;
        Font font2;
        Font font3;
        Font font3Bold;
        Font fontLink;
        MemoryStream output;
        PdfWriter writer;
        PdfContentByte cb;
        ColumnText ct;

        ProductSheetService psService = new ProductSheetService();

        public PdfGenerator(ProductSheet productSheet, string imagePath, string logoPath)
        {
            this.productsheet = productSheet;
            this.imagePath = imagePath;
            this.logoPath = logoPath;
        }


        public Stream CreatePdf()
        {
            Startup();

            AddDescription();
            AddPurpose();
            AddContactOwner();
            AddResolution();
            AddCoverage();
            AddProcessHistory();
            AddMaintenance();
            AddDistribution();
            //Todo get from application schema
            //AddFeatureTypes();
            //Todo get from application schema
            //AddAttributes();
            AddLinks();

            WriteToColumns();


            PdfStructureTreeRoot root = writer.StructureTreeRoot;
            PdfStructureElement div = new PdfStructureElement(root, new PdfName("Div"));
            cb.BeginMarkedContentSequence(div);
            cb.EndMarkedContentSequence();


            doc.CloseDocument();
            output.Flush();
            output.Position = 0;


            return output;
        }

        private void WriteToColumns()
        {
            float gutter = -20f;

            float colwidth = (doc.Right - doc.Left - gutter) / 2;


            int status = 0;
            int i = 0;
            int count = 0;
            bool newPage = false;

            //Checks the value of status to determine if there is more text
            //If there is, status is 2, which is the value of NO_MORE_COLUMN

            while (ColumnText.HasMoreText(status))
            {
                if (i == 0)
                {
                    //Writing the first column
                    ct.SetSimpleColumn(doc.Left, doc.Bottom, doc.Right - colwidth, doc.Top);
                    i++;
                }
                else
                {
                    //write the second column
                    ct.SetSimpleColumn(doc.Left + colwidth, doc.Bottom, doc.Right, doc.Top);

                }
                //Needs to be here to prevent app from hanging
                if (!newPage)
                    ct.YLine = doc.Top - 80f;
                else
                    ct.YLine = doc.Top - 30f;

                //Commit the content of the ColumnText to the document
                //ColumnText.Go() returns NO_MORE_TEXT (1) and/or NO_MORE_COLUMN (2)
                //In other words, it fills the column until it has either run out of column, or text, or both

                status = ct.Go();

                if (++count > 1)
                {
                    count = 0;
                    if (ColumnText.HasMoreText(status))
                    {
                        doc.NewPage(); newPage = true;

                    }

                    i = 0;
                }
            }
        }

        private void Startup()
        {
            doc = new Document();

            bf = BaseFont.CreateFont(@"C:\WINDOWS\Fonts\Arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            font1 = FontFactory.GetFont("Arial", BaseFont.CP1252, BaseFont.EMBEDDED, 22f, Font.NORMAL, BaseColor.BLACK);
            font2 = FontFactory.GetFont("Arial", BaseFont.CP1252, BaseFont.EMBEDDED, 12f, Font.NORMAL, BaseColor.BLACK);
            font3 = FontFactory.GetFont("Arial", BaseFont.CP1252, BaseFont.EMBEDDED, 10f, Font.NORMAL, BaseColor.BLACK);
            font3Bold = FontFactory.GetFont("Arial", BaseFont.CP1252, BaseFont.EMBEDDED, 10f, Font.BOLD, BaseColor.BLACK);
            fontLink = FontFactory.GetFont("Arial", BaseFont.CP1252, BaseFont.EMBEDDED, 10f, Font.UNDERLINE);
            fontLink.SetColor(0, 0, 255);

            output = new MemoryStream();

            writer = PdfWriter.GetInstance(doc, output);
            writer.CloseStream = false;
            writer.PdfVersion = PdfWriter.VERSION_1_7;
            //Tagged
            writer.SetTagged();
            writer.UserProperties = true;


            // must be initialized before document is opened.
            writer.PageEvent = new PdfHeaderFooter(productsheet, imagePath, logoPath);

            doc.Open();
            doc.AddTitle(productsheet.Metadata.Title);
            doc.AddLanguage("Norwegian");

            cb = writer.DirectContent;


            ct = new ColumnText(cb);
            cb.BeginText();
            cb.SetFontAndSize(bf, 22);
            cb.SetTextMatrix(doc.Left, doc.Top - 60);
            cb.ShowText("Produktark: " + productsheet.Metadata.Title);
            cb.EndText();
        }

        private void AddLinks()
        {
            ct.AddElement(writeTblFooter(""));
            ct.AddElement(writeTblHeader("LENKER"));

            Paragraph linksParagraph = new Paragraph("", font3);

            List links = new List(List.UNORDERED);
            links.SetListSymbol("\u2022");
            links.IndentationLeft = 5;

            ListItem metaData = new ListItem();
            Anchor metaDatalink = new Anchor("Link til metadata i Geonorge", fontLink);
            metaDatalink.Reference = "https://kartkatalog.geonorge.no/metadata/uuid/" + productsheet.Metadata.Uuid;
            metaData.Add(metaDatalink);
            links.Add(metaData);

            if (!string.IsNullOrWhiteSpace(productsheet.Metadata.ProductSpecificationUrl))
            {
                ListItem ProductSpecificationUrl = new ListItem();
                Anchor ProductSpecificationLink = new Anchor("Link til produktspesifikasjon", fontLink);
                ProductSpecificationLink.Reference = productsheet.Metadata.ProductSpecificationUrl;
                ProductSpecificationUrl.Add(ProductSpecificationLink);
                links.Add(ProductSpecificationUrl);
            }

            if (!string.IsNullOrWhiteSpace(productsheet.Metadata.LegendDescriptionUrl))
            {
                ListItem legendDescription = new ListItem();
                Anchor legendDescriptionUrl = new Anchor("Link til tegnregler", fontLink);
                legendDescriptionUrl.Reference = productsheet.Metadata.LegendDescriptionUrl;
                legendDescription.Add(legendDescriptionUrl);
                links.Add(legendDescription);
            }

            if (!string.IsNullOrWhiteSpace(productsheet.Metadata.ProductPageUrl))
            {
                ListItem productPage = new ListItem();
                Anchor productPageUrl = new Anchor("Link til produktside", fontLink);
                productPageUrl.Reference = productsheet.Metadata.ProductPageUrl;
                productPage.Add(productPageUrl);
                links.Add(productPage);
            }

            linksParagraph.Add(links);
            ct.AddElement(linksParagraph);
        }

        //private void AddAttributes()
        //{
        //    if (!string.IsNullOrWhiteSpace(productsheet.ListOfAttributes))
        //    {
        //        ct.AddElement(writeTblFooter(""));
        //        ct.AddElement(writeTblHeader("EGENSKAPSLISTE"));

        //        List listOfAttributes = new List(List.UNORDERED);
        //        listOfAttributes.SetListSymbol("\u2022");
        //        listOfAttributes.IndentationLeft = 5;

        //        var Attributes = Regex.Split(productsheet.ListOfAttributes, "\r\n");
        //        foreach (string Attribute in Attributes)
        //        {
        //            ListItem liAttribute = new ListItem(Attribute, font3);
        //            listOfAttributes.Add(liAttribute);
        //        }

        //        ct.AddElement(listOfAttributes);
        //    }

        //}

        //private void AddFeatureTypes()
        //{
        //    if (!string.IsNullOrWhiteSpace(productsheet.ListOfFeatureTypes))
        //    {
        //        ct.AddElement(writeTblFooter(""));
        //        ct.AddElement(writeTblHeader("OBJEKTTYPELISTE"));

        //        List listOfFeatureTypes = new List(List.UNORDERED);
        //        listOfFeatureTypes.SetListSymbol("\u2022");
        //        listOfFeatureTypes.IndentationLeft = 5;

        //        var FeatureTypes = Regex.Split(productsheet.ListOfFeatureTypes, "\r\n");
        //        foreach (string FeatureType in FeatureTypes)
        //        {
        //            ListItem liFeature = new ListItem(FeatureType, font3);
        //            listOfFeatureTypes.Add(liFeature);
        //        }

        //        ct.AddElement(listOfFeatureTypes);

        //    }
        //}

        private void AddDistribution()
        {
            ct.AddElement(writeTblHeader("LEVERANSEBESKRIVELSE"));

            if (productsheet.Metadata.DistributionFormatName != null)
            {
                Phrase distributionFormatHeading = new Phrase("Format (versjon)", font3Bold);
                ct.AddElement(distributionFormatHeading);

                Paragraph distributionFormat = new Paragraph("", font3);
                List format = new List(List.UNORDERED);
                format.SetListSymbol("\u2022");
                format.IndentationLeft = 5;
                string formatVersion = productsheet.Metadata.DistributionFormatName;
                if (!string.IsNullOrWhiteSpace(productsheet.Metadata.DistributionFormatVersion))
                {
                    formatVersion = formatVersion + ", " + productsheet.Metadata.DistributionFormatVersion;
                }

                ListItem liFormat = new ListItem(formatVersion, font3);
                format.Add(liFormat);
                distributionFormat.Add(format);
                ct.AddElement(distributionFormat);
            }

            if (productsheet.Metadata.ReferenceSystems != null)
            {
                Phrase projectionsHeading = new Phrase("\n" + "Projeksjoner", font3Bold);
                ct.AddElement(projectionsHeading);

                List listOfProjections = new List(List.UNORDERED);
                listOfProjections.SetListSymbol("\u2022");
                listOfProjections.IndentationLeft = 5;

                var Projections = productsheet.Metadata.ReferenceSystems;
                foreach (var projection in Projections)
                {
                    ListItem liProjection = new ListItem(psService.GetReferenceSystemName(projection.CoordinateSystem), font3);
                    listOfProjections.Add(liProjection);
                }

                ct.AddElement(listOfProjections);

            }

            if (productsheet.Metadata.AccessConstraints != null )
            {
                Phrase accessConstraintsHeading = new Phrase("\n" + "Tilgangsrestriksjoner", font3Bold);
                ct.AddElement(accessConstraintsHeading);
                Phrase accessConstraints = new Phrase(productsheet.AccessConstraintFromRegister, font3);
                ct.AddElement(accessConstraints);
            }

            //Todo get ServiceDetails
            //if (!string.IsNullOrWhiteSpace(productsheet.ServiceDetails))
            //{
            //    Phrase serviceDetailsHeading = new Phrase("\n" + "Tjeneste", font3Bold);
            //    ct.AddElement(serviceDetailsHeading);

            //    Phrase serviceDetails = new Phrase(productsheet.ServiceDetails, font3);
            //    ct.AddElement(serviceDetails);
            //}
        }

        private void AddMaintenance()
        {
            ct.AddElement(writeTblHeader("AJOURFØRING OG OPPDATERING"));

            if (!string.IsNullOrWhiteSpace(productsheet.MaintenanceFrequencyFromRegister))
            {
                Phrase maintenanceFrequency = new Phrase(productsheet.MaintenanceFrequencyFromRegister, font3);
                ct.AddElement(maintenanceFrequency);
            }

            if (!string.IsNullOrWhiteSpace(productsheet.Metadata.Status))
            {
                Phrase statusHeading = new Phrase("Status", font3Bold);
                ct.AddElement(statusHeading);
                Phrase statusValue = new Phrase(productsheet.StatusFromRegister, font3);
                ct.AddElement(statusValue);
            }

            ct.AddElement(writeTblFooter(""));
        }

        private void AddProcessHistory()
        {
            ct.AddElement(writeTblHeader("KILDER OG METODE"));

            if (!string.IsNullOrWhiteSpace(productsheet.Metadata.ProcessHistory))
            {
                Phrase processHistory = new Phrase(productsheet.Metadata.ProcessHistory, font3);
                ct.AddElement(processHistory);
            }

            ct.AddElement(writeTblFooter(""));
        }

        private void AddCoverage()
        {
            ct.AddElement(writeTblHeader("UTSTREKNINGSINFORMASJON"));

            if (productsheet.Metadata.KeywordsPlace != null)
            {
                Phrase keywordsPlaceHeading = new Phrase("Utstrekningsbeskrivelse", font3Bold);
                ct.AddElement(keywordsPlaceHeading);
                ct.AddElement(new Phrase(string.Join(",", productsheet.Metadata.KeywordsPlace), font3));
            }

            //todo get CoverageArea
            //if (!string.IsNullOrWhiteSpace(productsheet.CoverageArea))
            //{

            //    Phrase coverageAreaUrlPhrase = new Phrase();
            //    Anchor coverageAreaUrl = new Anchor("Dekningsoversikt", fontLink);
            //    coverageAreaUrl.Reference = productsheet.CoverageArea;
            //    coverageAreaUrlPhrase.Add(coverageAreaUrl);
            //    ct.AddElement(coverageAreaUrlPhrase);

            //}

            ct.AddElement(writeTblFooter(""));
        }

        private void AddResolution()
        {
            ct.AddElement(writeTblHeader("DATASETTOPPLØSNING"));

            if (!string.IsNullOrWhiteSpace(productsheet.Metadata.ResolutionScale))
            {
                Chunk resolutionScaleHeading = new Chunk("Målestokktall: ", font3Bold);
                Chunk resolutionScaleValue = new Chunk(productsheet.Metadata.ResolutionScale, font3);
                Phrase resolutionScale = new Phrase();
                resolutionScale.Add(resolutionScaleHeading);
                resolutionScale.Add(resolutionScaleValue);
                ct.AddElement(resolutionScale);
            }

            //Todo get from somewhere
            //if (!string.IsNullOrWhiteSpace(productsheet.PrecisionInMeters))
            //{
            //    Chunk precisionInMetersHeading = new Chunk("Stedfestingsnøyaktighet (meter): ", font3Bold);
            //    Chunk precisionInMeters = new Chunk(productsheet.PrecisionInMeters, font3);

            //    Phrase precision = new Phrase();
            //    precision.Add(precisionInMetersHeading);
            //    precision.Add(precisionInMeters);
            //    ct.AddElement(precision);
            //}

            ct.AddElement(writeTblFooter(""));
        }

        private void AddContactOwner()
        {
            ct.AddElement(writeTblHeader("EIER/KONTAKTPERSON"));

            Paragraph contactOwnerOrganization = new Paragraph(productsheet.Metadata.ContactOwner.Organization, font3);

            Phrase contactPublisher = new Phrase();

            if (!string.IsNullOrWhiteSpace(productsheet.Metadata.ContactPublisher.Name))
            {
                Chunk contactPublisherHeading = new Chunk("Datateknisk: ", font3Bold);
                Chunk contactPublisherName = new Chunk(productsheet.Metadata.ContactPublisher.Name, font3);
                contactPublisher.Add(contactPublisherHeading);
                contactPublisher.Add(contactPublisherName);
                if (!string.IsNullOrWhiteSpace(productsheet.Metadata.ContactPublisher.Email))
                {
                    Chunk contactPublisherEmail = new Chunk(", " + productsheet.Metadata.ContactPublisher.Email, font3);
                    contactPublisher.Add(contactPublisherEmail);
                }
            }
            ct.AddElement(contactOwnerOrganization);
            ct.AddElement(contactPublisher);

            Phrase contactOwner = new Phrase();

            if (!string.IsNullOrWhiteSpace(productsheet.Metadata.ContactOwner.Name))
            {
                Chunk contactOwnerHeading = new Chunk("Fagekspert: ", font3Bold);
                Chunk contactOwnerName = new Chunk(productsheet.Metadata.ContactOwner.Name, font3);
                contactOwner.Add(contactOwnerHeading);
                contactOwner.Add(contactOwnerName);
                if (!string.IsNullOrWhiteSpace(productsheet.Metadata.ContactOwner.Email))
                {
                    Chunk contactOwnerEmail = new Chunk(", " + productsheet.Metadata.ContactOwner.Email, font3);
                    contactOwner.Add(contactOwnerEmail);
                }
            }

            ct.AddElement(contactOwner);

            ct.AddElement(writeTblFooter(""));
        }

        private void AddPurpose()
        {
            ct.AddElement(writeTblHeader("FORMÅL/BRUKSOMRÅDE"));

            if (!string.IsNullOrWhiteSpace(productsheet.Metadata.Purpose))
            {
                Paragraph purposeHeading = new Paragraph(productsheet.Metadata.Purpose, font3);
                ct.AddElement(purposeHeading);
            }

            if (!string.IsNullOrWhiteSpace(productsheet.Metadata.SpecificUsage))
            {
                Paragraph specificUsage = new Paragraph("\n" + productsheet.Metadata.SpecificUsage, font3);

                ct.AddElement(specificUsage);
            }

            if (!string.IsNullOrWhiteSpace(productsheet.Metadata.UseLimitations))
            {
                Paragraph useLimitations = new Paragraph("\n" + productsheet.Metadata.UseLimitations, font3);
                ct.AddElement(useLimitations);
            }

            ct.AddElement(writeTblFooter(""));
        }

        private void AddDescription()
        {
            ct.AddElement(writeTblHeader("BESKRIVELSE"));

            if (!string.IsNullOrWhiteSpace(productsheet.GetThumbnail()))
            {
                Paragraph descriptionHeading = new Paragraph();
                try
                {
                    var imageMap = Image.GetInstance(new Uri(productsheet.GetThumbnail()), true);
                    imageMap.Alt = "Bilde av karteksempel";
                    imageMap.ScaleToFit(140f, 100f);
                    imageMap.SpacingBefore = 4;
                    ct.AddElement(imageMap);
                    ct.AddElement(descriptionHeading);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                }

            }

            if (!string.IsNullOrWhiteSpace(productsheet.Metadata.Abstract))
            {
                Paragraph Description = new Paragraph(productsheet.Metadata.Abstract, font3);
                ct.AddElement(Description);
            }
            if (!string.IsNullOrWhiteSpace(productsheet.Metadata.SupplementalDescription))
            {
                Paragraph supplementalDescription = new Paragraph("\n" + productsheet.Metadata.SupplementalDescription, font3);
                ct.AddElement(supplementalDescription);
            }

            ct.AddElement(writeTblFooter(""));
        }

        PdfPTable writeTblHeader(string txt)
        {
            Font fontHead = FontFactory.GetFont("Arial", BaseFont.CP1252, BaseFont.EMBEDDED, 10f, Font.BOLD, BaseColor.WHITE);

            Anchor content = new Anchor(txt, fontHead);
            content.Name = txt;

            PdfOutline root = writer.RootOutline;
            PdfOutline mbot = new PdfOutline(root, PdfAction.GotoLocalPage(txt, false), txt);

            PdfPTable table = new PdfPTable(1);
            table.WidthPercentage = 100;
            PdfPCell cell = new PdfPCell(content);
            cell.BackgroundColor = new BaseColor(0, 150, 0);
            cell.Border = 0;
            cell.PaddingTop = 0;
            cell.PaddingBottom = 3;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            table.AddCell(cell);
            return table;

        }

        PdfPTable writeTblFooter(string txt)
        {

            PdfPTable table = new PdfPTable(1);
            table.WidthPercentage = 100;
            table.SpacingAfter = 20;
            PdfPCell cell = new PdfPCell();
            cell.Border = 0;
            table.AddCell(cell);
            return table;

        }

    }
}