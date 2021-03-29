using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AppServer.Controllers
{
    public class MapReducerController : Controller
    {
        public string Text;
        public ActionResult Index()
        {
            return View();
        }

        public List<string> Map()
        {
            string line = Text;
            string writePath = @"C:\Users\Admin\Desktop\Code\Files\Map\1.txt";
            
                var onlyText = Regex.Replace(line, @"\.|;|:|,|'", "");
                var words = onlyText.Split();
                using (StreamWriter sw = new StreamWriter(writePath, false, System.Text.Encoding.Default))
                {
                    foreach (var word in words)
                    {
                        sw.WriteLine("{0}\t1", word);
                    }
                }
            return words.ToList();
        }
        public string Shuffle()
        {
            return "Shuffle";
        }
        public string Reduce(List<string> listWords)
        {
            Dictionary<string, int> words = new Dictionary<string, int>();
            string writePath = @"C:\Users\Admin\Desktop\Code\Files\Reduce\1.txt";
            string line = Text;
            int i = 0;
            while (line.Split().Length > i)
            {
                var sArr = listWords;
                string word = sArr[i];
                int count = 1;

                if (words.ContainsKey(word))
                {
                    words[word] += count;
                }
                else
                {
                    words.Add(word, count);
                }
                i++;
            }
            using (StreamWriter sw = new StreamWriter(writePath, false, System.Text.Encoding.Default))
            {
                foreach (var word in words)
                {
                    sw.WriteLine("{0}\t{1}", word.Key, word.Value);
                }
            }
            return "Reduce";
        }

        

        [HttpGet]
        public async Task<string> MapReducer(string text)
        {
            Text = text;
            List<string> words = Map();
            Reduce(words);
            return "localhost:1";
        }
    }
}
