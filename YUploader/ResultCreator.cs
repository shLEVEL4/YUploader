using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YUploader
{
    public partial class MainWindow
    {
        public Canvas GetResult(ResultModel result)
        {
            Canvas can = new Canvas();
            can.Tag = result.ItemCode;
            can.Height = 250;
            if (result.Imgs.Count() > 0)
            {
                Image img = GetImage(result.Imgs[result.CurrentImgIndex]);
                can.Children.Add(img);
            }
            Button upload = GetUploadButton(result);
            can.Children.Add(upload);
            TextBlock utb = UploadResultTB(result);
            can.Children.Add(utb);
            Button clear = GetClearButton(result);
            can.Children.Add(clear);
            if (result.Imgs.Count() > 1)
            {
                TextBlock ni = GetNextImageTB(result);
                can.Children.Add(ni);
                TextBlock pi = GetPrevImageTB(result);
                can.Children.Add(pi);
            }
            Label name_l = GetLabel("商品名", 46, 235, 20);
            can.Children.Add(name_l);
            Label price_l = GetLabel("通常販売価格", 81, 235, 52);
            can.Children.Add(price_l);
            Label path_l = GetLabel("パス", 29, 235, 84);
            can.Children.Add(path_l);
            Label item_code_l = GetLabel("商品コード", 61, 235, 116);
            can.Children.Add(item_code_l);
            Label opt_l = GetLabel("オプション", 61, 235, 148);
            can.Children.Add(opt_l);
            Label cate_l = GetLabel("カテゴリ", 61, 424, 118);
            can.Children.Add(cate_l);
            TextBox name = GetNameTB(result);
            can.Children.Add(name);
            TextBox price = GetPriceTB(result);
            can.Children.Add(price);
            TextBox addi2 = GetAddi2TB(result);
            can.Children.Add(addi2);
            ComboBox path = GetPath(result);
            can.Children.Add(path);
            TextBox item_code = GetItemCodeTB(result);
            can.Children.Add(item_code);
            TextBox opt = GetOptionTB(result);
            can.Children.Add(opt);
            ComboBox cb = GetCategory(result);
            can.Children.Add(cb);
            return can;
        }

        public Label GetLabel(string title, int width, int left, int top)
        {
            Label lb = new Label();
            lb.Content = title;
            lb.Width = width;
            Canvas.SetLeft(lb, left);
            Canvas.SetTop(lb, top);
            return lb;
        }

        public Image GetImage(string ImageUrl)
        {
            Image img = new Image();
            img.Height = 164;
            img.Width = 186;
            img.Source = new BitmapImage(new Uri(ImageUrl));
            LengthConverter myLengthConverter = new LengthConverter();
            Canvas.SetLeft(img, 22);
            Canvas.SetTop(img, 23);
            return img;
        }

        public Button GetUploadButton(ResultModel result)
        {
            Button btn = new Button();
            btn.Content = "出品";
            btn.Tag = result.ItemCode;
            btn.Width = 75;
            btn.Height = 30;
            btn.Background = Brushes.DeepSkyBlue;
            btn.Foreground = Brushes.White;
            btn.BorderBrush = Brushes.DeepSkyBlue;
            btn.Click += UploadButtonMouseDownAsync;
            Canvas.SetLeft(btn, 570);
            Canvas.SetTop(btn, 179);
            return btn;
        }

        public Button GetClearButton(ResultModel result)
        {
            Button btn = new Button();
            btn.Content = "クリア";
            btn.Tag = result.ItemCode;
            btn.Width = 75;
            btn.Height = 30;
            btn.Background = Brushes.OrangeRed;
            btn.Foreground = Brushes.White;
            btn.BorderBrush = Brushes.OrangeRed;
            btn.Click += ClearButtonMouseDown;
            Canvas.SetLeft(btn, 656);
            Canvas.SetTop(btn, 179);
            return btn;
        }

        public ComboBox GetPath(ResultModel result)
        {
            ComboBox cb = new ComboBox();
            cb.Tag = result.ItemCode;
            foreach (string path in paths)
            {
                cb.Items.Add(path);
            }
            cb.SelectionChanged += Path_Changed;
            cb.Text = result.Path;
            cb.Height = 26;
            Canvas.SetLeft(cb, 280);
            Canvas.SetTop(cb, 87);
            return cb;
        }

        public TextBlock GetNextImageTB(ResultModel result)
        {
            TextBlock tb = new TextBlock();
            tb.Uid = result.ItemCode + "GNTB";
            tb.Text = "＞";
            tb.Tag = result.ItemCode;
            tb.Width = 14;
            tb.PreviewMouseDown += NextImage_PreviewMouseDown;
            Canvas.SetLeft(tb, 210);
            Canvas.SetTop(tb, 94);
            return tb;
        }

        public TextBlock GetPrevImageTB(ResultModel result)
        {
            TextBlock tb = new TextBlock();
            tb.Uid = result.ItemCode + "GPTB";
            tb.Text = "＜";
            tb.Tag = result.ItemCode;
            tb.Width = 14;
            tb.Visibility = Visibility.Hidden;
            tb.PreviewMouseDown += PrevImage_PreviewMouseDown;
            Canvas.SetLeft(tb, 5);
            Canvas.SetTop(tb, 94);
            return tb;
        }

        public TextBlock UploadResultTB(ResultModel result)
        {
            TextBlock tb = new TextBlock();
            tb.Uid = result.ItemCode + "URTB";
            tb.Text = "";
            Canvas.SetLeft(tb, 570);
            Canvas.SetTop(tb, 210);
            return tb;
        }

        public TextBox GetNameTB(ResultModel result)
        {
            TextBox tb = new TextBox();
            tb.Tag = result.ItemCode;
            tb.TextChanged += Name_TextChanged;
            tb.Text = result.Name;
            tb.Height = 26;
            tb.Width = 457;
            Canvas.SetLeft(tb, 280);
            Canvas.SetTop(tb, 23);
            return tb;
        }

        public TextBox GetAddi2TB(ResultModel result)
        {
            TextBox tb = new TextBox();
            tb.Tag = result.ItemCode;
            tb.TextChanged += Addi2_TextChanged;
            tb.Text = result.Additional2;
            tb.Height = 190;
            tb.Width = 457;
            Canvas.SetLeft(tb, 780);
            Canvas.SetTop(tb, 20);
            return tb;
        }

        public TextBox GetPriceTB(ResultModel result)
        {
            TextBox tb = new TextBox();
            tb.Tag = result.ItemCode;
            tb.TextChanged += Price_TextChanged;
            tb.Text = result.Price;
            tb.Height = 26;
            tb.Width = 68;
            Canvas.SetLeft(tb, 322);
            Canvas.SetTop(tb, 55);
            return tb;
        }

        public TextBox GetPathTB(ResultModel result)
        {
            TextBox tb = new TextBox();
            tb.Tag = result.ItemCode;
            tb.TextChanged += Path_TextChanged;
            tb.Text = result.Path;
            tb.Height = 26;
            tb.Width = 457;
            Canvas.SetLeft(tb, 280);
            Canvas.SetTop(tb, 87);
            return tb;
        }

        public TextBox GetItemCodeTB(ResultModel result)
        {
            TextBox tb = new TextBox();
            tb.Tag = result.ItemCode;
            tb.TextChanged += ItemCode_TextChanged;
            tb.Text = result.ItemCode;
            tb.Height = 26;
            tb.Width = 108;
            Canvas.SetLeft(tb, 300);
            Canvas.SetTop(tb, 119);
            return tb;
        }

        public TextBox GetOptionTB(ResultModel result)
        {
            TextBox tb = new TextBox();
            tb.Tag = result.ItemCode;
            tb.TextChanged += Options_TextChanged;
            tb.Text = result.Options;
            tb.Height = 26;
            tb.Width = 410;
            Canvas.SetLeft(tb, 300);
            Canvas.SetTop(tb, 151);
            return tb;
        }
        public ComboBox GetCategory(ResultModel result)
        {
            ComboBox cb = new ComboBox();
            cb.Tag = result.ItemCode;
            cb.ItemsSource = categoryList;
            cb.SelectedValuePath = "Key";
            cb.DisplayMemberPath = "Value";
            cb.SelectionChanged += Category_Changed;
            cb.Text = result.Category;
            cb.Height = 26;
            Canvas.SetLeft(cb, 471);
            Canvas.SetTop(cb, 120);
            return cb;
        }
    }
}
