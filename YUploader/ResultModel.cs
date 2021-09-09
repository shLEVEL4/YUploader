using System.Collections.Generic;

namespace YUploader
{
    public class ResultModel
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public string SellerId { get; set; }
        public string ItemCode { get; set; }
        public string EditItemCode { get; set; }
        public string Path { get; set; }
        public string Category { get; set; }
        public List<string> Imgs { get; set; }
        public int CurrentImgIndex { get; set; }
        public string Additional2 { get; set; }
        public string Options { get; set; }
    }
}
