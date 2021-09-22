using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace DawPastrator.Core
{
    public class AES
    {
        private readonly Aes algorithm_;

        /// <summary>
        /// 构造函数，设置基本数据
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="masterPassword">主密码</param>
        public AES(in string userName, in string masterPassword)
        {
            algorithm_ = Aes.Create();
            algorithm_.KeySize = 256;
            algorithm_.Key = masterPassword.PasswordBasedEncrypt(userName); // 主密码迭代加密后的bytes作为算法的密钥
            algorithm_.Mode = CipherMode.CBC;
            algorithm_.Padding = PaddingMode.PKCS7;
            algorithm_.BlockSize = 128;
            algorithm_.GenerateIV();
        }

        /// <summary>
        /// 生成随机区块并加密
        /// </summary>
        /// <param name="plainText">要加密的字符串</param>
        /// <returns>加密后的字节数组</returns>
        public byte[] Encrypt(in string plainText)
        {
            // CBC模式，第一个区块为随机生成的内容，后面接明文
            byte[] bytesForEncrypt = new byte[algorithm_.BlockSize + Encoding.UTF8.GetByteCount(plainText)];

            using (RNGCryptoServiceProvider csp = new RNGCryptoServiceProvider())
                csp.GetBytes(bytesForEncrypt, 0, algorithm_.BlockSize);

            Encoding.UTF8.GetBytes(plainText, 0, plainText.Length, bytesForEncrypt, algorithm_.BlockSize);

            // 加密
            ICryptoTransform encryptor = algorithm_.CreateEncryptor();
            byte[] encryptedData = encryptor.TransformFinalBlock(bytesForEncrypt, 0, bytesForEncrypt.Length);

            return encryptedData;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="bytesForDecrypt">要解密的字节数组</param>
        /// <returns>解密后的字符串</returns>
        public string Decrypt(in byte[] bytesForDecrypt)
        {
            ICryptoTransform decryptor = algorithm_.CreateDecryptor();
            byte[] decryptedData = decryptor.TransformFinalBlock(bytesForDecrypt, 0, bytesForDecrypt.Length);

            // 解密需要跳过前面Blocksize字节的随机数据
            string plainText = Encoding.UTF8.GetString(decryptedData, algorithm_.BlockSize, decryptedData.Length - algorithm_.BlockSize);
            return plainText;
        }
    }

    public class ECDSA
    {
        private readonly ECDsa algorithm_;
        private ECCurve curve_;

        private readonly string masterPassword_;
        private readonly string privateKeyPath_;

        private bool hasKey = false;

        /// <summary>
        /// 初始化构造函数
        /// </summary>
        /// <param name="masterPassword">主密码</param>
        /// <param name="privateKeyPath">私钥路径</param>
        public ECDSA(in string masterPassword, in string privateKeyPath)
        {
            masterPassword_ = masterPassword;
            privateKeyPath_ = privateKeyPath;

            curve_ = ECCurve.CreateFromFriendlyName("secP256k1");
            algorithm_ = ECDsa.Create(curve_);
        }

        /// <summary>
        /// 生成密钥
        /// </summary>
        public void GenerateKey()
        {
            algorithm_.GenerateKey(curve_);
            hasKey = true;
        }

        /// <summary>
        /// 从文件解密并导入密钥
        /// </summary>
        /// <returns>成功返回true，否则false</returns>
        public bool ImportKeyFromFile()
        {
            try
            {
                using FileStream fs = new FileStream(privateKeyPath_, FileMode.Open, FileAccess.Read);
                byte[] encryptedPrivateKey = new byte[fs.Length];
                fs.Read(encryptedPrivateKey, 0, (int)fs.Length);

                algorithm_.ImportEncryptedPkcs8PrivateKey(masterPassword_, encryptedPrivateKey, out _);

                hasKey = true;
                Console.WriteLine("密钥导入成功");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("密钥导入失败，原因：{0}", e);
                return false;
            }
        }

        /// <summary>
        /// 保存加密后的私钥至文件
        /// </summary>
        /// <returns></returns>
        public bool SavePrivateKeyToFile()
        {
            if (!hasKey)
                throw new Exception("密钥未生成或导入");

            PbeParameters pbe = new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 1000);
            byte[] encryptedPrivateKey = algorithm_.ExportEncryptedPkcs8PrivateKey(masterPassword_, pbe);

            try
            {
                using FileStream fs = new FileStream(privateKeyPath_, FileMode.Create, FileAccess.Write);
                fs.Write(encryptedPrivateKey, 0, encryptedPrivateKey.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("私钥保存失败，原因：{0}", e);
                return false;
            }
            Console.WriteLine("私钥保存成功");
            return true;
        }

        /// <summary>
        /// 将字符串签名
        /// </summary>
        /// <param name="text">要签名的字符串</param>
        /// <returns>签名后的base64编码</returns>
        public string Sign(in string text)
        {
            return Convert.ToBase64String(algorithm_.SignData(Encoding.UTF8.GetBytes(text), HashAlgorithmName.SHA256));
        }

        /// <summary>
        /// 验证签名的有效性
        /// </summary>
        /// <param name="text">原文</param>
        /// <param name="signature">原文签名生成的base64字符串</param>
        /// <returns></returns>
        public bool Verify(in string text, in string signature)
        {
            return algorithm_.VerifyData(Encoding.UTF8.GetBytes(text), Convert.FromBase64String(signature), HashAlgorithmName.SHA256);
        }
    }

    public static class CompressExtensions
    {
        /// <summary>
        /// 使用Gzip算法压缩二进制数据
        /// </summary>
        /// <param name="input">要压缩的数据</param>
        /// <returns>压缩后的数据</returns>
        public static byte[] GzipCompress(this byte[] input)
        {
            using MemoryStream compressStream = new MemoryStream();
            using (GZipStream zipStream = new GZipStream(compressStream, CompressionMode.Compress))
                zipStream.Write(input, 0, input.Length);

            byte[] output = compressStream.ToArray();
            Console.WriteLine("压缩率：{0} / {1}", output.Length, input.Length);

            return output;
        }

        /// <summary>
        /// 使用Gzip算法解压缩二进制数据
        /// </summary>
        /// <param name="input">要解压缩的二进制数据</param>
        /// <returns>解压缩后的二进制数据</returns>
        public static byte[] GzipDecompress(this byte[] input)
        {
            using MemoryStream compressStream = new MemoryStream(input);
            using GZipStream zipStream = new GZipStream(compressStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            zipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
    }

    public static class AESExtensions
    {
        /// <summary>
        /// 字符串加密
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="userName">用户名</param>
        /// <param name="masterPassword">主密码</param>
        /// <param name="compress">是否压缩</param>
        /// <returns>加密后的base64字符串</returns>
        public static string AesEncryptToBase64String(this string plainText, in string userName, in string masterPassword, in bool compress)
        {
            AES alg = new AES(userName, masterPassword);

            byte[] encryptedData;

            if (compress)
                encryptedData = alg.Encrypt(plainText).GzipCompress();
            else
                encryptedData = alg.Encrypt(plainText);

            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// 字符串加密
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="userName">用户名</param>
        /// <param name="masterPassword">主密码</param>
        /// <param name="compress">是否压缩</param>
        /// <returns>加密后的byte[]</returns>
        public static byte[] AesEncryptToBytes(this string plainText, in string userName, in string masterPassword, in bool compress)
        {
            AES alg = new AES(userName, masterPassword);

            if (compress)
                return alg.Encrypt(plainText).GzipCompress();
            else
                return alg.Encrypt(plainText);
        }

        /// <summary>
        /// 字符串解密
        /// </summary>
        /// <param name="encryptedBase64String">加密生成的base64字符串</param>
        /// <param name="userName">用户名</param>
        /// <param name="masterPassword">主密码</param>
        /// <param name="decompress">是否解压缩</param>
        /// <returns>明文字符串</returns>
        public static string AesDecryptToString(this string encryptedBase64String, in string userName, in string masterPassword, in bool decompress)
        {
            byte[] encryptedData;

            if (decompress)
                encryptedData = Convert.FromBase64String(encryptedBase64String).GzipDecompress();
            else
                encryptedData = Convert.FromBase64String(encryptedBase64String);

            AES alg = new AES(userName, masterPassword);
            return alg.Decrypt(encryptedData);
        }

        /// <summary>
        /// byte[]解密
        /// </summary>
        /// <param name="encryptedBytes">加密生成的byte[]</param>
        /// <param name="userName">用户名</param>
        /// <param name="masterPassword">主密码</param>
        /// <param name="decompress">是否解压缩</param>
        /// <returns>明文字符串</returns>
        public static string AesDecryptToString(this byte[] encryptedBytes, in string userName, in string masterPassword, in bool decompress = true)
        {
            AES alg = new AES(userName, masterPassword);

            if (decompress)
                return alg.Decrypt(encryptedBytes.GzipDecompress());
            else
                return alg.Decrypt(encryptedBytes);
        }

        /// <summary>
        /// 用主密码生成密钥
        /// </summary>
        /// <param name="masterPassword">主密码</param>
        /// <param name="salt">盐，可以用用户名之类的来做盐</param>
        /// <returns>pbe生成的256位密钥</returns>
        public static byte[] PasswordBasedEncrypt(this string masterPassword, in string salt, in int iterations = 10000)
        {
            using Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(masterPassword, salt.Sha256ToBytes(), iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32);
        }
    }

    public static class HashExtensions
    {
        // 将字符串SHA256
        public static byte[] Sha256ToBytes(this string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentNullException("哈希内容为空");

            using var algorithm = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return algorithm.ComputeHash(bytes);
        }

        // 将二进制SHA256
        public static byte[] Sha256ToBytes(this byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException("哈希内容为空");

            using var algorithm = SHA256.Create();
            return algorithm.ComputeHash(input);
        }

        // 将字符串SHA512
        public static byte[] Sha512ToBytes(this string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentNullException("哈希内容为空");

            using var algorithm = SHA512.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return algorithm.ComputeHash(bytes);
        }

        // 将二进制SHA512
        public static byte[] Sha512ToBytes(this byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException("哈希内容为空");

            using var algorithm = SHA512.Create();
            return algorithm.ComputeHash(input);
        }

        /// <summary>
        /// 密码存储前处理
        /// </summary>
        /// <param name="input">要处理的密码</param>
        /// <param name="salt">盐，可以来自于服务端config</param>
        /// <returns>主密码加密后的base64字符串</returns>
        public static string AddSaltAndEncrypt(this string password, in string salt)
        {
            using Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt.Sha256ToBytes(), 10000, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }
    }
}
