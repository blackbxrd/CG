using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace lab3
{
    public partial class Form1 : Form
    {
        

        // Переменные для рисования
        private Point startPoint;
        private Point endPoint;
        private bool isDrawing = false;
        private Pen drawingPen = new Pen(Color.Black, 2);
        private Bitmap drawingBitmap;
       

        // Флаги режимов
        private bool isLineMode = false;
        private bool isFillMode = false;
        private Color fillColor = Color.Red;

        public Form1()
        {
            InitializeComponent();

            // Инициализируем Bitmap для рисования
            drawingBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = drawingBitmap;
            ClearDrawingCanvas();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void invertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Загрузите изображение сначала!");
                return;
            }

            Bitmap originalImage = new Bitmap(pictureBox1.Image);
            Bitmap invertedImage = new Bitmap(originalImage.Width, originalImage.Height);

            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color originalColor = originalImage.GetPixel(x, y);
                    Color invertedColor = Color.FromArgb(
                        255 - originalColor.R,
                        255 - originalColor.G,
                        255 - originalColor.B
                    );
                    invertedImage.SetPixel(x, y, invertedColor);
                }
            }

            pictureBox1.Image = invertedImage;
            drawingBitmap = new Bitmap(invertedImage);
            originalImage.Dispose();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearDrawingCanvas();
        }

        private void ClearDrawingCanvas()
        {
            using (Graphics g = Graphics.FromImage(drawingBitmap))
            {
                g.Clear(Color.White);
            }
            pictureBox1.Image = drawingBitmap;
            pictureBox1.Refresh();
        }

        private void lineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Активируем режим рисования линий
            isLineMode = true;
            isFillMode = false;
            pictureBox1.Cursor = Cursors.Cross;
        }

        private void fillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isFillMode = true;
            isLineMode = false;
            pictureBox1.Cursor = Cursors.Arrow;
        }

        // События мыши для рисования линий
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (isLineMode && e.Button == MouseButtons.Left)
            {
                // Начало рисования линии
                startPoint = e.Location;
                isDrawing = true;
            }
            else if (isFillMode && e.Button == MouseButtons.Left)
            {
                // Выполняем заливку
                FloodFill(e.Location);
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && isLineMode)
            {
                // Рисуем предварительную линию
                endPoint = e.Location;

                // Создаем временную копию изображения для предварительного просмотра
                Bitmap tempBitmap = new Bitmap(drawingBitmap);

                // Используем алгоритм Брезенхема для рисования линии
                DrawBresenhamLine(tempBitmap, startPoint, endPoint, drawingPen.Color);

                pictureBox1.Image = tempBitmap;
                pictureBox1.Refresh();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDrawing && isLineMode && e.Button == MouseButtons.Left)
            {
                // Завершаем рисование линии
                endPoint = e.Location;

                // Рисуем финальную линию на основном изображении с помощью алгоритма Брезенхема
                DrawBresenhamLine(drawingBitmap, startPoint, endPoint, drawingPen.Color);

                pictureBox1.Image = drawingBitmap;
                pictureBox1.Refresh();
                isDrawing = false;
            }
        }

        // Метод заливки (Flood Fill)
        private void FloodFill(Point startPoint)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Сначала загрузите изображение или создайте холст!");
                return;
            }

            Bitmap bmp = new Bitmap(drawingBitmap);
            Color targetColor = bmp.GetPixel(startPoint.X, startPoint.Y);
            Color replacementColor = fillColor;

            // Если цвет уже совпадает с цветом заливки, выходим
            if (targetColor.ToArgb() == replacementColor.ToArgb())
                return;

            // Используем стек для реализации алгоритма заливки
            Stack<Point> pixels = new Stack<Point>();
            pixels.Push(startPoint);

            while (pixels.Count > 0)
            {
                Point pt = pixels.Pop();

                // Проверяем границы
                if (pt.X < 0 || pt.X >= bmp.Width || pt.Y < 0 || pt.Y >= bmp.Height)
                    continue;

                // Проверяем цвет пикселя
                if (bmp.GetPixel(pt.X, pt.Y).ToArgb() != targetColor.ToArgb())
                    continue;

                // Заменяем цвет
                bmp.SetPixel(pt.X, pt.Y, replacementColor);

                // Добавляем соседние пиксели в стек
                pixels.Push(new Point(pt.X - 1, pt.Y)); // слева
                pixels.Push(new Point(pt.X + 1, pt.Y)); // справа
                pixels.Push(new Point(pt.X, pt.Y - 1)); // сверху
                pixels.Push(new Point(pt.X, pt.Y + 1)); // снизу
            }

            // Обновляем изображение
            drawingBitmap = bmp;
            pictureBox1.Image = drawingBitmap;
            pictureBox1.Refresh();
        }

        // Алгоритм Брезенхема для рисования линии
        private void DrawBresenhamLine(Bitmap bitmap, Point p1, Point p2, Color color)
        {
            int x0 = p1.X;
            int y0 = p1.Y;
            int x1 = p2.X;
            int y1 = p2.Y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                // Проверяем границы изображения
                if (x0 >= 0 && x0 < bitmap.Width && y0 >= 0 && y0 < bitmap.Height)
                {
                    bitmap.SetPixel(x0, y0, color);
                }

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }



        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void linesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }
    }
}