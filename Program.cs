using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Octokit;
using System.ComponentModel;
using System.IO;

namespace GitHubProjectStatistic
{
    public class ProjCommiter
    {
        public ProjCommiter(string title, int add, int del, int com, string login=null)
        {
            this.title = title;
            this.add = add;
            this.del = del;
            this.com = com;
            this.login = login;
        }

        public string title { get; set; }

        public int add { get; set; }

        public int del { get; set; }

        public int com { get; set; }

        public string login { get; set; }
    }

    class Program
    {
        static string TeamInfo(string login)
        {
            switch (login)
            {
                case "chekist131":
                case "gggcode":
                    return "Альфа";
                case "jknightmmcs":
                case "vladpyslaru":
                case "almikh":
                    return "TrueDevelopers";
                case "unrealprox":
                case "mmcsIT2015":
                    return "Source_Code";
                case "kazanfar":
                case "PConst4":
                    return "DoubleK";
                case "TheVigor":
                case "oltr":
                    return "Стрела в колено";
                case "BatMich":
                case "AndranikTukhikov":
                    return "Ultimate";
                case "RybinIM":
                    return "Академ(";
                default:
                    return "UNKNOWN";
            }
        }

        static string nameInfo(string login)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            d["chekist131"] = "Валяев";
            d["gggcode"] = "Гулканян";
            d["jknightmmcs"] = "Леонтьев";
            d["vladpyslaru"] = "Пыслару";
            d["almikh"] = "Михайличенко";
            d["unrealprox"] = "Гончаров";
            d["mmcsIT2015"] = "Гончаров+Пак";
            d["kazanfar"] = "Гаджиев";
            d["PConst4"] = "Папиж";
            d["TheVigor"] = "Проскуряков";
            d["oltr"] = "Троицкий";
            d["BatMich"] = "Батраков";
            d["AndranikTukhikov"] = "Тухиков";
            d["RybinIM"] = "Рыбин";
            return d[login];
        }

        static void Main(string[] args)
        {
            string filename = "out.txt";
            string owner = "mmcsIT2015";
            string project = "OptimizedCompilersProject";
            var client = new GitHubClient(new ProductHeaderValue("GitHubProjectStatistic"));

            if (args.Length > 0)
            {
                var basicAuth = new Credentials(args[0], args[1]);
                client.Credentials = basicAuth;
            }
            else
            {
                var basicAuth = new Credentials("your GitHub login", "your GitHub password");
                client.Credentials = basicAuth;
            }

            //client.Repository.Statistics.GetCodeFrequency(ownder, project).Result.AdditionsAndDeletionsByWeek;

            var studs = client.Repository.Statistics.GetContributors(owner, project).Result.Select(s =>
            {
                //var weeks1 = s.Weeks;
                var weeks1 = s.Weeks.Where(e => e.Week.Date >= new DateTime(2015, 9, 27));
                return new ProjCommiter(
                    nameInfo(s.Author.Login),
                    weeks1.Select(e => e.Additions).Sum(),
                    weeks1.Select(e => e.Deletions).Sum(),
                    weeks1.Select(e => e.Commits).Sum(),
                    s.Author.Login);
            }
                );
            var teams = studs.GroupBy(e => TeamInfo(e.login),
                (a, b) => new ProjCommiter(a,
                b.Select(q => q.add).Sum(),
                b.Select(q => q.del).Sum(),
                b.Select(q => q.com).Sum()));

            List<string> text = new List<string>();
            addTablesGroup(text, "TEAMS", teams);
            addTablesGroup(text, "STUDENTS", studs);

            if (File.Exists(filename))
                File.Delete(filename);
            File.WriteAllLines(filename, text);
        }

        delegate int parameterSelector(ProjCommiter p);

        private static void addTablesGroup(List<string> text, string title, IEnumerable<ProjCommiter> comers)
        {
            text.Add(string.Empty);
            text.Add(title);

            //addTable(text, "Additions", comers, q => q.add);
            //addTable(text, "Deletions", comers, q => q.del);
            addTable(text, "Commits", comers, q => q.com);
            //addTable(text, "AVG additions per commit", comers, q => q.add / q.com);
            //addTable(text, "AVG deletions per commit", comers, q => q.del / q.com);
            addTable(text, "Additions + Deletions", comers, q => q.add + q.del);
            addTable(text, "AVG Additions + Deletions per commit", comers, q => (q.add + q.del) / q.com);
        }

        private static void addTable(List<string> text, string title, IEnumerable<ProjCommiter> commiters, parameterSelector p)
        {
            text.Add(string.Empty);
            text.Add("__" + title + "__");
            text.Add(string.Empty);
            text.Add("| No | Commiter | Count");
            text.Add("|:---:|:---:|:---:|");
            foreach (var stud in commiters.OrderByDescending(q => p(q)).ToList().Select((d, i) => new { d, i = i + 1 }))
                text.Add("| " + stud.i + " | " + stud.d.title + " | " + p(stud.d));
        }
    }
}
