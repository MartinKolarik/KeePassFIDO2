using System.Linq;
using System.Text;
using KeePassLib.Utility;

namespace KeePassFIDO2
{
	/**
	 * Prepares a 256-byte buffer that is passed to the C++ module. The current layout is:
	 *
	 * 000 - 031 - placeholder used for validation
	 * 033       - pin length
	 * 034 - 039 - not used
	 * 040 - 103 - pin bytes
	 * 104 - 135 - master key (used during "create" operation)
	 * 136 - 223 - not used
	 * 224 - 255 - placeholder used for validation
	 *
	 * In case of a "get" operation, the same buffer is used to receive data back from the C++ module.
	 * The response starts at position 32 and contains 44-byte base64 encoded key.
	 */
	public class DeviceCommunicatorData
	{
		private const string Placeholder = "....KeePassFIDO2-placeholder....";

		public DeviceCommunicatorData(byte[] pinBytes)
		{

			Buffer = Encoding.ASCII.GetBytes(string.Concat(Enumerable.Repeat(Placeholder, 8)));
			Buffer[33] = (byte) pinBytes.Length;
			pinBytes.CopyTo(Buffer, 40);
		}

		public void Clear()
		{
			MemUtil.ZeroByteArray(Buffer);
		}

		public void SetMasterKey(byte[] masterKey)
		{
			masterKey.CopyTo(Buffer, 104);
		}

		public byte[] Buffer { get; }
	}
}
