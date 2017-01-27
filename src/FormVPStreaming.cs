using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace VPStreaming
{
    public partial class FormVPStreaming : Form
    {
        public FormVPStreaming()
        {
            InitializeComponent();
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            // get environment variables that contain the gstreamer root directory
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
                    gst_pipeline = pipeline;
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
    }
}
