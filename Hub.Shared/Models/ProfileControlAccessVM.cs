namespace Hub.Shared.Models
{
    public class ProfileControlAccessVM
    {
        public ProfileControlAccessVM(long userId, string cookieToken)
        {
            UserId = userId;
            Token = cookieToken;
            Creation = DateTime.Now;
        }

        public long UserId { get; set; }
        public string Token { get; set; }
        public DateTime Creation { get; set; }

        public override string ToString()
        {
            return $"portalUser_{UserId}";
        }
    }
}
