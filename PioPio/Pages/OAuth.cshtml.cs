﻿using System;
using System.Threading.Tasks;
using LinqToTwitter;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace PioPio.Pages
{
    public class OAuthModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public OAuthModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public async Task<ActionResult> Begin()
        {
            //var auth = new MvcSignInAuthorizer
            var auth = new MvcAuthorizer
            {
                CredentialStore = new SessionStateCredentialStore(HttpContext.Session)
                {
                    ConsumerKey = _configuration["Twitter:ConsumerKey"],
                    ConsumerSecret = _configuration["Twitter:ConsumerSecret"]
                }
            };

            // to pass parameters that you can read in Complete(), via Request.QueryString, when Twitter returns
            //var parameters = new Dictionary<string, string> { { "my_custom_param", "val" } };
            //string twitterCallbackUrl = Request.GetDisplayUrl().Replace("Begin", "Complete");
            //return await auth.BeginAuthorizationAsync(new Uri(twitterCallbackUrl), parameters);

            string twitterCallbackUrl = Request.GetDisplayUrl().Replace("Begin", "Complete");
            return await auth.BeginAuthorizationAsync(new Uri(twitterCallbackUrl));
        }

        public async Task<ActionResult> Complete()
        {
            var auth = new MvcAuthorizer
            {
                CredentialStore = new SessionStateCredentialStore(HttpContext.Session)
            };

            await auth.CompleteAuthorizeAsync(new Uri(Request.GetDisplayUrl()));

            // This is how you access credentials after authorization.
            // The oauthToken and oauthTokenSecret do not expire.
            // You can use the userID to associate the credentials with the user.
            // You can save credentials any way you want - database,
            //   isolated storage, etc. - it's up to you.
            // You can retrieve and load all 4 credentials on subsequent
            //   queries to avoid the need to re-authorize.
            // When you've loaded all 4 credentials, LINQ to Twitter will let
            //   you make queries without re-authorizing.
            //
            //var credentials = auth.CredentialStore;
            //string oauthToken = credentials.OAuthToken;
            //string oauthTokenSecret = credentials.OAuthTokenSecret;
            //string screenName = credentials.ScreenName;
            //ulong userID = credentials.UserID;
            //

            return RedirectToPage("Index");
        }
    }
}