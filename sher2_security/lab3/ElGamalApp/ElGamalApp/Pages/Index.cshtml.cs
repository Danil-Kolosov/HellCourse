using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ElGamalApp.Models;
using ElGamalApp.Services;

namespace ElGamalApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IElGamalService _elGamalService;
        private static ElGamalKeys _currentKeys;

        public IndexModel(IElGamalService elGamalService)
        {
            _elGamalService = elGamalService;
        }

        [BindProperty]
        public AppModel Model { get; set; } = new AppModel();

        public string KeySizes { get; set; } = string.Empty;

        public void OnGet()
        {
            KeySizes = _elGamalService.GetKeySizes();
        }

        public IActionResult OnPost(string action)
        {
            KeySizes = _elGamalService.GetKeySizes();

            try
            {
                if (action == "encrypt" && !string.IsNullOrEmpty(Model.InputText))
                {
                    // Генерируем новые ключи при каждом шифровании
                    _currentKeys = _elGamalService.GenerateKeys();

                    // Шифруем текст
                    Model.EncryptedText = _elGamalService.Encrypt(Model.InputText, _currentKeys);

                    // Сохраняем ключи для показа
                    Model.PublicKey = $"p={_currentKeys.P}\ng={_currentKeys.G}\ny={_currentKeys.Y}";
                    Model.PrivateKey = _currentKeys.X.ToString();
                }
                else if (action == "decrypt" && !string.IsNullOrEmpty(Model.EncryptedText) && _currentKeys != null)
                {
                    // Дешифруем текст текущими ключами
                    Model.DecryptedText = _elGamalService.Decrypt(Model.EncryptedText, _currentKeys);
                }
            }
            catch (Exception ex)
            {
                Model.Error = $"Ошибка: {ex.Message}";
            }

            return Page();
        }
    }
}