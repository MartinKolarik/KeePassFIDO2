using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using KeePassLib.Utility;

namespace KeePassFIDO2
{
	public static class DeviceCommunicator
	{
		public static DeviceCommunicatorResult ExecuteCreate(byte[] pinBytes, byte[] keyBytes)
		{
			// keyBytes will be stored as a null-terminated string, so we convert it to base64 here.
			// Each 3 byte sequence in the source data becomes a 4 byte sequence in the character array.
			// https://docs.microsoft.com/en-us/dotnet/api/system.convert.tobase64chararray
			var keyCharsLength = (long) (4.0d / 3.0d * keyBytes.Length);

			// the output length is always a multiple of 4
			if (keyCharsLength % 4 != 0) {
				keyCharsLength += 4 - keyCharsLength % 4;
			}

			var base64Chars = new char[keyCharsLength];
			Convert.ToBase64CharArray(keyBytes, 0, keyBytes.Length, base64Chars, 0);
			var base64Bytes = Encoding.ASCII.GetBytes(base64Chars);

			// prepare a new shared buffer
			var deviceCommunicatorData = new DeviceCommunicatorData(pinBytes);
			deviceCommunicatorData.SetMasterKey(base64Bytes);

			var sharedBufferPtr = Marshal.AllocCoTaskMem(deviceCommunicatorData.Buffer.Length);
			Marshal.Copy(deviceCommunicatorData.Buffer, 0, sharedBufferPtr, deviceCommunicatorData.Buffer.Length);

			// clear all sensitive variables
			MemUtil.ZeroByteArray(pinBytes);
			MemUtil.ZeroByteArray(keyBytes);
			MemUtil.ZeroByteArray(base64Bytes);
			MemUtil.ZeroArray(base64Chars);

			var process = Run($"{Process.GetCurrentProcess().Id} create {sharedBufferPtr.ToString()}");
			return new DeviceCommunicatorResult(process, sharedBufferPtr, deviceCommunicatorData);
		}

		public static DeviceCommunicatorResult ExecuteGet(byte[] pinBytes)
		{
			// prepare a new shared buffer
			var deviceCommunicatorData = new DeviceCommunicatorData(pinBytes);
			var sharedBufferPtr = Marshal.AllocCoTaskMem(deviceCommunicatorData.Buffer.Length);

			// copy the prepared data to the shared buffer location
			Marshal.Copy(deviceCommunicatorData.Buffer, 0, sharedBufferPtr, deviceCommunicatorData.Buffer.Length);

			// clear the PIN parameter
			MemUtil.ZeroByteArray(pinBytes);

			var process = Run($"{Process.GetCurrentProcess().Id} get {sharedBufferPtr.ToString()}");
			return new DeviceCommunicatorResult(process, sharedBufferPtr, deviceCommunicatorData);
		}

		private static Process Run(string arguments)
		{
			var process = new Process
			{
				StartInfo =
				{
					FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DeviceCommunicator.exe"),
					Arguments = arguments,
					UseShellExecute = true,
					Verb = "runas",
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden,
				}
			};

			process.Start();
			process.WaitForExit();

			return process;
		}
	}
}
