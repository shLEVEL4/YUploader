using AngleSharp.Html.Dom;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace YUploader
{
    internal class BUYMASearcher : ItemSearcher
    {
        private string BASEURL = "https://www.buyma.com/";
        public BUYMASearcher(YUModel model) : base(model)
        {
            Console.WriteLine("BUYMA Class");
        }

        async public Task<SearchResult> GetResult()
        {
            SearchResult sr = new SearchResult();
            List<ResultModel> rms = new List<ResultModel>();
            List<string> urls = new List<string>();
            var doc = default(IHtmlDocument);
            if (model.IsSearchPage)
            {
                string url;
                if (model.Seller != null && model.Seller.Length > 0)
                {
                    url = BASEURL + "r/-R120/" + HttpUtility.UrlEncode(model.Seller) + "/";
                }
                else
                {
                    url = model.Url;
                }
                try
                {
                    Console.WriteLine("SearchPage Url: " + url);
                    using (var stream = await this.client.GetStreamAsync(new Uri(url)))
                    {
                        var parser = new HtmlParser();
                        doc = await parser.ParseDocumentAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    sr.Error = "検索結果画面が見つかりませんでした";
                    return sr;
                }
                var productEls = doc.QuerySelectorAll(".product");
                if (productEls == null)
                {
                    Console.WriteLine("failed to find productEls");
                    sr.Error = "検索結果が見つかりませんでした";
                    return sr;
                }
                foreach (var productEl in productEls)
                {
                    string item_id = productEl.GetAttribute("item-id");
                    urls.Add(BASEURL + "item/" + item_id + "/");
                }
            }
            else
            {
                urls.Add(model.Url);
            }
            foreach (string url in urls)
            {
                try
                {
                    using (var stream = await this.client.GetStreamAsync(new Uri(url)))
                    {
                        var parser = new HtmlParser();
                        doc = await parser.ParseDocumentAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    continue;
                }
                ResultModel rm = ResultModelLogic.NewModel(this.model);
                rm.Price = GetPrice(doc);
                rm.Imgs = GetImages(doc);
                rm.ItemCode = GetItemCode();
                rm.EditItemCode = rm.ItemCode;
                rm.Name = GetName(doc);
                rm.Additional2 = GetAddi2(doc);
                rm.Options = GetOptions(doc);
                Console.WriteLine("====GetAddi2====");
                Console.WriteLine(rm.Options);
                Console.WriteLine("====GetAddi2====");
                rms.Add(rm);
                await Task.Delay(500);
            }
            if (rms.Count() == 0)
            {
                sr.Error = "結果を取得できませんでした";
                return sr;
            }
            else
            {
                sr.Results = rms;
                sr.Msg = rms.Count() + "件見つかりました。";
            }
            return sr;
        }

        private string GetPrice(IHtmlDocument doc)
        {
            var priceEl = doc.QuerySelector("#abtest_display_pc");
            if (priceEl == null)
            {
                Console.WriteLine("failed to find price tag");
                return "NULL";
            }
            string priceStr = Regex.Replace(priceEl.TextContent, @"[^0-9]", "");
            int price;
            try
            {
                price = int.Parse(priceStr);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return "NULL";
            }
            int priceCalced = PriceCalc(price);
            if (priceCalced < 0)
            {
                Console.WriteLine("failed to calc price");
                return "NULL";
            }
            return priceCalced.ToString();
        }

        private string GetName(IHtmlDocument doc)
        {
            var nameEl = doc.QuerySelector("#item_h1 span");
            if (nameEl == null)
            {
                Console.WriteLine("failed to find name tag");
                return "NULL";
            }
            return nameEl.TextContent;
        }

        private string GetOptions(IHtmlDocument doc)
        {
            var res = "";
            bool colorSel = false;
            var sizeEls = doc.QuerySelectorAll(".item_color_name");
            if (sizeEls == null)
            {
                Console.WriteLine("failed to find sizeEls");
            }
            else
            {
                colorSel = true;
                res += "カラー#";
                for (int i=0; i < sizeEls.Count(); i++)
                {
                    var tmp = sizeEls[i].TextContent.Replace(Environment.NewLine, "");
                    res += tmp.Trim();
                    if (i != sizeEls.Count() - 1)
                    {
                        res += ",";
                    }
                }
            }
            var colorEls = doc.QuerySelectorAll(".cse-set__table tr");
            if (colorEls == null)
            {
                Console.WriteLine("failed to find .cse-set__table");
            }
            else if (colorEls.Count() > 1)
            {
                if (colorSel)
                {
                    res += "|";
                }
                res += "サイズ#";
                for (int j=1; j<colorEls.Count(); j++)
                {
                    var td = colorEls[j].QuerySelector("td");
                    var tmp = td.TextContent.Replace(Environment.NewLine, "");
                    res += tmp.Trim();
                    if (j != colorEls.Count() - 1)
                    {
                        res += ",";
                    }
                }
            }
            return res;
        }

        private string GetAddi2(IHtmlDocument doc)
        {
            string res = "";
            IElement addiEL1 = doc.QuerySelector(".cse-set__table-wrap");
            IElement addiEL2 = doc.QuerySelector(".cse-detail");
            IElement addiEL3 = doc.QuerySelector("#item_maincol .free_txt");
            if (addiEL1 != null)
            {
                string tmp = Utils.RemoveAttrs(addiEL1.InnerHtml);
                tmp = Regex.Replace(tmp, @"<a></span>", "");
                tmp = Regex.Replace(tmp, @"</a>", "");
                res += tmp.Replace("<table", "<table border=\"1\" style=\"white-space: nowrap\"") + "<br><br>";
            }
            if (addiEL2 != null)
            {
                string tmp = Utils.RemoveAttrs(addiEL2.InnerHtml);
                tmp = Regex.Replace(tmp, @"<a></span>", "");
                tmp = Regex.Replace(tmp, @"</a>", "");
                res += tmp.Replace("<table", "<table border=\"1\" style=\"white-space: nowrap\"") + "<br><br>";
            }
            if (addiEL3 != null)
            {
                res += addiEL3.InnerHtml;
            }
            if (res == "")
            {
                res = "NULL";
            }

            return "以下　仕入れ先からの説明文となります。<br><br>" + res;
        }

        private List<string> GetImages(IHtmlDocument doc)
        {
            List<string> srcs = new List<string>();
            var imageEls = doc.QuerySelectorAll(".item-main-image");
            if (imageEls == null)
            {
                Console.WriteLine("failed to find images");
                return srcs;
            }
            foreach(var imageEl in imageEls)
            {
                string src = imageEl.GetAttribute("src");
                if (ValidateImgUrl(src))
                {
                    srcs.Add(src);
                }
            }
            return srcs;
        }

        private string GetItemCode()
        {
            DateTime dt = DateTime.Now;
            return "YBU-" + GetItemCodeSuf();
        }
    }
}
