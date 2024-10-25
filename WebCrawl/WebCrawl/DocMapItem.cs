using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCrawler
{
    public class DocMapItem
    {
        public string DPIA { get; set; }

        public string Scenario { get; set; }

        public string Url { get; set; }

        public string Path { get; set; }

        public string Repo { get; set; }

        public string MDContent { get; set; }

        public string HtmlContent { get; set; }

        public string Type { get; set; }

        public string CleanedHtmlContent { get; set; }

        public string ConvertedMDContent { get; set; }

    }
}
