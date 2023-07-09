using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace oeis_sanitization
{
    public class Program
    {
        public static void CreateOeisDbJson(string dbJson)
        {
            File.Delete("names");
            File.Delete("names.gz");
            Utils.Download("https://oeis.org/names.gz");
            Utils.Decompress(new FileInfo("names.gz"));
            int count = 0;
            List<string> sequences = File.ReadAllLines("names").Where(x => x.StartsWith('A')).ToList();
            sequences = sequences.Select(x => x.Split(' ')[0]).ToList();
            DateTime startDt = DateTime.Now;
            List<JObject?> db = new List<JObject?>();
            Parallel.For(0, sequences.Count, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, i =>
            {
                string sequence = sequences[i];
                double percentage = 100 * (++count) / (float)(sequences.Count);
                TimeSpan estimateEnd = (100 - percentage) * (DateTime.Now - startDt) / percentage;
                Console.WriteLine(percentage + "\t" + estimateEnd.Days + "dd " + estimateEnd.Hours + "h " + estimateEnd.Minutes + "m " + estimateEnd.Seconds + "s");
                string body = Utils.Get("https://oeis.org/search?q=id:" + sequence + "&fmt=json");
                if (!string.IsNullOrEmpty(body))
                {
                    JObject? j = JObject.Parse(body);
                    bool ok = (j != null) && (j["results"] != null) && (j["results"]?.Count() > 0) && (j["results"]?[0] != null);
                    if (ok)
                        db.Add((JObject?)(j?["results"]?[0]));
                    else
                        Console.WriteLine("ERROR");
                }
            });

            string str = JsonConvert.SerializeObject(db);
            Console.WriteLine("Completed with " + (sequences.Count - db.Count).ToString() + " errors");
            File.WriteAllText(dbJson, str);
        }

        public static void Sanitization(List<RDb> oeisDb)
        {
            Processes.BrokenLinks(oeisDb);
            Processes.DoneDeprecatedKeyword(oeisDb);
            Processes.DupeDeprecatedKeyword(oeisDb);
            Processes.HugeDeprecatedKeyword(oeisDb);
            Processes.LookDeprecatedKeyword(oeisDb);
            Processes.PartDeprecatedKeyword(oeisDb);
            Processes.UnedSequences(oeisDb);
            Processes.ObscDeprecatedKeyword(oeisDb);
            Processes.NeedMoreTerms(oeisDb);
            Processes.Allocated(oeisDb);
            Processes.MapleCode(oeisDb);
            Processes.MathematicaCode(oeisDb);
            Processes.SomeExamples(oeisDb);
            Processes.Unknown(oeisDb);
            Processes.Dead(oeisDb);
            Processes.MathematicaError(oeisDb);
        }

        public static void Main()
        {
            string dbJson = "db.json";

            if (File.Exists(dbJson) && (DateTime.Now - File.GetCreationTime(dbJson)).TotalDays < 2)
                Console.WriteLine(dbJson + " has been created less then 2 days ago. ");
            else
            {
                Console.WriteLine("Creating " + dbJson + "...");
                CreateOeisDbJson(dbJson);
                Console.WriteLine("Done");
            }

            Console.WriteLine("Reading db.json...");
            string file = File.ReadAllText(dbJson);
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