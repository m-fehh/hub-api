namespace Hub.Shared.Models.VMs
{
    public class LoginVM
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponseVM
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public long? ProfileId { get; set; }
        public bool Administrator { get; set; }
        public bool Inactive { get; set; }
        public long? DefaultOrgStructureId { get; set; }
        public bool AllowMultipleAccess { get; set; }

    }
}
