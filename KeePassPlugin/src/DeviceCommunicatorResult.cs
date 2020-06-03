using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using KeePassLib.Utility;

namespace KeePassFIDO2
{
	public class DeviceCommunicatorResult
	{
		private readonly Process process;
		private readonly IntPtr sharedBufferPtr;
		private readonly DeviceCommunicatorData deviceCommunicatorData;

		public DeviceCommunicatorResult(Process process, IntPtr sharedBufferPtr, DeviceCommunicatorData deviceCommunicatorData)
		{
			this.process = process;
			this.sharedBufferPtr = sharedBufferPtr;
			this.deviceCommunicatorData = deviceCommunicatorData;
		}

		public void Clear()
		{
			deviceCommunicatorData.Clear();
			Marshal.Copy(deviceCommunicatorData.Buffer, 0, sharedBufferPtr, deviceCommunicatorData.Buffer.Length);
		}

		public byte[] ReadKey()
		{
			// copy base64 bytes from the shared buffer
			var base64KeyBytes = new byte[44];
			Marshal.Copy(sharedBufferPtr + 32, base64KeyBytes, 0, 44);

			// covert to chars
			var base64KeyChars = Encoding.ASCII.GetChars(base64KeyBytes);

			// decode from base64 to raw key bytes
			var keyBytes = Convert.FromBase64CharArray(base64KeyChars, 0, base64KeyChars.Length);

			MemUtil.ZeroByteArray(base64KeyBytes);
			MemUtil.ZeroArray(base64KeyChars);

			return keyBytes;
		}

		public int ExitCode => process.ExitCode;
	}
}
