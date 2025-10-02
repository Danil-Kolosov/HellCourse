using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace RSACipher.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string InputText { get; set; } = "";

        [BindProperty]
        public string PublicKey { get; set; } = "";

        [BindProperty]
        public string PrivateKey { get; set; } = "";

        [BindProperty]
        public string Modulus { get; set; } = "";

        [BindProperty]
        public string EncryptedText { get; set; } = "";

        [BindProperty]
        public string DecryptedText { get; set; } = "";

        // Добавляем временное хранение ключей в TempData
        private const string PublicKeyTemp = "PublicKey";
        private const string PrivateKeyTemp = "PrivateKey";
        private const string ModulusTemp = "Modulus";

        public void OnGet()
        {
            // Восстанавливаем ключи из TempData при загрузке страницы
            PublicKey = TempData[PublicKeyTemp]?.ToString() ?? "";
            PrivateKey = TempData[PrivateKeyTemp]?.ToString() ?? "";
            Modulus = TempData[ModulusTemp]?.ToString() ?? "";

            // Сохраняем обратно в TempData для следующего запроса
            TempData.Keep(PublicKeyTemp);
            TempData.Keep(PrivateKeyTemp);
            TempData.Keep(ModulusTemp);
        }

        public IActionResult OnPostGenerateKeys()
        {
            try
            {
                // Генерация простых чисел p и q разрядностью не менее 12 бит
                BigInteger p = GenerateLargePrime(128);//12 мин
                BigInteger q = GenerateLargePrime(128);//12 мин

                while (p == q)
                {
                    q = GenerateLargePrime(128);
                }

                BigInteger n = p * q;
                BigInteger phi = (p - 1) * (q - 1);

                // Выбор открытой экспоненты e
                BigInteger e = 65537;

                // Проверяем, что e взаимно просто с phi
                while (BigInteger.GreatestCommonDivisor(e, phi) != 1)
                {
                    e = GenerateLargePrime(16);
                }

                // Вычисление секретной экспоненты d
                BigInteger d = ModInverse(e, phi);

                // Сохраняем ключи в свойствах и TempData
                PublicKey = e.ToString();
                PrivateKey = d.ToString();
                Modulus = n.ToString();

                TempData[PublicKeyTemp] = PublicKey;
                TempData[PrivateKeyTemp] = PrivateKey;
                TempData[ModulusTemp] = Modulus;

                TempData["Message"] = "Ключи успешно сгенерированы!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при генерации ключей: {ex.Message}";
            }

            return Page();
        }

        public IActionResult OnPostEncrypt()
        {
            try
            {
                // Восстанавливаем ключи из TempData
                PublicKey = TempData[PublicKeyTemp]?.ToString() ?? "";
                PrivateKey = TempData[PrivateKeyTemp]?.ToString() ?? "";
                Modulus = TempData[ModulusTemp]?.ToString() ?? "";



                if (string.IsNullOrEmpty(InputText) || string.IsNullOrEmpty(PublicKey) || string.IsNullOrEmpty(Modulus))
                {
                    TempData["Error"] = "Введите текст и сгенерируйте ключи!";

                    // Сохраняем ключи обратно
                    TempData.Keep(PublicKeyTemp);
                    TempData.Keep(PrivateKeyTemp);
                    TempData.Keep(ModulusTemp);
                    return Page();
                }

                BigInteger e = BigInteger.Parse(PublicKey);
                BigInteger n = BigInteger.Parse(Modulus);

                //Или нужно всегда новые генерировать?
                //if (string.IsNullOrEmpty(InputText))
                //{
                //    TempData["Error"] = "Введите текст!";
                //    return Page();
                //}
                //BigInteger p = GenerateLargePrime(128);
                //BigInteger q = GenerateLargePrime(128);
                //while (p == q)
                //{
                //    q = GenerateLargePrime(128);
                //}

                //BigInteger n = p * q;
                //BigInteger phi = (p - 1) * (q - 1);
                //BigInteger e = 65537;

                //if (BigInteger.GreatestCommonDivisor(e, phi) != 1)
                //{
                //    TempData["Error"] = "Не удалось подобрать p и q. Попробуйте еще раз.";
                //    return Page();
                //}

                //BigInteger d = ModInverse(e, phi);
                //// Сохраняем ключи в свойствах и TempData
                //PublicKey = e.ToString();
                //PrivateKey = d.ToString();
                //Modulus = n.ToString();

                //TempData[PublicKeyTemp] = PublicKey;
                //TempData[PrivateKeyTemp] = PrivateKey;
                //TempData[ModulusTemp] = Modulus;

                //TempData["Message"] = "Ключи успешно сгенерированы!";

                // Преобразуем текст в байты
                byte[] bytes = Encoding.UTF8.GetBytes(InputText);

                // Преобразуем байты в большое число
                BigInteger message = new BigInteger(bytes, isUnsigned: true);

                // Проверяем, что сообщение меньше модуля
                if (message >= n)
                {
                    TempData["Error"] = "Сообщение слишком большое для выбранного ключа!";
                    TempData.Keep(PublicKeyTemp);
                    TempData.Keep(PrivateKeyTemp);
                    TempData.Keep(ModulusTemp);
                    return Page();
                }

                // Шифруем: c = m^e mod n
                BigInteger encrypted = BigInteger.ModPow(message, e, n);

                EncryptedText = encrypted.ToString();
                TempData["Message"] = "Текст успешно зашифрован!";

                // Сохраняем ключи обратно
                TempData.Keep(PublicKeyTemp);
                TempData.Keep(PrivateKeyTemp);
                TempData.Keep(ModulusTemp);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при шифровании: {ex.Message}";
                TempData.Keep(PublicKeyTemp);
                TempData.Keep(PrivateKeyTemp);
                TempData.Keep(ModulusTemp);
            }

            return Page();
        }

        public IActionResult OnPostDecrypt()
        {
            try
            {
                // Восстанавливаем ключи из TempData
                PublicKey = TempData[PublicKeyTemp]?.ToString() ?? "";
                PrivateKey = TempData[PrivateKeyTemp]?.ToString() ?? "";
                Modulus = TempData[ModulusTemp]?.ToString() ?? "";

                if (string.IsNullOrEmpty(EncryptedText) || string.IsNullOrEmpty(PrivateKey) || string.IsNullOrEmpty(Modulus))
                {
                    TempData["Error"] = "Введите зашифрованный текст и убедитесь, что ключи сгенерированы!";
                    TempData.Keep(PublicKeyTemp);
                    TempData.Keep(PrivateKeyTemp);
                    TempData.Keep(ModulusTemp);
                    return Page();
                }

                BigInteger d = BigInteger.Parse(PrivateKey);
                BigInteger n = BigInteger.Parse(Modulus);
                BigInteger encrypted = BigInteger.Parse(EncryptedText);

                // Дешифруем: m = c^d mod n
                BigInteger decrypted = BigInteger.ModPow(encrypted, d, n);

                // Преобразуем большое число обратно в байты
                byte[] decryptedBytes = decrypted.ToByteArray(isUnsigned: true);

                // Если последний байт равен 0 (из-за особенностей BigInteger), удаляем его
                if (decryptedBytes.Length > 0 && decryptedBytes[decryptedBytes.Length - 1] == 0)
                {
                    decryptedBytes = decryptedBytes.Take(decryptedBytes.Length - 1).ToArray();
                }

                DecryptedText = Encoding.UTF8.GetString(decryptedBytes);
                TempData["Message"] = "Текст успешно расшифрован!";

                // Сохраняем ключи обратно
                TempData.Keep(PublicKeyTemp);
                TempData.Keep(PrivateKeyTemp);
                TempData.Keep(ModulusTemp);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при расшифровании: {ex.Message}";
                TempData.Keep(PublicKeyTemp);
                TempData.Keep(PrivateKeyTemp);
                TempData.Keep(ModulusTemp);
            }

            return Page();
        }

        // Генерация большого простого числа
        private BigInteger GenerateLargePrime(int bitLength)
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                while (true)
                {
                    byte[] bytes = new byte[bitLength / 8 + 1];
                    rng.GetBytes(bytes);
                    bytes[bytes.Length - 1] &= 0x7F; // Убедимся, что число положительное

                    BigInteger candidate = new BigInteger(bytes);

                    // Убедимся, что число нечетное и достаточно большое
                    if (candidate.IsEven)
                        candidate++;

                    if (candidate < BigInteger.Pow(2, bitLength - 1))
                        candidate += BigInteger.Pow(2, bitLength - 1);

                    if (IsProbablePrime(candidate))
                        return candidate;
                }
            }
        }

        // Тест Миллера-Рабина на простоту
        private bool IsProbablePrime(BigInteger n, int certainty = 100)
        {
            if (n == 2 || n == 3)
                return true;
            if (n < 2 || n.IsEven)
                return false;

            BigInteger d = n - 1;
            int s = 0;

            while (d.IsEven)
            {
                d /= 2;
                s++;
            }

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[n.ToByteArray().LongLength];

                for (int i = 0; i < certainty; i++)
                {
                    BigInteger a;
                    do
                    {
                        rng.GetBytes(bytes);
                        a = new BigInteger(bytes);
                    }
                    while (a < 2 || a >= n - 1);

                    BigInteger x = BigInteger.ModPow(a, d, n);
                    if (x == 1 || x == n - 1)
                        continue;

                    for (int r = 1; r < s; r++)
                    {
                        x = BigInteger.ModPow(x, 2, n);
                        if (x == 1)
                            return false;
                        if (x == n - 1)
                            break;
                    }

                    if (x != n - 1)
                        return false;
                }
            }

            return true;
        }

        // Вычисление модульного обратного с помощью расширенного алгоритма Евклида
        private BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            BigInteger m0 = m;
            BigInteger y = 0, x = 1;

            if (m == 1)
                return 0;

            while (a > 1)
            {
                BigInteger q = a / m;
                BigInteger t = m;

                m = a % m;
                a = t;
                t = y;

                y = x - q * y;
                x = t;
            }

            if (x < 0)
                x += m0;

            return x;
        }
    }
}