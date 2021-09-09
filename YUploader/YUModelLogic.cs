using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YUploader
{
    class YUModelLogic
    {
        static public YUModel InitModel()
        {
            YUModel newOne = new YUModel();
            newOne.AppID = "";
            newOne.Store = "";
            newOne.Url = "";
            newOne.Seller = "";
            newOne.AllCategory = "";
            return newOne;
        }
    }
}
