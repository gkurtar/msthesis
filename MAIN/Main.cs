using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MAIN
{
	public partial class LiveKinectData : Form
	{
		public LiveKinectData()
		{
			InitializeComponent();
		}

		private void btnExit_Click(object sender, EventArgs e)
		{
			if (null != this.sensor) {
				this.sensor.Stop();
				this.sensor = null;
			}
			Application.Exit();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (null != this.sensor)
			{
				this.sensor.Stop();
				this.sensor = null;
			}

		}
	}
}
