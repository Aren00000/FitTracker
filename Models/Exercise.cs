namespace FitTracker.Models
{
    /// <summary>
    /// Модель упражнения с параметрами веса и повторений
    /// </summary>
    public class Exercise
    {
        public string Name { get; set; } = string.Empty;
        public int Weight { get; set; }
        public int Reps { get; set; }
        public int Sets { get; set; }

        public Exercise() { }

        public Exercise(string name, int weight, int reps, int sets)
        {
            Name = name;
            Weight = weight;
            Reps = reps;
            Sets = sets;
        }
    }
}
