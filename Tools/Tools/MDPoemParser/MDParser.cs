using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tools.MDPoemParser
{

    public class Poem
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public string Dynasty { get; set; }
        public string Source { get; set; }
    }

    public class MDParser
    {
        public static  List<Poem> MdFileToPoem(string filePath)
        {

            // Create a list to store extracted poems
            List<Poem> poems = new List<Poem>();

            string sourceFileName = Path.GetFileName(filePath);

            // Read the file
            string markdownContent = File.ReadAllText(filePath);

            // Regex patterns for extraction
            string titlePattern = @"## (.*?)\n"; // Matches titles
            string authorDynastyPattern = @"#### (.*?) - (.*?)\n"; // Matches author and dynasty
            string contentPattern = @"(.*?)\n\n"; // Matches poem content



            // Match titles
            MatchCollection titleMatches = Regex.Matches(markdownContent, titlePattern);

            // Extract details for each title
            foreach (Match match in titleMatches)
            {
                string title = match.Groups[1].Value;

                // Find the position of this title in the content
                int titleIndex = markdownContent.IndexOf(match.Value);
                int nextTitleIndex = markdownContent.IndexOf("##", titleIndex + match.Value.Length);

                // Extract the block for this poem
                string poemBlock = markdownContent.Substring(
                    titleIndex + match.Value.Length,
                    (nextTitleIndex == -1 ? markdownContent.Length : nextTitleIndex) - titleIndex - match.Value.Length
                ).Trim();

                // Extract author and dynasty
                Match authorDynastyMatch = Regex.Match(poemBlock, authorDynastyPattern);
                string author = authorDynastyMatch.Success ? authorDynastyMatch.Groups[1].Value : "Unknown";
                string dynasty = authorDynastyMatch.Success ? authorDynastyMatch.Groups[2].Value : "Unknown";

                // Extract content
                string content = Regex.Match(poemBlock, contentPattern).Groups[1].Value;

                // Add to list
                poems.Add(new Poem
                {
                    Title = title,
                    Content = content,
                    Author = author,
                    Dynasty = dynasty,
                    Source = sourceFileName
                });
            }

            return poems;
        }

        static void ExportToExcel(List<Poem> poems, string outputPath)
        {
            // Enable EPPlus license (required for non-commercial use)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Poems");

                // Add headers
                worksheet.Cells[1, 1].Value = "Title";
                worksheet.Cells[1, 2].Value = "Content";
                worksheet.Cells[1, 3].Value = "Author";
                worksheet.Cells[1, 4].Value = "Dynasty";
                worksheet.Cells[1, 5].Value = "Source";

                // Add data
                for (int i = 0; i < poems.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = poems[i].Title;
                    worksheet.Cells[i + 2, 2].Value = poems[i].Content;
                    worksheet.Cells[i + 2, 3].Value = poems[i].Author;
                    worksheet.Cells[i + 2, 4].Value = poems[i].Dynasty;
                    worksheet.Cells[i + 2, 5].Value = poems[i].Source;
                }

                // Save to file
                FileInfo fileInfo = new FileInfo(outputPath);
                package.SaveAs(fileInfo);
            }
        }



    }
}
