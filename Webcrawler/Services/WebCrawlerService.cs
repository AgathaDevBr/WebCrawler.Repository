using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Data.SqlClient;
using WebCrawler.Entities;
using System.Data.SqlClient;
using WebCrawler.Context;


public class WebCrawlerService
{
    private const string UrlBase = "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc";
    private readonly List<ProxyInfo> _proxies = new List<ProxyInfo>();
    private int _totalPaginas = 0;
    private int _totalProxies = 0;

    private ApplicationBdContext _context;

    private SemaphoreSlim _semaphore = new SemaphoreSlim(3);

    public async Task IniciarCrawlAsync()
    {
        var informacaoExecucao = new ExecutionInfo
        {
            StartTime = DateTime.Now
        };

        var tarefas = new List<Task>();

        for (int pagina = 1; pagina <= 10; pagina++)
        {
            var urlPagina = $"{UrlBase}?page={pagina}";
            tarefas.Add(Task.Run(async () =>
            {
                await _semaphore.WaitAsync();
                try
                {
                    await AcessarPaginaAsync(urlPagina);
                }
                finally
                {
                    _semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(tarefas);

        informacaoExecucao.EndTime = DateTime.Now;
        informacaoExecucao.TotalPages = _totalPaginas;
        informacaoExecucao.TotalProxies = _totalProxies;
        informacaoExecucao.JsonFilePath = SalvarJson();

        await _context.SalvarInformacaoExecucaoAsync(informacaoExecucao);

    }

    private async Task AcessarPaginaAsync(string url)
    {
        HttpWebRequest requisicaoWeb = (HttpWebRequest)WebRequest.Create(url);
        requisicaoWeb.Method = "GET";
        requisicaoWeb.UserAgent = "MeuApp/1.0";
        requisicaoWeb.Accept = "application/json";

        requisicaoWeb.Headers.Add("Authorization", "Bearer SEU_TOKEN_AQUI");

        try
        {
            HttpWebResponse resposta = (HttpWebResponse)requisicaoWeb.GetResponse();
            string respostaDados = string.Empty;

            using (StreamReader leitorResposta = new StreamReader(resposta.GetResponseStream()))
            {
                respostaDados = leitorResposta.ReadToEnd();
            }

            File.WriteAllText($"pagina_{_totalPaginas + 1}.html", respostaDados);
            _totalPaginas++;

            ExtrairProxies(respostaDados);

        }
        catch (WebException erroWeb)
        {
            Console.WriteLine($"Erro 403 ou outro erro ao acessar {url}: {erroWeb.Message}");
        }
    }

    private void ExtrairProxies(string htmlPagina)
    {
        var linhas = htmlPagina.Split(new[] { "<tr>" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var linha in linhas)
        {
            if (linha.Contains("<td>"))
            {
                var colunas = linha.Split(new[] { "<td>" }, StringSplitOptions.RemoveEmptyEntries);

                if (colunas.Length >= 4)
                {
                    var proxy = new ProxyInfo
                    {
                        IPAddress = colunas[0].Split(new[] { "</td>" }, StringSplitOptions.None)[0].Trim(),
                        Port = colunas[1].Split(new[] { "</td>" }, StringSplitOptions.None)[0].Trim(),
                        Country = colunas[2].Split(new[] { "</td>" }, StringSplitOptions.None)[0].Trim(),
                        Protocol = colunas[3].Split(new[] { "</td>" }, StringSplitOptions.None)[0].Trim()
                    };

                    _proxies.Add(proxy);
                    _totalProxies++;
                }
            }
        }
    }

    private string SalvarJson()
    {
        var caminhoArquivoJson = "proxies.json";
        var json = JsonConvert.SerializeObject(_proxies, Formatting.Indented);
        File.WriteAllText(caminhoArquivoJson, json);
        return caminhoArquivoJson;
    }
}
