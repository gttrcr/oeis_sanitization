namespace OeisSanitization.Sanitization
{
    public static class Interesting
    {
        public static void MostProductive(List<RDb> oeisDb)
        {
            Console.WriteLine("MostProductive...");
            List<Tuple<string, int>> authors = oeisDb.GroupBy(x => x.author).Select(x => Tuple.Create(x.Key, x.Count())).OrderByDescending(x => x.Item2).ToList();
            
            int stop = 100;
            authors = authors.GetRange(0, stop);
            List<RDb> subset = new(); //oeisDb.Where(x => x.data.Count > avg && x.keyword.Contains("more")).ToList();
            Utils.MarkdownPage("List of " + stop + " most productive authors", "interesting", "most_productive", subset, 3);
            Console.WriteLine("Done");
        }
    }
}