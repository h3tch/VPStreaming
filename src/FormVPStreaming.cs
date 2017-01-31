using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace VPStreaming
{
    public partial class FormVPStreaming : Form
    {
        public FormVPStreaming()
        {
            InitializeComponent();

            // load the logo if the file exists
            if (File.Exists("logo.png"))
            {
				// this step resizes the windows to deal with Mono/.Net differences
				MaximumSize = new Size(MaximumSize.Width, MaximumSize.Width);
				MinimumSize = new Size(MinimumSize.Width, MaximumSize.Width);

				// load the logo and resize it
                var logo = Image.FromFile("logo.png");
				var offset = PictureLogo.Width - PictureLogo.Height;
				// resize the app so the picture containing the logo is squared
				MaximumSize = new Size(MaximumSize.Width, MaximumSize.Height + offset);
				MinimumSize = new Size(MinimumSize.Width, MinimumSize.Height + offset);
				// set the logo
                PictureLogo.Image = ResizeImage(logo, PictureLogo.Width, PictureLogo.Height);
            }
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
			// get environment variables that contain the gstreamer root directory (windows)
            var gstDirectory = Environment.GetEnvironmentVariable("GSTREAMER_1_0_ROOT_X86");
            if (gstDirectory == null)
                gstDirectory = Environment.GetEnvironmentVariable("GSTREAMER_1_0_ROOT_X86_64");

            // set default values for gstreamer
            var gst_exe = "gst-launch-1.0";
            var gst_launch = gstDirectory != null ? $"{gstDirectory}bin/" : "./";
            var gst_pipeline = "videotestsrc ! autovideosink";
            
            // load settings xml file
            var xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.Load("settings.xml");

                var exe = xmlDocument["Settings"]?["gstreamer"]?["executable"]?.InnerText;
                var launch = xmlDocument["Settings"]?["gstreamer"]?["directory"]?.InnerText;
                var pipeline = xmlDocument["Settings"]?["gstreamer"]?["pipeline"]?.InnerText;

                if (exe != null && exe.Length > 0)
                    gst_exe = exe;
                if (launch != null && launch.Length > 0)
                    gst_launch = launch;
                if (pipeline != null && pipeline.Length > 0)
                {
                    pipeline = Regex.Replace(pipeline, "{IP}", TextIpAddress.Text);
                    gst_pipeline = Regex.Replace(pipeline, "{password}", TextPassword.Text);
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "File not found");
            }

            // set process info
            var startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.FileName = $"{gst_launch}{gst_exe}";
            startInfo.Arguments = gst_pipeline;

            // create process
            var process = new Process();
            process.StartInfo = startInfo;

            try
            {
                // start process
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destImage = new Bitmap(width, height);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.DrawImage(image, 0, 0, destImage.Width, destImage.Height);
            }

            return destImage;
        }
    }
}
