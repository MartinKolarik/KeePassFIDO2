using System;
using System.Linq;
using System.Windows.Forms;
using KeePassLib.Utility;

namespace KeePassFIDO2
{
	public partial class FIDO2OptionsForm : Form
	{
		private readonly KeePassFIDO2Ext keePassFIDO2Ext;

		public FIDO2OptionsForm(KeePassFIDO2Ext ext)
		{
			InitializeComponent();
			keePassFIDO2Ext = ext;

			if (keePassFIDO2Ext.PluginHost.Database.MasterKey == null)
			{
				ShowStatusMessage("Please unlock the database first");
				buttonAddKey.Visible = false;
			}
			else
			{
				ShowInstructions("Use the button below to register a new authenticator.");
			}
		}

		private void AddKeyButtonClick(object sender, EventArgs e)
		{
			if (keePassFIDO2Ext.PluginHost.Database.MasterKey.UserKeyCount > 1)
			{
				ShowStatusMessage("Error: The database is protected by more than one user key. Only one user key is supported at this time.");
				return;
			}

			var keyBytes = keePassFIDO2Ext.PluginHost.Database.MasterKey.UserKeys.First().KeyData.ReadData();

			if (keyBytes.Length != 32)
			{
				ShowStatusMessage("Error: Only 32-byte long keys are supported at this time.");
				return;
			}

			byte[] pinBytes;

			using (var pinForm = new PinForm())
			{
				if (pinForm.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				pinBytes = pinForm.Pin;
			}

			if (pinBytes.Length > 63) // max length according to the spec
			{
				ShowStatusMessage("Error: max PIN length is 63 characters.");
				MemUtil.ZeroByteArray(pinBytes);
				return;
			}

			var result = DeviceCommunicator.ExecuteCreate(pinBytes, keyBytes);

			// zero out all sensitive data
			result.Clear();

			if (result.ExitCode != 0) {
				ShowStatusMessage($"Error: device communicator exited with code {result.ExitCode}.");
				return;
			}

			ShowStatusMessage("Key added. You can now unlock the database using the authenticator.");
		}

		private void ShowInstructions(string message)
		{
			textBoxTop.Visible = true;
			textBoxTop.Text = message;
		}

		private void ShowStatusMessage(string message)
		{
			textBoxBottom.Visible = true;
			textBoxBottom.Text = message;
		}
	}
}
