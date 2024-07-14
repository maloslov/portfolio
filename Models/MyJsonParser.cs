using System.Text;

namespace portfolio.Models
{
    public enum MyJsonContentType
    {
        None,
        Object,
        Array,
        Content
    }

    public class MyJsonElement
    {
        public string Name = "";
        public MyJsonContentType ContentType;
        public MyJsonElement[]? Content;
    }

    public class MyJsonParser
    {
        public MyJsonElement RootElement { get; set; }

        public MyJsonParser(string json)
        {
            var a = (json
                .Replace("\\n","")
                .Replace("\n","")
                .Replace("  ","")
                .Replace("\": ","\":")
                );
            a = Format(a);
            RootElement = Parse(a.Split("\n"), 0);
            RootElement.Name = "root";
        }

        public List<string> FindProperty(string prop)
        {
            return FindProperty(prop, RootElement);
        }
        
        // RECURSIVE SEARCH
        private List<string> FindProperty(string prop, MyJsonElement elem)
        {
            var list = new List<string>();

            if (elem.Content != null)
                foreach (var el in elem.Content)
                {
                    if(elem.Name.Equals(prop)
                        && el.ContentType == MyJsonContentType.Content)
                    {
                        list.Add(el.Name);
                    }
                    else
                    {
                        var l2 = FindProperty(prop, el);
                        if (l2.Count() > 0)
                            list.AddRange(l2);
                    }
                }
            return list;
        }

        private string Format(string json)
        {
            var res = new StringBuilder();
            string str;
            int depth_cnt = 0;
            char c;

            for (var i = 0; i < json.Length; i++)
            {
                c = json[i];
                str = c.ToString();

                switch (c)
                {
                    case ':':
                        str = (json[i - 1].Equals('\"')
                            ? "\t"
                            : c.ToString()
                            );
                        break;
                    case ',':
                        str = (json[i + 1].Equals('\"') 
                            || json[i + 1].Equals('{')
                            ? $"\t{depth_cnt}\n"
                            : c.ToString()
                            );
                        break;
                    case '{':
                        depth_cnt++;
                        str = (json[i - 1 >= 0 ? i - 1 : i].Equals(':')
                            || json[i + 1].Equals('\"')
                            ? $"\t{depth_cnt}\n"
                            : "{"
                            );
                        break;
                    case '}':
                        str = (json[i + 1 < json.Length ? i + 1 : i].Equals(']') 
                            || json[i + 1 < json.Length ? i + 1 : i].Equals('}')
                            ? $"\t{depth_cnt}"
                            : "}"
                            );
                        depth_cnt--;
                        break;

                }
                res.Append(str);
            }
            return res.ToString().Trim('[').Trim(']');
        }
        // RECURSIVE PARSING
        private MyJsonElement Parse(string[] formatted, int depth)
        {
            var res = new MyJsonElement();

            var elements = new List<MyJsonElement>();
            var list = new List<string>();

            for (var i = 0; i < formatted.Count(); i++)
            {
                var items = formatted[i].Split("\t");

                if(items.Count() < 2)
                    continue;

                if (depth < Convert.ToInt32(items[items.Count() > 2 ? 2 : 1]))
                {
                    if (items.Count() > 2)
                        list.Add(formatted[i]);
                }
                else
                {
                    if (list.Count() > 0)
                    {
                        elements.Add(Parse(list.ToArray(), depth + 1));
                        list.Clear();
                    }
                    else
                    {
                        elements.Add(new MyJsonElement
                        {
                            Name = items[0].Trim('\"').Length > 0
                                ? items[0].Trim('\"')
                                : items[2],
                            ContentType = MyJsonContentType.Object,
                            Content = [new MyJsonElement
                            {
                                Name = items[1].Trim('\"'),
                                ContentType = MyJsonContentType.Content
                            }]
                        });
                    }
                }
            }
            if (list.Count() > 0)
            {
                elements.Add(Parse(list.ToArray(), depth + 1));
                list.Clear();
            }

            if (elements.Count > 1)
            {
                res.Content = elements.ToArray();
                res.ContentType = MyJsonContentType.Array;
            }
            else if(elements.Count > 0)
            {
                res = elements[0];
                res.ContentType = MyJsonContentType.Object;
            }
            else
            {
                res.ContentType = MyJsonContentType.None;
            }
            
            return res;
        }
    }
}
