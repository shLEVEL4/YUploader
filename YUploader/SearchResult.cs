using System.Collections.Generic;

namespace YUploader
{
    public class SearchResult
    {
        public string Msg { get; set; }
        public string Error { get; set; }
        public List<ResultModel> Results { get; set; }
    }
}
