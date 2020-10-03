using Promenade.Models.Abstract;

namespace Promenade.Models
{
    public class User : IIdentity
    {
        public string Id { get; set; }
        public CategoryForUser[] Categories { get; set; }
    }
}