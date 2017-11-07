﻿using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleHttpClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            HttpClient client = new HttpClient();
            var fooResult = await client.GetStringAsync("http://vulcanwebapi.azurewebsites.net/api/values/777");
            Console.WriteLine($"{fooResult}");
            Console.WriteLine($"Press any key to Exist...{Environment.NewLine}");
            Console.ReadKey();
        }
    }
}
