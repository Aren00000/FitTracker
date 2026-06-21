using FitTracker.Models;

namespace FitTracker.Services
{
    /// <summary>
    /// Сервис для анализа данных тренировок
    /// Вычисляет средние значения, минимум, максимум и другие метрики
    /// </summary>
    public class AnalysisService
    {
        /// <summary>
        /// Результат анализа тренировок
        /// </summary>
        public class AnalysisResult
        {
            public double AverageWeight { get; set; }
            public double MaxWeight { get; set; }
            public double MinWeight { get; set; }
            public int TotalVolume { get; set; }
            public int TotalWorkouts { get; set; }
            public int TotalExercises { get; set; }
        }

        /// <summary>
        /// Полный анализ всех тренировок
        /// </summary>
        public AnalysisResult Analyze(List<Workout> workouts)
        {
            var result = new AnalysisResult();

            if (workouts == null || workouts.Count == 0)
            {
                return result;
            }

            result.TotalWorkouts = workouts.Count;

            var allExercises = workouts.SelectMany(w => w.Exercises).ToList();
            result.TotalExercises = allExercises.Count;

            if (allExercises.Count == 0)
            {
                return result;
            }

            // Вычисление среднего веса
            result.AverageWeight = allExercises.Average(e => e.Weight);

            // Максимальный вес (личный рекорд)
            result.MaxWeight = allExercises.Max(e => e.Weight);

            // Минимальный вес
            result.MinWeight = allExercises.Min(e => e.Weight);

            // Общий объем (вес * повторения * подходы)
            result.TotalVolume = allExercises.Sum(e => e.Weight * e.Reps * e.Sets);

            return result;
        }

        /// <summary>
        /// Анализ прогресса по конкретному упражнению
        /// </summary>
        public List<(string date, int weight)> GetExerciseProgress(List<Workout> workouts, string exerciseName)
        {
            var progress = new List<(string date, int weight)>();

            foreach (var workout in workouts.OrderBy(w => w.Date))
            {
                foreach (var exercise in workout.Exercises)
                {
                    if (exercise.Name.Equals(exerciseName, StringComparison.OrdinalIgnoreCase))
                    {
                        progress.Add((workout.Date, exercise.Weight));
                    }
                }
            }

            return progress;
        }

        /// <summary>
        /// Получение списка всех уникальных упражнений
        /// </summary>
        public List<string> GetAllExerciseNames(List<Workout> workouts)
        {
            return workouts
                .SelectMany(w => w.Exercises)
                .Select(e => e.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();
        }
    }
}
