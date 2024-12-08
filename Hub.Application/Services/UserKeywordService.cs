using Hub.Domain.Entity;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Infrastructure.Extensions;
using System.Text.RegularExpressions;

namespace Hub.Application.Services
{
    public class UserKeywordService
    {
        private readonly IRepository<PortalUser> _repository;

        public UserKeywordService(IRepository<PortalUser> repository)
        {
            _repository = repository;
        }

        public bool IsKeywordValid(string keyword)
        {
            return Regex.IsMatch(keyword, "^([.]|-|[a-zA-Z]|[0-9]){1,}$");
        }

        public bool IsKeywordInUse(long id, string keyword)
        {
            return _repository.Table.Any(x => x.Keyword == keyword && x.Id != id);
        }

        public string GenerateKeyword(string name)
        {
            var firstName = name.Split(' ')[0].RemoveAccents();
            var lastName = name.Split(' ').Count() == 1 ? null : name.Split(' ').Last().RemoveAccents();
            var defaultKeyword = string.Format("{0}{1}", firstName, lastName);

            if (!_repository.Table.Any(x => x.Keyword == defaultKeyword))
            {
                return defaultKeyword;
            }

            var allA = _repository.Table.Select(x => new { x.Id, x.Name, x.Keyword }).ToList();
            var allB = allA.Select(x => new
            {
                x.Id,
                FirstName = x.Name.Split(' ')[0].RemoveAccents(),
                LastName = x.Name.Split(' ').Count() == 1 ? null : x.Name.Split(' ').Last().RemoveAccents(),
                x.Keyword
            });

            var ordinal = allB.Count(x => x.FirstName == firstName && x.LastName == lastName);
            do
            {
                ordinal++;
                defaultKeyword = string.Format("{0}{1}{2}", firstName, lastName, ordinal);
            }
            while (allB.Any(x => x.Keyword == defaultKeyword));

            return defaultKeyword;
        }
    }
}
