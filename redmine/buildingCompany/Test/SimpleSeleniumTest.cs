using Microsoft.AspNetCore.Routing;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Test;

[TestFixture]
public class HomePageTests
{
    // IWebDriver - основной интерфейс Selenium, представляет собой "водителя"
    // который управляет браузером. Через него:
    // - открываются страницы (Navigate().GoToUrl)
    // - ищутся элементы (FindElement)
    // - получаются информацию (Title, Url)
    // - взаимодействие с элементами (Click, SendKeys)
    private IWebDriver driver;
    private string baseUrl = "http://localhost:7021";

    // [SetUp] - атрибут NUnit, метод выполняется ПЕРЕД КАЖДЫМ тестом
    // Здесь создается и настраивается браузер
    [SetUp]
    public void Setup()
    {
        driver = new ChromeDriver();
        driver.Manage().Window.Maximize();
    }

    [TearDown]
    public void TearDown()
    {
        driver.Quit();
        driver.Dispose();
    }

    [Test]
    public void HomePage_Title_ShouldBeCorrect()
    {
        // Открытие главной страницы
        driver.Navigate().GoToUrl(baseUrl);

        // Проверка заголовка страницы
        //Title формируется из Index.cshtml файла приложения и макета (общей разметки) _Layout
        string pageTitle = driver.Title;

        NUnit.Framework.Assert.That(pageTitle, Is.EqualTo("Главная - СтройКомпания"),
            "Заголовок страницы не соответствует ожидаемому");
    }
}