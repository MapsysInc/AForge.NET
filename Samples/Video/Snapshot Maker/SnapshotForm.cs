using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using AForge.Imaging.Filters;

namespace Snapshot_Maker
{
    public partial class SnapshotForm : Form
    {
        private bool _drawing;
        private bool _firstPoint;
        private Point _initialPoint;
        Point _startPoint;
        Point _endPoint;

        public SnapshotForm()
        {
            InitializeComponent();
            pictureBox.Paint += PictureBox_Paint;
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (_drawing && _startPoint != null && _endPoint != null)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    Size size = new Size(_endPoint.X - _startPoint.X, _endPoint.Y - _startPoint.Y);
                    Rectangle rect = new Rectangle(_startPoint, size);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }
        }

        public void SetImage(Bitmap bitmap)
        {
            timeBox.Text = DateTime.Now.ToLongTimeString();

            lock (this)
            {
                Bitmap old = (Bitmap)pictureBox.Image;
                pictureBox.Image = bitmap;

                if (old != null)
                {
                    old.Dispose();
                }
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(saveFileDialog.FileName).ToLower();
                ImageFormat format = ImageFormat.Jpeg;

                if (ext == ".bmp")
                {
                    format = ImageFormat.Bmp;
                }
                else if (ext == ".png")
                {
                    format = ImageFormat.Png;
                }

                try
                {
                    lock (this)
                    {
                        decimal ratioX, ratioY;
                        ratioX = (decimal)pictureBox.Image.Width / (decimal)pictureBox.Width;
                        ratioY = (decimal)pictureBox.Image.Height / (decimal)pictureBox.Height;
                        Size size = new Size(Convert.ToInt32(ratioX * (_endPoint.X - _startPoint.X)), Convert.ToInt32(ratioY * (_endPoint.Y - _startPoint.Y)));
                        Rectangle rect = new Rectangle(new Point(Convert.ToInt32(ratioX * _startPoint.X), Convert.ToInt32(ratioY * _startPoint.Y)), size);
                        Crop crop = new Crop(rect);
                        using (Bitmap image = crop.Apply((Bitmap)pictureBox.Image))
                        {

                            image.Save(saveFileDialog.FileName, format);
                        }
;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed saving the snapshot.\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SnapshotForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (_firstPoint)
            {
                _initialPoint = e.Location;
                _startPoint = e.Location;
                _firstPoint = false;
            }
            this._endPoint = e.Location;
            if (_startPoint.X > e.Location.X)
            {
                _endPoint.X = _initialPoint.X;
                _startPoint.X = e.Location.X;
            }
            else
            {
                _endPoint.X = e.Location.X;
                _startPoint.X = _initialPoint.X;
            }
            if (_startPoint.Y > e.Location.Y)
            {
                _endPoint.Y = _initialPoint.Y;
                _startPoint.Y = e.Location.Y;
            }
            else
            {
                _startPoint.Y = _initialPoint.Y;
                _endPoint.Y = e.Location.Y;
            }
            pictureBox.Invalidate();
            tbStartX.Text = _startPoint.X.ToString();
            tbStartY.Text = _startPoint.Y.ToString();
            tbEndX.Text = _endPoint.X.ToString();
            tbEndY.Text = _endPoint.Y.ToString();
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {

            _drawing = true;
            _firstPoint = true;
            pictureBox.MouseMove += SnapshotForm_MouseMove;

        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {

            _drawing = false;
            pictureBox.MouseMove -= SnapshotForm_MouseMove;

        }

    }
}
