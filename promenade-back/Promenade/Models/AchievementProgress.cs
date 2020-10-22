namespace Promenade.Models
{
    public class AchievementProgress : Achievement
    {
        public int Done { get; set; }
        
        public AchievementProgress(Achievement a)
        {
            Name = a.Name;
            CategoryId = a.CategoryId;
            Icon = a.Icon;
            Count = a.Count;
            Done = 0;
        }
        
        public AchievementProgress()
        {
            
        }
    }
}