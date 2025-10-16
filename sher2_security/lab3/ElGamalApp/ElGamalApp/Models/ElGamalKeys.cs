using System.Numerics;

namespace ElGamalApp.Models
{
    public class ElGamalKeys
    {
        public BigInteger P { get; set; }
        public BigInteger G { get; set; }
        public BigInteger X { get; set; }
        public BigInteger Y { get; set; }
    }

    public class AppModel
    {
        public string InputText { get; set; } = string.Empty;
        public string EncryptedText { get; set; } = string.Empty;
        public string DecryptedText { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}