namespace PioPio.Models
{
    public class Tweet
    {
        public string Text { get; set; }
        public int? FavoriteCount { get; set; }
        public string UserScreenName { get; set; }
        public string Url { get; set; }
    }
}