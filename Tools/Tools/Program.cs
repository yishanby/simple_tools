using Microsoft.Graph;
using Azure.Identity;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Security;
using Newtonsoft.Json.Linq;
using Azure.Core;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Markdig;


namespace SharePointAccess
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var md = File.ReadAllText(@"MDPoemParser\files\小学必背70首.md");
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var document = Markdown.Parse(md, pipeline);

            foreach (var item in document)
            {
                Console.WriteLine(item);
            }


        }


    }
}
