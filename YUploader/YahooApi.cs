using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;

namespace YUploader
{
    internal class YahooApi
    {
        private string SELLER_ID = "primopasso";
        private string TOKEN_NEW_PATH = @"token_new.json";
        private HttpClient client;
        private YUModel model;
        private string authid;
        private DateTime authid_limit;
        private string _basicKey;
        private string explanationStr;
        private string optionsStr;
        private string spAdditionalStr;
        public YahooApi(YUModel model)
        {
            this.client = new HttpClient();
            Encoding enc = Encoding.GetEncoding(50220);
            this._basicKey = Convert.ToBase64String(enc.GetBytes(model.AppID + ":" + ConstStr.SECRET));
            this.model = model;
            this.authid = "";
            using (var reader = new StreamReader(ConstStr.EXPLANATION_FILE_PATH))
            {
                explanationStr = reader.ReadToEnd();
            }
            using (var reader = new StreamReader(ConstStr.OPTIONS_FILE_PATH))
            {
                optionsStr = reader.ReadToEnd();
            }
            using (var reader = new StreamReader(ConstStr.SP_ADDITIONAL_FILE_PATH))
            {
                spAdditionalStr = reader.ReadToEnd();
            }
        }

        private bool IsAuthidRefreshTime()
        {
            if (authid == "")
            {
                return true;
            }
            if (authid_limit < DateTime.Now)
            {
                return true;
            }
            return false;
        }

        private void logging(ResultModel result, string str)
        {
            var now = DateTime.Now;
            var file = String.Format(ConstStr.YAPILOG, now.ToString("yyyyMMdd"));
            Encoding enc = Encoding.GetEncoding("Shift_JIS");
            StreamWriter writer = new StreamWriter(file, true, enc);
            writer.WriteLine(result.Name + ": " + str);
            writer.Close();
        }

        public List<Category> GetCategories()
        {
            List<Category> categories = new List<Category> { };
            string url = string.Format(Urls.GET_CATEGORY, this.model.AppID);
            Console.WriteLine(url);
            XElement xml;
            try
            {
                xml = XElement.Load(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine("WebClient error: GetCategories.");
                Console.WriteLine(ex.StackTrace);
                return categories;
            }
            var ns = xml.GetDefaultNamespace();
            var items = xml.Descendants(ns + "Child");
            Console.WriteLine(items.Count());
            foreach (XElement item in items)
            {
                // Console.WriteLine(item.Value);
                Category category = new Category();
                category.ID = item.Element(ns + "Id").Value;
                category.Title = item.Element(ns + "Title").Element(ns + "Medium").Value;
                categories.Add(category);
            }
            return categories;
        }

        public async Task<int> RegistNewProductAsync(ResultModel result)
        {
            bool alreadyPosted = false;
            StreamReader reader = new StreamReader(ConstStr.UploadedItemCodeFile);
            while (reader.Peek() > -1)
            {
                string ic = reader.ReadLine().Trim();
                if (result.Name.Trim() == ic)
                {
                    alreadyPosted = true;
                }
            }
            if (alreadyPosted)
            {
                reader.Close();
                return -2;
            }
            reader.Close();
            if (IsAuthidRefreshTime())
            {
                int j = await GetAuthId();
                if (j < 0)
                {
                    return -1;
                }
            }
            string expl = string.Format(ConstStr.EXPLANATION, result.Name, explanationStr);
            string opts = "";
            if (result.Options.Length > 1)
            {
                opts = result.Options + "|";
            }
            var parameters = new Dictionary<string, string>()
            {
                { "seller_id", SELLER_ID },
                { "item_code", result.EditItemCode },
                { "name", result.Name },
                { "price", result.Price },
                { "path", result.Path },
                { "explanation", expl },
                { "additional1", Utils.GetAdditional1(result) },
                { "additional2", string.Format(ConstStr.Additional2, result.Additional2) },
                { "additional3", ConstStr.Additional3 },
                { "sp_additional", spAdditionalStr },
                { "options", opts + optionsStr },
                // { "product_category", result.Category },
            };
            var content = new FormUrlEncodedContent(parameters);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authid);
            string url = string.Format(Urls.REGIST_NEW_PRODUCT);
            HttpResponseMessage resp;
            try
            {
                resp = await client.PostAsync(url, content);
            }
            catch(Exception ex)
            {
                Console.WriteLine("WebClient error: RegistNewProductAsync.");
                Console.WriteLine(ex.StackTrace);
                return -1;
            }
            var stream = await resp.Content.ReadAsStreamAsync();
            XElement xml = XElement.Load(stream);
            Console.WriteLine("=======RegistNewProductAsync========");
            Console.WriteLine(xml.Value);
            Console.WriteLine("=======RegistNewProductAsync========");
            if (xml.Value.Contains("OK") == false)
            {
                Console.WriteLine("NG: RegistNewProductAsync: " + xml.Value);
                logging(result, xml.Value);
                return -1;
            }
            await Task.Delay(1000);
            var res = await SetStockAsync(result);
            if (res != 0)
            {
                Console.WriteLine("NG: failed SetStockAsync in RegistNewProductAsync");
                return -1;
            }
            return 0;
        }

        public async Task<int> RegistAllImages(ResultModel result)
        {
            if (IsAuthidRefreshTime())
            {
                int j = await GetAuthId();
                if (j < 0)
                {
                    return -1;
                }
            }
            try
            {
                Directory.CreateDirectory(result.ItemCode);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return -2;
            }
            WebClient wc = new WebClient();
            int i = 0;
            foreach(var url in result.Imgs)
            {
                if (i == 20)
                {
                    break;
                }
                string _index = "";
                if (i > 0)
                {
                    _index = "_" + i.ToString();
                }
                string[] tmp = url.Split('.');
                string suffix = tmp[tmp.Length - 1];
                foreach(string suff in Utils.GetSuffs())
                {
                    if (suffix == suff)
                    {
                        suffix = "." + suff;
                    }
                }
                try
                {
                    wc.DownloadFile(url, result.ItemCode + "\\" + result.ItemCode + _index + suffix);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("DownloadFile failed.");
                    Console.WriteLine(ex.StackTrace);
                }
                i++;
            }
            ZipFile.CreateFromDirectory(result.ItemCode, result.ItemCode + ".zip");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authid);
            string upl_url = string.Format(Urls.UPLOAD_ALL_IMGS, SELLER_ID);
            var multipart = new MultipartFormDataContent();
            var finfo = new FileInfo(result.ItemCode + ".zip");
            var fileContent = new StreamContent(File.OpenRead(finfo.FullName));
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                Name = "file",
                FileName = finfo.Name
            };
            multipart.Add(fileContent);
            HttpResponseMessage resp;
            try
            {
                resp = await client.PostAsync(upl_url, multipart);
            }
            catch (Exception ex)
            {
                Console.WriteLine("WebClient error: RegistAllImages.");
                Console.WriteLine(ex.StackTrace);
                return -1;
            }
            var stream = await resp.Content.ReadAsStreamAsync();
            XElement xml = XElement.Load(stream);
            Console.WriteLine("=======RegistAllImages========");
            Console.WriteLine(xml.Value);
            Console.WriteLine("=======RegistAllImages========");
            try
            {
                Directory.Delete(result.ItemCode, true);
                File.Delete(result.ItemCode + ".zip");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Delete dir and files: RegistAllImages.");
                Console.WriteLine(ex.StackTrace);
            }
            if (xml.Value.Contains("OK") == false)
            {
                Console.WriteLine("NG: RegistAllImages: " + xml.Value);
                logging(result, xml.Value);
                return -1;
            }
            StreamWriter sr = new StreamWriter(ConstStr.UploadedItemCodeFile, true);
            sr.WriteLine(result.Name);
            sr.Close();
            return 0;
        }

        public async Task<int> PublishAsync()
        {
            if (IsAuthidRefreshTime())
            {
                int j = await GetAuthId();
                if (j < 0)
                {
                    return -1;
                }
            }
            var parameters = new Dictionary<string, string>()
            {
                { "seller_id", SELLER_ID },
                { "mode", "1" },
            };
            var content = new FormUrlEncodedContent(parameters);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authid);
            string url = string.Format(Urls.PUBLISH_ALL);
            HttpResponseMessage resp;
            try
            {
                resp = await client.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("WebClient error: PublishAsync.");
                Console.WriteLine(ex.StackTrace);
                return -1;
            }
            var stream = await resp.Content.ReadAsStreamAsync();
            XElement xml = XElement.Load(stream);
            Console.WriteLine("=======PublishAsync========");
            Console.WriteLine(xml.Value);
            Console.WriteLine("=======PublishAsync========");
            if (xml.Value.Contains("OK") == false)
            {
                Console.WriteLine("NG: PublishAsync: " + xml.Value);
                return -1;
            }
            return 0;
        }

        public async Task<int> SetStockAsync(ResultModel result)
        {
            if (IsAuthidRefreshTime())
            {
                int j = await GetAuthId();
                if (j < 0)
                {
                    return -1;
                }
            }
            var parameters = new Dictionary<string, string>()
            {
                { "seller_id", SELLER_ID },
                { "item_code", result.ItemCode },
                { "quantity", "+5" },
                { "allow_overdraft", "1" },
            };
            var content = new FormUrlEncodedContent(parameters);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authid);
            string url = string.Format(Urls.V1_SETSTOCK);
            HttpResponseMessage resp;
            try
            {
                resp = await client.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("WebClient error: SetStockAsync.");
                Console.WriteLine(ex.StackTrace);
                return -1;
            }
            var stream = await resp.Content.ReadAsStreamAsync();
            XElement xml = XElement.Load(stream);
            Console.WriteLine("=======SetStockAsync========");
            Console.WriteLine(stream.ToString());
            Console.WriteLine("=======SetStockAsync========");
            if (xml.Value.Contains(result.ItemCode) == false)
            {
                Console.WriteLine("NG: SetStockAsync: " + xml.Value);
                logging(result, xml.Value);
                return -1;
            }
            return 0;
        }

        public async Task<int> GetAllImgs(ResultModel result)
        {
            if (IsAuthidRefreshTime())
            {
                int j = await GetAuthId();
                if (j < 0)
                {
                    return -1;
                }
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authid);
            string url = string.Format(Urls.V1_GETALLIMGS, SELLER_ID, result.ItemCode);
            HttpResponseMessage resp;
            try
            {
                resp = await client.GetAsync(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine("WebClient error: GetAllImgs.");
                Console.WriteLine(ex.StackTrace);
                return -1;
            }
            var stream = await resp.Content.ReadAsStreamAsync();
            XElement xml = XElement.Load(stream);
            Console.WriteLine("====ALLIMG====" + xml.Value + "====ALLIMG====");
            var ns = xml.GetDefaultNamespace();
            var urls = xml.Descendants(ns + "Url");
            foreach(var item in urls)
            {
                Console.WriteLine("====URL====" + item.Value + "====URL====");
            }
            return 0;
        }

        public async Task<int> V1GetToken(string code)
        {
            var parameters = new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", "https://www.yahoo.co.jp/" },
            };
            var content = new FormUrlEncodedContent(parameters);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _basicKey);
            string url = string.Format(Urls.V1_GETTOKEN);
            HttpResponseMessage resp;
            try
            {
                resp = await client.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("WebClient error: V1GetToken.");
                Console.WriteLine(ex.StackTrace);
                return -1;
            }
            var resstr = await resp.Content.ReadAsStringAsync();
            Console.WriteLine(resstr);
            TokenNew tokenNew = JSONTokenNew(resstr);
            if (tokenNew == null)
            {
                return -1;
            }
            Console.WriteLine("=======access_token=========");
            Console.WriteLine(tokenNew.access_token);
            Console.WriteLine("=======access_token=========");
            using (var sw = new StreamWriter(TOKEN_NEW_PATH, false, System.Text.Encoding.UTF8))
            {
                sw.Write(resstr);
            }
            authid = tokenNew.access_token;
            return 0;
        }

        public async Task<int> V1RefreshToken(string refresh_token)
        {
            var parameters = new Dictionary<string, string>()
            {
                { "grant_type", "refresh_token " },
                { "refresh_token", refresh_token },
            };
            var content = new FormUrlEncodedContent(parameters);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _basicKey);
            string url = string.Format(Urls.V1_GETTOKEN);
            HttpResponseMessage resp;
            try
            {
                resp = await client.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("WebClient error: V1RefreshToken.");
                Console.WriteLine(ex.StackTrace);
                return -1;
            }
            var resstr = await resp.Content.ReadAsStringAsync();
            Console.WriteLine(resstr);
            TokenRefresh tokenRef = JSONTokenRefresh(resstr);
            if (tokenRef == null)
            {
                return -1;
            }
            Console.WriteLine("=======access_token=========");
            Console.WriteLine(tokenRef.access_token);
            Console.WriteLine("=======access_token=========");
            if (tokenRef.access_token == null || tokenRef.access_token.Length == 0)
            {
                Console.WriteLine("access_token is null or length is 0");
                return -1;
            }
            authid = tokenRef.access_token;
            authid_limit = DateTime.Now.AddMinutes(8);
            return 0;
        }

        public async Task<int> GetAuthId()
        {
            TokenNew tokenNew;
            if (File.Exists(TOKEN_NEW_PATH))
            {
                using (var reader = new StreamReader(TOKEN_NEW_PATH))
                {
                    string tokenNewStr = reader.ReadToEnd();
                    tokenNew = JSONTokenNew(tokenNewStr);
                }
                int res = await V1RefreshToken(tokenNew.refresh_token);
                if (res < 0)
                {
                    Console.WriteLine("token refresh is failed...");
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                Console.WriteLine("token new file is not exists...");
            }

            // あきらめてAuthorizationエンドからやり直し
            string url = string.Format(Urls.V1_AUTH_END, this.model.AppID);
            Console.WriteLine(url);
            HttpResponseMessage resp;
            try
            {
                resp = await client.GetAsync(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine("WebClient error: GetAuthId.");
                Console.WriteLine(ex.StackTrace);
                return -1;
            }
            Console.WriteLine("=========GetAuthId===========");
            Console.WriteLine(resp.StatusCode);
            Console.WriteLine(resp.Headers);
            Console.WriteLine(resp.Headers.Location);
            Console.WriteLine("=========GetAuthId===========");
            string redir_url = resp.RequestMessage.RequestUri.ToString();
            Console.WriteLine(redir_url);
            Process proc = Process.Start(redir_url);
            return 0;
        }

        public void SetAuthID(string _authid)
        {
            authid = _authid;
        }

        private TokenNew JSONTokenNew(string jsonstr)
        {
            try
            {
                return JsonConvert.DeserializeObject<TokenNew>(jsonstr);
            }
            catch
            {
                Console.WriteLine("failed deserialize TokenNew: jsonstr: " + jsonstr);
                return null;
            }
        }

        private TokenRefresh JSONTokenRefresh(string jsonstr)
        {
            try
            {
                return JsonConvert.DeserializeObject<TokenRefresh>(jsonstr);
            }
            catch
            {
                Console.WriteLine("failed deserialize TokenRefresh: jsonstr: " + jsonstr);
                return null;
            }
        }

        private int WriteTokenNew(TokenNew obj)
        {
            string jsonData;
            try
            {
                jsonData = JsonConvert.SerializeObject(obj);
            }
            catch
            {
                Console.WriteLine("failed serialize TokenNew: access_token: " + obj.access_token);
                return -1;
            }

            using (var sw = new StreamWriter(TOKEN_NEW_PATH, false, Encoding.UTF8))
            {
                sw.Write(jsonData);
            }
            return 0;
        }
    }
}
