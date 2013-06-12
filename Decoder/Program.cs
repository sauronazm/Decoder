using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Decoder {
    class Program {
        static void Main(string[] args) {
            Encoding badEncoding = Encoding.GetEncodings().First(x => x.Name == "cp866").GetEncoding();
            Encoding goodEncoding = Encoding.UTF8;
            String rootDirectory = @"D:\\tmp\recovery\foto\";
            if (!Directory.Exists(rootDirectory)) {
                throw new DirectoryNotFoundException("Directory " + rootDirectory + " not found!");
            }
            var badChars = GetBadChars(badEncoding, goodEncoding);
            DoWork(rootDirectory, badChars, badEncoding, goodEncoding, SearchOption.AllDirectories);
        }

        private static string GetBadChars(Encoding badEncoding, Encoding goodEncoding) {
            string badChars = "";
            for (int i = 1040; i < 1104; i++) {
                badChars += Convert.ToChar(i);
            }
            badChars = badEncoding.GetString(goodEncoding.GetBytes(badChars.ToCharArray()));
            return badChars;
        }

        private static void DoWork(string targetDirectory, string badChars, Encoding badEncoding, Encoding goodEncoding, SearchOption searchMode) {
            var filesAndFolders = Directory.GetFileSystemEntries(targetDirectory, "*", searchMode);
            if (filesAndFolders != null) {
                FileInfo f;
                DirectoryInfo d;
                foreach (var objectPath in filesAndFolders) {
                    if (ObjectNameInBadEcoding(objectPath, badChars)) {
                        if (Directory.Exists(objectPath)) {
                            d = new DirectoryInfo(objectPath);
                            var newName = Path.Combine(d.Parent.FullName, GetFixedName(d.Name, badEncoding, goodEncoding));
                            Directory.Move(d.FullName, newName);
                        }
                        if (File.Exists(objectPath)) {
                            f = new FileInfo(objectPath);
                            var newName = Path.Combine(f.Directory.FullName, GetFixedName(f.Name, badEncoding, goodEncoding));
                            File.Move(f.FullName, newName);
                        }
                    }
                }
            }
        }

        private static bool ObjectNameInBadEcoding(string name, string badChars) {
            bool result = false;
            foreach (var chr in badChars) {
                try {
                    if (name.IndexOf(chr.ToString() + badChars.ElementAt(badChars.IndexOf(chr) + 1).ToString()) > -1) {
                        result = true;
                        break;
                    }
                }
                catch (Exception) { }
            }
            return result;
        }

        private static string GetFixedName(string name, Encoding badEncoding, Encoding goodEncoding) {
            string result = "";
            result = goodEncoding.GetString(badEncoding.GetBytes(name.ToCharArray()));
            return result;
        }
    }
}
