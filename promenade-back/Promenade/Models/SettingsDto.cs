namespace Promenade.Models
{
    public class SettingsDto
    {
        public CategorySetting[] Categories { get; set; }
    }

    public class CategorySetting
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }
        
        public CategorySetting(in int id, in bool enabled)
        {
            Id = id;
            Enabled = enabled;
        }
    }
}