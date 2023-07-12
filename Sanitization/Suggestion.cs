namespace OeisSanitization.Sanitization
{
    public static class Suggestion
    {
        public static void MaybeNotMore(List<RDb> oeisDb)
        {
            Console.WriteLine("Maybe not more...");
            List<double> lengthOfNonMore = oeisDb.Where(x => !x.keyword.Contains("more")).Select(x => (double)x.data.Count).ToList();
            double avg = lengthOfNonMore.Average();
            List<RDb> subset = oeisDb.Where(x => x.data.Count > avg && x.keyword.Contains("more")).ToList();
            Utils.MarkdownPage("Sequences with more data than the average number of data (" + Math.Round(avg, 0) + ") but with 'more' keyword", "suggestions", "maybe_not_more", subset, 3);
            Console.WriteLine("Done");
        }

        public static void MaybeMore(List<RDb> oeisDb)
        {
            Console.WriteLine("Maybe more...");
            List<double> lengthOfNonMore = oeisDb.Where(x => !x.keyword.Contains("more")).Select(x => (double)x.data.Count).ToList();
            double avg = lengthOfNonMore.Average();
            double percentage = 0.1;
            List<RDb> subset = oeisDb.Where(x => x.data.Count < avg * percentage && !x.keyword.Contains("more")).ToList();
            Utils.MarkdownPage("Sequences with less data than " + percentage * 100 + "% of the average number of data (" + Math.Round(avg, 0) + ") but without 'more' keyword", "suggestions", "maybe_more", subset, 3);
            Console.WriteLine("Done");
        }
    }
}