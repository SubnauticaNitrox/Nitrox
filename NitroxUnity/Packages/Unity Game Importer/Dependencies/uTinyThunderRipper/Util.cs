using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using uTinyRipper;
using uTinyRipper.Classes.Misc;

namespace ThunderKit.uTinyRipper
{
	public class Util
	{
		public static int ComputeFileID(string ns, string name)
		{
			using (HashAlgorithm hash = new MD4())
			{
				byte[] idBytes = Encoding.UTF8.GetBytes($"s\0\0\0{ns}{name}");
				byte[] hashed = hash.ComputeHash(idBytes);

				int result = 0;

				for (int i = 3; i >= 0; --i)
				{
					result <<= 8;
					result |= hashed[i];
				}

				return result;
			}
		}
		public static Guid GetAssemblyHashGuid(string assemblyPath)
		{
			using (var md5 = MD5.Create())
			{
				string shortName = Path.GetFileNameWithoutExtension(assemblyPath);
				byte[] shortNameBytes = Encoding.Default.GetBytes(shortName);
				var shortNameHash = md5.ComputeHash(shortNameBytes);
				return new Guid(shortNameHash);
			}
		}
		public static string GetAssemblyHash(string assemblyPath)
		{
			Guid guid = GetAssemblyHashGuid(assemblyPath);
			var cleanedGuid = guid.ToString().ToLower().Replace("-", "");
			return cleanedGuid;
		}
		public static string FormatAssemblyHash(Guid guid)
		{
			var cleanedGuid = guid.ToString().ToLower().Replace("-", "");
			return cleanedGuid;
		}

		//public static void SetGUID(global::uTinyRipper.Classes.Object asset, byte[] guid)
		//{
		//	var swapped = new byte[guid.Length];
		//	for (int i = 0; i < guid.Length; i++)
		//	{
		//		var x = guid[i];
		//		swapped[i] = (byte)((x & 0x0F) << 4 | (x & 0xF0) >> 4);
		//	}
		//	asset.AssetInfo.GUID = new UnityGUID(swapped);
		//}
	}
}