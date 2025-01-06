namespace ShopBanDoGiaDung.Common
{
    public class LoginInfo
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool KeepLoggedIn = false;

        public string? previousPage {get;set;}
    }
}
