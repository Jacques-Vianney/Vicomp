using System;
using System.Text;
using System.IO.Compression;
using System.IO;
using System.Security.Cryptography;

namespace Vicomp
{
    class KeySalt
    {
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }

        public KeySalt(byte[] Key,byte[] IV)
        {
            this.Key = Key;
            this.IV = IV;
        }
    }


    class Vicomp
    {
        static readonly byte[] Key = { 23, 6, 104, 230, 16, 104, 230, 250, 104, 213, 16, 104, 240, 16, 104, 213 };

        static readonly byte[] IV = { 0, 36,54, 16, 104,73,35,22,43,3,96,69,2,1,3,7 };

        static void Main(string[] args)
        {
            ArgsAndCrypt(args);

        }

        private static byte[] AES_Encrypt(byte[] clearBytes, byte[] passBytes, byte[] saltBytes)
        {
            byte[] encryptedBytes = null;

            var key = new Rfc2898DeriveBytes(passBytes, saltBytes, 32768);

            using (Aes aes = new AesManaged())
            {
                aes.KeySize = 256;
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(),
                           CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }
            return encryptedBytes;
        }

        private static byte[] AES_Decrypt(byte[] cryptBytes,
            byte[] passBytes, byte[] saltBytes)
        {
            byte[] clearBytes = null;

            var key = new Rfc2898DeriveBytes(passBytes, saltBytes, 32768);

            using (Aes aes = new AesManaged())
            {
                aes.KeySize = 256;
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(),
                        CryptoStreamMode.Write))
                    {
                        cs.Write(cryptBytes, 0, cryptBytes.Length);
                        cs.Close();
                    }
                    clearBytes = ms.ToArray();
                }
            }
            return clearBytes;
        }

        public static byte[] VCompress(byte[] data)
        {

            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();

        }

        public static byte[] VDecompress(byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

        static void  ArgsAndCrypt(string[] arg)
        {


            if (arg.Length == 0)
            {
                Console.WriteLine("Type vicomp -help for help.");
                Environment.Exit(1);
            }
            byte[] file;
            byte[] stream;
            byte[] FinalFile;
            byte[] key;
            key = Vicomp.Key;
            string name = "";

            if(arg.Length > 0)
            {
                name = Path.GetFileNameWithoutExtension(arg[1]);

            }

            for (int i = 0; i < arg.Length; i++)
            {
                if (arg[i] == "-o")
                {
                    name = arg[i +1];
                }
            }

            if (arg[0] == "-help")
            {
                Console.WriteLine(@"
                       ___________Compress___________

                -help                      show this text
                -cmp                       compress file    (with native key)
                -dcmp                      decompress file  (with native key)
                -cmp  -key  (-o filename)  compress file     with personal key 
                -dcmp -key                 decompress file   with personal key");
                Environment.Exit(1);
            }

            else if (arg[0] == "-cmp")
            {

                if (arg.Length > 2)
                {
                    if (arg[2] == "-key")
                    {
                        key = Encoding.ASCII.GetBytes(arg[3]);
                    }
                }
               file= File.ReadAllBytes(arg[1]);
               stream = VCompress(file);
               FinalFile = AES_Encrypt(stream,key, Vicomp.IV);
               ByteArrayToFile(name + ".via", FinalFile);

            }
            else if (arg[0] == "-dcmp")
            {

                if (arg.Length > 2)
                {
                    if (arg[2] == "-key")
                    {
                        key = Encoding.ASCII.GetBytes(arg[3]);

                    }
                }
                file = File.ReadAllBytes(arg[1]);
                stream = AES_Decrypt(file,key, Vicomp.IV);
                FinalFile = VDecompress(stream);
                ByteArrayToFile("Decompressed" +".via", FinalFile);

            }

        }

        public static bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }

    }



}
