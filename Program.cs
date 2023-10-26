using System;
using System.IO;

namespace UnPSARC
{
    internal class Program
    {
        private static string archiveExtension = ".psarc";

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("Unpsarc.exe <psarc path/folder> [destination folder]");
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
            Console.WriteLine("UnPSARC (PSARC Archive Tool For Uncharted4 PC)");
            Console.WriteLine("By NoobInCoding");
            Console.WriteLine("https://github.com/rm-NoobInCoding/UnPSARC");
            Console.WriteLine("");

            if (args.Length < 1)
            {
                PrintUsage();
                return;
            }

            if (CheckForOodle() == false)
            {
                Console.WriteLine("'oo2core_9_win64.dll' does not exist in the current application path!!!");
                Console.WriteLine("To fix this, copy the dll from your uncharted game directory to the same directory that UnPSARC.exe is stored.");
                return;
            }

            bool isFile = File.Exists(args[0]);
            bool isDirectory = Directory.Exists(args[0]);
            string outputDirectory = null;

            if (args.Length > 1)
            {
                if (Directory.Exists(args[1]))
                    outputDirectory = args[1];
                else
                {
                    Console.WriteLine("Output directory does not exist, or is not a valid path! Make sure your path is in quotation marks.");
                    PrintUsage();
                    return;
                }
            }
            if (isFile && !isDirectory)
            {
                if (Path.GetExtension(args[0]) == archiveExtension)
                {
                    if (outputDirectory == null)
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
                        UnpackArchiveFile(args[0], outputDirectory);
                    }
                }
            }
            else if (isDirectory && !isFile)
            {
                foreach (string file in Directory.GetFiles(args[0]))
                {
                    if (Path.GetExtension(file) == archiveExtension)
                    {
                        if (outputDirectory == null)
                        {
                            string customOutputDirectory = args[0] + "\\" + Path.GetFileNameWithoutExtension(file) + "_Unpacked";
                            if (Directory.Exists(customOutputDirectory) == false)
                                Directory.CreateDirectory(customOutputDirectory);
                            UnpackArchiveFile(file, customOutputDirectory);
                        }
                        else
                        {
                            string localOutputDirectory = outputDirectory + "\\" + Path.GetFileNameWithoutExtension(file);
                            UnpackArchiveFile(file, localOutputDirectory);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Input path argument is not a valid file/directory, or does not exist! (Make sure your path is in quotation marks)");
                PrintUsage();
            }
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