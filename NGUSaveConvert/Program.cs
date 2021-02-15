using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace NGUSaveConvert
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var savedir = args.Length > 0 ? args[0] : Environment.ExpandEnvironmentVariables("%appdata%\\..\\locallow\\ngu industries\\ngu idle\\");
            var d = new DirectoryInfo(savedir);
            Directory.CreateDirectory(savedir + "/json");
            foreach (var savefile in d.GetFiles("*.txt"))
            {
                Console.WriteLine("Converting " + savefile.Name);
                var data = ReadSaveData(savefile.FullName);

                if (data == null)
                    return;

                File.WriteAllText(savefile.DirectoryName +"/json/"+ savefile.Name + ".json", data);
            }
        }

        private static string ReadSaveData(string infile = "")
        {
            var path = infile;
            
            if (!File.Exists(path))
            {
                Console.WriteLine("Bad filepath");
                return null;
            }

            try
            {
                var content = File.ReadAllText(path);
                var data = DeserializeBase64<SaveData>(content);
                var checksum = GetChecksum(data.playerData);
                if (checksum != data.checksum)
                {
                    Console.WriteLine("Bad checksum");
                    return null;
                }

                var playerData = DeserializeBase64<PlayerData>(data.playerData);

                var jsonData = JsonConvert.SerializeObject(playerData);
                return jsonData;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        //Code sort of taken from ngu-save-analyzer
        private static T DeserializeBase64<T>(string base64Data)
        {
            var bytes = Convert.FromBase64String(base64Data);
            var formatter = new BinaryFormatter();

            using (var memoryStream = new MemoryStream(bytes))
            {
                return (T)formatter.Deserialize(memoryStream);
            }
        }

        private static string GetChecksum(string data)
        {
            var md5 = new MD5CryptoServiceProvider();

            return Convert.ToBase64String(md5.ComputeHash(Convert.FromBase64String(data)));
        }
    }
}
