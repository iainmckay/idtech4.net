namespace idTech4
{
	partial class SystemConsole
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SystemConsole));
			this._input = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this._log = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// _input
			// 
			this._input.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this._input.Location = new System.Drawing.Point(12, 380);
			this._input.Name = "_input";
			this._input.Size = new System.Drawing.Size(520, 20);
			this._input.TabIndex = 0;
			this._input.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnInputKeyDown);
			this._input.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnInputKeyPressed);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(12, 407);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "Copy";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(93, 407);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 2;
			this.button2.Text = "Clear";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.OnClearClicked);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(457, 407);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 23);
			this.button3.TabIndex = 3;
			this.button3.Text = "Quit";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.OnQuitClicked);
			// 
			// _log
			// 
			this._log.BackColor = System.Drawing.Color.DarkBlue;
			this._log.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this._log.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._log.ForeColor = System.Drawing.Color.Gold;
			this._log.Location = new System.Drawing.Point(12, 13);
			this._log.Multiline = true;
			this._log.Name = "_log";
			this._log.ReadOnly = true;
			this._log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this._log.Size = new System.Drawing.Size(520, 361);
			this._log.TabIndex = 4;
			// 
			// SystemConsole
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(544, 442);
			this.Controls.Add(this._log);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this._input);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "SystemConsole";
			this.Text = "DOOM 3";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox _input;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.TextBox _log;
	}
}