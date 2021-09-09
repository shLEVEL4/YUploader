using System.Collections.Generic;

namespace YUploader
{
    public class ResultModelLogic
    {
        public static List<ResultModel> GetTestData()
        {
            string PATH = @"【スポーツ用品】";

            List<ResultModel> res = new List<ResultModel>() { };
            ResultModel one = new ResultModel();
            one.Imgs = new List<string>();
            //one.Imgs.Add("https://i.ebayimg.com/images/g/WigAAOSwd0dfjTm-/s-l64.png");
            //one.Imgs.Add("https://i.ebayimg.com/images/g/DoUAAOSw789fjTmy/s-l64.png");
            one.CurrentImgIndex = 0;
            one.ItemCode = "1456";
            one.EditItemCode = "1456";
            one.Name = "NEW Apple iPhone X 64GB | 256GB (UNLOCKED) Gray ║ Silver ❖SEALED❖";
            one.Price = "47556";
            one.Path = PATH;
            res.Add(one);

            ResultModel one2 = new ResultModel();
            one2.Imgs = new List<string>();
            one2.Imgs.Add("https://i.ebayimg.com/images/g/VFoAAOSwrJJgJKlC/s-l500.jpg");
            one2.Imgs.Add("https://i.ebayimg.com/images/g/qp0AAOSwBkBgJKlH/s-l64.jpg");
            one2.CurrentImgIndex = 0;
            one2.ItemCode = "27398";
            one2.EditItemCode = "27398";
            one2.Name = "NEW Apple iPhone X 64GB | 256GB (UNLOCKED) Gray ║ Silver ❖SEALED❖";
            one2.Price = "47556";
            one2.Path = PATH;
            res.Add(one2);
            ResultModel one3 = new ResultModel();
            one3.Imgs = new List<string>();
            one3.Imgs.Add("https://i.ebayimg.com/images/g/VFoAAOSwrJJgJKlC/s-l500.jpg");
            one3.Imgs.Add("https://i.ebayimg.com/images/g/qp0AAOSwBkBgJKlH/s-l64.jpg");
            one3.CurrentImgIndex = 0;
            one3.ItemCode = "37989";
            one3.EditItemCode = "37989";
            one3.Name = "NEW Apple iPhone X 64GB | 256GB (UNLOCKED) Gray ║ Silver ❖SEALED❖";
            one3.Price = "47556";
            one3.Path = PATH;
            res.Add(one3);
            return res;
        }

        public static ResultModel NewModel(YUModel yu)
        {
            ResultModel rm = new ResultModel();
            rm.Category = yu.AllCategory;
            rm.CurrentImgIndex = 0;
            return rm;
        }
    }
}
