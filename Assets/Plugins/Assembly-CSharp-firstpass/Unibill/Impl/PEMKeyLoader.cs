using System;
using System.IO;
using System.Security.Cryptography;

namespace Unibill.Impl
{
	public class PEMKeyLoader
	{
		private static byte[] SeqOID = new byte[15]
		{
			48, 13, 6, 9, 42, 134, 72, 134, 247, 13,
			1, 1, 1, 5, 0
		};

		private static bool CompareBytearrays(byte[] a, byte[] b)
		{
			if (a.Length != b.Length)
			{
				return false;
			}
			int num = 0;
			foreach (byte b2 in a)
			{
				if (b2 != b[num])
				{
					return false;
				}
				num++;
			}
			return true;
		}

		public static RSACryptoServiceProvider CryptoServiceProviderFromPublicKeyInfo(byte[] x509key)
		{
			byte[] array = new byte[15];
			if (x509key == null || x509key.Length == 0)
			{
				return null;
			}
			MemoryStream input = new MemoryStream(x509key);
			BinaryReader binaryReader = new BinaryReader(input);
			byte b = 0;
			ushort num = 0;
			try
			{
				switch (binaryReader.ReadUInt16())
				{
				case 33072:
					binaryReader.ReadByte();
					break;
				case 33328:
					binaryReader.ReadInt16();
					break;
				default:
					return null;
				}
				array = binaryReader.ReadBytes(15);
				if (!CompareBytearrays(array, SeqOID))
				{
					return null;
				}
				switch (binaryReader.ReadUInt16())
				{
				case 33027:
					binaryReader.ReadByte();
					break;
				case 33283:
					binaryReader.ReadInt16();
					break;
				default:
					return null;
				}
				if (binaryReader.ReadByte() != 0)
				{
					return null;
				}
				switch (binaryReader.ReadUInt16())
				{
				case 33072:
					binaryReader.ReadByte();
					break;
				case 33328:
					binaryReader.ReadInt16();
					break;
				default:
					return null;
				}
				num = binaryReader.ReadUInt16();
				byte b2 = 0;
				byte b3 = 0;
				switch (num)
				{
				case 33026:
					b2 = binaryReader.ReadByte();
					break;
				case 33282:
					b3 = binaryReader.ReadByte();
					b2 = binaryReader.ReadByte();
					break;
				default:
					return null;
				}
				byte[] value = new byte[4] { b2, b3, 0, 0 };
				int num2 = BitConverter.ToInt32(value, 0);
				if (binaryReader.PeekChar() == 0)
				{
					binaryReader.ReadByte();
					num2--;
				}
				byte[] modulus = binaryReader.ReadBytes(num2);
				if (binaryReader.ReadByte() != 2)
				{
					return null;
				}
				int count = binaryReader.ReadByte();
				byte[] exponent = binaryReader.ReadBytes(count);
				RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
				rSACryptoServiceProvider.ImportParameters(new RSAParameters
				{
					Modulus = modulus,
					Exponent = exponent
				});
				return rSACryptoServiceProvider;
			}
			finally
			{
				binaryReader.Close();
			}
		}

		public static RSACryptoServiceProvider CryptoServiceProviderFromPublicKeyInfo(string base64EncodedKey)
		{
			try
			{
				return CryptoServiceProviderFromPublicKeyInfo(Convert.FromBase64String(base64EncodedKey));
			}
			catch (FormatException)
			{
			}
			return null;
		}
	}
}
