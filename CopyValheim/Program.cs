using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CopyValheim
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] validInputs = new string[] { "D1", "D2", "NumPad1", "NumPad2" };
            try
            {
                var directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CopyValheim");
                var filePath = Path.Combine(directoryPath, "CopyValheim.txt");

                var folderExists = Directory.Exists(directoryPath);
                if (!folderExists)
                {
                    Console.WriteLine("Creando directorio de la app en la carpeta documentos...");
                    Directory.CreateDirectory(directoryPath);
                    SetInitValues(filePath);
                }

                var fileExist = File.Exists(filePath);
                if (!fileExist)
                {
                    SetInitValues(filePath);
                }

                var opcionValida = false;
                var opcionElegida = string.Empty;
                do
                {
                    PrintOptions();
                    var opcion = Console.ReadKey().Key;
                    var strOption = opcion.ToString();
                    if (!validInputs.Contains(strOption))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Esa opcion no existe papanatas. Vamos devuelta.");
                        Console.ReadKey();
                        Console.Clear();
                    }
                    else
                    {
                        opcionValida = true;
                        opcionElegida = strOption;
                    }

                } while (!opcionValida);

                Console.Clear();
                bool result = false;
                if (opcionElegida.Equals("D1") || opcionElegida.Equals("NumPad1"))
                {
                    result = ComprimirDatosDelMundo(filePath);
                }
                else
                {
                    result = ReemplazarDatosDelMundo();
                }

                if (result)
                    Console.WriteLine("El proceso termino 10 puntos! Es propicio.");
                else
                    Console.WriteLine("El proceso falló miserablemente. No me la contes.");

                Console.WriteLine("Presine una tecla para cerrar...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error!" + ex.Message);
                Console.WriteLine("El proceso falló miserablemente");
                Console.WriteLine("Presine una tecla para cerrar...");
                Console.ReadKey();
            }
        }

        public static bool ComprimirDatosDelMundo(string filePath)
        {
            var folderPath = GetWordPath();

            Console.WriteLine("Obteniendo archivos del mundo...");
            var valheimFiles = Directory.GetFiles(folderPath);

            Console.WriteLine("Obteniendo archivos del escritorio...");
            var desktopFiles = Directory.GetFiles(GetDesktopPath());

            Console.WriteLine("Borrando archivo anterior...");
            var existingFile = desktopFiles.FirstOrDefault(f => f.Contains("GinkgoBilobaValheim.rar"));
            if (existingFile != null) File.Delete(existingFile);

            var rarFile = Path.Combine(GetDesktopPath(), "GinkgoBilobaValheim.rar");
            var filesCollection = new List<string>();
            filesCollection.AddRange(valheimFiles);
            var winrarExePath = GetRARExePath(filePath);
            Console.WriteLine("Creando RAR del mundo...");
            return CreateRarFiles(rarFile, winrarExePath, filesCollection);
        }

        public static bool ReemplazarDatosDelMundo()
        {
            Console.WriteLine("Chequeando archivo rar del escritorio...");
            var filePath = Path.Combine(GetDesktopPath(), "GinkgoBilobaValheim.rar");
            var exists = File.Exists(filePath);
            if (!exists)
            {
                Console.WriteLine("No hay ningun archivo .rar con el nombre de 'GinkgoBilobaValheim' en el escritorio");
                return false;
            }
            Console.WriteLine("Archivo encontrado");
            try
            {
                Console.WriteLine("Reemplazando archivos del mundo...");
                using (var archive = RarArchive.Open(Path.Combine(GetDesktopPath(), "GinkgoBilobaValheim.rar")))
                {
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        entry.WriteToDirectory(GetWordPath(), new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error! " + ex.Message);
                return false;
            }
        }

        public static void SetInitValues(string copyValheimPath)
        {
            using (StreamWriter sw = File.CreateText(copyValheimPath))
            {
                Console.WriteLine("Ingresar la ruta del directorio donde esta el winrar.exe");
                var winrarExePath = Console.ReadLine();
                sw.WriteLine("WINRAR exe path: " + winrarExePath);
            }
        }

        public static string GetRARExePath(string copyValheimPath)
        {
            using (StreamReader sr = File.OpenText(copyValheimPath))
            {
                Console.WriteLine("Leyendo datos de guardado...");
                string result = string.Empty;
                result = sr.ReadLine();
                result = result.Replace("WINRAR exe path: ", "");
                return result;
            }
        }

        public static string GetWordPath()
        {
            var folderName = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "LocalLow", "IronGate", "Valheim", "worlds");
            return folderName.Replace(@"\Roaming", "");
        }

        public static string GetDesktopPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }

        public static bool CreateRarFiles(string rarFile, string rarExePath, List<string> collectionFiles)
        {
            try
            {
                var files = collectionFiles.Select(file => "\"" + file).ToList();
                var fileList = string.Join("\" ", files);
                fileList += "\"";
                if (rarFile == null) return false;
                var arguments = $"A \"{rarFile}\" {fileList} -ep1 -r";
                var processStartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    ErrorDialog = false,
                    UseShellExecute = true,
                    Arguments = arguments,
                    FileName = rarExePath,
                    CreateNoWindow = false,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };
                var process = System.Diagnostics.Process.Start(processStartInfo);
                process?.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error! " + ex.Message);
                return false;
            }
        }

        public static void PrintOptions()
        {
            Console.WriteLine("Que queres hacer?");
            Console.WriteLine("1. Comprimir datos del mundo");
            Console.WriteLine("2. Reemplazar datos del mundo");
        }
    }
}
