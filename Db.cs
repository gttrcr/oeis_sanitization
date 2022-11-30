namespace oeis_sanitization
{
    public class Db
    {
        public int number { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public List<string> comment { get; set; }
        public List<string> reference { get; set; }
        public List<string> link { get; set; }
        public List<string> formula { get; set; }
        public List<string> example { get; set; }
        public List<string> maple { get; set; }
        public List<string> mathematica { get; set; }
        public List<string> program { get; set; }
        public List<string> xref { get; set; }
        public string author { get; set; }
        public List<string> ext { get; set; }
        public int references { get; set; }
        public int revision { get; set; }

        public Db()
        {
            number = 0;
            id = "";
            name = "";
            comment = new List<string>();
            reference = new List<string>();
            link = new List<string>();
            formula = new List<string>();
            example = new List<string>();
            maple = new List<string>();
            mathematica = new List<string>();
            program = new List<string>();
            xref = new List<string>();
            author = "";
            ext = new List<string>();
            references = 0;
            revision = 0;
        }
    }

    public class PDb : Db
    {
        public string data { get; set; }
        public string offset { get; set; }
        public string keyword { get; set; }
        public string time { get; set; }
        public string created { get; set; }

        public PDb()
        {
            data = "";
            offset = "";
            keyword = "";
            time = "";
            created = "";
        }
    }

    public class RDb : Db
    {
        public List<string> data { get; set; }
        public List<string> offset { get; set; }
        public List<string> keyword { get; set; }
        public DateTime time { get; set; }
        public DateTime created { get; set; }

        public RDb()
        {
            data = new List<string>();
            offset = new List<string>();
            keyword = new List<string>();
            time = new DateTime();
            created = new DateTime();
        }

        public override string ToString()
        {
            return "|[" + number + "](https://oeis.org/A" + number.ToString().PadLeft(6, '0') + ")|"; // + name.Replace("{", " ").Replace("}", " ") + "|";
        }
    }
}