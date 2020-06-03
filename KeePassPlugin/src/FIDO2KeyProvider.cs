using System.Windows.Forms;
using KeePassLib.Keys;
using KeePassLib.Utility;

namespace KeePassFIDO2
{
	public class FIDO2KeyProvider : KeyProvider
	{
		public override byte[] GetKey(KeyProviderQueryContext ctx)
		{
			if (ctx.CreatingNewKey)
			{
				MessageService.ShowWarning("KeePassFIDO2 can't be used to create new keys.");
				return null;
			}

			byte[] pinBytes;

			// request device PIN via a new form
			using (var pinForm = new PinForm())
			{
				if (pinForm.ShowDialog() != DialogResult.OK)
				{
					return null;
				}

				pinBytes = pinForm.Pin;
			}

			// max ley length (spec)
			if (pinBytes.Length > 63)
			{
				MemUtil.ZeroByteArray(pinBytes);
				return null;
			}

			var result = DeviceCommunicator.ExecuteGet(pinBytes);

			if (result.ExitCode != 0) {
				// zero out all sensitive data
				result.Clear();
				MessageService.ShowWarning($"Device communicator exited with code {result.ExitCode}.");
				return null;
			}

			var keyBytes = result.ReadKey();

			// zero out all sensitive data
			result.Clear();

			return keyBytes;
		}

		public override string Name => "FIDO2 Key Provider";
		public override bool SecureDesktopCompatible => false; // secure desktop doesn't seem to work with the UAC prompt for DeviceCommunicator
		public override bool DirectKey => true; // we capture the already hashed key; this prevents hashing it second time
	}
}
