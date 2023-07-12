namespace OeisSanitization.Sanitization
{
    public static class Standard
    {
        public static void BrokenLinks(List<RDb> oeisDb)
        {
            Console.WriteLine("Broken links...");
            int index = 0;
            List<Tuple<RDb, List<string>>> broken = new List<Tuple<RDb, List<string>>>();
            broken = oeisDb.Select(x => new Tuple<RDb, List<string>>(x, Utils.AllLinks(x))).Where(x => x.Item2.Count > 0).ToList();
            broken = broken.AsParallel().Select(x =>
            {
                Console.WriteLine(index++ + "/" + broken.Count);
                return new Tuple<RDb, List<string>>(x.Item1, Utils.BrokenLinks(x.Item2));
            }).Where(x => x.Item2.Count > 0).ToList();
            Utils.MarkdownPage("Sequences that contains broken links", ".", "broken_links",
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
            Utils.MarkdownPage("Sequences with done deprecated keyword", ".", "done_keyword", subset, 3);
            Console.WriteLine("Done");
        }

        public static void DupeDeprecatedKeyword(List<RDb> oeisDb)
        {
            Console.WriteLine("Dupe deprecated keyword...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("dupe")).ToList();
            Utils.MarkdownPage("Sequences with dupe deprecated keyword", ".", "dupe_keyword", subset, 3);
            Console.WriteLine("Done");
        }

        public static void HugeDeprecatedKeyword(List<RDb> oeisDb)
        {
            Console.WriteLine("Huge deprecated keyword...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("huge")).ToList();
            Utils.MarkdownPage("Sequences with huge deprecated keyword", ".", "huge_keyword", subset, 3);
            Console.WriteLine("Done");
        }

        public static void LookDeprecatedKeyword(List<RDb> oeisDb)
        {
            Console.WriteLine("Look deprecated keyword...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("look")).ToList();
            Utils.MarkdownPage("Sequences with look deprecated keyword", ".", "look_keywords", subset, 3);
            Console.WriteLine("Done");
        }

        public static void PartDeprecatedKeyword(List<RDb> oeisDb)
        {
            Console.WriteLine("Part deprecated keyword...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("part")).ToList();
            Utils.MarkdownPage("Sequences with part deprecated keyword", ".", "part_keywords", subset, 3);
            Console.WriteLine("Done");
        }


        public static void UnedSequences(List<RDb> oeisDb)
        {
            Console.WriteLine("Uned sequences...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("uned")).ToList();
            Utils.MarkdownPage("Uned sequences", ".", "uned_sequences", subset, 3);
            Console.WriteLine("Done");
        }


        public static void ObscDeprecatedKeyword(List<RDb> oeisDb)
        {
            Console.WriteLine("Obsc deprecated keyword...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("obsc")).ToList();
            Utils.MarkdownPage("Obscure sequences", ".", "obsc_sequences", subset, 3);
            Console.WriteLine("Done");
        }


        public static void NeedMoreTerms(List<RDb> oeisDb)
        {
            Console.WriteLine("Need mode terms...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("more")).ToList();
            Utils.MarkdownPage("Sequences that needs more terms", ".", "need_more_terms", subset, 3);
            Console.WriteLine("Done");
        }

        public static void Allocated(List<RDb> oeisDb)
        {
            Console.WriteLine("Allocated...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.name.StartsWith("allocated for")).ToList();
            Utils.MarkdownPage("Allocated sequences", ".", "allocated", subset, 3);
            Console.WriteLine("Done");
        }


        public static void MapleCode(List<RDb> oeisDb)
        {
            Console.WriteLine("Maple code...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.maple.Count == 0).ToList();
            Utils.MarkdownPage("Sequences that needs maple code", ".", "need_maple", subset, 3);
            Console.WriteLine("Done");
        }


        public static void MathematicaCode(List<RDb> oeisDb)
        {
            Console.WriteLine("Mathematica code...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.mathematica.Count == 0).ToList();
            Utils.MarkdownPage("Sequences that needs mathematica code", ".", "need_mathematica", subset, 3);
            Console.WriteLine("Done");
        }

        public static void SomeExamples(List<RDb> oeisDb)
        {
            Console.WriteLine("Some examples...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.example.Count == 0).ToList();
            Utils.MarkdownPage("Sequences that needs examples", ".", "need_examples", subset, 3);
            Console.WriteLine("Done");
        }

        public static void Unknown(List<RDb> oeisDb)
        {
            Console.WriteLine("Unknown...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("unkn")).ToList();
            Utils.MarkdownPage("Unknown sequences", ".", "need_knowledge", subset, 3);
            Console.WriteLine("Done");
        }

        public static void Dead(List<RDb> oeisDb)
        {
            Console.WriteLine("Dead...");
            List<RDb> subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("dead")).ToList();
            Utils.MarkdownPage("Dead sequences", ".", "dead", subset, 3);
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
            //Utils.MarkdownPage("Sequences that needs mathematica code", "mathematica_error", subset);
            //Console.WriteLine("Done");
        }

        // public static void Distinct(List<RDb> oeisDb)
        // {
        //     List<string> keywords = oeisDb.Select(x => x.keyword).SelectMany(x => x).Distinct().ToList();
        //     List<string> autors = oeisDb.Select(x => x.author).Distinct().ToList();
        //     List<int> length = oeisDb.Select(x => x.data.Count).ToList();
        // }
    }
}