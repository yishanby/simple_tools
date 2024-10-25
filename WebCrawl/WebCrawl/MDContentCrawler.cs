using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DocCrawler
{
    internal class MDContentCrawler
    {
        public static async Task<string> DownloadMarkdownContentAsync(string filePath)
        {
            var url = filePath.Replace("https://github.com/", "https://raw.githubusercontent.com/");
            url = ReplaceFirstOccurrence(url, "/blob", "");

            try
            {
                using HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string markdownContent = await response.Content.ReadAsStringAsync();
                return markdownContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static string ConvertHtmlToMarkdown(string htmlContent)
        {
            var inputFile = Guid.NewGuid().ToString() + ".html";
            var outputFile = Guid.NewGuid().ToString() + ".md";
            File.WriteAllText(inputFile, htmlContent);

            var x = Process.Start("pandoc.exe", $"-f html -t markdown -o {outputFile} {inputFile}");
            x.WaitForExit();

            var content = File.ReadAllText(outputFile);
            File.Delete(inputFile);
            File.Delete(outputFile);
            return content;
        }


        static string ReplaceFirstOccurrence(string original, string oldValue, string newValue)
        {
            int index = original.IndexOf(oldValue);
            if (index < 0)
            {
                // Old value not found, return original string  
                return original;
            }

            return original.Substring(0, index) + newValue + original.Substring(index + oldValue.Length);
        }

        static int CountOccurrences(string original, string substring)
        {
            if (string.IsNullOrEmpty(substring))
            {
                return 0;
            }

            int count = 0;
            int index = 0;

            while ((index = original.IndexOf(substring, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                count++;
                index += substring.Length; // Move past the last occurrence  
            }

            return count;
        }


        public static async Task<string> GetFileContentFromGitHubwithAPI(string filePath, string token)
        {

            //string url = $"https://api.github.com/repos/{owner}/{repo}/contents/{filePath}";
            string url = filePath.Replace("https://github.com/", "https://api.github.com/repos/");
            using HttpClient client = new HttpClient();
            // Set up the HttpClient headers  
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DotNet", "1.0"));

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Send GET request  
            HttpResponseMessage response = await client.GetAsync(url);
            try
            {
                response.EnsureSuccessStatusCode();

                // Read response as string  
                string responseBody = await response.Content.ReadAsStringAsync();

                // Parse the response JSON to extract the file content  
                dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
                string fileContentBase64 = jsonResponse.content.ToString();
                byte[] data = Convert.FromBase64String(fileContentBase64);
                string fileContent = System.Text.Encoding.UTF8.GetString(data);
                return fileContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        #region  For Local File Operation


        private static void ExtractPath(DocMapItem item)
        {
            var localDict = new Dictionary<string, string> {
                { "https://learn.microsoft.com/en-us/azure", @"D:\Test\azure-docs\articles" },
                { "https://learn.microsoft.com/en-us/azure", @"D:\Test\azure-docs\articles" }
            };

            var Repo = new Dictionary<string, string> {
                { "https://learn.microsoft.com/en-us/azure", @"D:\Test\azure-docs\articles" },
                { "https://learn.microsoft.com/en-us/azure", @"D:\Test\azure-docs\articles" }
            };

            //@@ TODO 

            if (item.Url.StartsWith("https://learn.microsoft.com/en-us/azure"))
            {
                ExtractPathAndContent(item);
                //var filePath = Path.Combine(@"D:\Test\azure-docs\articles", item.Path) + ".md";
                //item.Content = ReadContent(filePath);

            }
            else if (item.Url.StartsWith("https://learn.microsoft.com/en-us/"))
            {
                item.Repo = "entra";
                item.Path = item.Url.Replace("https://learn.microsoft.com/en-us/microsoft-365/", string.Empty);
                if (item.Path.Contains("?"))
                    item.Path = item.Path.Substring(0, item.Path.IndexOf("?"));
                if (item.Path.Contains("#"))
                    item.Path = item.Path.Substring(0, item.Path.IndexOf("#"));
            }

        }


        private static void ExtractPathAndContent(DocMapItem item)
        {
            var dict = new Dictionary<string, string> {
                { "https://learn.microsoft.com/en-us/azure", @"D:\Test\azure-docs\articles" },
                { "https://learn.microsoft.com/en-us/azure", @"D:\Test\azure-docs\articles" }
            };


            item.Repo = "Azure";
            item.Path = item.Url.Replace("https://learn.microsoft.com/en-us/azure/", string.Empty);
            if (item.Path.Contains("?"))
                item.Path = item.Path.Substring(0, item.Path.IndexOf("?"));
            if (item.Path.Contains("#"))
                item.Path = item.Path.Substring(0, item.Path.IndexOf("#"));
            if (!string.IsNullOrEmpty(item.Path))
            {
                var filePath = Path.Combine(@"D:\Test\azure-docs\articles", item.Path) + ".md";
                if (System.IO.File.Exists(filePath))
                {
                    item.MDContent = ReadContent(filePath);
                }
            }
        }

        private static string ReadContent(string path)
        {
            return System.IO.File.ReadAllText(path);
        }

        #endregion 
    }
}
