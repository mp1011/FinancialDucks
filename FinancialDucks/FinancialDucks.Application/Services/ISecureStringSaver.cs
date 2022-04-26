using FinancialDucks.Application.Models;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace FinancialDucks.Application.Services
{
    public interface ISecureStringSaver
    {
        void Save(string key, string value);
        string Load(string key);
    }

    [SupportedOSPlatform("windows")]
    public class SecureStringSaver : ISecureStringSaver
    {
        private readonly ISettingsService _settingsService;

        public SecureStringSaver(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        //fixed random number to assist with encryption. Not required to be perfectly secret.
        private byte[] GetEntropy(string key)
        {
            byte[] rng = new byte[16];
            new Random(9485192).NextBytes(rng);

            var keyBytes = UnicodeEncoding.ASCII.GetBytes(key);
            return rng.Union(keyBytes).ToArray();
        }

        public string Load(string key)
        {
            try
            {
                var srcFile = $"{_settingsService.SourcePath.FullName}\\{key}";
                var encrypted = File.ReadAllBytes(srcFile);
                var decrypted = ProtectedData.Unprotect(encrypted, GetEntropy(key), DataProtectionScope.LocalMachine);
                return UnicodeEncoding.ASCII.GetString(decrypted);
            }
            catch
            {
                return String.Empty;
            }
        }

        public void Save(string key, string value)
        {
            var textBytes = UnicodeEncoding.ASCII.GetBytes(value);
            var encrypyted = ProtectedData.Protect(textBytes, GetEntropy(key), DataProtectionScope.LocalMachine);

            var destFile = $"{_settingsService.SourcePath.FullName}\\{key}";
            File.WriteAllBytes(destFile, encrypyted);
        }
    }

    public static class ISecureStringSaverExtensions
    {
        public static string GetUsername(this ISecureStringSaver secureStringSaver, ITransactionSource source)
        {
            return secureStringSaver.Load($"{source.Id}_u");
        }

        public static string GetPassword(this ISecureStringSaver secureStringSaver, ITransactionSource source)
        {
            return secureStringSaver.Load($"{source.Id}_p");
        }

        public static void SaveUsername(this ISecureStringSaver secureStringSaver, ITransactionSource source, string userName)
        {
            secureStringSaver.Save($"{source.Id}_u",userName);
        }

        public static void SavePassword(this ISecureStringSaver secureStringSaver, ITransactionSource source, string password)
        {
            secureStringSaver.Save($"{source.Id}_p", password);
        }
    }
}
