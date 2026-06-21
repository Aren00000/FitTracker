using System.Drawing;
using System.Windows.Forms;
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
        private Panel _panelFilter;
        private ChartControl _chartControl;
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
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(900, 650);

            // ========== ПАНЕЛЬ ВВОДА (ВЕРХ) ==========
            _panelInput = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(12),
                AutoSize = false
            };

            // Делаем верстку стабильной: TableLayoutPanel убирает слипание контролов
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 8,
                RowCount = 2,
                Margin = new Padding(0),
                Padding = new Padding(0),
                AutoSize = false
            };

            // Колонки
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));  // label: Дата
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145)); // txtDate
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));      // Упражнение label/txt
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110)); // txtWeight
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110)); // txtReps
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110)); // txtSets
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110)); // btnAdd
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110)); // btnDelete/Theme stack

            // Ряды: 0 - labels, 1 - inputs/buttons
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

            int labelFontSize = 9;

            // Helpers
            Label MakeLabel(string text)
            {
                return new Label
                {
                    Text = text,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoSize = false,
                    Font = new Font("Segoe UI", labelFontSize, FontStyle.Regular),
                    Padding = new Padding(0, 0, 6, 0)
                };
            }

            TextBox MakeTextBox(string placeholder)
            {
                return new TextBox
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(4, 2, 4, 2),
                    PlaceholderText = placeholder,
                    Font = new Font("Segoe UI", 10, FontStyle.Regular)
                };
            }

            // Дата
            var lblDate = MakeLabel("Дата:");
            _txtDate = MakeTextBox("");
            _txtDate.Text = DateTime.Now.ToString("yyyy-MM-dd");

            table.Controls.Add(lblDate, 0, 0);
            table.Controls.Add(_txtDate, 1, 1);

            // Название упражнения
            var lblName = MakeLabel("Упражнение:");
            _txtExerciseName = MakeTextBox("Например: Жим лежа");

            // В 2 колонки: use column 2 width (percent) so label fits
            table.SetColumnSpan(lblName, 1);
            table.Controls.Add(lblName, 2, 0);
            table.Controls.Add(_txtExerciseName, 2, 1);

            // Вес
            var lblWeight = MakeLabel("Вес (кг):");
            _txtWeight = MakeTextBox("0");
            table.Controls.Add(lblWeight, 3, 0);
            table.Controls.Add(_txtWeight, 3, 1);

            // Повторения
            var lblReps = MakeLabel("Повторения:");
            _txtReps = MakeTextBox("0");
            table.Controls.Add(lblReps, 4, 0);
            table.Controls.Add(_txtReps, 4, 1);

            // Подходы
            var lblSets = MakeLabel("Подходы:");
            _txtSets = MakeTextBox("0");
            table.Controls.Add(lblSets, 5, 0);
            table.Controls.Add(_txtSets, 5, 1);

            // Кнопки: добавление/удаление
            _btnAdd = new Button
            {
                Text = _langAdd,
                Dock = DockStyle.Fill,
                Height = 40,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(6, 2, 6, 2)
            };
            _btnAdd.FlatAppearance.BorderSize = 0;
            _btnAdd.Click += BtnAdd_Click;

            _btnDelete = new Button
            {
                Text = _langDelete,
                Dock = DockStyle.Fill,
                Height = 40,
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(6, 2, 6, 2)
            };
            _btnDelete.FlatAppearance.BorderSize = 0;
            _btnDelete.Click += BtnDelete_Click;

            // Theme/Language остаются справа, но вынесем их в верхнюю строку второй группы
            _btnTheme = new Button
            {
                Text = _langTheme,
                Dock = DockStyle.Fill,
                Height = 22,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Margin = new Padding(6, 4, 6, 2)
            };
            _btnTheme.Click += BtnTheme_Click;

            _btnLanguage = new Button
            {
                Text = _langLanguage,
                Dock = DockStyle.Fill,
                Height = 22,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Margin = new Padding(6, 2, 6, 4)
            };
            _btnLanguage.Click += BtnLanguage_Click;

            // Вставляем кнопки Add/Delete в колонки 6 и 7
            table.Controls.Add(_btnAdd, 6, 1);
            table.Controls.Add(_btnDelete, 7, 1);

            // В колонке 7 разместим theme/language в строках 0/1, чтобы не мешали add/delete
            // (labels уже заняты, поэтому делаем вложенный panel)
            var rightTopPanel = new Panel { Dock = DockStyle.Fill };
            rightTopPanel.Controls.Add(_btnTheme);
            rightTopPanel.Controls.Add(_btnLanguage);

            _btnTheme.Dock = DockStyle.Top;
            _btnLanguage.Dock = DockStyle.Bottom;

            table.SetColumnSpan(rightTopPanel, 1);
            table.Controls.Add(rightTopPanel, 7, 0);

            _panelInput.Controls.Add(table);

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

            // Фильтр сверху, чтобы не мешал графику
            _panelFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                Padding = new Padding(10, 8, 10, 6)
            };

            var filterTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0),
                AutoSize = false
            };
            filterTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
            filterTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _cmbExerciseFilter = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                MaxDropDownItems = 10
            };
            _cmbExerciseFilter.SelectedIndexChanged += CmbExerciseFilter_SelectedIndexChanged;

            var lblFilter = new Label
            {
                Text = "Фильтр по упражнению:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            filterTable.Controls.Add(lblFilter, 0, 0);
            filterTable.Controls.Add(_cmbExerciseFilter, 1, 0);

            _panelFilter.Controls.Add(filterTable);

            _lblStats = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(230, 230, 230),
                Padding = new Padding(10, 10, 0, 0)
            };

            _chartControl = new ChartControl
            {
                Dock = DockStyle.Fill
            };

            _panelChart.Controls.Add(_chartControl);
            _panelChart.Controls.Add(_panelFilter);
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
            var analysis = _analysisService.Analyze(_workouts);
            _lblStats.Text = $"📊 Тренировок: {analysis.TotalWorkouts} | " +
                            $"🏋️ Средний вес: {analysis.AverageWeight:F1} кг | " +
                            $"🔥 Макс: {analysis.MaxWeight} кг | " +
                            $"💪 Мин: {analysis.MinWeight} кг | " +
                            $"📈 Объем: {analysis.TotalVolume:N0} кг";

            // Прогресс по всем упражнениям (как сейчас в AnalysisService)
            var data = new List<(string date, int weight)>();
            foreach (var workout in _workouts)
            {
                foreach (var exercise in workout.Exercises)
                {
                    data.Add((workout.Date, exercise.Weight));
                }
            }

            _chartControl.Title = "Прогресс веса";
            _chartControl.Data = data;
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

            _lblStats.BackColor = isDark ? Color.FromArgb(60, 60, 60) : Color.FromArgb(230, 230, 230);
            _lblStats.ForeColor = textColor;

            _chartControl.SetTheme(isDark);

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

            // Дополнительно подсветим элементы фильтра (лежит внутри _panelFilter)
            foreach (Control ctrl in _panelFilter.Controls)
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
            string filter = _cmbExerciseFilter.SelectedItem?.ToString() ?? "";

            var data = new List<(string date, int weight)>();
            foreach (var workout in _workouts)
            {
                foreach (var exercise in workout.Exercises)
                {
                    if (string.IsNullOrEmpty(filter) || filter == "(Все упражнения)" ||
                        exercise.Name.Equals(filter, StringComparison.OrdinalIgnoreCase))
                    {
                        data.Add((workout.Date, exercise.Weight));
                    }
                }
            }

            _chartControl.Title = string.IsNullOrEmpty(filter) || filter == "(Все упражнения)"
                ? "Прогресс веса"
                : $"Прогресс: {filter}";

            _chartControl.Data = data;
        }

        #endregion
    }
}
