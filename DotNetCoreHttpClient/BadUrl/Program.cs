using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BadUrl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            #region 將這段程式碼解除註解，了解非同步程式呼叫的例外異常處理方式
            ////一般來說，若採用多執行緒運行，是無法捕捉到這些例外異常
            //try
            //{
            //    HttpClient clientCapture = new HttpClient();
            //    var fooResultCapture = await clientCapture.GetStringAsync("http://vulcanwebapi.azurewebsites.net/api/XXX/777");
            //    Console.WriteLine($"{fooResultCapture}");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"發現例外異常 {ex.Message}");
            //}
            #endregion

            HttpClient client = new HttpClient();
            var fooResult = await client.GetStringAsync("http://vulcanwebapi.azurewebsites.net/api/XXX/777");
            Console.WriteLine($"{fooResult}");
            Console.WriteLine($"Press any key to Exist...{Environment.NewLine}");
            Console.ReadKey();
        }
    }
}
