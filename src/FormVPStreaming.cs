using System;
using System.Diagnostics;
using System.Drawing;
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
            LoadLogo("logo.png");
        }

        /// <summary>
        /// Loads the logo.
        /// </summary>
        /// <param name="filename">Filename.</param>
        private void LoadLogo(string filename)
        {
            // load the logo if the file exists
            if (!File.Exists("logo.png"))
                return;

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
            PictureLogo.Image = logo.Resize(PictureLogo.Width, PictureLogo.Height);
        }

        /// <summary>
        /// Click event of the start button.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
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

            try
            {
                // load settings xml file
                var xmlDocument = new XmlDocument();
                xmlDocument.Load("settings.xml");

                // get settings specified in the xml
                var exe = xmlDocument["Settings"]?["gstreamer"]?["executable"]?.InnerText;
                var launch = xmlDocument["Settings"]?["gstreamer"]?["directory"]?.InnerText;
                var pipeline = xmlDocument["Settings"]?["gstreamer"]?["pipeline"]?.InnerText;

                // update values if necessary
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
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }
    }
}
