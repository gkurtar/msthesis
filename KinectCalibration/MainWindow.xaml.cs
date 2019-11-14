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
using System.Diagnostics;
using System.Globalization;
using System.Configuration;
using System.IO;
using Microsoft.Kinect;


namespace KinectCalibration
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow() {
			InitializeComponent();
		}
		
		private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution640x480Fps30;
		private const ColorImageFormat ColorFormatLow = ColorImageFormat.RgbResolution640x480Fps30;
		private const ColorImageFormat ColorFormatHigh = ColorImageFormat.RgbResolution1280x960Fps12;
		private const ColorImageFormat InfraRedFormat = ColorImageFormat.InfraredResolution640x480Fps30;

		private KinectSensor sensor;
		
		private WriteableBitmap colorBitmap;
		private WriteableBitmap irBitmap;
		private WriteableBitmap depthBitmap;

		private DepthImagePixel[] depthImagePixels;
		private short[] depthNumericPixelValues;

		private byte[] colorPixels;
		private byte[] irPixels;
		private byte[] depthPixels;

		private ColorImagePoint[] colorCoordinates;
		
		private int colorToDepthDivisor;
		
		private Boolean USE_RGB_CAMERA = false;

		private int counter = 0;
		private int xPosDepthStream = 0;
		private int yPosDepthStream = 0;

		private int xPosRgbIrStream = 0;
		private int yPosRgbIrStream = 0;

		//private DepthImagePixel[] rawDepthPixels;
		//private short[] depthNumericPixelValues;

		private void WindowLoaded(object sender, RoutedEventArgs e) {
			//System.Configuration.AppSettingsReader.
			String strCamType = ConfigurationManager.AppSettings["CAMERA_TYPE"];

			MessageBox.Show("CAMERA MODE: " + strCamType);

			if (strCamType == null) {
				USE_RGB_CAMERA = true;
			}
			else {
				if (strCamType.Equals("RGB")) {
					USE_RGB_CAMERA = true;
				}
				else if (strCamType.Equals("IR")) {
					USE_RGB_CAMERA = false;
				}
				else {
					USE_RGB_CAMERA = true;
				}
			}

			// Look through all sensors and start the first connected one.
			// This requires that a Kinect is connected at the time of app startup.
			// To make your app robust against plug/unplug, 
			// it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
			foreach (var potentialSensor in KinectSensor.KinectSensors) {
				if (potentialSensor.Status == KinectStatus.Connected) {
					this.sensor = potentialSensor;
					break;
				}
			}
			
			if (null != this.sensor) {
				if (USE_RGB_CAMERA) {
					this.sensor.ColorStream.Enable(ColorFormatHigh);
				}
				else {
					this.sensor.ColorStream.Enable(InfraRedFormat);
				}

				// Turn on the depth stream to receive depth frames
				this.sensor.DepthStream.Enable(DepthFormat);

				this.colorToDepthDivisor = this.sensor.ColorStream.FrameWidth / this.sensor.DepthStream.FrameWidth;

				// Turn on to get player masks
				this.sensor.SkeletonStream.Enable();
				// Allocate space to put the depth pixels we'll receive
				this.depthImagePixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];
				this.depthNumericPixelValues = new short[this.sensor.DepthStream.FramePixelDataLength];
				// Allocate space to put the color pixels we'll create
				this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
				this.irPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
				//this.depthPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
				this.depthPixels = new byte[this.sensor.DepthStream.FramePixelDataLength * sizeof(int)];
				this.colorCoordinates = new ColorImagePoint[this.sensor.DepthStream.FramePixelDataLength];
				
				// This is the bitmap we'll display on-screen
				this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth,
					this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
				this.irBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth,
					this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Gray16, null);
				this.depthBitmap = new WriteableBitmap(this.sensor.DepthStream.FrameWidth,
					this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

				//if (USE_RGB_CAMERA) {
				//	this.imgLeft.Source = this.colorBitmap;
				//}
				//else {
				//	this.imgLeft.Source = this.irBitmap;
				//}
				// Set the image we display to point to the bitmap where we'll put the image data
				this.imgLeft.Source = USE_RGB_CAMERA ? this.colorBitmap : this.irBitmap;
				this.imgRight.Source = this.depthBitmap;

				this.imgLeft.MouseMove += mouseMoveOverLeftStream;
				this.imgRight.MouseMove += mouseMoveOverRightStream;

				this.imgLeft.MouseLeave += mouseLeaveFromStreams;
				this.imgRight.MouseLeave += mouseLeaveFromStreams;

				//this.bottomRow.MouseEnter += mouseEnterBottomRow;

				//this.MouseMove += mouseMoveOverWindow;

				// Add an event handler to be called whenever there is new depth frame data
				this.sensor.AllFramesReady += this.SensorAllFramesReady;
				// Start the sensor!
				try {
					this.sensor.Start();
				} catch (IOException) {
					this.sensor = null;
				}
			}

			if (null == this.sensor) {
				this.statusBarText.Text = Properties.Resources.NoKinectReady;
			}
			return;
		}

		private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e) {
			// in the middle of shutting down, so nothing to do
			if (null == this.sensor) {
				return;
			}
			
			using (DepthImageFrame depthFrame = e.OpenDepthImageFrame()) {
				//if (null != depthFrame) {
				//	// Copy the pixel data from the image to a temporary array
				//	depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);
				//	depthReceived = true;
				//}
				if (counter++ == int.MaxValue) {
					counter = 0;
				}

				if (depthFrame != null) {
					// Copy the pixel data from the image to a temporary array
					depthFrame.CopyDepthImagePixelDataTo(this.depthImagePixels);
					//this.rawDepthPixels = depthFrame.GetRawPixelData();
					depthFrame.CopyPixelDataTo(this.depthNumericPixelValues);
					// Get the min and max reliable depth for the current frame
					int minDepth = depthFrame.MinDepth;
					int maxDepth = depthFrame.MaxDepth;
					// Convert the depth to RGB
					int depthPixelIndex = 0;

					for (int i = 0; i < this.depthImagePixels.Length; ++i) {
						// Get the depth for this pixel
						short depth = depthImagePixels[i].Depth;
						// To convert to a byte, we're discarding the most-significant
						// rather than least-significant bits.
						// We're preserving detail, although the intensity will "wrap."
						// Values outside the reliable depth range are mapped to 0 (black).
						byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);
						this.depthPixels[depthPixelIndex++] = intensity;
						this.depthPixels[depthPixelIndex++] = intensity; 
						this.depthPixels[depthPixelIndex++] = intensity;
						// We're outputting BGR, the last byte in the 32 bits is unused so skip it
						// If we were outputting BGRA, we would write alpha here.
						++depthPixelIndex;
					}

					// Write the pixel data into our bitmap
					this.depthBitmap.WritePixels(
						new Int32Rect(0, 0, this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight),
						this.depthPixels,
						this.depthBitmap.PixelWidth * sizeof(int),
						0);

					if (this.imgRight.IsMouseOver) {
						int nArrayPos = yPosDepthStream * this.sensor.DepthStream.FrameWidth + xPosDepthStream;

						if (this.depthImagePixels.Length > nArrayPos) {
							this.statusBarText.Text = String.Format("x:{0}, y:{1}," +
								" index: {2}, isKnown: {3}, depth {4}, counter: {5}",
								xPosDepthStream,
								yPosDepthStream,
								nArrayPos,
								this.depthImagePixels[nArrayPos].IsKnownDepth,
								this.depthImagePixels[nArrayPos].Depth,
								counter);
						} // end of if
					}
				}
			} // end of using

			using (ColorImageFrame colorFrame = e.OpenColorImageFrame()) {
				if (null != colorFrame) {
					// Copy the pixel data from the image to a temporary array
					//colorFrame.CopyPixelDataTo(this.colorPixels);
					if (USE_RGB_CAMERA) {
						colorFrame.CopyPixelDataTo(this.colorPixels);
						this.colorBitmap.WritePixels(
						new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
						this.colorPixels,
						this.colorBitmap.PixelWidth * sizeof(int),
						//this.colorBitmap.PixelWidth * colorFrame.BytesPerPixel,
						0);
						
						if (this.imgLeft.IsMouseOver) {
							int nArrayPos = yPosRgbIrStream * this.sensor.ColorStream.FrameWidth + xPosRgbIrStream;

							if (this.colorPixels.Length > nArrayPos) {
								this.statusBarText.Text = String.Format("x:{0}, y:{1}," +
									" counter: {2}", xPosRgbIrStream, yPosRgbIrStream, counter);
							} // end of if
						}
					}
					else {
						colorFrame.CopyPixelDataTo(this.irPixels);
						this.irBitmap.WritePixels(
							new Int32Rect(0, 0, this.irBitmap.PixelWidth, this.irBitmap.PixelHeight),
							this.irPixels,
							this.irBitmap.PixelWidth * colorFrame.BytesPerPixel,
							0);

						if (this.imgLeft.IsMouseOver) {
							int nArrayPos = yPosRgbIrStream * this.sensor.ColorStream.FrameWidth + xPosRgbIrStream;

							if (this.irPixels.Length > nArrayPos) {
								this.statusBarText.Text = String.Format("xPos:{0}, yPos:{1}," +
									" counter {2}", xPosRgbIrStream, yPosRgbIrStream, counter);
							} // end of if
						}
					}
				}
			} //end of using color frame
			
			/*
			// do our processing outside of the using block
			// so that we return resources to the kinect as soon as possible
			if (true == depthReceived) {
				this.sensor.CoordinateMapper.MapDepthFrameToColorFrame(
					DepthFormat,
					this.depthImagePixels,
					ColorFormatHigh,
					this.colorCoordinates);

				// loop over each row and column of the depth
				for (int y = 0; y < this.sensor.DepthStream.FrameHeight; ++y) {
					for (int x = 0; x < this.sensor.DepthStream.FrameWidth; ++x) {
						// calculate index into depth array
						int depthIndex = x + (y * this.sensor.DepthStream.FrameWidth);
						DepthImagePixel depthPixel = this.depthImagePixels[depthIndex];
						int player = depthPixel.PlayerIndex;

						// if we're tracking a player for the current pixel, sets it opacity to full
						if (player > 0) {
							// retrieve the depth to color mapping for the current depth pixel
							ColorImagePoint colorImagePoint = this.colorCoordinates[depthIndex];
							// scale color coordinates to depth resolution
							int colorInDepthX = colorImagePoint.X / this.colorToDepthDivisor;
							int colorInDepthY = colorImagePoint.Y / this.colorToDepthDivisor;
						}
					}
				}
			}
			*/
			return;
		}

		private void btnSaveRGBIR_Click(object sender, RoutedEventArgs e) {
			if (null == this.sensor) {
				this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
				return;
			}

			int colorWidth = this.sensor.ColorStream.FrameWidth;
			int colorHeight = this.sensor.ColorStream.FrameHeight;

			// create a render target that we'll render our controls to
			RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
				colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Pbgra32);

			DrawingVisual dv = new DrawingVisual();
			using (DrawingContext dc = dv.RenderOpen())
			{
				// render the backdrop
				//VisualBrush backdropBrush = new VisualBrush(Backdrop);
				//dc.DrawRectangle(backdropBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));

				// render the color image masked out by players
				VisualBrush colorBrush = new VisualBrush(imgLeft);
				dc.DrawRectangle(colorBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));
			}

			renderBitmap.Render(dv);
			// create a png bitmap encoder which knows how to save a .png file
			BitmapEncoder encoder = new PngBitmapEncoder();
			// create frame from the writable bitmap and add to encoder
			encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
			string time = System.DateTime.Now.ToString("hh'-'mm'-'ss",
				CultureInfo.CurrentUICulture.DateTimeFormat);
			string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
			string path = System.IO.Path.Combine(myPhotos,
				(USE_RGB_CAMERA ? "KinectRgbSnapshot-" : "KinectIrSnapshot-") + time + ".png");
			
			// write the new file to disk
			try {
				using (FileStream fs = new FileStream(path, FileMode.Create)) {
					encoder.Save(fs);
				}
				this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture,
					"{0} Saved image to {1}", Properties.Resources.ScreenshotWriteSuccess, path);
			} catch (IOException) {
				this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture,
					"{0} {1}", Properties.Resources.ScreenshotWriteFailed, path);
			}
			return;
		}

		private void btnSaveDepthClick(object sender, RoutedEventArgs e) {
			if (null == this.sensor) {
				this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
				return;
			}
			//int depthWidth = this.sensor.DepthStream.FrameWidth;
			//int depthHeight = this.sensor.DepthStream.FrameHeight;

			// create a render target that we'll render our controls to
			RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
				this.sensor.DepthStream.FrameWidth,
				this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Pbgra32);

			DrawingVisual dv = new DrawingVisual();
			using (DrawingContext dc = dv.RenderOpen()) {
				// render the color image masked out by players
				VisualBrush colorBrush = new VisualBrush(imgRight);
				dc.DrawRectangle(colorBrush, null, new Rect(
					new Point(),
					new Size(this.sensor.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight)));
			}

			renderBitmap.Render(dv);
			// create a png bitmap encoder which knows how to save a .png file
			BitmapEncoder encoder = new PngBitmapEncoder();
			// create frame from the writable bitmap and add to encoder
			encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

			string time = System.DateTime.Now.ToString("hh'-'mm'-'ss",
				CultureInfo.CurrentUICulture.DateTimeFormat);
			string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
			string pathSnapshot = System.IO.Path.Combine(myPhotos, "KinectDepthSnapshot-" + time + ".png");
			string pathDepth = System.IO.Path.Combine(myPhotos, "Depth" + time + ".txt");

			// write the new file to disk
			try {
				using (FileStream fs = new FileStream(pathSnapshot, FileMode.Create)) {
					encoder.Save(fs);
				}
				//this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture,
				//	"{0} {1}", Properties.Resources.ScreenshotWriteSuccess, pathSnapshot);
				
				using (StreamWriter sw = new StreamWriter(pathDepth)) {
					// loop over each row and column of the depth
					for (int y = 0; y < this.sensor.DepthStream.FrameHeight; ++y) {
						for (int x = 0; x < this.sensor.DepthStream.FrameWidth; ++x) {
							// calculate index into depth array
							int depthIndex = x + (y * this.sensor.DepthStream.FrameWidth);
							DepthImagePixel depthPixel = this.depthImagePixels[depthIndex];
							sw.WriteLine(x + " " + y + " " + depthPixel.Depth);
						}
					}
				}

				this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture,
					"{0} Saved depth image to {1} and depth data to {2}",
					Properties.Resources.ScreenshotWriteSuccess, pathSnapshot, pathDepth);

			} catch (IOException) {
				this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture,
					"{0}", Properties.Resources.ScreenshotWriteFailed);
			}
			return;
		}

		void mouseLeaveFromStreams(Object sender, MouseEventArgs e) {
			this.statusBarText.Text = "";
			return;
		}

		void mouseMoveOverLeftStream(Object sender, MouseEventArgs e) {
			this.xPosRgbIrStream = (int)Mouse.GetPosition(this.imgLeft).X;
			this.yPosRgbIrStream = (int)Mouse.GetPosition(this.imgLeft).Y;
			return;
		}

		void mouseMoveOverRightStream(Object sender, MouseEventArgs e) {
			int xPos = (int)Mouse.GetPosition(this).X;
			int yPos = (int)Mouse.GetPosition(this).Y;
			
			int xPosRelToRightStream = (int)Mouse.GetPosition(this.imgRight).X;
			int yPosRelToRightStream = (int)Mouse.GetPosition(this.imgRight).Y;

			this.xPosDepthStream = (int)Mouse.GetPosition(this.imgRight).X;
			this.yPosDepthStream = (int)Mouse.GetPosition(this.imgRight).Y;
			/*
			if (xPosRelToRightStream >= 0
				&& xPosRelToRightStream < this.sensor.DepthStream.FrameWidth
				&& yPosRelToRightStream >= 0
				&& yPosRelToRightStream < this.sensor.DepthStream.FrameHeight) {
				
				int nArrayPos = yPosRelToRightStream * this.sensor.DepthStream.FrameWidth
				+ xPosRelToRightStream;

				this.statusBarText.Text = String.Format("Positions: {0}, {1}," +
					" Rel to Right Img: {2}, {3}, Index: {4}, dimpx len: {5}, dnpx len: {6}",
				xPos, yPos, xPosRelToRightStream, yPosRelToRightStream, nArrayPos,
				this.depthImagePixels.Length, this.depthNumericPixelValues.Length);

				if (this.depthImagePixels.Length > nArrayPos) {
					short sTemp = this.depthNumericPixelValues[nArrayPos];
					int nTemp = sTemp & 0x0000FFFF;
					nTemp >>= DepthImageFrame.PlayerIndexBitmaskWidth;
					int first11 = (int)sTemp & 0x000007FF;
					int first12 = (int)sTemp & 0x00000FFF;
					int signbit = first12 >> 11;

					int nDepthValueFromRawData = sTemp >> DepthImageFrame.PlayerIndexBitmaskWidth;

					this.statusBarText.Text = String.Format("xPos:{0}, yPos:{1}," +
						" idx {2}, isKnown {3} depth {4}, pix {5}, msb {6}," +
						" twel {7}, elev {8}, depthFromRaw{9} ",
						xPosRelToRightStream,
						yPosRelToRightStream,
						nArrayPos,
						this.depthImagePixels[nArrayPos].IsKnownDepth,
						this.depthImagePixels[nArrayPos].Depth,
						this.depthNumericPixelValues[nArrayPos],
						signbit,
						first12,
						first11,
						nDepthValueFromRawData
					);
				}
				else {
					this.statusBarText.Text = String.Format("Gen: xPos:{0}, yPos:{1}." +
						" RelToRightImage: xPos:{2}, yPos:{3}.",
						xPos, yPos, xPosRelToRightStream, yPosRelToRightStream);
				}
			}
			else {
				this.statusBarText.Text = String.Format("xPos:{0}, yPos:{1}.", xPos, yPos);
			}
			*/
			return;
		}

		private void CheckBoxNearModeChanged(object sender, RoutedEventArgs e) {
			if (this.sensor != null) {
				// will not function on non-Kinect for Windows devices
				try {
					if (this.checkBoxNearMode.IsChecked.GetValueOrDefault()) {
						this.sensor.DepthStream.Range = DepthRange.Near;
					}
					else {
						this.sensor.DepthStream.Range = DepthRange.Default;
					}
				} catch (InvalidOperationException){
					;
				}
			}
			return;
		}

		private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
			if (null != this.sensor) {
				this.sensor.Stop();
				this.sensor = null;
			}
			return;
		}

		/*
		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			this.sensor.ElevationAngle = (int) sldTiltAngle.Value;
			this.statusBarText.Text = string.Format("Val:  {0}", sldTiltAngle.Value);
			return;
		}
		*/
	}
}