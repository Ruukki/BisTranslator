using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BisTranslator.external
{
    public static class ZipExtractor
    {
        public static void ExtractZipFile(string zipFilePath, string extractPath)
        {
            if (!File.Exists(zipFilePath))
            {
                throw new FileNotFoundException("The specified zip file does not exist.", zipFilePath);
            }

            try
            {
                // Ensure the extraction path exists
                if (!Directory.Exists(extractPath))
                {
                    Directory.CreateDirectory(extractPath);
                }

                // Extract the zip file to the specified directory
                ZipFile.ExtractToDirectory(zipFilePath, extractPath);
                Console.WriteLine("Files extracted successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during extraction: {ex.Message}");
            }
        }
    }
}
