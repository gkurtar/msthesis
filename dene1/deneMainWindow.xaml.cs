using System;
using System.Globalization;
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
		private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution320x240Fps30;
		private const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

		private int depthWidth;
		private int depthHeight;
		/// Inverse scaling factor between color and depth
		private int colorToDepthDivisor;

		/// Intermediate storage for the depth data received from the sensor
		private DepthImagePixel[] depthPixels;

		/// Intermediate storage for the color data received from the camera
		private byte[] colorPixels;
		private int[] playerPixelData;

		private ColorImagePoint[] colorCoordinates;
		private WriteableBitmap colorBitmap;
		private WriteableBitmap playerOpacityMaskImage = null;
		private int opaquePixelValue = -1;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void WindowLoaded(object sender, RoutedEventArgs e) {
			MessageBoxResult result = MessageBox.Show("Hello MessageBox");
			// Look through all sensors and start the first connected one.
			// This requires that a Kinect is connected at the time of app startup.
			// To make your app robust against plug/unplug, 
			// it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
			foreach (var potentialSensor in KinectSensor.KinectSensors) {
				if (potentialSensor.Status == KinectStatus.Connected)
				{
					this.sensor = potentialSensor;
					break;
				}
			}

			if (null != this.sensor) {

				// Turn on the depth stream to receive depth frames
				this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
				// Allocate space to put the depth pixels we'll receive
				this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];
				// Allocate space to put the color pixels we'll create
				this.colorPixels = new byte[this.sensor.DepthStream.FramePixelDataLength * sizeof(int)];
				// This is the bitmap we'll display on-screen
				this.colorBitmap = new WriteableBitmap(
					this.sensor.DepthStream.FrameWidth,
					this.sensor.DepthStream.FrameHeight,
					96.0, 96.0, PixelFormats.Bgr32, null);
				// Set the image we display to point to the bitmap where we'll put the image data
				this.MaskedColor.Source = this.colorBitmap;
				// Add an event handler to be called whenever there is new depth frame data
				this.sensor.DepthFrameReady += this.SensorDepthFrameReady;
				// Start the sensor!
				try {
					this.sensor.Start();
				} catch (IOException) {
					this.sensor = null;
				}
			}
			else {
				this.statusBarText.Text = Properties.Resources.NoKinectReady;
				//MessageBox.Show("no kinect ready: " + Properties.Resources.NoKinectReady);
			}

			//this.MaskedColor.
			this.MaskedColor.MouseMove += foo;
			//this.kin_grid.MouseMove += foo;
			return;
		}

		//infrared basics
		//color basics
		//depth basics
		//depth with color 3d
		//kinect explorer d2d
		//kinect explorer wpf

		void foo(Object sender, MouseEventArgs e) {
			this.tbx_Pos.Text = String.Format("dene {0}, {1}",
				Mouse.GetPosition(this).X,
				Mouse.GetPosition(this).Y);
			
			int X_Coord = (int) Mouse.GetPosition(this).X;
			int Y_Coord = (int) Mouse.GetPosition(this).Y;
			int nArrayPos = Y_Coord * this.sensor.DepthStream.FrameWidth + X_Coord;

			if (this.depthPixels.Length > nArrayPos) {
				this.tbx_Depth.Text = String.Format("fw {0}, idx {1}," +
					" arysz {2}, isKnown {3} val {4}",
				this.sensor.DepthStream.FrameWidth,
				nArrayPos,
				this.depthPixels.Length,
				this.depthPixels[nArrayPos].IsKnownDepth,
				this.depthPixels[nArrayPos].Depth);
			}
			else {
				this.tbx_Depth.Text = String.Format("dene {0}, {1}",
				Mouse.GetPosition(this).X,
				Mouse.GetPosition(this).Y);
			}
			//this.depthPixels[]

			return;
		}

		/// <summary>
		/// Event handler for Kinect sensor's DepthFrameReady event
		/// </summary>
		/// <param name="sender">object sending the event</param>
		/// <param name="e">event arguments</param>
		private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
		{
			using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
			{
				if (depthFrame != null) {
					// Copy the pixel data from the image to a temporary array
					depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

					// Get the min and max reliable depth for the current frame
					int minDepth = depthFrame.MinDepth;
					int maxDepth = depthFrame.MaxDepth;

					// Convert the depth to RGB
					int colorPixelIndex = 0;
					for (int i = 0; i < this.depthPixels.Length; ++i) {
						// Get the depth for this pixel
						short depth = depthPixels[i].Depth;

						// To convert to a byte, we're discarding the most-significant
						// rather than least-significant bits.
						// We're preserving detail, although the intensity will "wrap."
						// Values outside the reliable depth range are mapped to 0 (black).

						// Note: Using conditionals in this loop could degrade performance.
						// Consider using a lookup table instead when writing production code.
						// See the KinectDepthViewer class used by the KinectExplorer sample
						// for a lookup table example.
						byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

						// Write out blue byte
						this.colorPixels[colorPixelIndex++] = intensity;

						// Write out green byte
						this.colorPixels[colorPixelIndex++] = intensity;

						// Write out red byte                        
						this.colorPixels[colorPixelIndex++] = intensity;

						// We're outputting BGR, the last byte in the 32 bits is unused so skip it
						// If we were outputting BGRA, we would write alpha here.
						++colorPixelIndex;
					}

					// Write the pixel data into our bitmap
					this.colorBitmap.WritePixels(
						new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
						this.colorPixels,
						this.colorBitmap.PixelWidth * sizeof(int),
						0);
				}
			}
		}


		/*
		private void MaskedColor_MouseOver(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				Point mousePosition = e.GetPosition();
				//Compare the coordinates here with referencePoint and Zoom in or out 
			}
		}
		*/

		/*
		private void image1_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				Point mousePosition = e.GetPosition();
				//Compare the coordinates here with referencePoint and Zoom in or out 
			}
		}
		*/

		/// <summary>
		/// Execute shutdown tasks
		/// </summary>
		/// <param name="sender">object sending the event</param>
		/// <param name="e">event arguments</param>
		private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
			if (null != this.sensor) {
				this.sensor.Stop();
				this.sensor = null;
			}
		}

		/// <summary>
		/// Handles the user clicking on the screenshot button
		/// </summary>
		/// <param name="sender">object sending the event</param>
		/// <param name="e">event arguments</param>
		private void ButtonScreenshotClick(object sender, RoutedEventArgs e) {
			if (null == this.sensor) {
				this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
				return;
			}

			// create a png bitmap encoder which knows how to save a .png file
			BitmapEncoder encoder = new PngBitmapEncoder();

			// create frame from the writable bitmap and add to encoder
			encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));

			string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

			string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

			string path = System.IO.Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");

			// write the new file to disk
			try {
				using (FileStream fs = new FileStream(path, FileMode.Create))
				{
					encoder.Save(fs);
				}
				this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteSuccess, path);
			} catch (IOException) {
				this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteFailed, path);
			}
		}

		/// <summary>
		/// Handles the checking or unchecking of the near mode combo box
		/// </summary>
		/// <param name="sender">object sending the event</param>
		/// <param name="e">event arguments</param>
		private void CheckBoxNearModeChanged(object sender, RoutedEventArgs e)
		{
			if (this.sensor != null) {
				// will not function on non-Kinect for Windows devices
				try {
					if (this.checkBoxNearMode.IsChecked.GetValueOrDefault()) {
						this.sensor.DepthStream.Range = DepthRange.Near;
					}
					else {
						this.sensor.DepthStream.Range = DepthRange.Default;
					}
				} catch (InvalidOperationException) {
				}
			}
			return;
		}
	}
}
