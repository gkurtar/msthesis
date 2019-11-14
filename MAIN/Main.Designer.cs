using Microsoft.Kinect;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System;

namespace MAIN
{
	partial class LiveKinectData
	{
		/// <summary>
		///Gerekli tasarımcı değişkeni.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		private KinectSensor sensor;
		private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution320x240Fps30;
		private const ColorImageFormat ColorFormatLow = ColorImageFormat.RgbResolution640x480Fps30;
		private const ColorImageFormat ColorFormatHigh = ColorImageFormat.RgbResolution1280x960Fps12;
		private const ColorImageFormat InfraRedFormat = ColorImageFormat.InfraredResolution640x480Fps30;

		private bool COLOR_STREAM_ENABLED = false;

		private int depthWidth;
		private int depthHeight;
		/// Inverse scaling factor between color and depth
		private int colorToDepthDivisor;

		/// Intermediate storage for the depth data received from the sensor
		private DepthImagePixel[] depthPixels;
		private DepthImagePixel[] rawDepthPixels;
		private short[] depthNumericPixelValues;

		/// Intermediate storage for the color data received from the camera
		private byte[] colorPixels;
		private byte[] irPixels;
		private int[] playerPixelData;

		private ColorImagePoint[] colorCoordinates;
		private WriteableBitmap colorBitmap;
		private WriteableBitmap irBitmap;
		private WriteableBitmap playerOpacityMaskImage = null;
		private int opaquePixelValue = -1;

		/// <summary>
		///Kullanılan tüm kaynakları temizleyin.
		/// </summary>
		///<param name="disposing">yönetilen kaynaklar dispose edilmeliyse doğru; aksi halde yanlış.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private Bitmap BitmapFromSource(BitmapSource bmsImage) {
			Bitmap bmpTemp = null;
			using (MemoryStream outStream = new MemoryStream()) {
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(bmsImage));
				enc.Save(outStream);
				bmpTemp = new Bitmap(outStream);
			}
			return bmpTemp;
		}

		protected override void OnLoad(System.EventArgs e)
		{
			//COLOR_STREAM_ENABLED = kincal.Properties.Resources.UseRGB.Equals("true");

			//MessageBoxResult result = MessageBox.Show("Hello MessageBox");
			// Look through all sensors and start the first connected one.
			foreach (var potentialSensor in KinectSensor.KinectSensors) {
				if (potentialSensor.Status == KinectStatus.Connected) {
					this.sensor = potentialSensor;
					break;
				}
			}
			
			if (null != this.sensor) {
				// Turn on the color stream to receive color frames
				if (COLOR_STREAM_ENABLED) {
					this.sensor.ColorStream.Enable(ColorFormatHigh);
					// Allocate space to put the pixels we'll receive
					this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
					// This is the bitmap we'll display on-screen
					this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth,
						this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
				}
				else {
					this.sensor.ColorStream.Enable(InfraRedFormat);
					this.irPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
					this.irBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth,
						this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Gray16, null);
				}
				
				// Turn on the depth stream to receive depth frames
				//this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

				// Allocate space to put the depth pixels we'll receive
				//this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];
				//this.rawDepthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];
				//this.depthNumericPixelValues = new short[this.sensor.DepthStream.FramePixelDataLength];

				// Allocate space to put the color pixels we'll create
				//this.colorPixels = new byte[this.sensor.DepthStream.FramePixelDataLength * sizeof(int)];
				//this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength * sizeof(int)];

				// This is the bitmap we'll display on-screen
				//this.colorBitmap = new WriteableBitmap(
				//	this.sensor.ColorStream.FrameWidth,
				//	this.sensor.ColorStream.FrameHeight,
				//	96.0, 96.0, PixelFormats.Bgr32, null);
				// Set the image we display to point to the bitmap where we'll put the image data
				//this.MaskedColor.Source = this.colorBitmap;

				//this.pnlLeft.BackgroundImage = this.BitmapFromSource(this.colorBitmap);
				//this.imgBitmapLeft = this.BitmapFromSource(this.colorBitmap);
				//this.pbxLeft.Image = this.imgBitmapLeft;
				//this.pbxRight.Image = this.imgBitmapRight;

				// Add an event handler to be called whenever there is new depth frame data
				//this.sensor.DepthFrameReady += this.SensorDepthFrameReady;
				this.sensor.ColorFrameReady += this.SensorColorFrameReady;
				
				// Start the sensor!
				try {
					this.sensor.Start();
				} catch (IOException) {
					this.sensor = null;
				}
			}
			else {
				this.txtStatus.Text = kincal.Properties.Resources.NoKinectReady;
			}
			//this.MaskedColor.MouseMove += foo;
			return;
		}

		#region Windows Form Designer üretilen kod

		/// <summary>
		/// Tasarımcı desteği için gerekli metot - bu metodun 
		///içeriğini kod düzenleyici ile değiştirmeyin.
		/// </summary>
		private void InitializeComponent() {
			this.btnExit = new System.Windows.Forms.Button();
			this.pnlLeft = new System.Windows.Forms.Panel();
			this.pbxLeft = new System.Windows.Forms.PictureBox();
			this.pnlRightBottom = new System.Windows.Forms.Panel();
			this.pnlRightTop = new System.Windows.Forms.Panel();
			this.pbxRight = new System.Windows.Forms.PictureBox();
			this.txtStatus = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.pnlLeft.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbxLeft)).BeginInit();
			this.pnlRightTop.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbxRight)).BeginInit();
			this.SuspendLayout();
			// 
			// btnExit
			// 
			this.btnExit.Location = new System.Drawing.Point(765, 523);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new System.Drawing.Size(75, 23);
			this.btnExit.TabIndex = 1;
			this.btnExit.Text = "EXIT";
			this.btnExit.UseVisualStyleBackColor = true;
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			// 
			// pnlLeft
			// 
			this.pnlLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pnlLeft.Controls.Add(this.pbxLeft);
			this.pnlLeft.Location = new System.Drawing.Point(12, 12);
			this.pnlLeft.Name = "pnlLeft";
			this.pnlLeft.Size = new System.Drawing.Size(640, 480);
			this.pnlLeft.TabIndex = 2;
			// 
			// pbxLeft
			// 
			this.pbxLeft.BackColor = System.Drawing.SystemColors.InactiveCaption;
			this.pbxLeft.Location = new System.Drawing.Point(3, -2);
			this.pbxLeft.Margin = new System.Windows.Forms.Padding(0);
			this.pbxLeft.Name = "pbxLeft";
			this.pbxLeft.Size = new System.Drawing.Size(640, 480);
			this.pbxLeft.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pbxLeft.TabIndex = 0;
			this.pbxLeft.TabStop = false;
			// 
			// pnlRightBottom
			// 
			this.pnlRightBottom.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pnlRightBottom.Location = new System.Drawing.Point(658, 252);
			this.pnlRightBottom.Name = "pnlRightBottom";
			this.pnlRightBottom.Size = new System.Drawing.Size(320, 240);
			this.pnlRightBottom.TabIndex = 3;
			// 
			// pnlRightTop
			// 
			this.pnlRightTop.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pnlRightTop.Controls.Add(this.pbxRight);
			this.pnlRightTop.Location = new System.Drawing.Point(658, 12);
			this.pnlRightTop.Name = "pnlRightTop";
			this.pnlRightTop.Size = new System.Drawing.Size(320, 240);
			this.pnlRightTop.TabIndex = 3;
			// 
			// pbxRight
			// 
			this.pbxRight.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.pbxRight.Location = new System.Drawing.Point(30, 19);
			this.pbxRight.Name = "pbxRight";
			this.pbxRight.Size = new System.Drawing.Size(271, 204);
			this.pbxRight.TabIndex = 0;
			this.pbxRight.TabStop = false;
			// 
			// txtStatus
			// 
			this.txtStatus.BackColor = System.Drawing.SystemColors.InactiveCaption;
			this.txtStatus.Location = new System.Drawing.Point(12, 630);
			this.txtStatus.Name = "txtStatus";
			this.txtStatus.Size = new System.Drawing.Size(966, 20);
			this.txtStatus.TabIndex = 4;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(594, 545);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 5;
			this.button1.Text = "button1";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// LiveKinectData
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
			this.ClientSize = new System.Drawing.Size(1009, 662);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.txtStatus);
			this.Controls.Add(this.pnlRightTop);
			this.Controls.Add(this.pnlRightBottom);
			this.Controls.Add(this.pnlLeft);
			this.Controls.Add(this.btnExit);
			this.Name = "LiveKinectData";
			this.Text = "Live Kinect Data";
			this.pnlLeft.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pbxLeft)).EndInit();
			this.pnlRightTop.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pbxRight)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnExit;
		private System.Windows.Forms.Panel pnlLeft;
		private System.Windows.Forms.Panel pnlRightBottom;
		private System.Windows.Forms.Panel pnlRightTop;
		private System.Windows.Forms.TextBox txtStatus;
		private System.Windows.Forms.PictureBox pbxLeft;
		private System.Windows.Forms.PictureBox pbxRight;

		/*
		/// <summary>
		/// Event handler for Kinect sensor's DepthFrameReady event
		/// </summary>
		/// <param name="sender">object sending the event</param>
		/// <param name="e">event arguments</param>
		private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
		{
			opaquePixelValue++;
			if (opaquePixelValue % 1000 == 0) {
				MessageBox.Show("val is " + opaquePixelValue);
			}
			//this.tbxTemp.Text = kk++.ToString();
			using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
			{
				if (depthFrame != null)
				{
					// Copy the pixel data from the image to a temporary array
					depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

					this.rawDepthPixels = depthFrame.GetRawPixelData();
					depthFrame.CopyPixelDataTo(this.depthNumericPixelValues);
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
					//this.pnlLeft.BackgroundImage = this.BitmapFromSource(this.colorBitmap);
					//this.imgBitmapLeft = this.BitmapFromSource(this.colorBitmap);
				}
			}
			return;
		}
		*/

		IntPtr colorPtr;
		Bitmap bmpTemp;

		IntPtr irPtr;
		Bitmap bmpInfraRed;

		private void SensorColorFrameReadyDene(object sender, ColorImageFrameReadyEventArgs e)
		{
			//this.tbxTemp.Text = kk++.ToString();
			using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
			{
				if (colorFrame != null) {
					if (COLOR_STREAM_ENABLED) {
						colorFrame.CopyPixelDataTo(colorPixels);

						Marshal.FreeHGlobal(colorPtr);
						colorPtr = Marshal.AllocHGlobal(colorPixels.Length);
						Marshal.Copy(colorPixels, 0, colorPtr, colorPixels.Length);

						bmpTemp = new Bitmap(colorFrame.Width, colorFrame.Height,
							colorFrame.Width * colorFrame.BytesPerPixel,
							System.Drawing.Imaging.PixelFormat.Format32bppRgb,
							colorPtr);

						this.pbxLeft.Image = bmpTemp;
					}
					else {
						colorFrame.CopyPixelDataTo(irPixels);

						Marshal.FreeHGlobal(irPtr);
						irPtr = Marshal.AllocHGlobal(irPixels.Length);
						Marshal.Copy(irPixels, 0, irPtr, irPixels.Length);

						bmpInfraRed = new Bitmap(colorFrame.Width, colorFrame.Height,
							colorFrame.Width * colorFrame.BytesPerPixel,
							System.Drawing.Imaging.PixelFormat.Format1bppIndexed,
							irPtr);

						this.pbxLeft.Image = bmpInfraRed;
					}
				}
			}
			return;
		}


		private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
		{
			{
				if (COLOR_STREAM_ENABLED) {
					this.pbxLeft.Image = this.imgBitmapLeft;
				}
				else {
					this.pbxRight.Image = this.imgBitmapRight;
				}
			}

			//this.tbxTemp.Text = kk++.ToString();
			using (ColorImageFrame colorFrame = e.OpenColorImageFrame()) {
				if (colorFrame != null) {
					// Write the pixel data into our bitmap
					//this.colorBitmap.WritePixels(
					//	new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
					//	this.colorPixels,
					//	this.colorBitmap.PixelWidth * sizeof(int),
					//	0);

					if (COLOR_STREAM_ENABLED) {
						colorFrame.CopyPixelDataTo(colorPixels);
						// Write the pixel data into our bitmap
						this.colorBitmap.WritePixels(
							new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
							this.colorPixels,
							this.colorBitmap.PixelWidth * colorFrame.BytesPerPixel,
							0);
						this.imgBitmapLeft = this.BitmapFromSource(this.colorBitmap);
					}
					else {
						colorFrame.CopyPixelDataTo(irPixels);
						// Write the pixel data into our bitmap
						this.irBitmap.WritePixels(
							new Int32Rect(0, 0, this.irBitmap.PixelWidth, this.irBitmap.PixelHeight),
							this.irPixels,
							this.irBitmap.PixelWidth * colorFrame.BytesPerPixel,
							0);
						this.imgBitmapLeft = this.BitmapFromSource(this.irBitmap);
					}
				}
			}
			return;
		}

		protected override void OnClosing(CancelEventArgs e) {
			if (null != this.sensor) {
				this.sensor.Stop();
				this.sensor = null;
			}
		}

		private Bitmap imgBitmapLeft;
		private Bitmap imgBitmapRight;
		private System.Windows.Forms.Button button1;
	}
}

