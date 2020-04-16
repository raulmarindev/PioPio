using LinqToTwitter;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace PioPio.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly TwitterContext _twitterContext;

        public IndexModel(ILogger<IndexModel> logger, TwitterContext twitterContext)
        {
            _logger = logger;
            _twitterContext = twitterContext;
        }

        public void OnGet()
        {
        }
    }
}