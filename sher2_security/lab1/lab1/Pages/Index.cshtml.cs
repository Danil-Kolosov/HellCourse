using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Collections.Generic;

namespace lab1.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string InputText { get; set; } = "";

        [BindProperty]
        public string Key { get; set; } = "";

        [BindProperty]
        public string Result { get; set; } = "";

        private List<bool> upperKey = new List<bool>();
        private List<int> spaceAfterIndex = new List<int>();

        private const string RussianAlphabet = "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

        public void OnGet()
        {
        }

        public IActionResult OnPostEncrypt()
        {
            if (string.IsNullOrEmpty(InputText) || string.IsNullOrEmpty(Key))
            {
                Result = "Введите текст и ключ!";
                return Page();
            }

            Result = Encrypt(InputText/*.ToUpper()*/, Key.ToUpper());
            return Page();
        }

        public IActionResult OnPostDecrypt()
        {
            if (string.IsNullOrEmpty(InputText) || string.IsNullOrEmpty(Key))
            {
                Result = "Введите текст и ключ!";
                return Page();
            }

            Result = Decrypt(InputText/*.ToUpper()*/, Key.ToUpper());
            return Page();
        }

        private string Encrypt(string text, string key)
        {
            var result = new StringBuilder();
            var preparedText = PrepareText(text);
            var preparedKey = PrepareKey(key, preparedText.Length);

            for (int i = 0; i < preparedText.Length; i++)
            {
                var textChar = preparedText[i];
                var keyChar = preparedKey[i];

                if (RussianAlphabet.Contains(textChar) && RussianAlphabet.Contains(keyChar))
                {
                    int textIndex = RussianAlphabet.IndexOf(textChar);
                    int keyIndex = RussianAlphabet.IndexOf(keyChar);
                    int encryptedIndex = (textIndex + keyIndex) % RussianAlphabet.Length;
                    if (upperKey[i])
                        result.Append(RussianAlphabet[encryptedIndex]);
                    else
                        result.Append(char.ToLower(RussianAlphabet[encryptedIndex]));
                }
                else
                {                   
                    result.Append(textChar);
                }
            }
            string res = result.ToString();
            foreach (int i in spaceAfterIndex) 
            {
                res = res.Insert(i, " ");
            }
            spaceAfterIndex.Clear();
            return res;
        }

        private string Decrypt(string text, string key)
        {
            var result = new StringBuilder();
            var preparedText = PrepareText(text);
            var preparedKey = PrepareKey(key, preparedText.Length);

            for (int i = 0; i < preparedText.Length; i++)
            {
                var textChar = preparedText[i];
                var keyChar = preparedKey[i];

                if (RussianAlphabet.Contains(textChar) && RussianAlphabet.Contains(keyChar))
                {
                    int textIndex = RussianAlphabet.IndexOf(textChar);
                    int keyIndex = RussianAlphabet.IndexOf(keyChar);
                    int decryptedIndex = (textIndex - keyIndex + RussianAlphabet.Length) % RussianAlphabet.Length;
                    result.Append(RussianAlphabet[decryptedIndex]);
                }
                else
                {
                    result.Append(textChar);
                }
            }

            string res = result.ToString();
            foreach (int i in spaceAfterIndex)
            {
                res = res.Insert(i, " ");
            }
            spaceAfterIndex.Clear();
            return res;
        }

        private string PrepareText(string text)
        {
            var result = new StringBuilder();
            foreach (char c in text)
            {
                if (RussianAlphabet.Contains(char.ToUpper(c)))
                {
                    result.Append(char.ToUpper(c));
                    if (char.ToUpper(c) == c)
                        upperKey.Add(true);
                    else
                        upperKey.Add(false);
                }
                else
                    if (c == ' ')
                    {
                    spaceAfterIndex.Add(text.IndexOf(c));
                    text.Remove(text.IndexOf(c), 1);
                    }
            }
            return result.ToString();
        }

        private string PrepareKey(string key, int length)
        {
            var result = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                result.Append(key[i % key.Length]);
            }
            return result.ToString();
        }
    }
}