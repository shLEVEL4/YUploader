using System;
using System.Collections.Generic;
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
        private void ClearButtonMouseDown(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Console.WriteLine("ClearButtonMouseDown: " + button.Tag);
            RemoveResultModel((string)button.Tag);
        }

        private async void UploadButtonMouseDownAsync(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string itemCode = (string)button.Tag;
            ResultModel re = TargetResultModel(itemCode);
            Canvas ca = TargetCanvas(itemCode);
            TextBlock result_tb = (TextBlock)Utils.GetByUid(ca, itemCode + "URTB");
            UpdateTextBlock(result_tb, "アップロード中...", Colors.Green);
            Console.WriteLine("===============================");
            Console.WriteLine("Category: " + re.Category);
            Console.WriteLine("EditItemCode: " + re.EditItemCode);
            Console.WriteLine("ItemCode: " + re.ItemCode);
            Console.WriteLine("Name: " + re.Name);
            Console.WriteLine("Path: " + re.Path);
            Console.WriteLine("Price: " + re.Price);
            Console.WriteLine("===============================");
            int apires;
            apires = await yApi.RegistNewProductAsync(re);
            if (apires == -2)
            {
                UpdateTextBlock(result_tb, "すでに登録済みです", Colors.Red);
                return;
            }
            if (apires < 0)
            {
                UpdateTextBlock(result_tb, "商品登録に失敗しました", Colors.Red);
                return;
            }
            await Task.Delay(1000);
            apires = await yApi.RegistAllImages(re);
            if (apires < 0)
            {
                UpdateTextBlock(result_tb, "画像アップロードに失敗しました", Colors.Red);
                return;
            }
            UpdateTextBlock(result_tb, "アップロード完了", Colors.Green);
        }

        private async void Publish_Click(object sender, RoutedEventArgs e)
        {
            UpdateTextBlock(this.PublishResult, "反映中...", Colors.Green);
            var res = await yApi.PublishAsync();
            if (res < 0)
            {
                UpdateTextBlock(this.PublishResult, "反映に失敗しました", Colors.Red);
                return;
            }
            UpdateTextBlock(this.PublishResult, "反映完了しました", Colors.Green);
        }

        private async void UploadAll_Click(object sender, RoutedEventArgs e)
        {
            int interval = int.Parse(Interval.Text) * 1000;
            foreach(ResultModel res in ress)
            {
                Canvas ca = TargetCanvas(res.ItemCode);
                TextBlock result_tb = (TextBlock)Utils.GetByUid(ca, res.ItemCode + "URTB");
                UpdateTextBlock(result_tb, "アップロード中...", Colors.Green);
                int apires;
                apires = await yApi.RegistNewProductAsync(res);
                if (apires == -2)
                {
                    UpdateTextBlock(result_tb, "すでに登録済みです", Colors.Red);
                    continue;
                }
                if (apires < 0)
                {
                    UpdateTextBlock(result_tb, "商品登録に失敗しました", Colors.Red);
                    continue;
                }
                await Task.Delay(1000);
                apires = await yApi.RegistAllImages(res);
                if (apires < 0)
                {
                    UpdateTextBlock(result_tb, "画像アップロードに失敗しました", Colors.Red);
                    continue;
                }
                UpdateTextBlock(result_tb, "アップロード完了", Colors.Green);
                await Task.Delay(interval);
            }
        }

        private void NextImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            string itemCode = (string)tb.Tag;
            ResultModel re = TargetResultModel(itemCode);
            if (re.CurrentImgIndex < re.Imgs.Count - 1)
            {
                re.CurrentImgIndex++;
            }
            ImgChange(re);
        }

        private void PrevImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            string itemCode = (string)tb.Tag;
            ResultModel re = TargetResultModel(itemCode);
            if (re.CurrentImgIndex > 0)
            {
                re.CurrentImgIndex--;
            }
            ImgChange(re);
        }

        private void ImgChange(ResultModel re)
        {
            Canvas ca = TargetCanvas(re.ItemCode);
            bool ImageConved = false;
            for (int j = 0; j < ca.Children.Count; j++)
            {
                if (typeof(Image) == ca.Children[j].GetType())
                {
                    ca.Children.RemoveAt(j);
                    ImageConved = true;
                    break;
                }
            }
            if (ImageConved)
            {
                Image img = GetImage(re.Imgs[re.CurrentImgIndex]);
                ca.Children.Add(img);
            }
            TextBlock prev = (TextBlock)Utils.GetByUid(ca, re.ItemCode + "GPTB");
            TextBlock next = (TextBlock)Utils.GetByUid(ca, re.ItemCode + "GNTB");
            if (re.CurrentImgIndex == 0)
            {
                prev.Visibility = Visibility.Hidden;
                next.Visibility = Visibility.Visible;
            }
            else if (re.CurrentImgIndex == re.Imgs.Count - 1)
            {
                prev.Visibility = Visibility.Visible;
                next.Visibility = Visibility.Hidden;
            }
            else
            {
                prev.Visibility = Visibility.Visible;
                next.Visibility = Visibility.Visible;
            }
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string itemCode = (string)tb.Tag;
            for (int i=0; i<ress.Count; i++)
            {
                if (ress[i].ItemCode == itemCode)
                {
                    ress[i].Name = tb.Text;
                }
            }
        }

        private void Addi2_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string itemCode = (string)tb.Tag;
            for (int i = 0; i < ress.Count; i++)
            {
                if (ress[i].ItemCode == itemCode)
                {
                    ress[i].Additional2 = tb.Text;
                }
            }
        }

        private void Price_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string itemCode = (string)tb.Tag;
            for (int i = 0; i < ress.Count; i++)
            {
                if (ress[i].ItemCode == itemCode)
                {
                    ress[i].Price = tb.Text;
                }
            }
        }

        private void Path_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string itemCode = (string)tb.Tag;
            for (int i = 0; i < ress.Count; i++)
            {
                if (ress[i].ItemCode == itemCode)
                {
                    ress[i].Path = tb.Text;
                }
            }
        }

        private void ItemCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string itemCode = (string)tb.Tag;
            for (int i = 0; i < ress.Count; i++)
            {
                if (ress[i].ItemCode == itemCode)
                {
                    ress[i].EditItemCode = tb.Text;
                }
            }
        }

        private void Options_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string itemCode = (string)tb.Tag;
            for (int i = 0; i < ress.Count; i++)
            {
                if (ress[i].ItemCode == itemCode)
                {
                    ress[i].Options = tb.Text;
                }
            }
        }

        private void Category_Changed(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            string itemCode = (string)cb.Tag;
            for (int i = 0; i < ress.Count; i++)
            {
                if (ress[i].ItemCode == itemCode)
                {
                    ress[i].Category = (string)cb.SelectedValue;
                    Console.WriteLine("ress[i].Category: " + ress[i].Category);
                }
            }
        }

        private void Path_Changed(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            string itemCode = (string)cb.Tag;
            for (int i = 0; i < ress.Count; i++)
            {
                if (ress[i].ItemCode == itemCode)
                {
                    ress[i].Path = (string)cb.SelectedValue;
                    Console.WriteLine("ress[i].Path: " + ress[i].Path);
                }
            }
        }
    }
}
