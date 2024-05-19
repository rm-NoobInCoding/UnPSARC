using System;
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
            string oodleLocation = currentApplicationDirectory + "\\oo2core_9_win64.dll";

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
                        string customOutputDirectory = TempDir + "\\" + Path.GetFileNameWithoutExtension(args[0]) + "_Unpacked";
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
                if (File.Exists(Path.Combine(args[0], "Filenames.txt")))
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
                            if (outputName.StartsWith("\\")) outputName = outputName.Remove(0, 1);
                            outputName = Path.Combine(Environment.CurrentDirectory, outputName);
                        }
                        Console.WriteLine($"Packing {args[0]} to {outputName}");
                        PackArchiveFile(args[0], outputName);

                    }
                }
                else
                {
                    Console.WriteLine("Repack folder is not correct. (The folder must contain filenames.txt file)");
                    PrintUsage();
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
            File.WriteAllText(Path.Combine(contentFolderPath, "Filenames.txt"), File.ReadAllText(Path.Combine(contentFolderPath, "Filenames.txt")).Replace("\0", "\n").Replace("\n/", "\n"));
            File.WriteAllBytes(Path.Combine(contentFolderPath, "r.exe"), Packer.psarc);
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = Path.Combine(contentFolderPath, "r.exe"),
                Arguments = $"create -a --skip-missing-files --inputfile=filenames.txt --output=\"{outputFilename}\" -N -y",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = contentFolderPath,
                UseShellExecute = false,
                CreateNoWindow = true
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
            }
            File.Delete("r.exe");


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