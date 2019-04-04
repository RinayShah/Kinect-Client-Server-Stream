using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using ExampleLib;

namespace KinectStreams
{

    public partial class MainWindow : Window
    {
        #region Members

        Mode _mode = Mode.Color;

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;

        bool _displayBody = true;
        bool _recordBody = false;
        string filePath = "";
        DateTime timeRecordingStarted;
        ulong currentTrackingID = 0;

        private Client _client;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            _client = new Client();
        }

        #endregion

        #region Event handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();
            _client.Start(new CancellationToken());

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Color)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Depth
            using (var frame = reference.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Depth)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Infrared
            using (var frame = reference.InfraredFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Infrared)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    TimeSpan relativeTime = DateTime.Now - timeRecordingStarted;
                    string time = relativeTime.TotalSeconds.ToString();

                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {

                                // Draw skeleton.
                                if (_displayBody)
                                {
                                    canvas.DrawSkeleton(body);
                                }
                                // if trackingId not right, continue to next body
                                if (currentTrackingID == 0)
                                {
                                    currentTrackingID = body.TrackingId;
                                }
                                else if (currentTrackingID != body.TrackingId)
                                {
                                    continue;
                                }
                                // write file
                                if (_recordBody)
                                {
                                    if (_client.IsConnected)
                                    {
                                        body.SkeletonAsString(time, out string val);
                                        _client.Write(val);
                                    }

                                    body.WriteSkeleton(filePath, time);
                                }
                            }
                        }
                    }
                } 

            }
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Color;
        }

        private void Depth_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Depth;
        }

        private void Infrared_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Infrared;
        }

        private void Body_Click(object sender, RoutedEventArgs e)
        {
            _displayBody = !_displayBody;
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            _recordBody = !_recordBody;
            if (_recordBody == false)
            {
                recordButton.Content = "Record";
                return;
            }
            recordButton.Content = "Stop Recording";
            currentTrackingID = 0;
            // create a csv file and write file header
            string currPath = System.IO.Directory.GetCurrentDirectory();
            string folder = "recordings";
            string recordPath = System.IO.Path.Combine(currPath, folder);
            if (!Directory.Exists(recordPath))
            {
                Directory.CreateDirectory(recordPath);
            }
            timeRecordingStarted = DateTime.Now;
            string filename = timeRecordingStarted.ToString("yyyy-MM-dd HH-mm-ss");
            filename = filename + ".csv";
            filePath = System.IO.Path.Combine(recordPath, filename);
            string[] writtentext = new string[1 + 25*3];
            string[] jointNames = { "SpineBase", "SpineMid", "Neck", "Head", "ShoulderLeft", "ElbowLeft", "WristLeft", "HandLeft", "ShoulderRight", "ElbowRight", "WristRight", "HandRight", "HipLeft", "KneeLeft", "AnkleLeft", "FootLeft", "HipRight", "KneeRight", "AnkleRight", "FootRight", "SpineShoulder", "HandTipLeft", "ThumbLeft", "HandTipRight", "ThumbRight" };
            writtentext[0] = "Time";
            for (int i = 0; i < jointNames.Length; i++)
            {
                writtentext[1 + 3 * i + 0] = jointNames[i] + "X";
                writtentext[1 + 3 * i + 1] = jointNames[i] + "Y";
                writtentext[1 + 3 * i + 2] = jointNames[i] + "Z";

            }

            File.WriteAllText(filePath, string.Join(",", writtentext));

        }

        #endregion
    }

    public enum Mode
    {
        Color,
        Depth,
        Infrared
    }
}
