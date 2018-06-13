using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace IBExpertUpdate
{
    class Program
    {
        const string LINK_DOWNLOAD = "https://portal.ibexpert.net/file.php?CMD=get-product&product=2182";
        static Stopwatch cronometro = new Stopwatch();

        static void Main(string[] args)
        {
            const string ENV_EMAIL = "IBEXPERT_EMAIL";
            const string ENV_PASSWORD = "IBEXPERT_PASSWORD";
            const string MENSAGEM_ERRO_LOGIN = "Your username/password combination is invalid";

            var cliente = new HttpClient
            {
                Timeout = new TimeSpan(0, 5, 0),
            };
            const string URL_LOGIN = "https://portal.ibexpert.net/?CMD=login&redirect=home";

            try
            {
                var email = Environment.GetEnvironmentVariable(ENV_EMAIL) ?? 
                    throw new Exception($"Variável de ambiente {ENV_EMAIL} não definida.");

                var senha = Environment.GetEnvironmentVariable(ENV_PASSWORD) ?? 
                    throw new Exception($"Variável de ambiente {ENV_PASSWORD} não definida.");

                var formLogin = GerarFormulariologin(email, senha);

                var respostaLogin = cliente.PostAsync(URL_LOGIN, formLogin).Result;

                if (respostaLogin.Content.ReadAsStringAsync().Result.Contains(MENSAGEM_ERRO_LOGIN))
                {
                    throw new Exception("E-mail ou senha inválidos.");
                }

                if (!respostaLogin.IsSuccessStatusCode)
                {
                    throw new Exception($"Não foi possível fazer o login: {respostaLogin.ReasonPhrase}");
                }
                Console.WriteLine("Baixando...");
                cronometro.Start();
                if (respostaLogin.Headers.TryGetValues("Set-Cookie", out IEnumerable<string> cookies))
                {
                    var cookie = cookies.FirstOrDefault();
                    Download("setup_personal.exe", cookie);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} {ex.InnerException?.Message}");
                Environment.Exit(1);
            }
        }

        private static HttpContent GerarFormularioDownload()
        {
            var formulario = new Dictionary<string, string>
            {
                { "accept-personal", "true" }
            };
            return new FormUrlEncodedContent(formulario);
        }

        private static void Download(string caminho, string cookie)
        {
            using (var cliente = new WebClient())
            {
                cliente.Headers["Cookie"] = cookie;
                cliente.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                cliente.UploadProgressChanged += Cliente_UploadProgressChanged;
                var resposta = cliente.UploadStringTaskAsync(new Uri(LINK_DOWNLOAD), "accept-personal=true").Result;
                var bytes = Encoding.Default.GetBytes(resposta);
                using (var arquivo = new FileStream(caminho, FileMode.Create))
                {
                    arquivo.Write(bytes, 0, bytes.Length);
                }
            }
        }

        private static void Cliente_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            var porcentagem = string.Format("{0:0}%", 100 * e.BytesReceived / e.TotalBytesToReceive);
            var baixados = string.Format("{0:0.00} MB", e.BytesReceived / 1024.0 / 1024.0);
            var velocidade = string.Format("{0} Mb/s", (e.BytesReceived / 1024.0 / 1024.0 * 8 / cronometro.Elapsed.TotalSeconds).ToString("0.00"));
            var mensagem = $"{porcentagem} - {baixados} - {velocidade}";
            Console.Write(new string('\b', mensagem.Length) + mensagem);
        }

        private static HttpContent GerarFormulariologin(string email, string senha)
        {
            var formulario = new Dictionary<string, string>
            {
                { "email", email },
                { "password", senha }
            };
            return new FormUrlEncodedContent(formulario);
        }
    }
}
