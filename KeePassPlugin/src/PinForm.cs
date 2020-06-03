using System.Windows.Forms;
using KeePass.UI;

namespace KeePassFIDO2
{
	public partial class PinForm : Form
	{
		public PinForm()
		{
			InitializeComponent();
			SecureTextBoxEx.InitEx(ref textBox1);
		}

		public byte[] Pin => textBox1.TextEx.ReadUtf8();
	}
}

