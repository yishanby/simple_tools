using SharePointAccess;

namespace Tools.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
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