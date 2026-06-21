namespace FitTracker.Models
{
    /// <summary>
    /// Модель тренировки содержащая дату и список упражнений
    /// </summary>
    public class Workout
    {
        public string Date { get; set; } = string.Empty;
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();

        public Workout() { }

        public Workout(string date)
        {
            Date = date;
        }
    }
}
