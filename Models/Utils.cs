using System.Text;

namespace portfolio.Models
{
    public struct Article
    {
        public string name;
        public string url;
        public string desc;
        public string updated;
        public string lang;
        public string readme;

        public string GetDate()
        {
            return Convert.ToDateTime(updated).ToString("d");
        }
        public string GetUrlNorm()
        {
            return url.Replace("api.", "").Replace("/repos", "");
        }

        public void UpdateReadme(string text)
        {
            readme = text;
        }
    }

    public class Utils
    {
        public static string FormatRepoSummary(Article record)
        {
            var new_text = new StringBuilder();
            new_text.Append("<details>");
            new_text.Append($"<summary>{record.name}</summary>");
            new_text.Append($"<p>Language: {record.lang}</p>");
            new_text.Append($"<p>Description: {record.desc}</p>");
            new_text.Append($"<p>Updated: {Convert.ToDateTime(record.updated).ToString("D")}</p>");
            new_text.Append($"<p>Link: {record.url}</p>");
            new_text.Append("</details>");
            return new_text.ToString();
        }
        public static string FormatRepoContent(Article text)
        {
            throw new NotImplementedException();


        }

        public static async Task<Article[]> GetAllReposDataAsync(string username, HttpClient client)
        {
            var records = new List<Article>();

            var names = new List<string>();
            var descs = new List<string>();
            var updated = new List<string>();
            var lang = new List<string>();

            var parser = new MyJsonParser(
                await client.GetStringAsync($"https://api.github.com/users/{username}/repos")
            );

            names = parser.FindProperty("name");
            descs = parser.FindProperty("description");
            updated = parser.FindProperty("updated_at");
            lang = parser.FindProperty("language");

            if (names.Count()
                % descs.Count()
                % updated.Count()
                % lang.Count()
                == 0)
            {
                for (int i = 0; i < names.Count; i++)
                {
                    records.Add(new Article
                    {
                        name = names[i],
                        url = $"https://api.github.com/repos/{username}/{names[i]}",
                        desc = descs[i],
                        updated = updated[i],
                        lang = lang[i],
                        readme = ""
                    });
                }
            }

            records = records.OrderByDescending(x => x.updated).ToList();
            return records.ToArray();
        }

        public static async Task<Article[]> GetReadmeAsync(Article[] articles, int idx, HttpClient client)
        {
            var readme = "";

            try
            {
                readme = Encoding.UTF8.GetString(
                    Convert.FromBase64String(
                        new MyJsonParser(
                                await client.GetStringAsync(
                                    $"{articles[idx].url}/contents/README.md"
                                    )
                            )
                        .FindProperty("content")[0]
                        )
                    );

            }
            catch { }
            finally
            {
                articles[idx].readme = FormatReadme(readme);
            }
            
            return articles;
        }

        public static string FormatReadme(string text)
        {
            var new_text = new StringBuilder();
            new_text.Append("<div>");
            foreach (var line in text.Split("\n"))
            {
                if (line.StartsWith("#"))
                {
                    new_text.Append($"<h2>{line.Replace("#", "")}</h2>");
                }
                else
                {
                    new_text.Append($"{line}<br>\n");
                }
            }
            new_text.Append("</div><br/>");
            return new_text.ToString();
        }
    }
}
