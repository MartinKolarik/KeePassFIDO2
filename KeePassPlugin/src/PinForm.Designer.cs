using System.ComponentModel;

namespace KeePassFIDO2
{
	partial class PinForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
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
			this.textBox1 = new KeePass.UI.SecureTextBoxEx();
			this.textBoxTop = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// textBox1
			//
			this.textBox1.Location = new System.Drawing.Point(10, 60);
			this.textBox1.MaxLength = 63;
			this.textBox1.Name = "textBox1";
			this.textBox1.PasswordChar = '*';
			this.textBox1.Size = new System.Drawing.Size(310, 20);
			this.textBox1.TabIndex = 0;
			//
			// textBoxTop
			//
			this.textBoxTop.BackColor = System.Drawing.SystemColors.Control;
			this.textBoxTop.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBoxTop.Location = new System.Drawing.Point(10, 10);
			this.textBoxTop.Multiline = true;
			this.textBoxTop.Name = "textBoxTop";
			this.textBoxTop.ReadOnly = true;
			this.textBoxTop.Size = new System.Drawing.Size(310, 40);
			this.textBoxTop.TabIndex = 3;
			this.textBoxTop.TabStop = false;
			this.textBoxTop.Text = "If your device is protected by a PIN, please enter it below";
			this.textBoxTop.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			//
			// button1
			//
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Location = new System.Drawing.Point(10, 90);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(310, 20);
			this.button1.TabIndex = 2;
			this.button1.Text = "Submit";
			this.button1.UseVisualStyleBackColor = true;
			//
			// PinForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(330, 161);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.textBoxTop);
			this.Controls.Add(this.textBox1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PinForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "FIDO2 PIN";
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.Windows.Forms.Button button1;
		private KeePass.UI.SecureTextBoxEx textBox1;
		private System.Windows.Forms.TextBox textBoxTop;

		#endregion
	}
}

