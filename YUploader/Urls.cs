using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YUploader
{
    static class Urls
    {
        // public const string GET_AUTH_ID = @"https://auth.login.yahoo.co.jp/yconnect/v2/authorization?client_id={0}&response_type=token&redirect_uri=https%3A%2F%2Fwww.yahoo.co.jp%2F&scope=openid";
        public const string GET_CATEGORY = @"https://shopping.yahooapis.jp/ShoppingWebService/V1/categorySearch?appid={0}&category_id=1";
        public const string REGIST_NEW_PRODUCT = @"https://circus.shopping.yahooapis.jp/ShoppingWebService/V1/editItem";
        public const string UPLOAD_ALL_IMGS = @"https://circus.shopping.yahooapis.jp/ShoppingWebService/V1/uploadItemImagePack?seller_id={0}";
        public const string PUBLISH_ALL = @"https://circus.shopping.yahooapis.jp/ShoppingWebService/V1/reservePublish";
        public const string V1_GETTOKEN = @"https://auth.login.yahoo.co.jp/yconnect/v1/token";
        public const string V1_AUTH_END = @"https://auth.login.yahoo.co.jp/yconnect/v1/authorization?client_id={0}&response_type=code&redirect_uri=https%3A%2F%2Fwww.yahoo.co.jp%2F&scope=openid";
        public const string V1_GETALLIMGS = @"https://circus.shopping.yahooapis.jp/ShoppingWebService/V1/itemImageList?seller_id={0}&query={1}";
        public const string V1_SETSTOCK = @"https://circus.shopping.yahooapis.jp/ShoppingWebService/V1/setStock";
    }
}
