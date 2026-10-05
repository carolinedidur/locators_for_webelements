using Microsoft.Extensions.Configuration;
using NUnit.Framework.Internal;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace LocatorsForWebelements.Tests;

[TestFixture]
public class Tests
{
    private IWebDriver driver = null!;
    private WebDriverWait wait = null!;
    private string baseUrl = string.Empty;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        baseUrl = config["Environment:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl missing.");
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

    [TestCase(".NET","Georgia")]
    [TestCase("Python", "Portugal")]
    [TestCase("Java", "Ukraine")]
    public void SearchPosition_ByCriteria_LatestResultContainsKeyword(string language, string country)
    {
        var topNavigationRow = WaitUntilInteractable(By.ClassName("top-navigation__row"));
        topNavigationRow.FindElement(By.LinkText("Careers")).Click();

        var startSearchButton = WaitUntilInteractable(By.ClassName("pinned-button"));
        startSearchButton.FindElement(By.PartialLinkText("START YOUR SEARCH")).Click();

        WaitUntilInteractable(By.XPath("//button[normalize-space()='Accept All']")).Click();

        wait.Until(d => !d.FindElements(By.CssSelector("div[aria-label='Cookie banner']")).Any(e => e.Displayed));

        var locationSelectDropdown = WaitUntilInteractable(By.Id("react-select-2-input"));
        locationSelectDropdown.Clear();
        locationSelectDropdown.SendKeys(country);

        driver.FindElement(By.XPath($"//div[contains(@id,'-option-') and normalize-space()='{country}']")).Click();

        var searchKeywordTextbox = WaitUntilInteractable(By.XPath("//input[@data-testid='search-input']"));
        searchKeywordTextbox.Clear();
        searchKeywordTextbox.SendKeys(language);

        WaitUntilInteractable(By.XPath("//div[contains(@class,'sideMenu')]//label[contains(@for,'checkbox-vacancy_type-Remote')]")).Click();

        WaitUntilInteractable(By.XPath("//form/button[@data-testid='buttonComponent']")).Click();

        WaitForLoaderCycle();

        var firstJobCard = wait.Until(d =>
        {
            var cards = d.FindElements(By.CssSelector("div[class^='JobCard_panel_']"));
            return cards.Count > 0 ? cards[0] : null;
        })!;

        firstJobCard.FindElement(By.TagName("a")).Click();

        var jobTitle = WaitUntilVisible(By.CssSelector("h1[data-testid='job-details-banner-title']")).Text;

        Assert.That(jobTitle, Does.Contain(language));
    }

    [TestCase("BLOCKCHAIN")]
    [TestCase("Cloud")]
    [TestCase("Automation")]
    public void GlobalSearch_ByKeyword_AllResultLinksContainKeyword(string keyword)
    {
        driver.FindElement(By.CssSelector("button[class*='header-search__button']")).Click();

        var searchFieldTextbox = WaitUntilInteractable(By.Name("q"));
        searchFieldTextbox.Clear();
        searchFieldTextbox.SendKeys(keyword);

        driver.FindElement(By.XPath("//button[descendant::span[@class='bth-text-layer']]")).Click();

        var searchResultLinks = wait.Until(d =>
        {
            var foundLinks = d.FindElements(By.XPath("//div[@class='search-results__items']/child::article"));
            return foundLinks.Count > 0 ? foundLinks : null;
        });

        var mismatches = searchResultLinks
            .Select(link => link.Text)
            .Where(text => !text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.That(mismatches, Is.Empty, $"Links without '{keyword}': {string.Join(" | ", mismatches)}");
    }

    [TearDown]
    public void TearDown()
    {
        driver.Quit();
        driver.Dispose();
    }

    private IWebElement WaitUntilVisible(By locator) =>
    wait.Until(d =>
    {
        IWebElement element = d.FindElement(locator);
        return element.Displayed ? element : null;
    })!;

    private IWebElement WaitUntilInteractable(By locator) =>
    wait.Until(d =>
    {
        IWebElement element = d.FindElement(locator);
        return element.Displayed && element.Enabled ? element : null;
    })!;

    private void WaitForLoaderCycle()
    {
        try
        {
            new WebDriverWait(driver, TimeSpan.FromSeconds(3))
                .Until(d => d.FindElements(By.CssSelector("div[data-testid='preloader']")).Any(e => e.Displayed));
        }
        catch (WebDriverTimeoutException)
        {
            return;
        }

        wait.Until(d => !d.FindElements(By.CssSelector("div[data-testid='preloader']")).Any(e => e.Displayed));
    }
}
