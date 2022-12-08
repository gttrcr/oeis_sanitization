using System.IO.Compression;
using System.Net;

namespace oeis_sanitization
{
    public static class Utils
    {
        private static List<HttpStatusCode> _allowedWebResponses = new List<HttpStatusCode>() { HttpStatusCode.Forbidden, HttpStatusCode.Found };

        public static void Download(string uri)
        {
            HttpResponseMessage response = new HttpClient().GetAsync(new Uri(uri)).Result;
            using (var fs = new FileStream("names.gz", FileMode.CreateNew))
                response.Content.CopyToAsync(fs).Wait();
        }

        public static void Decompress(FileInfo fileToDecompress)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);
                using (FileStream decompressedFileStream = File.Create(newFileName))
                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    decompressionStream.CopyTo(decompressedFileStream);
            }
        }

        public static string Get(string uri)
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                    return response.Content.ReadAsStringAsync().Result;
            }

            throw new Exception();
        }

        public static bool IsLinkWorking(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.AllowAutoRedirect = true;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    response.Close();
                    return true;
                }
                else
                    return false;
            }
            catch (WebException ex)
            {
                return ex.Response != null;
                //if (ex.Response != null)
                //    return _allowedWebResponses.Contains(((HttpWebResponse)(ex.Response)).StatusCode);
                //return false;
            }
            catch
            {
                return false;
            }
        }
    }
}