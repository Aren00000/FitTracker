using System.Drawing;
using System.Windows.Forms;

namespace FitTracker
{
    /// <summary>
    /// Пользовательский элемент управления для рисования графика прогресса на GDI+
    /// Не требует внешних зависимостей
    /// </summary>
    public class ChartControl : Control
    {
        private List<(string date, int weight)> _data = new();
        private Color _lineColor = Color.FromArgb(33, 150, 243);
        private Color _gridColor = Color.FromArgb(200, 200, 200);
        private Color _textColor = Color.Black;
        private Color _bgColor = Color.White;
        private Color _pointColor = Color.FromArgb(33, 150, 243);
        private string _title = "";
        private bool _isDark = false;

        public List<(string date, int weight)> Data
        {
            get => _data;
            set { _data = value ?? new(); Invalidate(); }
        }

        public string Title
        {
            get => _title;
            set { _title = value ?? ""; Invalidate(); }
        }

        public void SetTheme(bool isDark)
        {
            _isDark = isDark;
            _gridColor = isDark ? Color.FromArgb(60, 60, 60) : Color.FromArgb(200, 200, 200);
            _textColor = isDark ? Color.White : Color.Black;
            _bgColor = isDark ? Color.FromArgb(40, 40, 40) : Color.White;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.Clear(_bgColor);

            if (_data.Count == 0)
            {
                e.Graphics.DrawString("Нет данных для отображения",
                    new Font("Segoe UI", 11, FontStyle.Italic),
                    Brushes.Gray, ClientRectangle,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                return;
            }

            // Отступы
            int margin = 60;
            int topMargin = 40;
            int left = margin;
            int right = ClientRectangle.Width - 20;
            int bottom = ClientRectangle.Height - 30;
            int chartHeight = bottom - topMargin;
            int chartWidth = right - left;

            int maxWeight = _data.Max(d => d.weight) + 10;
            int minWeight = Math.Max(0, _data.Min(d => d.weight) - 10);
            int weightRange = maxWeight - minWeight;
            if (weightRange == 0) weightRange = 10;

            // Сетка
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            int gridLines = 5;
            for (int i = 0; i <= gridLines; i++)
            {
                float y = topMargin + (chartHeight * i / gridLines);
                float weightValue = maxWeight - (weightRange * i / gridLines);

                using var pen = new Pen(_gridColor, 1);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                e.Graphics.DrawLine(pen, left, y, right, y);

                string label = weightValue.ToString("F0");
                using var brush = new SolidBrush(_textColor);
                using var font = new Font("Segoe UI", 9);
                SizeF size = e.Graphics.MeasureString(label, font);
                e.Graphics.DrawString(label, font, brush, left - size.Width - 5, y - size.Height / 2);
            }
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Оси
            using var axisPen = new Pen(_textColor, 2);
            e.Graphics.DrawLine(axisPen, left, topMargin, left, bottom);
            e.Graphics.DrawLine(axisPen, left, bottom, right, bottom);

            // Заголовок
            if (!string.IsNullOrEmpty(_title))
            {
                using var brush = new SolidBrush(_textColor);
                using var font = new Font("Segoe UI", 12, FontStyle.Bold);
                SizeF size = e.Graphics.MeasureString(_title, font);
                e.Graphics.DrawString(_title, font, brush,
                    (ClientRectangle.Width - size.Width) / 2, 5);
            }

            // Линия графика
            int step = Math.Max(1, (_data.Count - 1));
            for (int i = 0; i < _data.Count; i++)
            {
                float x = left + (chartWidth * i / Math.Max(1, step));
                float y = topMargin + (chartHeight * (maxWeight - _data[i].weight) / weightRange);

                if (i > 0)
                {
                    float prevX = left + (chartWidth * (i - 1) / Math.Max(1, step));
                    float prevY = topMargin + (chartHeight * (maxWeight - _data[i - 1].weight) / weightRange);

                    using var pen = new Pen(_lineColor, 2.5f);
                    e.Graphics.DrawLine(pen, prevX, prevY, x, y);
                }

                // Точка
                using var pointBrush = new SolidBrush(_pointColor);
                e.Graphics.FillEllipse(pointBrush, x - 4, y - 4, 8, 8);

                // Обводка точки
                using var pointPen = new Pen(_bgColor, 1.5f);
                e.Graphics.DrawEllipse(pointPen, x - 4, y - 4, 8, 8);

                // Подпись значения
                using var valBrush = new SolidBrush(_textColor);
                using var valFont = new Font("Segoe UI", 8, FontStyle.Bold);
                string valText = _data[i].weight.ToString();
                SizeF valSize = e.Graphics.MeasureString(valText, valFont);
                e.Graphics.DrawString(valText, valFont, valBrush, x - valSize.Width / 2, y - 20);

                // Подпись даты снизу
                using var dateBrush = new SolidBrush(_textColor);
                using var dateFont = new Font("Segoe UI", 7);
                string dateText = _data[i].date.Length > 5 ? _data[i].date.Substring(5) : _data[i].date;
                SizeF dateSize = e.Graphics.MeasureString(dateText, dateFont);
                e.Graphics.DrawString(dateText, dateFont, dateBrush, x - dateSize.Width / 2, bottom + 5);
            }

            // Подписи осей
            using var axisTitleBrush = new SolidBrush(_textColor);
            using var axisTitleFont = new Font("Segoe UI", 9, FontStyle.Bold);

            // Вертикальная подпись оси Y: делаем координаты так, чтобы текст гарантированно попадал внутрь контрола
            // (у RotateTransform область меняется — поэтому заранее учитываем левый отступ).
            e.Graphics.ResetTransform();
            var yAxisText = "Вес (кг)";

            // Сдвиг по X на центр левой области графика, чтобы подпись не уходила за край.
            float yAxisX = left - 45;
            float yAxisY = topMargin + chartHeight / 2f;

            e.Graphics.TranslateTransform(yAxisX, yAxisY);
            e.Graphics.RotateTransform(-90);

            // После RotateTransform рисуем текст относительно новой системы координат
            e.Graphics.DrawString(yAxisText, axisTitleFont, axisTitleBrush, 0, 0,
                new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });

            e.Graphics.ResetTransform();

            // Горизонтальная подпись оси X (внизу, по центру)
            e.Graphics.DrawString("Дата", axisTitleFont, axisTitleBrush,
                left + chartWidth / 2f, bottom + 5,
                new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
        }
    }
}
