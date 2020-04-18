using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LinqToTwitter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PioPio.Models;

namespace PioPio.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly TwitterContext _twitterContext;

        public IEnumerable<Tweet> Tweets { get; set; } = new List<Tweet>();

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _twitterContext = new TwitterContext(CreateMvcAuthorizer());
        }

        private MvcAuthorizer CreateMvcAuthorizer()
        {
            return new MvcAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore()
                {
                    ConsumerKey = _configuration["Twitter_ConsumerKey"],
                    ConsumerSecret = _configuration["Twitter_ConsumerSecret"],
                    OAuthToken = _configuration["Twitter_AccessToken"],
                    OAuthTokenSecret = _configuration["Twitter_AccessTokenSecret"]
                }
            };
        }

        public async Task<IActionResult> OnGet()
        {
            //if (!new SessionStateCredentialStore(HttpContext.Session).HasAllCredentials())
            //    return RedirectToPage("OAuth");

            Tweets = await GetHomeTimelineTweets();

            return Page();
        }

        private async Task<IEnumerable<Tweet>> GetHomeTimelineTweets()
        {
            const int MaxTotalResults = 800;

            // sinceID is the oldest id you already have for this search term
            // CurrentMaxId is used after the first query to track current session
            ulong? currentMaxID = null, previousMaxID = ulong.MaxValue;
            var partialStatuses = new List<Status>();
            var combinedStatuses = new List<Status>();
            const int MinFavoriteCount = 20;

            do
            {
                try
                {
                    partialStatuses = await _twitterContext.Status
                    .Where(FilterStatuses(currentMaxID))
                    .ToListAsync();

                    // one less than the newest id you've just queried
                    currentMaxID = partialStatuses.Min(status => status.StatusID) - 1;

                    Debug.Assert(currentMaxID < previousMaxID);

                    previousMaxID = currentMaxID;

                    combinedStatuses.AddRange(partialStatuses.Where(s => s.FavoriteCount > MinFavoriteCount));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            } while (partialStatuses.Any() && (combinedStatuses.Count() < MaxTotalResults));

            return MapStatusesToTweets(combinedStatuses);
        }

        private static IEnumerable<Tweet> MapStatusesToTweets(List<Status> combinedTweets)
        {
            return combinedTweets
                        .OrderByDescending(s => s.FavoriteCount)
                        .Select(s =>
                            new Tweet
                            {
                                FavoriteCount = s.FavoriteCount,
                                Text = s.Text,
                                UserScreenName = s.User.ScreenNameResponse,
                                Url = $"https://twitter.com/{s.User.ScreenNameResponse}/status/{s.StatusID}"
                            });
        }

        private static Expression<Func<Status, bool>> FilterStatuses(ulong? maxID)
        {
            const ulong SinceID = 1;
            const int MaxTweetsToReturn = 200;

            var status = Expression.Parameter(typeof(Status), "status");
            var expression = @$"status.Type == @0 &&
                        status.Count == @1 &&
                        status.ExcludeReplies == false &&
                        status.TweetMode == @2 &&
                        status.SinceID == @3
";

            if (maxID.HasValue)
            {
                expression += " && status.MaxID == @4";
            }

            return (Expression<Func<Status, bool>>)DynamicExpressionParser.ParseLambda(
                new[] { status }, typeof(bool), expression, StatusType.Home, MaxTweetsToReturn, TweetMode.Compat, SinceID, maxID
                );
        }
    }
}