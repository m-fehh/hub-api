using Hub.Domain;

namespace Hub.Infrastructure.Seeders.Management
{
    public class UserSeeder : ISeeder
    {
        private readonly DatabaseContext _context;

        public UserSeeder(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<bool> SeedAsync()
        {
            try
            {
                //if (!_context.Users.Any())
                //{
                //    _context.Users.Add(new User { Name = "Admin", Email = "admin@example.com", Role = "Administrator" });
                //    await _context.SaveChangesAsync();
                //}
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
