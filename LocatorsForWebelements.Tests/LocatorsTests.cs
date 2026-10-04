using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace LocatorsForWebelements.Tests;

[TestFixture]
public class Tests
{
    private IWebDriver driver = null!;
    private WebDriverWait wait = null!;
    private string baseUrl;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        baseUrl = config["BaseUrl"] ?? throw new InvalidOperationException("BaseUrl missing.");
    }


    [SetUp]
    public void Setup()
    {
        driver = new ChromeDriver();
        driver.Manage().Window.Maximize();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        driver.Navigate().GoToUrl(baseUrl);
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }

    [TearDown]
    public void TearDown()
    {
        driver.Quit();
        driver.Dispose();
    }
}
