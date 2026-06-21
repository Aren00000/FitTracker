namespace FitTracker.Models
{
    /// <summary>
    /// Модель настроек приложения (сохраняется в JSON)
    /// </summary>
    public class AppSettings
    {
        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "ru";
        public string Unit { get; set; } = "kg";

        public AppSettings() { }
    }
}
