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
            //if there are no arguments given, print the usage of the app and don't continue
            if (args.Length < 1)
            {
                PrintUsage();
                return; //dont continue with the rest of the program.
            }

            //if there is no oodle dll in the current app directory, then don't continue.
            //(we will error out anyway but we can tell the user whats wrong and what to do to fix it)
            if (CheckForOodle() == false)
            {
                Console.WriteLine("'oo2core_9_win64.dll' does not exist in the current application path!!!");
                Console.WriteLine("To fix this, copy the dll from your uncharted game directory to the same directory that UnPSARC.exe is stored.");
                return; //dont continue with the rest of the program.
            }

            //some quality of life checks
            bool isFile = File.Exists(args[0]); //if input path is a single file
            bool isDirectory = Directory.Exists(args[0]); //if input path is a directory
            string outputDirectory = null; //the final output directory for the files.

            //if there is a second argument specified, its the output directory.
            if (args.Length > 1)
            {
                //if the directory exists or is valid, then assign it
                if (Directory.Exists(args[1]))
                    outputDirectory = args[1];
                else
                {
                    //since there is a second argument, but there isn't an valid output directory specified we'll just stop the program.
                    Console.WriteLine("Output directory does not exist, or is not a valid path! Make sure your path is in quotation marks.");
                    PrintUsage();
                    return; //dont continue with the rest of the program.
                }
            }

            //if the input path is a single file (SINGLE MODE)
            if (isFile && !isDirectory)
            {
                //if the input file has the .psarc extension
                if (Path.GetExtension(args[0]) == archiveExtension)
                {
                    //if there is no output directory specified (it will be null), then build a custom one.
                    if (outputDirectory == null)
                    {
                        //build the directory on the same folder where the input file is stored.
                        string TempDir = Path.GetDirectoryName(args[0]);
                        if (TempDir == "") TempDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                        string customOutputDirectory = TempDir + "\\" + Path.GetFileNameWithoutExtension(args[0]) + "_Unpacked";

                        //if it doesn't exist then build the directory on the disk.
                        if (Directory.Exists(customOutputDirectory) == false)
                            Directory.CreateDirectory(customOutputDirectory);

                        //unpack the file into the custom output directory
                        UnpackArchiveFile(args[0], customOutputDirectory);
                    }
                    else
                    {
                        //unpack the file into the user specified output directory
                        UnpackArchiveFile(args[0], outputDirectory);
                    }
                }
            }
            //if the input path is a directory (BATCH MODE)
            else if (isDirectory && !isFile)
            {
                //loop through each of the files in the directory
                foreach (string file in Directory.GetFiles(args[0]))
                {
                    //if the input file has the .psarc extension
                    if (Path.GetExtension(file) == archiveExtension)
                    {
                        //if there is no output directory specified (it will be null), then build a custom one.
                        if (outputDirectory == null)
                        {
                            //build the directory on the same folder where the input files are stored.
                            string customOutputDirectory = args[0] + "\\" + Path.GetFileNameWithoutExtension(file) + "_Unpacked";

                            //if it doesn't exist then build the directory on the disk.
                            if (Directory.Exists(customOutputDirectory) == false)
                                Directory.CreateDirectory(customOutputDirectory);

                            //unpack the file into the custom output directory
                            UnpackArchiveFile(file, customOutputDirectory);
                        }
                        else
                        {
                            //build the directory on the same folder where the input file is stored.
                            string localOutputDirectory = outputDirectory + "\\" + Path.GetFileNameWithoutExtension(file);

                            //unpack the file into the user specified output directory into our local output folder for this archive file.
                            UnpackArchiveFile(file, localOutputDirectory);
                        }
                    }
                }
            }
            else //user didn't specify or have avalid input argument path, so just print the usage of the program to remind them...
            {
                //instead of printing just the usage, tell them what they did wrong for good ux.
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