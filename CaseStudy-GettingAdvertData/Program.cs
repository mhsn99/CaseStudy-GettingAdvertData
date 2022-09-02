using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using ScrapySharp.Network;


namespace CaseStudy_GettingAdvertData
{
    internal class Program
    {
        static ScrapingBrowser _browser = new ScrapingBrowser();
        static void Main(string[] args)
        {
            string url = "https://www.sahibinden.com";

            List<string> advertLinkList = GetAdvertLinks(url);
            foreach (var link in advertLinkList)
            {
                Console.WriteLine(link);
            }
        }

        /// <summary>
        /// This method used for getting the page HTML script and return HTML Nodes.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        static HtmlNode GetHtml(string url)
        {
            WebPage page = _browser.NavigateToPage(new Uri(url));
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


    }
}
