using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using ElGamalApp.Models;

namespace ElGamalApp.Services
{
    public interface IElGamalService
    {
        ElGamalKeys GenerateKeys();
        string Encrypt(string text, ElGamalKeys keys);
        string Decrypt(string encryptedText, ElGamalKeys keys);
        string GetKeySizes();
    }

    public class ElGamalService : IElGamalService
    {
        // Битность согласно ТЗ:
        private readonly int _pBits = 20;    //64 по заданию: p не менее 20 бит
        private readonly int _gBits = 12;    //32 по заданию: g не менее 12 бит  
        private readonly int _xBits = 12;    //32 по заданию: x не менее 12 бит
        private readonly int _kBits = 20;    //32 по заданию: k не менее 20 бит

        public string GetKeySizes()
        {
            return $"p: {_pBits} бит, g: {_gBits} бит, x: {_xBits} бит, k: {_kBits} бит";
        }

        public ElGamalKeys GenerateKeys()
        {
            var keys = new ElGamalKeys();

            // Генерация простого числа p
            keys.P = GeneratePrime(_pBits);

            // Генерация g
            do
            {
                keys.G = GeneratePrime(_gBits);
            } while (keys.G >= keys.P - 1 || keys.G <= 1);

            // Генерация закрытого ключа x
            do
            {
                keys.X = GeneratePrime(_xBits);
            } while (keys.X >= keys.P - 1 || keys.X <= 1);

            // Вычисление открытого ключа y
            keys.Y = BigInteger.ModPow(keys.G, keys.X, keys.P);

            return keys;
        }

        public string Encrypt(string text, ElGamalKeys keys)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var result = new StringBuilder();
            var random = RandomNumberGenerator.Create();

            // Шифруем каждый байт отдельно
            foreach (byte byteValue in bytes)
            {
                BigInteger M = byteValue;

                // Генерация случайного k
                BigInteger k;
                do
                {
                    k = GenerateRandomBigInteger(keys.P - 2, random);
                } while (k <= 1 || k.GetBitLength() < _kBits);

                // Шифрование
                var a = BigInteger.ModPow(keys.G, k, keys.P);
                var b = (M * BigInteger.ModPow(keys.Y, k, keys.P)) % keys.P;

                // Сохраняем как сплошные цифры, но с разделителями
                result.Append(a.ToString());
                result.Append("|"); // разделитель между a и b
                result.Append(b.ToString());
                result.Append(";"); // разделитель между блоками
            }

            return result.ToString();
        }

        public string Decrypt(string encryptedText, ElGamalKeys keys)
        {
            var resultBytes = new List<byte>();

            try
            {
                // Разбиваем на блоки по разделителю
                var blocks = encryptedText.Split(';', StringSplitOptions.RemoveEmptyEntries);

                foreach (var block in blocks)
                {
                    // Разбиваем каждую пару a|b
                    var parts = block.Split('|');
                    if (parts.Length == 2)
                    {
                        // Парсим как BigInteger (не Int32!)
                        if (BigInteger.TryParse(parts[0], out BigInteger a) &&
                            BigInteger.TryParse(parts[1], out BigInteger b))
                        {
                            // Дешифрование
                            var s = BigInteger.ModPow(a, keys.X, keys.P);
                            var sInv = BigInteger.ModPow(s, keys.P - 2, keys.P);
                            var M = (b * sInv) % keys.P;

                            // Преобразуем обратно в byte
                            if (M >= 0 && M <= 255)
                            {
                                resultBytes.Add((byte)M);
                            }
                        }
                    }
                }

                return Encoding.UTF8.GetString(resultBytes.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка дешифрования: {ex.Message}");
            }
        }

        private BigInteger GeneratePrime(int bitLength)
        {
            using var rng = RandomNumberGenerator.Create();
            int attempts = 0;
            while (attempts < 1000)
            {
                var candidate = GenerateRandomBigInteger(BigInteger.Pow(2, bitLength), rng);
                if (candidate.GetBitLength() >= bitLength && IsProbablePrime(candidate, 5))
                    return candidate;
                attempts++;
            }
            throw new Exception($"Не удалось сгенерировать простое число {bitLength} бит");
        }

        private BigInteger GenerateRandomBigInteger(BigInteger max, RandomNumberGenerator rng)
        {
            var bytes = max.ToByteArray();
            BigInteger result;
            do
            {
                rng.GetBytes(bytes);
                bytes[^1] &= 0x7F;
                result = new BigInteger(bytes);
            } while (result >= max || result <= 0);
            return result;
        }

        private bool IsProbablePrime(BigInteger source, int certainty)
        {
            if (source == 2 || source == 3)
                return true;
            if (source < 2 || source % 2 == 0)
                return false;

            BigInteger d = source - 1;
            int s = 0;

            while (d % 2 == 0)
            {
                d /= 2;
                s += 1;
            }

            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[source.ToByteArray().LongLength];

            for (int i = 0; i < certainty; i++)
            {
                BigInteger a;
                do
                {
                    rng.GetBytes(bytes);
                    a = new BigInteger(bytes);
                }
                while (a < 2 || a >= source - 2);

                BigInteger x = BigInteger.ModPow(a, d, source);
                if (x == 1 || x == source - 1)
                    continue;

                for (int r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, source);
                    if (x == 1)
                        return false;
                    if (x == source - 1)
                        break;
                }

                if (x != source - 1)
                    return false;
            }

            return true;
        }
    }
}