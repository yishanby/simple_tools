﻿using Microsoft.Graph;
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


namespace SharePointAccess
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var authOptions = new AuthOptions
            {
                Scopes = new[] { ".default" },
            };

            var helper = new SharePointHelper(authOptions);
            var siteId = helper.GetSiteId("437h7r.sharepoint.com", "epfiles").Result;


            var itemId = helper.GetItemId(siteId, @"Test\Inner\Inner2\S3601 (11).wav").Result;
            using (var itemContent = helper.GetItemContent(siteId, itemId).Result)
            {
                helper.WriteStreamToLocalFileAsync(itemContent, "C:\\test\\test.wav").Wait();
            }
            //var strem = helper.GetFileStream("https://437h7r.sharepoint.com/:v:/s/epfiles/Eb7WQXg_o6FDkW_AKf8Agg4BAh50arl6CgSR9HGfKZt-cA?e=0IniuT").Result;


            var folderId = helper.GetItemId(siteId, @"Test\Inner\Inner2").Result;
            var children = helper.ListChildren(siteId, folderId).Result;

            using (var stream = File.OpenRead("C:\\test\\test.wav"))
            {
                var response = helper.UploadFileContent(siteId, folderId, "test.wav", stream).Result;
            }

            using (var stream = File.OpenRead("C:\\test\\gdpr.wav"))
            {
                var response = helper.UploadFileContent(siteId, folderId, "test.wav", stream).Result;
            }



        }


    }
}
