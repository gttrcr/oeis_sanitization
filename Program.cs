using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace oeis_sanitization
{
    public class Program
    {
        public static void CreateOeisDbJson(string db_json)
        {
            File.Delete("names");
            File.Delete("names.gz");
            Utils.Download("https://oeis.org/names.gz");
            Utils.Decompress(new FileInfo("names.gz"));
            int count = 0;
            List<string> sequences = File.ReadAllLines("names").Where(x => x.StartsWith('A')).ToList();
            sequences = sequences.Select(x => x.Split(' ')[0]).ToList();
            List<JObject?> db = sequences.AsParallel().Select(x =>
            {
                Console.WriteLine(100 * (++count) / (float)(sequences.Count));
                JObject? j = JObject.Parse(Utils.Get("https://oeis.org/search?q=id:" + x + "&fmt=json"));
                if (j != null)
                    if (j["result"] != null)
                        if (j["result"]?.Count() > 0 && j?["results"]?[0] != null)
                            return (JObject?)(j?["results"]?[0]);
                throw new Exception();
            }).ToList();
            string str = JsonConvert.SerializeObject(db);
            File.WriteAllText(db_json, str);
        }

        public static void Sanitization(List<RDb> oeisDb)
        {
            Console.WriteLine("Broken links...");
            Processes.BrokenLinks(ref oeisDb);
            Console.WriteLine("Done");

            Console.WriteLine("Done deprecated keyword...");
            Processes.DoneDeprecatedKeyword(ref oeisDb);
            Console.WriteLine("Done");

            //Console.WriteLine("Dupe deprecated keyword...");
            //subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("dupe")).ToList();
            //MarkdownPage("Sequences with dupe deprecated keyword", "dupe_keyword", subset, 3);
            //Console.WriteLine("Done");
            //
            //Console.WriteLine("Huge deprecated keyword...");
            //subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("huge")).ToList();
            //MarkdownPage("Sequences with huge deprecated keyword", "huge_keyword", subset, 3);
            //Console.WriteLine("Done");
            //
            //Console.WriteLine("Look deprecated keyword...");
            //subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("look")).ToList();
            //MarkdownPage("Sequences with look deprecated keyword", "look_keywords", subset, 3);
            //Console.WriteLine("Done");
            //
            //Console.WriteLine("Part deprecated keyword...");
            //subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("part")).ToList();
            //MarkdownPage("Sequences with part deprecated keyword", "part_keywords", subset, 3);
            //Console.WriteLine("Done");
            //
            //Console.WriteLine("Uned sequences...");
            //subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("uned")).ToList();
            //MarkdownPage("Uned sequences", "uned_sequences", subset, 3);
            //Console.WriteLine("Done");
            //
            //Console.WriteLine("Obsc deprecated keyword...");
            //subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("obsc")).ToList();
            //MarkdownPage("Obscure sequences", "obsc_sequences", subset, 3);
            //Console.WriteLine("Done");
            //
            //Console.WriteLine("Need mode terms...");
            //subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("more")).ToList();
            //MarkdownPage("Sequences that needs more terms", "need_more_terms", subset, 3);
            //Console.WriteLine("Done");
            //
            //Console.WriteLine("Allocated...");
            //subset = oeisDb.AsParallel().Where(x => x.name.StartsWith("allocated for")).ToList();
            //MarkdownPage("Allocated sequences", "allocated", subset, 3);
            //Console.WriteLine("Done");
            //
            //Console.WriteLine("Maple code...");
            //subset = oeisDb.AsParallel().Where(x => x.maple.Count == 0).ToList();
            //MarkdownPage("Sequences that needs maple code", "need_maple", subset, 3);
            //Console.WriteLine("Done");
            //
            //Console.WriteLine("Mathematica code...");
            //subset = oeisDb.AsParallel().Where(x => x.mathematica.Count == 0).ToList();
            //MarkdownPage("Sequences that needs mathematica code", "need_mathematica", subset, 3);
            //Console.WriteLine("Done");
            //
            //Console.WriteLine("Some examples...");
            //subset = oeisDb.AsParallel().Where(x => x.example.Count == 0).ToList();
            //MarkdownPage("Sequences that needs examples", "need_examples", subset, 3);
            //Console.WriteLine("Done");
            //
            //Console.WriteLine("Unknown...");
            //subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("unkn")).ToList();
            //MarkdownPage("Unknown sequences", "need_knowledge", subset, 3);
            //Console.WriteLine("Done");
            //
            //Console.WriteLine("Dead...");
            //subset = oeisDb.AsParallel().Where(x => x.keyword.Contains("dead")).ToList();
            //MarkdownPage("Dead sequences", "dead", subset, 3);
            //Console.WriteLine("Done");

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

        public static void Main()
        {
            string db_json = "db.json";

            //Console.WriteLine("Creating " + db_json);
            //CreateOeisDbJson(db_json);

            Console.WriteLine("Reading db.json...");
            string file = File.ReadAllText(db_json);
            List<PDb>? pDb = new List<PDb>();
            try
            {
                pDb = JsonConvert.DeserializeObject<List<PDb>>(file);
            }
            catch (JsonSerializationException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.LineNumber);
                Console.WriteLine(ex.LinePosition);
                return;
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.LineNumber);
                Console.WriteLine(ex.LinePosition);
                return;
            }
            Console.WriteLine("Done");

            Console.WriteLine("Creating restrictive object...");
            List<RDb> oeisDb = new List<RDb>();
            for (int i = 0; i < pDb?.Count; i++)
            {
                RDb ret = new RDb();
                ret.number = pDb[i].number;
                ret.id = pDb[i].id;
                ret.data = pDb[i].data.Split(',').ToList();
                ret.name = pDb[i].name;
                ret.comment = pDb[i].comment;
                ret.reference = pDb[i].reference;
                ret.link = pDb[i].link;
                ret.formula = pDb[i].formula;
                ret.example = pDb[i].example;
                ret.maple = pDb[i].maple;
                ret.mathematica = pDb[i].mathematica;
                ret.program = pDb[i].program;
                ret.xref = pDb[i].xref;
                ret.keyword = pDb[i].keyword.Split(',').ToList();
                ret.offset = pDb[i].offset.Split(',').ToList();
                ret.author = pDb[i].author;
                ret.ext = pDb[i].ext;
                ret.references = pDb[i].references;
                ret.revision = pDb[i].revision;
                if (DateTime.TryParse(pDb[i].time, out DateTime tmp))
                    ret.time = tmp;
                if (DateTime.TryParse(pDb[i].created, out tmp))
                    ret.created = tmp;

                oeisDb.Add(ret);
            }
            Console.WriteLine("Done");

            Sanitization(oeisDb);
        }
    }
}