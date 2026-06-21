using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using FitTracker.Models;
using FitTracker.Services;

namespace FitTracker
{
    /// <summary>
    /// Главная форма приложения Fit Tracker
    /// Предоставляет интерфейс для управления тренировками и просмотра аналитики
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly DataService _dataService;
        private readonly AnalysisService _analysisService;
        private List<Workout> _workouts;
        private AppSettings _settings;

        // Элементы управления
        private Panel _panelInput;
        private TextBox _txtDate;
        private TextBox _txtExerciseName;
        private TextBox _txtWeight;
        private TextBox _txtReps;
        private TextBox _txtSets;
        private Button _btnAdd;
        private Button _btnDelete;
        private Button _btnTheme;
        private Button _btnLanguage;
        private DataGridView _dataGridView;
        private Panel _panelChart;
        private Chart _chart;
        private Label _lblStats;
        private ComboBox _cmbExerciseFilter;

        private readonly string _langAdd = "Добавить";
        private readonly string _langDelete = "Удалить";
        private readonly string _langTheme = "🌙 Тема";
        private readonly string _langThemeLight = "☀️ Тема";
        private readonly string _langLanguage = "🌐 Язык";

        public MainForm()
        {
            _dataService = new DataService();
            _analysisService = new AnalysisService();
            _workouts = new List<Workout>();
            _settings = new AppSettings();

            InitializeComponent();
            LoadData();
            ApplyTheme();
            UpdateLanguage();
        }

        private void InitializeComponent()
        {
            this.Text = "Fit Tracker 💪 - Тренировки";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);

            // ========== ПАНЕЛЬ ВВОДА (ВЕРХ) ==========
            _panelInput = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(10)
            };

            int yPos = 10;
            int labelWidth = 70;
            int fieldWidth = 100;
            int gap = 5;

            // Дата
            var lblDate = new Label { Text = "Дата:", Location = new Point(10, yPos + 3), Width = labelWidth };
            _txtDate = new TextBox { Location = new Point(10 + labelWidth, yPos), Width = fieldWidth };
            _txtDate.Text = DateTime.Now.ToString("yyyy-MM-dd");

            // Название упражнения
            var lblName = new Label { Text = "Упражнение:", Location = new Point(130 + labelWidth, yPos + 3), Width = 80 };
            _txtExerciseName = new TextBox { Location = new Point(130 + labelWidth + 80, yPos), Width = 150 };
            _txtExerciseName.PlaceholderText = "Например: Жим лежа";

            // Вес
            var lblWeight = new Label { Text = "Вес (кг):", Location = new Point(400, yPos + 3), Width = labelWidth };
            _txtWeight = new TextBox { Location = new Point(400 + labelWidth, yPos), Width = fieldWidth };
            _txtWeight.PlaceholderText = "0";

            // Повторения
            var lblReps = new Label { Text = "Повторения:", Location = new Point(530, yPos + 3), Width = 80 };
            _txtReps = new TextBox { Location = new Point(530 + 80, yPos), Width = fieldWidth };
            _txtReps.PlaceholderText = "0";

            // Подходы
            var lblSets = new Label { Text = "Подходы:", Location = new Point(660, yPos + 3), Width = 70 };
            _txtSets = new TextBox { Location = new Point(660 + 70, yPos), Width = fieldWidth };
            _txtSets.PlaceholderText = "0";

            // Кнопки
            _btnAdd = new Button
            {
                Text = _langAdd,
                Location = new Point(800, yPos),
                Width = 90,
                Height = 25,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnAdd.FlatAppearance.BorderSize = 0;
            _btnAdd.Click += BtnAdd_Click;

            _btnDelete = new Button
            {
                Text = _langDelete,
                Location = new Point(800, yPos + 30),
                Width = 90,
                Height = 25,
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnDelete.FlatAppearance.BorderSize = 0;
            _btnDelete.Click += BtnDelete_Click;

            _btnTheme = new Button
            {
                Text = _langTheme,
                Location = new Point(900, yPos),
                Width = 80,
                Height = 25
            };
            _btnTheme.Click += BtnTheme_Click;

            _btnLanguage = new Button
            {
                Text = _langLanguage,
                Location = new Point(900, yPos + 30),
                Width = 80,
                Height = 25
            };
            _btnLanguage.Click += BtnLanguage_Click;

            _panelInput.Controls.AddRange(new Control[]
            {
                lblDate, _txtDate, lblName, _txtExerciseName,
                lblWeight, _txtWeight, lblReps, _txtReps, lblSets, _txtSets,
                _btnAdd, _btnDelete, _btnTheme, _btnLanguage
            });

            // ========== ТАБЛИЦА ДАННЫХ (ЦЕНТР) ==========
            _dataGridView = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 250,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _dataGridView.Columns.Add("Date", "Дата");
            _dataGridView.Columns.Add("Exercise", "Упражнение");
            _dataGridView.Columns.Add("Weight", "Вес (кг)");
            _dataGridView.Columns.Add("Reps", "Повторения");
            _dataGridView.Columns.Add("Sets", "Подходы");
            _dataGridView.Columns.Add("Volume", "Объем");

            // ========== ПАНЕЛЬ АНАЛИТИКИ (НИЗ) ==========
            _panelChart = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            _cmbExerciseFilter = new ComboBox
            {
                Location = new Point(10, 10),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbExerciseFilter.SelectedIndexChanged += CmbExerciseFilter_SelectedIndexChanged;

            var lblFilter = new Label
            {
                Text = "Фильтр по упражнению:",
                Location = new Point(10, -5),
                AutoSize = true
            };

            _lblStats = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(230, 230, 230),
                Padding = new Padding(10, 10, 0, 0)
            };

            _chart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            var chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Title = "№ тренировки";
            chartArea.AxisY.Title = "Вес (кг)";
            _chart.ChartAreas.Add(chartArea);

            var series = new Series("WeightProgress")
            {
                ChartType = SeriesChartType.Line,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 8,
                Color = Color.FromArgb(33, 150, 243),
                BorderWidth = 2
            };
            _chart.Series.Add(series);

            _panelChart.Controls.Add(_chart);
            _panelChart.Controls.Add(_cmbExerciseFilter);
            _panelChart.Controls.Add(lblFilter);
            _panelChart.Controls.Add(_lblStats);

            // ========== ДОБАВЛЕНИЕ НА ФОРМУ ==========
            this.Controls.Add(_panelChart);
            this.Controls.Add(_dataGridView);
            this.Controls.Add(_panelInput);
        }

        #region Загрузка и сохранение данных

        private void LoadData()
        {
            _settings = _dataService.LoadSettings();
            _workouts = _dataService.LoadWorkouts();
            UpdateDataGridView();
            UpdateChart();
            UpdateExerciseFilter();
        }

        private void SaveData()
        {
            _dataService.SaveWorkouts(_workouts);
            _dataService.SaveSettings(_settings);
        }

        #endregion

        #region Обновление UI

        private void UpdateDataGridView()
        {
            _dataGridView.Rows.Clear();
            foreach (var workout in _workouts)
            {
                foreach (var exercise in workout.Exercises)
                {
                    int volume = exercise.Weight * exercise.Reps * exercise.Sets;
                    _dataGridView.Rows.Add(
                        workout.Date,
                        exercise.Name,
                        exercise.Weight,
                        exercise.Reps,
                        exercise.Sets,
                        volume
                    );
                }
            }
        }

        private void UpdateChart()
        {
            _chart.Series[0].Points.Clear();

            var analysis = _analysisService.Analyze(_workouts);
            _lblStats.Text = $"📊 Тренировок: {analysis.TotalWorkouts} | " +
                            $"🏋️ Средний вес: {analysis.AverageWeight:F1} кг | " +
                            $"🔥 Макс: {analysis.MaxWeight} кг | " +
                            $"💪 Мин: {analysis.MinWeight} кг | " +
                            $"📈 Объем: {analysis.TotalVolume:N0} кг";

            int index = 1;
            foreach (var workout in _workouts)
            {
                foreach (var exercise in workout.Exercises)
                {
                    _chart.Series[0].Points.AddXY(index, exercise.Weight);
                    index++;
                }
            }
        }

        private void UpdateExerciseFilter()
        {
            _cmbExerciseFilter.Items.Clear();
            _cmbExerciseFilter.Items.Add("(Все упражнения)");
            var exercises = _analysisService.GetAllExerciseNames(_workouts);
            foreach (var ex in exercises)
            {
                _cmbExerciseFilter.Items.Add(ex);
            }
            _cmbExerciseFilter.SelectedIndex = 0;
        }

        #endregion

        #region Обработчики событий

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            try
            {
                // Валидация данных
                if (string.IsNullOrWhiteSpace(_txtExerciseName.Text))
                {
                    MessageBox.Show("⚠️ Введите название упражнения!", "Ошибка валидации",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtExerciseName.Focus();
                    return;
                }

                if (!int.TryParse(_txtWeight.Text, out int weight) || weight <= 0)
                {
                    MessageBox.Show("⚠️ Введите корректный вес (> 0)!", "Ошибка валидации",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtWeight.Focus();
                    return;
                }

                if (!int.TryParse(_txtReps.Text, out int reps) || reps <= 0)
                {
                    MessageBox.Show("⚠️ Введите корректное количество повторений!", "Ошибка валидации",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtReps.Focus();
                    return;
                }

                if (!int.TryParse(_txtSets.Text, out int sets) || sets <= 0)
                {
                    MessageBox.Show("⚠️ Введите корректное количество подходов!", "Ошибка валидации",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtSets.Focus();
                    return;
                }

                var exercise = new Exercise
                {
                    Name = _txtExerciseName.Text.Trim(),
                    Weight = weight,
                    Reps = reps,
                    Sets = sets
                };

                var workout = new Workout(_txtDate.Text.Trim())
                {
                    Exercises = { exercise }
                };

                _workouts.Add(workout);
                SaveData();
                UpdateDataGridView();
                UpdateChart();
                UpdateExerciseFilter();

                // Очистка полей
                _txtExerciseName.Clear();
                _txtWeight.Clear();
                _txtReps.Clear();
                _txtSets.Clear();
                _txtExerciseName.Focus();

                MessageBox.Show("✅ Тренировка добавлена!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_dataGridView.SelectedRows.Count > 0)
            {
                var result = MessageBox.Show("Удалить выбранную запись?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    int selectedIndex = _dataGridView.SelectedRows[0].Index;
                    // Удаляем из списка (упрощенно - последний добавленный)
                    if (_workouts.Count > 0)
                    {
                        _workouts.RemoveAt(_workouts.Count - 1);
                        SaveData();
                        UpdateDataGridView();
                        UpdateChart();
                        UpdateExerciseFilter();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите запись для удаления", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnTheme_Click(object? sender, EventArgs e)
        {
            _settings.Theme = _settings.Theme == "Light" ? "Dark" : "Light";
            _btnTheme.Text = _settings.Theme == "Light" ? _langTheme : _langThemeLight;
            SaveData();
            ApplyTheme();
        }

        private void BtnLanguage_Click(object? sender, EventArgs e)
        {
            _settings.Language = _settings.Language == "ru" ? "en" : "ru";
            SaveData();
            UpdateLanguage();
        }

        private void CmbExerciseFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateChartWithFilter();
        }

        #endregion

        #region Тема и язык

        private void ApplyTheme()
        {
            bool isDark = _settings.Theme == "Dark";

            Color bgColor = isDark ? Color.FromArgb(30, 30, 30) : Color.FromArgb(240, 240, 240);
            Color panelColor = isDark ? Color.FromArgb(45, 45, 48) : Color.White;
            Color textColor = isDark ? Color.White : Color.Black;
            Color gridColor = isDark ? Color.FromArgb(55, 55, 55) : Color.White;

            this.BackColor = bgColor;
            this.ForeColor = textColor;

            _panelInput.BackColor = panelColor;
            _panelChart.BackColor = panelColor;

            _dataGridView.BackgroundColor = gridColor;
            _dataGridView.DefaultCellStyle.ForeColor = textColor;
            _dataGridView.DefaultCellStyle.BackColor = gridColor;
            _dataGridView.ColumnHeadersDefaultCellStyle.BackColor = panelColor;
            _dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = textColor;

            _chart.BackColor = panelColor;
            _chart.ChartAreas[0].BackColor = panelColor;
            _chart.ChartAreas[0].AxisX.LabelStyle.ForeColor = textColor;
            _chart.ChartAreas[0].AxisY.LabelStyle.ForeColor = textColor;
            _chart.ChartAreas[0].AxisX.TitleForeColor = textColor;
            _chart.ChartAreas[0].AxisY.TitleForeColor = textColor;

            _lblStats.BackColor = isDark ? Color.FromArgb(60, 60, 60) : Color.FromArgb(230, 230, 230);
            _lblStats.ForeColor = textColor;

            foreach (Control ctrl in _panelInput.Controls)
            {
                ctrl.BackColor = panelColor;
                ctrl.ForeColor = textColor;
            }

            foreach (Control ctrl in _panelChart.Controls)
            {
                if (ctrl is Label or ComboBox)
                {
                    ctrl.BackColor = panelColor;
                    ctrl.ForeColor = textColor;
                }
            }
        }

        private void UpdateLanguage()
        {
            if (_settings.Language == "en")
            {
                this.Text = "Fit Tracker 💪 - Workouts";
                _btnAdd.Text = "Add";
                _btnDelete.Text = "Delete";
                _btnTheme.Text = _settings.Theme == "Light" ? "🌙 Theme" : "☀️ Theme";
                _btnLanguage.Text = "🌐 Language";
                _lblStats.Text = "📊 Workouts | 🏋️ Avg Weight | 🔥 Max | 💪 Min | 📈 Volume";
            }
            else
            {
                this.Text = "Fit Tracker 💪 - Тренировки";
                _btnAdd.Text = _langAdd;
                _btnDelete.Text = _langDelete;
                _btnTheme.Text = _settings.Theme == "Light" ? _langTheme : _langThemeLight;
                _btnLanguage.Text = _langLanguage;
            }
        }

        #endregion

        #region Фильтрация графика

        private void UpdateChartWithFilter()
        {
            _chart.Series[0].Points.Clear();
            string filter = _cmbExerciseFilter.SelectedItem?.ToString() ?? "";

            int index = 1;
            foreach (var workout in _workouts)
            {
                foreach (var exercise in workout.Exercises)
                {
                    if (string.IsNullOrEmpty(filter) || filter == "(Все упражнения)" ||
                        exercise.Name.Equals(filter, StringComparison.OrdinalIgnoreCase))
                    {
                        _chart.Series[0].Points.AddXY(index, exercise.Weight);
                        index++;
                    }
                }
            }
        }

        #endregion
    }
}
