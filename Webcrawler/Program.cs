using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawler.Context;

namespace WebCrawler
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Inciando crawler");

            using var context = new ApplicationBdContext();
            var crawler = new WebCrawlerService();
            await crawler.IniciarCrawlAsync();

            Console.WriteLine("Crawl concluído com sucesso.");
        }
    }
}
