using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnPSARC.Helpers;

namespace UnPSARC
{
    internal class Program
    {
        private static string archiveExtension = ".psarc";
        public static bool oodleExist = false;

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("Unpack: Unpsarc.exe <psarc path> [destination folder]");
            Console.WriteLine(" Pack : Unpsarc.exe <content folder> [archive filename to create]");
            Console.WriteLine("\nPress any key to exit");
            Console.ReadKey();
        }

        private static bool CheckForOodle()
        {
            string currentApplicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string currentApplicationDirectory = Path.GetDirectoryName(currentApplicationPath);
            string oodleLocation = Path.Combine(currentApplicationDirectory + "oo2core_9_win64.dll");

            return File.Exists(oodleLocation);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("UnPSARC (Archive Tool For PlayStation Archive files 'PSARC')");
            Console.WriteLine("By NoobInCoding");
            Console.WriteLine("https://github.com/rm-NoobInCoding/UnPSARC");
            Console.WriteLine("");

            if (args.Length < 1)
            {
                PrintUsage();
                return;
            }

            oodleExist = CheckForOodle();

            bool isFile = File.Exists(args[0]);
            bool isDirectory = Directory.Exists(args[0]);
            string outputName = null;

            if (args.Length > 1)
            {
                if ((CommendHelper.IsFullPath(args[1]) && CommendHelper.IsValidPath(args[1])) || !CommendHelper.IsFullPath(args[1]))
                    outputName = args[1];
                else
                {
                    Console.WriteLine("Output directory is not a valid path! Make sure your path is in quotation marks.");
                    PrintUsage();
                    return;
                }
            }
            if (isFile && !isDirectory)
            {
                if (Path.GetExtension(args[0]) == archiveExtension)
                {
                    if (outputName == null)
                    {
                        string TempDir = Path.GetDirectoryName(args[0]);
                        if (TempDir == "") TempDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                        string customOutputDirectory = Path.GetFileNameWithoutExtension(args[0]) + "_Unpacked";
                        // string customOutputDirectory = Path.GetFileNameWithoutExtension(args[0]); // Extract to same folder instead
                        if (Directory.Exists(customOutputDirectory) == false)
                            Directory.CreateDirectory(customOutputDirectory);
                        UnpackArchiveFile(args[0], customOutputDirectory);
                    }
                    else
                    {
                        UnpackArchiveFile(args[0], outputName);
                    }
                }
            }
            else if (isDirectory && !isFile)
            {
                if (outputName == null)
                {
                    Console.WriteLine($"Packing {args[0]} to {Path.GetFileNameWithoutExtension(args[0]) + ".psarc"}");
                    PackArchiveFile(args[0], "../" + Path.GetFileNameWithoutExtension(args[0]) + ".psarc");
                }
                else
                {
                    if (!CommendHelper.IsFullPath(outputName))
                    {
                        if (outputName.StartsWith(Path.DirectorySeparatorChar.ToString())) outputName = outputName.Remove(0, 1);
                        outputName = Path.Combine(Environment.CurrentDirectory, outputName);
                    }
                    Console.WriteLine($"Packing {args[0]} to {outputName}");
                    PackArchiveFile(args[0], outputName);

                }

            }
            else
            {
                Console.WriteLine("Input path argument is not a valid file/directory, or does not exist! (Make sure your path is in quotation marks)");
                PrintUsage();
            }
        }

        private static void PackArchiveFile(string contentFolderPath, string outputFilename)
        {

            File.WriteAllText(Path.Combine(contentFolderPath, "Filenames.txt"), MakeFileNameTable(contentFolderPath));
            File.WriteAllBytes(Path.Combine(contentFolderPath, "r.exe"), Packer.psarc);
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = Path.Combine(contentFolderPath, "r.exe"),
                Arguments = $"create -a --skip-missing-files --inputfile=filenames.txt --output=\"{outputFilename}\" -N -y",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = contentFolderPath,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using (var process = new Process { StartInfo = psi })
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        Console.WriteLine(e.Data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        Console.Error.WriteLine(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                File.Delete(Path.Combine(contentFolderPath, "r.exe"));
                File.Delete(Path.Combine(contentFolderPath, "Filenames.txt"));
            }

        }

        private static string MakeFileNameTable(string contentFolderPath)
        {
            List<string> files = new List<string>();
            foreach (string fname in Directory.GetFiles(contentFolderPath, "*.*", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(fname) == "Filenames.txt")
                    continue;
                string _ = fname.Replace(contentFolderPath + Path.DirectorySeparatorChar.ToString(), "").Replace(Path.DirectorySeparatorChar.ToString(), "/");
                files.Add(_);
            }
            return string.Join("\n", files);
        }

        private static void UnpackArchiveFile(string inputPath, string outputDirectory)
        {
            Console.WriteLine("Unpacking {0}...", Path.GetFileName(inputPath));

            Stream R = File.OpenRead(inputPath);
            Archive.Unpack(R, outputDirectory);
            R.Close();
        }
    }
}
