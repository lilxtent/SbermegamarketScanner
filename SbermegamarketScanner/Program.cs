using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

#region user input

    const string url = """https://megamarket.ru/catalog/page-{0}/?q=ssd%20512?filters=%7B"88C83F68482F447C9F4E401955196697"%3A%7B"max"%3A6884%7D%7D""";
    const int pages = 10;

#endregion

var driver = new ChromeDriver();

driver.Manage().Window.Maximize();

var result = new List<Summary>();

for (var pageNumber = 1; pageNumber <= pages; pageNumber++)
{
    driver.Navigate().GoToUrl(string.Format(url, pageNumber));

    var elem = driver.FindElement(By.ClassName("catalog-items-list"));

    var html = new HtmlDocument();
    html.LoadHtml(elem.GetAttribute("innerHTML"));

    var nodes = html.DocumentNode.SelectNodes("//div[@data-list-id='main']");

    var summaries = nodes
        .Select(GetSummary)
        .ToList();
    
    result.AddRange(summaries);
}

foreach (var summary in result.OrderBy(x => x.Price - (x.Bonuses ?? 0)))
{
    Console.WriteLine(summary.ToString());
}
    
Summary GetSummary(HtmlNode node)
{
    var prices = node
        .SelectSingleNode(".//div[contains(@class, 'catalog-item-regular-desktop__price-conditions')]")
        .SelectSingleNode(".//div[contains(@class, 'catalog-item-regular-desktop__price-block')]");

    var priceMoneyString = prices
        .SelectSingleNode(".//div[@class='catalog-item-regular-desktop__price']")
        .InnerText
        .Replace("&nbsp;₽", "")
        .Replace(" ", "");

    var bonusesString = prices
        .SelectSingleNode(".//div[contains(@class, 'catalog-item-regular-desktop__bonus')]")
        ?.SelectSingleNode(".//span[contains(@class, 'bonus-amount')]")
        ?.InnerText
        ?.Replace(" ", "");

    var priceMoney = int.Parse(priceMoneyString);
    int.TryParse(bonusesString, out var bonuses);
     
    var href = node
        .SelectSingleNode(".//a[contains(@class, 'ddl_product_link')]")
        .GetAttributeValue("href", "");
    
    return new Summary
    {
        Link = $"https://megamarket.ru{href}",
        Price = priceMoney,
        Bonuses = bonuses
    };
}


class Summary
{
    public string Link { get; set; }
    public int Price { get; set; }
    public int? Bonuses { get; set; }

    public override string ToString()
    {
        return $"Price: {Price}, Bonuses: {Bonuses ?? 0}, End price: {Price - (Bonuses ?? 0)}, Link: {Link}";
    }
}
