using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace YUploader
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private string code = "";
        public Dictionary<string, string> categoryList { get; set; }
        private YahooApi yApi;
        private string[] paths;

        List<ResultModel> ress = new List<ResultModel>();
        List<Canvas> results = new List<Canvas>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            DisableButton(this.Search);
            string catesStr = "";
            using (var reader = new StreamReader(ConstStr.CATE_FILE_PATH))
            {
                catesStr = reader.ReadToEnd();
            }
            paths = catesStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            foreach(string path in paths)
            {
                AllPath.Items.Add(path);
            }

            categoryList = new Dictionary<string, string>();
            categoryList.Add("", "");
            DataContext = this;
            if (File.Exists(Setting.SETTING_JSON))
            {
                // 過去の検索結果を取得
                GetLastResult();
            }
            else
            {
                // Settingの初期設定
                GetInitSetting();
            }
            YUModel model = getCurrentYU();
            yApi = new YahooApi(model);
            List<Category> result;
            try
            {
                // Console.Write("hello");
                result = yApi.GetCategories();
                foreach (Category category in result)
                {
                    categoryList.Add(category.ID, category.Title);
                }
            }
            catch (Exception ex)
            {
                UpdateTextBlock(this.SearchResult, "カテゴリが取得できません", Colors.Red);
                UpdateTextBlock(this.LoginStatus, "AppIDを確認してください", Colors.Red);
                Console.Write(ex.StackTrace);
            }
            EnableButton(this.Search);
            // UpdateAllSearchResults(ResultModelLogic.GetTestData());
        }

        public void UpdateAllSearchResults(List<ResultModel> allRessults)
        {
            RemoveAllResultModel();
            ress = allRessults;
            SyncCategory();
            SyncPath();
            UpdateCanvas();
        }

        public void UpdateCanvas()
        {
            results = new List<Canvas>();
            foreach (ResultModel res in ress)
            {
                Canvas can = GetResult(res);
                results.Add(can);
            }
            foreach (Canvas can in results)
            {
                this.ResultStack.Children.Add(can);
            }
        }

        public void SyncCategory()
        {
            for (int i = 0; i < ress.Count; i++)
            {
                ress[i].Category = (string)AllCategory.SelectedValue;
            }
        }

        public void SyncPath()
        {
            for (int i = 0; i < ress.Count; i++)
            {
                ress[i].Path = (string)AllPath.SelectedValue;
            }
        }

        public void RemoveResultModel(string itemCode)
        {
            ResultModel rm = TargetResultModel(itemCode);
            Canvas can = TargetCanvas(itemCode);
            if (rm != null)
            {
                ress.Remove(rm);
            }
            if (can != null)
            {
                results.Remove(can);
                this.ResultStack.Children.Remove(can);
            }
            return;
        }

        public ResultModel TargetResultModel(string itemCode)
        {
            foreach (ResultModel res in ress)
            {
                if (res.ItemCode == itemCode)
                {
                    return res;
                }
            }
            return null;
        }

        public Canvas TargetCanvas(string itemCode)
        {
            foreach (Canvas item in results)
            {
                if ((string)item.Tag == itemCode)
                {
                    return item;
                }
            }
            return null;
        }

        public void RemoveAllResultModel()
        {
            ress = new List<ResultModel>();
            results = new List<Canvas>();
            this.ResultStack.Children.Clear();
        }

            private void GetLastResult()
        {
            SetSetting(GetAllSetting());
        }

        private void GetInitSetting()
        {
            SetSetting(InitSetting());
        }

        private void SetSetting(YUModel item)
        {
            this.Yid.Text = item.AppID;
            this.Store.Text = item.Store;
            this.Url.Text = item.Url;
            this.Seller.Text = item.Seller;
            this.AllCategory.Text = item.AllCategory;
            this.AllPath.Text = item.AllPath;
            this.Interval.Text = item.Interval;
            this.IsSearchPage.IsChecked = item.IsSearchPage;
        }

        private async void ItemSearch(object sender, RoutedEventArgs e)
        {
            UpdateTextBlock(this.SearchResult, "検索中...", Colors.Green);
            // 入力項目を保存する
            YUModel model = getCurrentYU();
            SaveAllSetting(model);
            SearchResult res = await SearchResultLogic.GetSearchResult(model);
            if(res.Error != null)
            {
                UpdateTextBlock(this.SearchResult, res.Error, Colors.Red);
                return;
            }
            foreach(ResultModel re in res.Results)
            {
                Console.WriteLine("Category: " + re.Category);
                Console.WriteLine("EditItemCode: " + re.EditItemCode);
                Console.WriteLine("ItemCode: " + re.ItemCode);
                Console.WriteLine("Name: " + re.Name);
                Console.WriteLine("Path: " + re.Path);
                Console.WriteLine("Price: " + re.Price);
            }
            UpdateAllSearchResults(res.Results);
            UpdateTextBlock(this.SearchResult, res.Msg, Colors.Green);
        }

        private YUModel getCurrentYU()
        {
            YUModel model = new YUModel();
            model.AppID = this.Yid.Text;
            model.Store = this.Store.Text;
            model.Url = this.Url.Text;
            model.Seller = this.Seller.Text;
            model.AllCategory = this.AllCategory.Text;
            model.AllPath = this.AllPath.Text;
            model.Interval = this.Interval.Text;
            model.IsSearchPage = this.IsSearchPage.IsChecked != null ? (bool)this.IsSearchPage.IsChecked : false;
            return model;
        }

        private void SomeElmChanged()
        {
            YUModel model = getCurrentYU();
            SaveAllSetting(model);
        }

        private void Yid_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableButton(this.Search);
            SomeElmChanged();
        }

        private void Url_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableButton(this.Search);
            SomeElmChanged();
        }

        private void AllClear_Click(object sender, RoutedEventArgs e)
        {
            RemoveAllResultModel();
        }

        private void GetToken_Click(object sender, RoutedEventArgs e)
        {
            UpdateTextBlock(this.TokenResult, "トークン取得中...", Colors.Green);
            string token = Token.Text;
            if (token.Split('?').Count() == 1)
            {
                UpdateTextBlock(this.TokenResult, "トークン取得失敗: 100", Colors.Red);
                return;
            }
            string[] kvs = token.Split('?')[1].Split('&');
            foreach (string kv in kvs)
            {
                string[] tmp = kv.Split('=');
                if (tmp[0] == "code")
                {
                    code = tmp[1];
                }
                else
                {
                    Console.WriteLine("not found this key: " + tmp[0]);
                }
            }
            // yApi.SetAuthID(access_token);
            if (code == "")
            {
                UpdateTextBlock(this.TokenResult, "トークン取得失敗: 101", Colors.Red);
                return;
            }
            var res = yApi.V1GetToken(code);
            UpdateTextBlock(this.TokenResult, "トークン取得完了", Colors.Green);
        }

        private void ComboBoxElmChanged(object sender, SelectionChangedEventArgs e)
        {
            SomeElmChanged();
        }

        private void AllPathChanged(object sender, SelectionChangedEventArgs e)
        {
            SomeElmChanged();
            SyncPath();
            this.ResultStack.Children.Clear();
            UpdateCanvas();
        }

        private void TextBoxElmChanged(object sender, TextChangedEventArgs e)
        {
            SomeElmChanged();
        }

        private void IsSearchPage_Changed(object sender, RoutedEventArgs e)
        {
            SomeElmChanged();
        }

        private void Seller_TextChanged(object sender, TextChangedEventArgs e)
        {
            SomeElmChanged();
        }

        private void Interval_TextChanged(object sender, TextChangedEventArgs e)
        {
            SomeElmChanged();
        }
    }
}
