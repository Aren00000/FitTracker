using System.Text;
using System.Text.Json;
using FitTracker.Models;

namespace FitTracker.Services
{
    /// <summary>
    /// Сервис для работы с файлами данных (JSON и CSV)
    /// Реализует шифрование данных для CSV файлов
    /// </summary>
    public class DataService
    {
        private readonly string _jsonPath;
        private readonly string _csvPath;
        private const int XorKey = 123;

        public DataService()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _jsonPath = Path.Combine(baseDir, "settings.json");
            _csvPath = Path.Combine(baseDir, "trainings.csv.enc");
        }

        #region JSON Settings

        /// <summary>
        /// Загрузка настроек из JSON файла
        /// </summary>
        public AppSettings LoadSettings()
        {
            if (!File.Exists(_jsonPath))
            {
                var defaultSettings = new AppSettings();
                SaveSettings(defaultSettings);
                return defaultSettings;
            }

            string json = File.ReadAllText(_jsonPath, Encoding.UTF8);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }

        /// <summary>
        /// Сохранение настроек в JSON файл
        /// </summary>
        public void SaveSettings(AppSettings settings)
        {
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            File.WriteAllText(_jsonPath, json, Encoding.UTF8);
        }

        #endregion

        #region CSV Workouts (with encryption)

        /// <summary>
        /// Простое XOR шифрование для защиты данных
        /// </summary>
        private string Encrypt(string text)
        {
            var result = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                result.Append((char)(c ^ XorKey));
            }
            return result.ToString();
        }

        /// <summary>
        /// Расшифровка данных
        /// </summary>
        private string Decrypt(string text)
        {
            var result = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                result.Append((char)(c ^ XorKey));
            }
            return result.ToString();
        }

        /// <summary>
        /// Сохранение тренировок в зашифрованный CSV файл
        /// Формат: DATE|дата / EX|название|вес|повт|подх / END
        /// </summary>
        public void SaveWorkouts(List<Workout> workouts)
        {
            var sb = new StringBuilder();
            foreach (var workout in workouts)
            {
                sb.AppendLine($"DATE|{workout.Date}");
                foreach (var exercise in workout.Exercises)
                {
                    sb.AppendLine($"EX|{exercise.Name}|{exercise.Weight}|{exercise.Reps}|{exercise.Sets}");
                }
                sb.AppendLine("END");
            }

            string encrypted = Encrypt(sb.ToString());
            File.WriteAllText(_csvPath, encrypted, Encoding.UTF8);
        }

        /// <summary>
        /// Загрузка и парсинг тренировок из зашифрованного CSV файла
        /// </summary>
        public List<Workout> LoadWorkouts()
        {
            var workouts = new List<Workout>();

            if (!File.Exists(_csvPath))
            {
                return workouts;
            }

            string encrypted = File.ReadAllText(_csvPath, Encoding.UTF8);
            string content = Decrypt(encrypted);
            string[] lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Workout? currentWorkout = null;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("DATE|"))
                {
                    string date = trimmed.Substring(5);
                    currentWorkout = new Workout(date);
                }
                else if (trimmed.StartsWith("EX|") && currentWorkout != null)
                {
                    string[] parts = trimmed.Split('|');
                    if (parts.Length >= 5)
                    {
                        var exercise = new Exercise
                        {
                            Name = parts[1],
                            Weight = ParseInt(parts[2]),
                            Reps = ParseInt(parts[3]),
                            Sets = ParseInt(parts[4])
                        };
                        currentWorkout.Exercises.Add(exercise);
                    }
                }
                else if (trimmed == "END" && currentWorkout != null)
                {
                    workouts.Add(currentWorkout);
                    currentWorkout = null;
                }
            }

            return workouts;
        }

        private int ParseInt(string value)
        {
            return int.TryParse(value, out int result) ? result : 0;
        }

        #endregion
    }
}
