using System.Text;
using System.Text.RegularExpressions;

namespace oeis_sanitization
{
    public static class Processes
    {
        private static void MarkdownPage(string title, string mdFile, List<RDb> list, int numberOfColumns, List<List<string>>? options = null, List<string>? optionsName = null)
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
                Directory.CreateDirectory("oeis_mds");
            File.WriteAllText("oeis_mds/" + mdFile + ".md", md.ToString());
        }

        private static List<string> LinkExtractor(string str)
        {
            Regex extractDateRegex = new Regex("https?:\\/\\/(?:www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b(?:[-a-zA-Z0-9()@:%_\\+.~#?&\\/=]*)");
            return extractDateRegex.Matches(str).Cast<Match>().Select(m => m.Value).ToList();
        }

        private static List<string> LinkExtractor(List<string> list)
        {
            return LinkExtractor(string.Join("", list));
        }

        private static List<string> AllLinks(RDb rDb)
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

        private static List<string> BrokenLinks(List<string> links)
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

        public static void BrokenLinks(ref List<RDb> oeisDb)
        {
            int index = 0;
            List<Tuple<RDb, List<string>>> broken = new List<Tuple<RDb, List<string>>>();
            broken = oeisDb.Select(x => new Tuple<RDb, List<string>>(x, AllLinks(x))).Where(x => x.Item2.Count > 0).ToList();
            broken = broken.AsParallel().Select(x =>
            {
                Console.WriteLine(index++ + "/" + broken.Count);
                return new Tuple<RDb, List<string>>(x.Item1, BrokenLinks(x.Item2));
            }).Where(x => x.Item2.Count > 0).ToList();
            MarkdownPage("Sequences that contains broken links", "broken_links",
                broken.Select(x => x.Item1).ToList(), //list of sequences
                1,
                broken.Select(x => x.Item2.Select(y => "[" + y + "](" + y + ")").ToList()).ToList(),  //list of broken links for every sequence
                new List<string>() { "Links" });
        }

        public static void DoneDeprecatedKeyword(ref List<RDb> oeisDb)
        {
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("done")).ToList();
            MarkdownPage("Sequences with done deprecated keyword", "done_keyword", subset, 3);
        }
    }
}