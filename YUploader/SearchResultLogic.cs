using System.Threading.Tasks;

namespace YUploader
{
    public class SearchResultLogic
    {
        static public SearchResult GetNotFound()
        {
            SearchResult res = new SearchResult();
            res.Error = ConstStr.NOT_FOUND;
            return res;
        }

        static async public Task<SearchResult> GetSearchResult(YUModel model)
        {
            // return SearchResultLogic.GetNotFound();
            if (model.Store == "ebay")
            {
                EbaySearcher searcher = new EbaySearcher(model);
                return await searcher.GetResult();
            }
            else if (model.Store == "BUYMA")
            {
                BUYMASearcher searcher = new BUYMASearcher(model);
                return await searcher.GetResult();
            }
            else if (model.Store == "ETOREN")
            {
                ETORENSearcher searcher = new ETORENSearcher(model);
                return await searcher.GetResult();
            }
            else
            {
                return SearchResultLogic.GetNotFound();
            }
        }
    }
}
