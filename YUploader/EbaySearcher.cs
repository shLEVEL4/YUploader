using AngleSharp;
using AngleSharp.Html;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace YUploader
{
    internal class EbaySearcher : ItemSearcher
    {
        private string BASEURL = "https://www.ebay.com/";
        public EbaySearcher(YUModel model) : base(model)
        {
            Console.WriteLine("Ebay Class");
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
                    url = BASEURL + "sch/i.html?_nkw=&_in_kw=1&_ex_kw=&_sacat=0&_udlo=&_udhi=&_ftrt=901&_ftrv=1&_sabdlo=&_sabdhi=&_samilow=&_samihi=&_sadis=15&_stpos=&_sargn=-1%26saslc%3D1&_salic=1&_fss=1&_fsradio=%26LH_SpecificSeller%3D1&_saslop=1&_sasl=" + model.Seller + "&_sop=12&_dmd=1&_ipg=200&_fosrp=1";
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
                var productEls = doc.QuerySelectorAll(".vip");
                if (productEls == null)
                {
                    Console.WriteLine("failed to find productEls");
                    sr.Error = "検索結果が見つかりませんでした";
                    return sr;
                }
                foreach (var productEl in productEls)
                {
                    //Console.WriteLine(productEl.TextContent);
                    string href = productEl.GetAttribute("href");
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
            var priceEl = doc.QuerySelector("#convbidPrice");
            if (priceEl == null)
            {
                Console.WriteLine("failed to find convbidPrice tag");
                priceEl = doc.QuerySelector("#convbinPrice");
                if (priceEl == null)
                {
                    Console.WriteLine("failed to find convbinPrice tag");
                    return "NULL";
                }
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
            var nameEl = doc.QuerySelector("#itemTitle");
            if (nameEl == null)
            {
                Console.WriteLine("failed to find name tag");
                return "NULL";
            }
            return Regex.Replace(nameEl.TextContent, @"^Details\s+about\s+", "");
        }

        private string GetOptions(IHtmlDocument doc)
        {
            var res = "";
            /*var nameEl = doc.QuerySelector(".vi-msku-cntr label");
            if (nameEl == null)
            {
                Console.WriteLine("failed to find vi-msku-cntr");
                return res;
            }
            var tmp = nameEl.TextContent.Replace(Environment.NewLine, "");
            res += tmp.Trim() + "#";*/
            var opts = doc.QuerySelectorAll("#msku-sel-1 option");
            if (opts == null || opts.Count() < 2)
            {
                Console.WriteLine("failed to find msku-sel-1");
                return res;
            }
            var opts2 = doc.QuerySelectorAll("#msku-sel-2 option");
            if (opts2 != null)
            {
                res += "カラー#";
            }
            else
            {
                res += "サイズ#";
            }
            for(int i=1;i<opts.Count();i++)
            {
                res += opts[i].TextContent;
                if (i != opts.Count()-1)
                {
                    res += ",";
                }
            }
            if (opts2 != null && opts2.Count() > 1)
            {
                res += "|サイズ#";
                for (int i = 1; i < opts2.Count(); i++)
                {
                    res += opts2[i].TextContent;
                    if (i != opts2.Count() - 1)
                    {
                        res += ",";
                    }
                }
            }
            return res;
        }
        private string GetAddi2(IHtmlDocument doc)
        {
            var addiELs = doc.QuerySelectorAll("#vi-desc-maincntr .itemAttr table");
            if (addiELs == null)
            {
                return "NULL";
            }
            string addi = "";
            addi += Utils.RemoveAttrsHTMLTag(addiELs[0].OuterHtml) + "<br><br>";
            if (addiELs.Count() > 1)
            {
                addi += Utils.RemoveAttrsHTMLTag(addiELs[1].OuterHtml);
            }
            return "以下　英文ですが仕入れ先からの説明文となります。<br><br>" + addi;
        }

        private List<string> GetImages(IHtmlDocument doc)
        {
            List<string> srcs = new List<string>();
            var icImgEl = doc.QuerySelector("#icImg");
            if (icImgEl == null)
            {
                Console.WriteLine("failed to find icImg tag");
                return srcs;
            }
            string icImg = icImgEl.GetAttribute("src");
            string[] tmp = icImg.Split('/');
            string fileName = tmp[tmp.Count() - 1];
            if (ValidateImgUrl(icImg))
            {
                srcs.Add(icImg);
            }

            var imageEls = doc.QuerySelectorAll("#vi_main_img_fs_slider .tdThumb");
            if (imageEls == null)
            {
                Console.WriteLine("failed to find imageEls");
                return srcs;
            }
            bool first = true;
            foreach (var imageEl in imageEls)
            {
                if (first)
                {
                    first = false;
                    continue;
                }
                var item = imageEl.QuerySelector("img");
                string src = item.GetAttribute("src");
                string[] tmp2 = src.Split('/');
                tmp2[tmp2.Count() - 1] = fileName;
                string newSrc = String.Join("/", tmp2);
                if (ValidateImgUrl(newSrc))
                {
                    srcs.Add(newSrc);
                }
            }
            return srcs;
        }

        private string GetItemCode()
        {
            DateTime dt = DateTime.Now;
            return "YEB-" + GetItemCodeSuf();
        }
    }
}
