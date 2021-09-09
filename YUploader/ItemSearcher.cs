using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace YUploader
{
    internal class ItemSearcher
    {
        protected YUModel model { set; get; }
        protected HttpClient client { set; get; }
        public List<PriceCalc> pcs;
        public ItemSearcher(YUModel model)
        {
            this.model = model;
            this.client = new HttpClient();
            Console.WriteLine("base class");
            pcs = new List<PriceCalc>();
            string catesStr = "";
            using (var reader = new StreamReader(ConstStr.PRICES_FILE_PATH))
            {
                catesStr = reader.ReadToEnd();
            }
            string[] paths = catesStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach(string path in paths)
            {
                string[] items = path.Split(',');
                if (items.Length != 3)
                {
                    Console.WriteLine("item column num is not 3: " + path);
                    continue;
                }
                string[] fromto = items[0].Split('-');
                if (fromto.Length != 2)
                {
                    Console.WriteLine("fromto array num is not 2: " + path);
                    continue;
                }
                string _from = fromto[0];
                if (_from == "")
                {
                    _from = "-10";
                }
                string _to = fromto[1];
                if (_to == "")
                {
                    _to = "220000000";
                }
                try
                {
                    PriceCalc pc = new PriceCalc();
                    pc.from = int.Parse(_from);
                    pc.to = int.Parse(_to);
                    pc.bairitsu = double.Parse(items[1]);
                    pc.souryou = int.Parse(items[2]);
                    pcs.Add(pc);
                }
                catch
                {
                    Console.WriteLine("item line is something failed: " + path);
                    continue;
                }
            }
        }

        public int PriceCalc(int price)
        {
            foreach(PriceCalc pc in pcs)
            {
                if (pc.from <= price && price < pc.to)
                {
                    int calced = (int)Math.Round(price * pc.bairitsu / 100);
                    return calced * 100 + pc.souryou;
                }
            }
            return price;
        }

        public string GetItemCodeSuf()
        {
            DateTime dt = DateTime.Now;
            return dt.ToString("yyyyMMddHHmmss") + "-" + Utils.GenerateRandom(4);
        }

        public bool ValidateImgUrl(string url)
        {
            string[] tmp = url.Split('.');
            string sfx = tmp[tmp.Length - 1].ToLower();
            foreach (string suff in Utils.GetSuffs())
            {
                if (sfx == suff)
                    return true;
            }
            Console.WriteLine("ValidateImgUrl NG: suff is " + sfx);
            return false;
        }
    }
}
