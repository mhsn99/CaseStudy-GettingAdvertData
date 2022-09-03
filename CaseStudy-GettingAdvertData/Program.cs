using System;
using System.Collections.Generic;
using System.IO;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScrapySharp.Network;


namespace CaseStudy_GettingAdvertData
{
    internal class Program
    {
        static ScrapingBrowser _browser = new ScrapingBrowser();
        static void Main(string[] args)
        {
            string url = "https://www.sahibinden.com";
            double sumPrice = 0;
            double averagePrice = 0;

            List<string> advertLinkList = GetAdvertLinks(url);
            var advertsInfo = GetAdvertsInfo(advertLinkList);

            Console.WriteLine("-------------------------------------");
            Console.WriteLine("****  Adverts Info ****");
            // Showing Advert Info
            foreach (var advert in advertsInfo)
            {
                Console.WriteLine("Title Advert: " + advert.Title);
                Console.WriteLine("Price: " + advert.Price);
                Console.WriteLine("-----------------------------");
            }

            // Sum of Price and showing average price
            foreach (var advert in advertsInfo)
            {
                sumPrice += advert.Price;
            }
            averagePrice = sumPrice/ advertsInfo.Count;

            Console.WriteLine("Average Price: " + averagePrice);

            // Saving data to txt file
            SaveData(advertsInfo);
        }

        /// <summary>
        /// This method used for getting the page HTML script and return HTML Nodes.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        static HtmlNode GetHtml(string url)
        {   
            
            WebPage page = _browser.NavigateToPage(new Uri(url));
            _browser.ClearCookies();
            return page.Html;
        }

        /// <summary>
        /// This method gets the links of the ads on the homepage and returns link's list.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        static List<string> GetAdvertLinks(string url)
        {
            List<string> advertLinks = new List<string>();

            var html = GetHtml(url);

            var linkNodes = html.SelectNodes("//ul[@class = 'vitrin-list clearfix']/li/a");

            Uri baseUrl = new Uri(url);

            foreach (var node in linkNodes)
            {
                string link = node.Attributes["href"].Value;

                // filtering links for belonging to the ad or not
                if (link.StartsWith("/ilan"))
                {
                    link = new Uri(baseUrl, link).AbsoluteUri;
                    advertLinks.Add(link);
                }
            }

            return advertLinks;
        }

        /// <summary>
        /// This method takes the names and price information of the adverts. Returns the informations in the form of a list.
        /// </summary>
        /// <param name="linkList"></param>
        /// <returns></returns>
        static List<Advert> GetAdvertsInfo(List<string> linkList)
        {
            int count = 1;
            var adverts = new List<Advert>();

            foreach (var link in linkList)
            {

                System.Threading.Thread.Sleep(15000); // It refers to the waiting time between requests to the sites in order to avoid the 429 code error
                
                var html = GetHtml(link);
                
                var advert = new Advert();
                
                advert.Title = html.SelectSingleNode("//h1").InnerText;


                // The products' prices are kept in 3 different xpath.
                string xpath1 = "//div[@class=\"classifiedInfo \"]/h3";
                string xpath2 = "//div[@class=\"classifiedInfo \"]/span";
                string xpath3 = "//div[@class=\"price-section \"]/span";

                if (html.SelectSingleNode(xpath1) == null)
                {
                    if (html.SelectSingleNode(xpath2) != null)
                        advert.Price = ConvertPriceToNumber(html.SelectSingleNode(xpath2).InnerText.Trim());
                    else
                    {
                        var priceNode = html.SelectSingleNode(xpath3);
                        foreach (var item in priceNode.SelectNodes("*"))
                        {
                            if (item.Attributes["class"].Value != "currency")
                                item.Remove();
                        }

                        advert.Price = ConvertPriceToNumber(priceNode.InnerText.Trim());
                    }
                }
                else
                {
                    var priceNode = html.SelectSingleNode(xpath1);
                    foreach (var item in priceNode.SelectNodes("*"))
                    {
                        item.Remove();
                    }

                    advert.Price = ConvertPriceToNumber(priceNode.InnerText.Trim());

                }

                Console.WriteLine("{0}. advert's data is loaded...", count);
                
                count++;

                adverts.Add(advert);
            }

            return adverts;
        }

        /// <summary>
        /// This method used for converting price information to numerical data.
        /// </summary>
        /// <param name="priceText"></param>
        /// <returns></returns>
        static int ConvertPriceToNumber(string priceText)
        {
            int price;
            string text = string.Empty;
            foreach (var item in priceText)
            {
                if (char.IsDigit(item))
                    text += item;
            }

            if (text == "")
                price = 0;
            else
                price = Convert.ToInt32(text);

            return price;
        }

        static void SaveData(List<Advert> adverts)
        {
            string json = JsonConvert.SerializeObject(adverts.ToArray());
            json = JValue.Parse(json).ToString(Formatting.Indented);
            File.WriteAllText("..\\..\\..\\advertsInfo.txt", json);
            Console.WriteLine("The file with the name 'advertisE Info.txt' has been saved.!!!");
        }
    }
}
