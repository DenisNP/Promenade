namespace Promenade.Models
{
    public class CategoryForUser : CategoryMeta
    {
        public bool Enabled { get; set; }

        private CategoryForUser() { }

        public CategoryForUser(CategoryMeta meta, bool enabled)
        {
            Id = meta.Id;
            Name = meta.Name;
            Icon = meta.Icon;
            Enabled = enabled;
        }
    }
}