using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace OeisSanitization
{
    public static class Utils
    {
        public static void Download(string uri)
        {
            HttpResponseMessage response = new HttpClient().GetAsync(new Uri(uri)).Result;
            FileStream fs = new FileStream("names.gz", FileMode.CreateNew);
            response.Content.CopyToAsync(fs).Wait();
            fs.Close();
            fs.Dispose();
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
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = client.GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                    return response.Content.ReadAsStringAsync().Result;
            }
            catch
            {
                return string.Empty;
            }

            return string.Empty;
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

        public static void MarkdownPage(string title, string mdFolder, string mdFile, List<RDb> list, int numberOfColumns, List<List<string>>? options = null, List<string>? optionsName = null)
        {
            StringBuilder md = new StringBuilder();
            md.AppendLine("---");
            md.AppendLine("layout: page");
            md.AppendLine("title: " + title);
            md.AppendLine("---");
            md.AppendLine();
            md.AppendLine("There are " + list.Count + " sequences in the table. Last update is " + File.GetCreationTime("db.json"));
            md.AppendLine();

            if (list.Count > 0)
            {
                md.AppendLine(string.Concat(Enumerable.Repeat("|Number|" + (optionsName != null ? string.Join("|", optionsName) + "|" : ""), numberOfColumns)));
                md.AppendLine(string.Concat(Enumerable.Repeat("|-", (2 + (optionsName != null ? optionsName.Count : 0)) * numberOfColumns - 1)) + "|");

                int i = 0;
                for (i = 0; i < list.Count; i++)
                {
                    for (int n = 0; n < numberOfColumns && i * numberOfColumns + n < list.Count; n++)
                    {
                        md.Append(list[i * numberOfColumns + n].ToString());
                        if (options != null)
                            md.Append(string.Join(", ", options[i * numberOfColumns + n]) + "|");
                    }

                    md.AppendLine();
                }
            }

            if (!Directory.Exists("oeis_mds"))
            {
                Directory.CreateDirectory("oeis_mds");
                Directory.CreateDirectory("oeis_mds/" + mdFolder);
            }

            if (!Directory.Exists("oeis_mds/" + mdFolder))
                Directory.CreateDirectory("oeis_mds/" + mdFolder);

            File.WriteAllText("oeis_mds/" + mdFolder + "/" + mdFile + ".md", md.ToString());
        }

        public static List<string> LinkExtractor(string str)
        {
            Regex extractDateRegex = new Regex("https?:\\/\\/(?:www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b(?:[-a-zA-Z0-9()@:%_\\+.~#?&\\/=]*)");
            return extractDateRegex.Matches(str).Cast<Match>().Select(m => m.Value).ToList();
        }

        public static List<string> LinkExtractor(List<string> list)
        {
            return LinkExtractor(string.Join("", list));
        }

        public static List<string> AllLinks(RDb rDb)
        {
            List<string> list = new List<string>();
            list.AddRange(LinkExtractor(rDb.id));
            list.AddRange(LinkExtractor(rDb.data));
            list.AddRange(LinkExtractor(rDb.name));
            list.AddRange(LinkExtractor(rDb.comment));
            list.AddRange(LinkExtractor(rDb.reference));
            list.AddRange(LinkExtractor(rDb.link));
            list.AddRange(LinkExtractor(rDb.formula));
            list.AddRange(LinkExtractor(rDb.example));
            list.AddRange(LinkExtractor(rDb.maple));
            list.AddRange(LinkExtractor(rDb.mathematica));
            list.AddRange(LinkExtractor(rDb.program));
            list.AddRange(LinkExtractor(rDb.xref));
            list.AddRange(LinkExtractor(rDb.offset));
            list.AddRange(LinkExtractor(rDb.author));
            list.AddRange(LinkExtractor(rDb.keyword));

            return list;
        }

        public static List<string> BrokenLinks(List<string> links)
        {
            List<string> ret = new List<string>();
            Parallel.ForEach(links, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, link =>
            {
                if (!link.ToLower().Contains("arxiv.org") && !Utils.IsLinkWorking(link))
                {
                    Console.WriteLine(link);
                    ret.Add(link);
                }
            });

            return ret;
        }
    }
}