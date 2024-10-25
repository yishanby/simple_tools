using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCrawler
{
    internal class HtmlSourceCrawler
    {

        public static async Task ProcessDocMapItem(List<DocMapItem> docMapItems)
        {
            List<Task> tasks = new List<Task>();
            var i = 0;
            foreach (var docMapItem in docMapItems)
            {
                //await CrawlUrlAsync(docMapItem.Url);
                //await ProcessDocMapItem(docMapItem);
                //tasks.Add(Task.Run(async () =>
                //{
                await ProcessDocMapItem(docMapItem);
                Thread.Sleep(300);
                Console.WriteLine($"Processed {++i} of {docMapItems.Count} items.");
                //}));

            }

            await Task.WhenAll(tasks);
        }

        static async Task ProcessDocMapItem(DocMapItem docMapItem)
        {
            var html = await CrawlUrlAsync(docMapItem.Url);
            docMapItem.HtmlContent = html;
            if (!string.IsNullOrEmpty(html))
            {
                var repoLink = ParseHtmlContent(html, docMapItem.Url);
                if (!string.IsNullOrEmpty(repoLink))
                {
                    docMapItem.Repo = repoLink;
                }
            }
        }


        static async Task<string> CrawlUrlAsync(string url)
        {
            try
            {
                string htmlContent = await DownloadHtmlContentAsync(url);
                return htmlContent;
                //ParseHtmlContent(htmlContent, url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while processing {url}: {ex.Message}");
                return null;
            }
        }

        static async Task<string> DownloadHtmlContentAsync(string url)
        {
            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string htmlContent = await response.Content.ReadAsStringAsync();
            return htmlContent;
        }

        static string ParseHtmlContent(string htmlContent, string url)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlContent);

            // Example: Extract and print specific links based on given conditions  
            var specificLinks = document.DocumentNode.SelectNodes("//a[@title='Edit This Document' and @href]");

            if (specificLinks != null && specificLinks.Count > 0)
            {
                Console.WriteLine($"Specific links found on {url}:");
                var link = specificLinks[0];
                {
                    string href = link.GetAttributeValue("href", string.Empty);
                    string text = link.InnerText.Trim();
                    Console.WriteLine($"Link: {href}, Text: {text}");
                    return href;
                }
            }
            else
            {
                Console.WriteLine($"No specific links found on {url}. ");
            }

            return null;
        }

        public static string CleanHtml(string htmlContent)
        {
            // Load the HTML document  
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // Remove <script> and <style> elements  
            RemoveElementsByTagName(htmlDoc, "script");
            RemoveElementsByTagName(htmlDoc, "style");
            RemoveElementsByTagName(htmlDoc, "link");
            RemoveElementsByTagName(htmlDoc, "meta");

            // Remove attributes from all elements  
            foreach (var node in htmlDoc.DocumentNode.SelectNodes("//*"))
            {
                node.Attributes.RemoveAll();
            }

            // Return cleaned HTML as a string  
            using (var stringWriter = new StringWriter())
            {
                htmlDoc.Save(stringWriter);
                return stringWriter.ToString();
            }
        }

        static void RemoveElementsByTagName(HtmlDocument document, string tagName)
        {
            var nodes = document.DocumentNode.SelectNodes($"//{tagName}");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    node.Remove();
                }
            }
        }

    }
}
