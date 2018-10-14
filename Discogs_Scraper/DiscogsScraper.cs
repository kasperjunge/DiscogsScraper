﻿using System;
using System.IO;
using HtmlAgilityPack;

namespace Discogs_Scraper
{
    class DiscogsScraper
    {
        // PROPERTIES
        public string startUrl; // Page to start scraping from.
        public string genre; // Genre 
        public int numData; // Number of data instances to collect.

        //CONSTRUCTER
        public DiscogsScraper(string startUrl = "", string genre = "", int numData = 0)
        {
            this.startUrl = startUrl;
            this.numData = numData;
            this.genre = genre;
        }

        // TRIGGER FUNCTION
        public void BeginScraping()
        {
            Console.WriteLine("Begin Scraping..");
            //CreateListingLinks(); // Get listing page links and stores them in listingLinks.txt
            //GetMultiGenreReleaseLinks();
            GetSingleGenreData();


        }

        private void GetMultiGenreReleaseLinks()
        {
            TextReader tr = new StreamReader(@"C:\Users\Kasper\Documents\code\Discogs_Scraper\listingLinks.txt");
            TextWriter tw = new StreamWriter(@"C:\Users\Kasper\Documents\code\Discogs_Scraper\multiGenreReleaseLinks.txt");
            int numberOfLinks = 0;
            string currentlyProcessedLink;
            string xPath;
            string link;
            
            for (int i = 1; i <= numData/250; i++)
            {
                
                currentlyProcessedLink = tr.ReadLine();
                try
                {
                    
                    HtmlDocument doc = GetHtmlDoc(currentlyProcessedLink);

                    for (int j = 1; j <= 250; j++)
                    {

                        xPath = string.Format(@"//*[@id='search_results']/div[{0}]/h4/a", j);
                        HtmlNode linkNode = doc.DocumentNode.SelectSingleNode(xPath);
                        HtmlAttribute hrefAttribute = linkNode.Attributes["href"];
                        link = "https://www.discogs.com" + hrefAttribute.Value;
                        numberOfLinks++;
                        tw.WriteLine(link);

                        Console.WriteLine(link); 
                        Console.WriteLine(numberOfLinks);

                    }
                }
                catch (System.Net.WebException)
                {
                    Console.WriteLine("Error!");
                    i = i - 1; //if fail, go back 1 i.
                    continue;
                }
                

            }

            tr.Close();


        }

        private HtmlDocument GetHtmlDoc(string url)
        {
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = hw.Load(url);

            return doc;
        }

        private void CreateListingLinks()
        {
            //Function: creates number of listing-pages needed to scrape numData links. The links are saved to a file as they are obtained.

            Console.WriteLine("Creating listing links.."); // User message
            int numPages = numData / 250; // Calculates the number of listingpages to scrape to achieve numData datapoints. (Assumes that one listing page has 250 releases)
            int lastIndex = startUrl.Length - 1; 
            startUrl = startUrl.Remove(lastIndex, 1) + "{0}";

            TextWriter tw = new StreamWriter(@"C:\Users\Kasper\Documents\code\Discogs_Scraper\listingLinks.txt");
            for (int i = 1; i <= numPages; i++ ) // Create urls
            {
                string url = string.Format(startUrl, i);
                tw.WriteLine(url);
                
            }

            tw.Close();

        }
      
        private void GetSingleGenreData()
        {

            string currentlyProcessedLink;
            int numOfLinks = File.ReadAllLines(@"C:\Users\Kasper\Documents\code\Discogs_Scraper\multiGenreReleaseLinks.txt").Length;
            int singleGenreCount = 0, multiGenreCount = 0;

            TextReader tr = new StreamReader(@"C:\Users\Kasper\Documents\code\Discogs_Scraper\multiGenreReleaseLinks.txt");
            TextWriter tw = new StreamWriter(@"C:\Users\Kasper\Documents\code\Discogs_Scraper\singleGenreReleaseLinks.txt");
            TextWriter twIMG = new StreamWriter(string.Format(@"C:\Users\Kasper\Documents\code\Discogs_Scraper\{0}Covers.txt", genre));

            Console.WriteLine("Now filtering single genre releases..");
            Console.WriteLine(string.Format(@"C:\Users\Kasper\Documents\code\Discogs_Scraper\{0}Covers.txt", genre));

            for (int i = 1; i <= numOfLinks; i++)
            {

                currentlyProcessedLink = tr.ReadLine();

                try
                {
                    HtmlDocument doc = GetHtmlDoc(currentlyProcessedLink);
                    if (IsSingleGenre(doc)) // Check if release has a single genre.
                    {
                        singleGenreCount++;
                        Console.WriteLine("Currently processed link:" + currentlyProcessedLink);
                        Console.WriteLine("Singel genre: {0}    Multi genre: {1}", singleGenreCount, multiGenreCount);
                        tw.WriteLine(currentlyProcessedLink);
                        twIMG.WriteLine(ExtractCoverImg(doc));

                    }
                    else
                    {
                        multiGenreCount++;
                    }

                }
                catch (System.Net.WebException)
                {
                    Console.WriteLine("Error!");
                    i = i - 1; //if fail, go back 1 i.
                    continue;
                }
            }
            tr.Close();
            tw.Close();
            twIMG.Close();
        }

        private bool IsSingleGenre(HtmlDocument doc)
        {
            if (doc.DocumentNode.SelectSingleNode("//*[@id='page_content']/div[1]/div[3]/div[2]/a[2]") == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string ExtractCoverImg(HtmlDocument doc)
        {
            // Extracting cover img link
            HtmlNode picNode = doc.DocumentNode.SelectSingleNode("//*[@id='page_content']/div[1]/div[1]/a/span[2]/img"); // Get img node
            if (picNode != null)
            {
                string picHtml = picNode.WriteTo();                 // Convert node to string
                int indexStart = picHtml.IndexOf("=") + 2;          // Define img link start index
                int indexStop = picHtml.IndexOf("alt=") - 2;        // Define img link stop index
                int lenght = indexStop - indexStart;                // Calculate lenght of img link
                string picLink = picHtml.Substring(indexStart, lenght); // Cut out img link from html
                return picLink;
            }
            return "Error in ExtractCoverImg-method";
        }

    }
}




//private bool CheckForSingleGenre()
//{
//    Console.WriteLine("Checking if the release has a single or multiple genres.");
//}

// DiscogsScraper Method: GetHtml();
// DiscogsScraper Method: GetReleaseLinks();
// DiscogsScraper Method: DownloadImage();
// DiscogsScraper Method: DetectNumberOfReleasesOnListingPage();


// Release Method: Release.CheckForSingleGenre();
// Release Method: Release.GetData();

