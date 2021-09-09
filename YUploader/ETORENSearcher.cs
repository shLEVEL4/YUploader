using AngleSharp.Html.Dom;
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
    internal class ETORENSearcher : ItemSearcher
    {
        // private string BASEURL = "https://jp.etoren.com/";
        public ETORENSearcher(YUModel model) : base(model)
        {
            Console.WriteLine("ETOREN Class");
        }

        async public Task<SearchResult> GetResult()
        {
            SearchResult sr = new SearchResult();
            List<ResultModel> rms = new List<ResultModel>();
            List<string> urls = new List<string>();
            var doc = default(IHtmlDocument);
            if (model.IsSearchPage)
            {
                string url = model.Url;
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
                var productEls = doc.QuerySelectorAll(".item-title");
                if (productEls == null)
                {
                    Console.WriteLine("failed to find productEls");
                    sr.Error = "検索結果が見つかりませんでした";
                    return sr;
                }
                foreach (var productEl in productEls)
                {
                    var a_dir = productEl.QuerySelector("a");
                    string href = a_dir.GetAttribute("href");
                    urls.Add(href);
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
                rm.Options = "";
                /*rm.Options = GetOptions(doc);
                Console.WriteLine("====GetAddi2====");
                Console.WriteLine(rm.Options);
                Console.WriteLine("====GetAddi2====");*/
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
            var priceEl = doc.QuerySelector(".price-new");
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
            catch (Exception ex)
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
            var nameEl = doc.QuerySelector(".product-page-title");
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
            var nameEls = doc.QuerySelectorAll(".variation-product option");
            if (nameEls == null || nameEls.Count() < 2)
            {
                Console.WriteLine("failed to find nameEls tag");
                return res;
            }
            res += "バリエーション#";
            for (var i=1;i<nameEls.Count();i++)
            {
                var tmp = nameEls[i].TextContent.Replace(Environment.NewLine, "");
                tmp = tmp.Replace(",", ".");
                tmp = tmp.Replace("text-warning", "");
                tmp = tmp.Replace("text-success", "");
                tmp = tmp.Replace("text-error", "");
                res += tmp.Trim();
                if (i != nameEls.Count()-1)
                {
                    res += ",";
                }
            }
            return res;
        }

        private string GetAddi2(IHtmlDocument doc)
        {
            var addiEL = doc.QuerySelector(".tab-specs table");
            if (addiEL == null)
            {
                return "NULL";
            }
            var addiEL2 = doc.QuerySelector(".content-desc");
            if (addiEL2 == null)
            {
                return Utils.RemoveAttrsHTMLTag(addiEL.OuterHtml);
            }
            return "以下　英文ですが仕入れ先からの説明文となります。<br><br>" + Utils.RemoveAttrsHTMLTag(addiEL.OuterHtml) + "<br><br>" + Utils.RemoveAttrsHTMLTag(addiEL2.OuterHtml);
        }

        private List<string> GetImages(IHtmlDocument doc)
        {
            List<string> srcs = new List<string>();
            var imageEl = doc.QuerySelector("#large_product_image");
            string src = imageEl.GetAttribute("src");
            if (ValidateImgUrl(src))
            {
                srcs.Add(src);
            }
            return srcs;
        }

        private string GetItemCode()
        {
            DateTime dt = DateTime.Now;
            return "YET-" + GetItemCodeSuf();
        }
    }
}
