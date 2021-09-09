using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace YUploader
{
    public partial class MainWindow
    {
        private FileStream fs;
        private void UpdateTextBlock(TextBlock block, String msg, Color color)
        {
            block.Text = msg;
            block.Foreground = new SolidColorBrush(color);
        }

        private void SaveAllSetting(YUModel item)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(YUModel));
            fs = new FileStream(Setting.SETTING_JSON, FileMode.Create);
            try
            {
                serializer.WriteObject(fs, item);
            }
            finally
            {
                fs.Close();
            }
        }

        private YUModel GetAllSetting()
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(YUModel));
            fs = new FileStream(Setting.SETTING_JSON, FileMode.OpenOrCreate);
            try
            {
                return (YUModel)serializer.ReadObject(fs);
            }
            catch
            {
                return InitSetting();
            }
            finally
            {
                fs.Close();
            }
        }

        private YUModel InitSetting()
        {
            YUModel newOne = YUModelLogic.InitModel();
            return newOne;
        }

        private void DisableButton(Button button)
        {
            button.IsEnabled = false;
        }

        private void EnableButton(Button button)
        {
            if(button == this.Search)
            {
                if (this.Yid.Text != "")
                {
                    button.IsEnabled = true;
                }
            }
            else
            {
                button.IsEnabled = true;
            }
        }
    }
}
