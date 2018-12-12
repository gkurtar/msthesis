using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Microsoft.Kinect;

namespace dene1
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private KinectSensor sensor;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void WindowLoaded(object sender, RoutedEventArgs e)
		{
			MessageBoxResult result = MessageBox.Show("Hello MessageBox");
			// Look through all sensors and start the first connected one.
			// This requires that a Kinect is connected at the time of app startup.
			// To make your app robust against plug/unplug, 
			// it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
			foreach (var potentialSensor in KinectSensor.KinectSensors)
			{
				if (potentialSensor.Status == KinectStatus.Connected)
				{
					this.sensor = potentialSensor;
					break;
				}
			}

			if (null != this.sensor)
			{
				//// Turn on the depth stream to receive depth frames
				//this.sensor.DepthStream.Enable(DepthFormat);

				//this.depthWidth = this.sensor.DepthStream.FrameWidth;

				//this.depthHeight = this.sensor.DepthStream.FrameHeight;

				//this.sensor.ColorStream.Enable(ColorFormat);

				//int colorWidth = this.sensor.ColorStream.FrameWidth;
				//int colorHeight = this.sensor.ColorStream.FrameHeight;

				//this.colorToDepthDivisor = colorWidth / this.depthWidth;

				//// Turn on to get player masks
				//this.sensor.SkeletonStream.Enable();

				//// Allocate space to put the depth pixels we'll receive
				//this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];

				//// Allocate space to put the color pixels we'll create
				//this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

				//this.playerPixelData = new int[this.sensor.DepthStream.FramePixelDataLength];

				//this.colorCoordinates = new ColorImagePoint[this.sensor.DepthStream.FramePixelDataLength];

				//// This is the bitmap we'll display on-screen
				//this.colorBitmap = new WriteableBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

				//// Set the image we display to point to the bitmap where we'll put the image data
				//this.MaskedColor.Source = this.colorBitmap;

				//// Add an event handler to be called whenever there is new depth frame data
				//this.sensor.AllFramesReady += this.SensorAllFramesReady;

				// Start the sensor!
				try
				{
					this.sensor.Start();
				}
				catch (IOException)
				{
					this.sensor = null;
				}
			}

			if (null == this.sensor)
			{
				this.statusBarText.Text = Properties.Resources.NoKinectReady;
			}
		}
	}
}
