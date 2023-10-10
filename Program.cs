using System;
using System.IO.Ports;
using System.Runtime.CompilerServices;

namespace RT_890_Flasher_CLI {
	internal class Program {
		static void Usage()
		{
			string[] Parts = Environment.CommandLine.Split(new char[] { System.IO.Path.DirectorySeparatorChar });
			string Exe = Parts[Parts.Length - 1];
			Exe = Exe.Trim();
			Console.WriteLine("Usage:");
			Console.WriteLine("\t" + Exe + " -l                        List available COM ports");
			Console.WriteLine("\t" + Exe + " -p COMx -f firmware.bin   Flash a file");
		}

		static void Main(string[] args)
		{
			Console.WriteLine("RT-890-Flash-CLI (c) Copyright 2003 Dual Tachyon\n");
			switch (args.Length) {
			case 1:
				if (args[0] != "-l") {
					Usage();
					break;
				}
				var Ports = SerialPort.GetPortNames();
				Console.Write("Ports available:");
				foreach (var Port in Ports) {
					Console.Write(" " + Port);
				}
				Console.WriteLine();
				break;
			case 4:
				if (args[0] != "-p") {
					Usage();
					break;
				}
				if (args[2] != "-f") {
					Usage();
					break;
				}

				byte[] Firmware;
				try {
					Firmware = System.IO.File.ReadAllBytes(args[3]);
				} catch {
					Console.WriteLine("Failed to read file!");
					break;
				}

				RT_890_UART RT = new RT_890_UART();
				try {
					RT.Open(args[1]);
				} catch {
					Console.WriteLine("Failed to open COM port!");
					break;
				}

				try {
					if (!RT.IsBootLoaderMode()) {
						Console.WriteLine("RT-890 is not in bootloader mode!");
						break;
					}
				} catch {
					Console.WriteLine("Timeout error! Is the radio in bootloader mode?");
					break;
				}
				try {
					if (!RT.Command_EraseFlash()) {
						Console.WriteLine("Failed to erase flash!");
						break;
					}
				} catch (Exception Ex) {
					Console.WriteLine("\rUnexpected failure erasing flash! Error: ", Ex.Message);
					break;
				}

				try {
					ushort i;

					for (i = 0; i < Firmware.Length; i += 128) {
						Console.Write("\rFlashing at 0x" + i.ToString("X4"));
						Console.Out.Flush();
						if (!RT.Command_WriteFlash(i, Firmware)) {
							Console.WriteLine("\rFailed to flash at 0x" + i.ToString("X4") + "!");
							Console.Out.Flush();
							break;
						}
					}
					if (i == Firmware.Length) {
						Console.WriteLine("\rFlashing complete!");
					}
				} catch (Exception Ex) {
					Console.WriteLine("\rUnexpected failure writing to flash! Error: ", Ex.Message);
				}
				Console.WriteLine();
				RT.Close();
				break;

			default:
				Usage();
				break;
			}
		}
	}
}
