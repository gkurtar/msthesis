namespace MAIN
{
	partial class Form1
	{
		/// <summary>
		///Gerekli tasarımcı değişkeni.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///Kullanılan tüm kaynakları temizleyin.
		/// </summary>
		///<param name="disposing">yönetilen kaynaklar dispose edilmeliyse doğru; aksi halde yanlış.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer üretilen kod

		/// <summary>
		/// Tasarımcı desteği için gerekli metot - bu metodun 
		///içeriğini kod düzenleyici ile değiştirmeyin.
		/// </summary>
		private void InitializeComponent()
		{
			this.button1 = new System.Windows.Forms.Button();
			this.btnExit = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnDene = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(536, 55);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "aaa";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// btnExit
			// 
			this.btnExit.Location = new System.Drawing.Point(546, 281);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new System.Drawing.Size(75, 23);
			this.btnExit.TabIndex = 1;
			this.btnExit.Text = "EXIT";
			this.btnExit.UseVisualStyleBackColor = true;
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			// 
			// panel1
			// 
			this.panel1.Location = new System.Drawing.Point(65, 55);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(410, 200);
			this.panel1.TabIndex = 2;
			// 
			// btnDene
			// 
			this.btnDene.Location = new System.Drawing.Point(0, 0);
			this.btnDene.Name = "btnDene";
			this.btnDene.Size = new System.Drawing.Size(75, 23);
			this.btnDene.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(633, 325);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.btnExit);
			this.Controls.Add(this.button1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button btnExit;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button btnDene;
	}
}

