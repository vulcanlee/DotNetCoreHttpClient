﻿using DataModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GetQueryStringMethod
{
    class Program
    {
        static async Task Main(string[] args)
        {
            #region 使用 Get 與 QueryString 方法呼叫 Web API 的結果
            #region 呼叫 Web API 得到成功的結果
            var fooAPIData = new APIData()
            {
                Id = 777,
                Name = "Vulcan",
                Filename = "",
            };
            var foo = await GetQueryStringAsync(fooAPIData);
            Console.WriteLine($"使用 Get 與 QueryString 方法呼叫 Web API 的結果");
            Console.WriteLine($"結果狀態 : {foo.Success}");
            Console.WriteLine($"結果訊息 : {foo.Message}");
            Console.WriteLine($"Payload : {foo.Payload}");
            Console.WriteLine($"");

            Console.WriteLine($"Press any key to Exist...{Environment.NewLine}");
            Console.ReadKey();
            #endregion

            #region 呼叫 Web API 得到失敗的結果
            fooAPIData = new APIData()
            {
                Id = 123,
                Name = "Vulcan",
                Filename = "",
            };
            foo = await GetQueryStringAsync(fooAPIData);
            Console.WriteLine($"使用 Get 與 QueryString 方法呼叫 Web API 的結果");
            Console.WriteLine($"結果狀態 : {foo.Success}");
            Console.WriteLine($"結果訊息 : {foo.Message}");
            Console.WriteLine($"Payload : {foo.Payload}");
            Console.WriteLine($"");

            Console.WriteLine($"Press any key to Exist...{Environment.NewLine}");
            Console.ReadKey();
            #endregion
            #endregion

        }

        private static async Task<APIResult> GetQueryStringAsync(APIData apiData)
        {
            APIResult fooAPIResult;
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    try
                    {
                        #region 呼叫遠端 Web API
                        string FooUrl = $"http://vulcanwebapi.azurewebsites.net/api/values/QueryStringGet";
                        HttpResponseMessage response = null;

                        #region  設定相關網址內容
                        var fooFullUrl = $"{FooUrl}";

                        // Accept 用於宣告客戶端要求服務端回應的文件型態 (底下兩種方法皆可任選其一來使用)
                        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept
                        //client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Content-Type 用於宣告遞送給對方的文件型態
                        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Type
                        //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                        #region 組合 QueryString
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add(nameof(apiData.Id), apiData.Id.ToString());
                        dic.Add(nameof(apiData.Name), apiData.Name.ToString());
                        dic.Add(nameof(apiData.Filename), apiData.Filename.ToString());

                        string queryString = "?";

                        foreach (string key in dic.Keys)
                        {
                            queryString += key + "=" + dic[key] + "&";
                        }
                        queryString = queryString.Remove(queryString.Length - 1, 1);

                        #endregion

                        response = await client.GetAsync(fooFullUrl + queryString);
                        #endregion
                        #endregion

                        #region 處理呼叫完成 Web API 之後的回報結果
                        if (response != null)
                        {
                            if (response.IsSuccessStatusCode == true)
                            {
                                // 取得呼叫完成 API 後的回報內容
                                String strResult = await response.Content.ReadAsStringAsync();
                                fooAPIResult = JsonConvert.DeserializeObject<APIResult>(strResult, new JsonSerializerSettings { MetadataPropertyHandling = MetadataPropertyHandling.Ignore });
                            }
                            else
                            {
                                fooAPIResult = new APIResult
                                {
                                    Success = false,
                                    Message = string.Format("Error Code:{0}, Error Message:{1}", response.StatusCode, response.RequestMessage),
                                    Payload = null,
                                };
                            }
                        }
                        else
                        {
                            fooAPIResult = new APIResult
                            {
                                Success = false,
                                Message = "應用程式呼叫 API 發生異常",
                                Payload = null,
                            };
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        fooAPIResult = new APIResult
                        {
                            Success = false,
                            Message = ex.Message,
                            Payload = ex,
                        };
                    }
                }
            }

            return fooAPIResult;
        }
    }
}