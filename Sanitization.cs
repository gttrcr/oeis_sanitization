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

        public static void BrokenLinks(List<RDb> oeisDb)
        {
            Console.WriteLine("Broken links...");
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
            Console.WriteLine("Done");
        }

        public static void DoneDeprecatedKeyword(List<RDb> oeisDb)
        {
            Console.WriteLine("Done deprecated keyword...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("done")).ToList();
            MarkdownPage("Sequences with done deprecated keyword", "done_keyword", subset, 3);
            Console.WriteLine("Done");
        }

        public static void DupeDeprecatedKeyword(List<RDb> oeisDb)
        {
            Console.WriteLine("Dupe deprecated keyword...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("dupe")).ToList();
            MarkdownPage("Sequences with dupe deprecated keyword", "dupe_keyword", subset, 3);
            Console.WriteLine("Done");
        }

        public static void HugeDeprecatedKeyword(List<RDb> oeisDb)
        {
            Console.WriteLine("Huge deprecated keyword...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("huge")).ToList();
            MarkdownPage("Sequences with huge deprecated keyword", "huge_keyword", subset, 3);
            Console.WriteLine("Done");
        }

        public static void LookDeprecatedKeyword(List<RDb> oeisDb)
        {
            Console.WriteLine("Look deprecated keyword...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("look")).ToList();
            MarkdownPage("Sequences with look deprecated keyword", "look_keywords", subset, 3);
            Console.WriteLine("Done");
        }

        public static void PartDeprecatedKeyword(List<RDb> oeisDb)
        {
            Console.WriteLine("Part deprecated keyword...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("part")).ToList();
            MarkdownPage("Sequences with part deprecated keyword", "part_keywords", subset, 3);
            Console.WriteLine("Done");
        }


        public static void UnedSequences(List<RDb> oeisDb)
        {
            Console.WriteLine("Uned sequences...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("uned")).ToList();
            MarkdownPage("Uned sequences", "uned_sequences", subset, 3);
            Console.WriteLine("Done");
        }


        public static void ObscDeprecatedKeyword(List<RDb> oeisDb)
        {
            Console.WriteLine("Obsc deprecated keyword...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("obsc")).ToList();
            MarkdownPage("Obscure sequences", "obsc_sequences", subset, 3);
            Console.WriteLine("Done");
        }


        public static void NeedMoreTerms(List<RDb> oeisDb)
        {
            Console.WriteLine("Need mode terms...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("more")).ToList();
            MarkdownPage("Sequences that needs more terms", "need_more_terms", subset, 3);
            Console.WriteLine("Done");
        }

        public static void Allocated(List<RDb> oeisDb)
        {
            Console.WriteLine("Allocated...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.name.StartsWith("allocated for")).ToList();
            MarkdownPage("Allocated sequences", "allocated", subset, 3);
            Console.WriteLine("Done");
        }


        public static void MapleCode(List<RDb> oeisDb)
        {
            Console.WriteLine("Maple code...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.maple.Count == 0).ToList();
            MarkdownPage("Sequences that needs maple code", "need_maple", subset, 3);
            Console.WriteLine("Done");
        }


        public static void MathematicaCode(List<RDb> oeisDb)
        {
            Console.WriteLine("Mathematica code...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.mathematica.Count == 0).ToList();
            MarkdownPage("Sequences that needs mathematica code", "need_mathematica", subset, 3);
            Console.WriteLine("Done");
        }

        public static void SomeExamples(List<RDb> oeisDb)
        {
            Console.WriteLine("Some examples...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.example.Count == 0).ToList();
            MarkdownPage("Sequences that needs examples", "need_examples", subset, 3);
            Console.WriteLine("Done");
        }

        public static void Unknown(List<RDb> oeisDb)
        {
            Console.WriteLine("Unknown...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("unkn")).ToList();
            MarkdownPage("Unknown sequences", "need_knowledge", subset, 3);
            Console.WriteLine("Done");
        }

        public static void Dead(List<RDb> oeisDb)
        {
            Console.WriteLine("Dead...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("dead")).ToList();
            MarkdownPage("Dead sequences", "dead", subset, 3);
            Console.WriteLine("Done");
        }

        public static void MathematicaError(List<RDb> oeisDb)
        {
            //Console.WriteLine("Mathematica error...");
            //Link link = new Link();
            //int index = 0;
            //subset = oeisDb.Where(x =>
            //{
            //    if (x.mathematica.Count > 0)
            //    {
            //        Console.WriteLine(index++);
            //        string query = string.Join(";", x.mathematica) + ";" + "Table[a[n], {n, " + x.offset[0] + ", " + (x.data.Count - 1 + int.Parse(x.offset[0])) + "}]";
            //        string text = link.Engine.Execute(query).Text;
            //        return text == "(" + string.Join(", ", x.data) + ")";
            //    }
            //
            //    return false;
            //}).ToList();
            //MarkdownPage("Sequences that needs mathematica code", "mathematica_error", subset);
            //Console.WriteLine("Done");
        }

        public static void MaybeNotMore(List<RDb> oeisDb)
        {
            Console.WriteLine("Maybenot more...");
            List<double> lengthOfNonMore = oeisDb.Where(x => x.keyword.Contains("more")).Select(x => (double)x.data.Count).ToList();
            double avg = lengthOfNonMore.Average();
            List<RDb> subset = oeisDb.Where(x => x.data.Count > avg && x.keyword.Contains("more")).ToList();
            MarkdownPage("Sequences with more terms than the average (" + Math.Round(avg, 0) + ") but with 'more' keyword", "maybe_not_more", subset, 3);
            Console.WriteLine("Done");
        }

        // public static void Distinct(List<RDb> oeisDb)
        // {
        //     List<string> keywords = oeisDb.Select(x => x.keyword).SelectMany(x => x).Distinct().ToList();
        //     List<string> autors = oeisDb.Select(x => x.author).Distinct().ToList();
        //     List<int> length = oeisDb.Select(x => x.data.Count).ToList();
        // }
    }
}