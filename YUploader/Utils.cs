using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace YUploader
{
    static class Utils
    {
        public static string GenerateRandom(int length)
        {
            string passwordChars = "0123456789abcdefghijklmnopqrstuvwxyz";
            StringBuilder sb = new StringBuilder(length);
            Random r = new Random();

            for (int i = 0; i < length; i++)
            {
                //文字の位置をランダムに選択
                int pos = r.Next(passwordChars.Length);
                //選択された位置の文字を取得
                char c = passwordChars[pos];
                //パスワードに追加
                sb.Append(c);
            }

            return sb.ToString();
        }

        public static UIElement GetByUid(DependencyObject rootElement, string uid)
        {
            foreach (UIElement element in LogicalTreeHelper.GetChildren(rootElement).OfType<UIElement>())
            {
                if (element.Uid == uid)
                    return element;
                UIElement resultChildren = GetByUid(element, uid);
                if (resultChildren != null)
                    return resultChildren;
            }
            return null;
        }

        public static string GetAdditional1(ResultModel rm)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<div style=\"text-align:center\">");
            for (int i=0; i<rm.Imgs.Count(); i++)
            {
                sb.Append("<img src = \"https://item-shopping.c.yimg.jp/i/n/primopasso_");
                if (i == 0)
                {
                    sb.Append(rm.ItemCode.ToLower());
                }
                else
                {
                    sb.Append(rm.ItemCode.ToLower() + "_" + i.ToString());
                }
                sb.Append("\"><br><br>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        public static string RemoveAttrs(string str)
        {
            return Regex.Replace(str, @" \w+="".*""", "");
        }

        public static string RemoveAttrsHTMLTag(string str)
        {
            string parsed = "";
            parsed = Regex.Replace(str.Replace(Environment.NewLine, ""), @"\s+", "");
            parsed = Regex.Replace(parsed, @"<[^>]*?>", " ");
            parsed = Regex.Replace(parsed, @"<![^>]*?>", "");
            parsed = Regex.Replace(parsed, @"\s+", " ");
            parsed = Regex.Replace(parsed, @"-->", "");
            // parsed = Regex.Replace(parsed, @"\s{2,}", ", ");
            return parsed;
        }

        public static string[] GetSuffs()
        {
            string[] suffs = { "jpg", "gif", "jpe", "jpeg" };
            return suffs;
        }
    }
}
