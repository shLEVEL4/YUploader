namespace YUploader
{
    public class YUModel
    {
        public string AppID { get; set; }
        public string Store { get; set; }
        public string Url { get; set; }
        public bool IsSearchPage { get; set; }
        public string Seller { get; set; }
        public string Token { get; set; }
        public string AllCategory { get; set; }
        public string AllPath { get; set; }
        public string Interval { get; set; }
    }

    public class TokenNew
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
        public string id_token { get; set; }
    }
    public class TokenRefresh
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
    }
}
