using FinancialDucks.Application.Extensions;
using System.Text;
using System.Xml.Linq;

namespace FinancialDucks.Application.Services
{
    public interface IExcelToCSVConverter
    {
        FileInfo ConvertExcelToCSV(FileInfo workbook);
    }

    public class Word97ExcelConverter : IExcelToCSVConverter
    {
        public FileInfo ConvertExcelToCSV(FileInfo excelFile)
        {
            FileInfo csvFile = new FileInfo(Path.ChangeExtension(excelFile.FullName, "csv"));
            if (csvFile.Exists)
                csvFile.Delete();

            using var fileStream = GetCleanedFileStream(excelFile);
            var xml = XElement.Load(fileStream);

            var trElements = xml.DescendantNodes()
                .OfType<XElement>()
                .Where(p=>p.Name == "tr")
                .ToArray();

            var csvBuilder = new StringBuilder();
            foreach (var trElement in trElements)
                BuildCSVLine(csvBuilder, trElement);
          
            File.WriteAllText(csvFile.FullName, csvBuilder.ToString());

            excelFile.Delete();
            return csvFile;
        }

        private void BuildCSVLine(StringBuilder csvBuilder, XElement trElement)
        {
            var cells = trElement
                .Descendants()
                .Where(p => p.Name == "td")
                .Select(p => p.Value.Trim().WrapInQuotes())
                .ToArray();

            csvBuilder.AppendLine(string.Join(",", cells));
        }

        public TextReader GetCleanedFileStream(FileInfo file)
        {
            var text = File.ReadAllText(file.FullName);
            text = text.Replace("&nbsp;", "&#160;");
            text = text.Replace("<br>", "");
            text = text.Replace("</html>", "");
            text = text.Replace("<html>", "");

            return new StringReader(text);
        }
    }
}
