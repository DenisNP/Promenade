namespace Promenade.Models.Web.Request
{
    public class SettingsRequest : BaseRequest
    {
        public CategorySettingParameter[] Categories { get; set; }
    }

    public class CategorySettingParameter
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }
    }
}