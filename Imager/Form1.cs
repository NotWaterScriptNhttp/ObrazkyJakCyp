using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Imager
{
    public partial class Form1 : Form
    {
        private Bitmap _baseImage = null;
        private Dictionary<string, string> _validImages = new Dictionary<string, string>();

        private Point _startPoint = new Point(527, 37);
        private Point _endPoint = new Point(1015, 687);
        private Size _targetSize;

        public Form1()
        {
            _targetSize = new Size()
            {
                Width = _endPoint.X - _startPoint.X,
                Height = _endPoint.Y - _startPoint.Y
            };

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var asm = Assembly.GetExecutingAssembly();
            var res = asm.GetManifestResourceStream("Imager.BasePainting.png");
            _baseImage = new Bitmap(res);
            panel1.BackgroundImage = _baseImage;

            if (!Directory.Exists("Paintings"))
                Directory.CreateDirectory("Paintings");

            foreach (var imgPath in Directory.GetFiles("Paintings"))
            {
                try
                {
                    using (var img = Bitmap.FromFile(imgPath))
                    {
                        var name = Path.GetFileNameWithoutExtension(imgPath);
                        _validImages.Add(name, imgPath);
                        listBox1.Items.Add(name);
                    }
                } catch (Exception _) { Console.WriteLine("Painting: '{0}' is not a valid image file!", imgPath); }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = listBox1.SelectedItem;
            if (item == null || !_validImages.TryGetValue((string)item, out var image))
            {
                panel1.BackgroundImage = _baseImage;
                return;
            }

            using (var bmp = Bitmap.FromFile(image)) // Initial image
            {
                if (bmp.Width > bmp.Height) // Rotate if the image is wider
                    bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);

                var final = new Bitmap(_baseImage.Width, _baseImage.Height); // Create the final image object
                using (var g = Graphics.FromImage(final))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;

                    g.DrawImage(_baseImage, 0, 0); // Render the template image that will be propagated with our loaded image
                    g.DrawImage(bmp, new Rectangle(_startPoint, _targetSize)); // Set empty area to our image
                }

                panel1.BackgroundImage = final; // Show image to the user
            }
        }
    }
}
