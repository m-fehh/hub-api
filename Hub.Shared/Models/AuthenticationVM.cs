namespace Hub.Shared.Models
{
    public class AuthenticationVM
    {
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public bool RememberMe { get; private set; }

        public AuthenticationVM(string userName, string password, bool rememberMe)
        {
            UserName = userName;
            Password = password;
            RememberMe = rememberMe;
        }
    }
}
