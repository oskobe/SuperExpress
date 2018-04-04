using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ITextPDFTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Stream myStream;
            SaveFileDialog file = new SaveFileDialog();
            file.Filter = "pdf files (*.pdf)|*.pdf|All files (*.*)|*.*"; ;
            file.FilterIndex = 1;
            file.RestoreDirectory = true;
            var result = file.ShowDialog();

            if (result.HasValue && result.Value)
            {
                string localFilePath = file.FileName.ToString();
                string fileNameExt = localFilePath.Substring(localFilePath.LastIndexOf("\\") + 1);
                myStream = file.OpenFile();

                Document document = new Document(PageSize.LETTER);
                PdfWriter writer = PdfWriter.GetInstance(document, myStream);
                document.Open();

                Font bold_big = new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.BOLD | Font.UNDERLINE);
                Font bold = new Font(Font.FontFamily.TIMES_ROMAN, 10, Font.BOLD);
                Font normal = new Font(Font.FontFamily.TIMES_ROMAN, 10);

                iTextSharp.text.Paragraph title = new iTextSharp.text.Paragraph("SHIPPING INVOICE", bold_big);
                title.Alignment = iTextSharp.text.Rectangle.ALIGN_CENTER;
                document.Add(title);

                PdfPTable table = new PdfPTable(6);
                
                PdfPCell cell = new PdfPCell(new Phrase(" \n "));
                cell.Border = 0;
                cell.Colspan = 6;
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Invoice No.", bold));
                cell.Border = 0;
                cell.Colspan = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("80001882", normal));
                cell.Border = 0;
                cell.Colspan = 5;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Sender:", bold));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Recipient:", bold));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("David Lee", normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Brain Jobs", normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("3-4100 Av Dupuis", normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("5662 Av Goyer", normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Montreal Quebec, Canada", normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Laval Quebec, Canada", normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("H5T5T2", normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("H7T2T1", normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(" \n "));
                cell.Border = 0;
                cell.Colspan = 6;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Service Type", bold));
                cell.Colspan = 2;
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Guaranteed Service", bold));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Delivery Days", bold));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Estimated Delivery Date", bold));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Amount", bold));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("UPS Express Early", normal));
                cell.Colspan = 2;
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Yes", normal));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("2", normal));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("10/03/2018 11:00", normal));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("120.00 CAD", normal));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(" \n \n \n \n "));
                cell.Border = 0;
                cell.Colspan = 6;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("SUB TOTAL", bold));
                cell.Border = 0;
                cell.Colspan = 5;
                cell.HorizontalAlignment = 2;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("120.00 CAD", normal));
                cell.Border = 0;
                cell.HorizontalAlignment = 2;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("SERVICE TAX (15%)", bold));
                cell.Border = 0;
                cell.Colspan = 5;
                cell.HorizontalAlignment = 2;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("18.00.00 CAD", normal));
                cell.Border = 0;
                cell.HorizontalAlignment = 2;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("TOTAL", bold));
                cell.Border = 0;
                cell.Colspan = 5;
                cell.HorizontalAlignment = 2;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("138.00 CAD", normal));
                cell.Border = 0;
                cell.HorizontalAlignment = 2;
                table.AddCell(cell);

                document.Add(table);

                document.Close();
                myStream.Close();


                //gridBill.item
                Grid myGrid = new Grid();
                

            }



        }
    }
}
