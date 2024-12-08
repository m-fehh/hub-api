namespace Hub.Shared.Interfaces
{
    public interface IUser : IBaseEntity
    {
        string Login { get; set; }

        string Name { get; set; }
        public string Email { get; set; }
        public string IpAddress { get; set; }
        IProfileGroup Profile { get; set; }
        public long? DefaultOrgStructureId { get; set; }
        public List<long> OrgStructures { get; set; }
    }
}
