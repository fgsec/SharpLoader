using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Text;

namespace Sharploader {

	public class Encryption {

		public static string comm_key = null;
		public static string comm_iv = null;
		public static string comm_guid = null;

		public Encryption(string key, string iv) {
			comm_key = key;
			comm_iv = iv;
		}

		public static string encodeBase64(string value) {
			var plt = System.Text.Encoding.UTF8.GetBytes(value);
			return System.Convert.ToBase64String(plt);
		}

		public static string decodeBase64(string value) {
			var plt = System.Convert.FromBase64String(value);
			return System.Text.Encoding.UTF8.GetString(plt);
		}

		public static void generateKey() {

			using (Aes myAes = Aes.Create()) {
				try {
					var IV = Convert.ToBase64String(myAes.IV);
					var key = Convert.ToBase64String(myAes.Key);
					comm_key = key;
					comm_iv = IV;
				} catch (Exception ex) {
					Console.WriteLine("Error during key creation:" + ex.Message);
				}
			}

		}


		public string encrypt(string block) {

			try {
				byte[] key = Convert.FromBase64String(comm_key);
				byte[] IV = Convert.FromBase64String(comm_iv);
				byte[] encrypted = EncryptStringToBytes_Aes(block, key, IV);
				return Convert.ToBase64String(encrypted);
			} catch (Exception ex) {
				Console.Write("Error trying to encrypt: " + ex);
			}

			return null;
		}


		public string decrypt(string block) {

			string decoded = null;
			try { 
				byte[] encryptedString = Convert.FromBase64String(block);
				byte[] key = Convert.FromBase64String(comm_key);
				byte[] IV = Convert.FromBase64String(comm_iv);
				decoded = DecryptStringFromBytes_Aes(encryptedString, key, IV);
			} catch (Exception ex) {
				Console.Write("Error trying to decrypt: " + ex);
			}

			return decoded;
		}

		static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV) {
			if (plainText == null || plainText.Length <= 0)
				throw new ArgumentNullException("plainText");
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException("Key");
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException("IV");
			byte[] encrypted;
			using (Aes aesAlg = Aes.Create()) {
				aesAlg.Key = Key;
				aesAlg.IV = IV;
				ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
				using (MemoryStream msEncrypt = new MemoryStream()) {
					using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
						using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {
							swEncrypt.Write(plainText);
						}
						encrypted = msEncrypt.ToArray();
					}
				}
			}
			return encrypted;
		}

		static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV) {
			if (cipherText == null || cipherText.Length <= 0)
				throw new ArgumentNullException("cipherText");
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException("Key");
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException("IV");
			string plaintext = null;
			using (Aes aesAlg = Aes.Create()) {
				aesAlg.Key = Key;
				aesAlg.IV = IV;
				ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
				using (MemoryStream msDecrypt = new MemoryStream(cipherText)) {
					using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
						using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {
							plaintext = srDecrypt.ReadToEnd();
						}
					}
				}
			}
			return plaintext;
		}



	}
}