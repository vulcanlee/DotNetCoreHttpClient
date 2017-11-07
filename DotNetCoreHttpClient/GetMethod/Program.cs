using DataModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GetMethod
{
    class Program
    {
        static async Task Main(string[] args)
        {
            #region 使用 Get 方法呼叫 Web API 的結果
            var foo = await HttpGetAsync();
            Console.WriteLine($"使用 Get 方法呼叫 Web API 的結果");
            Console.WriteLine($"結果狀態 : {foo.Success}");
            Console.WriteLine($"結果訊息 : {foo.Message}");
            var fooAPIData = JsonConvert.DeserializeObject<List<APIData>>(foo.Payload.ToString());
            foreach (var item in fooAPIData)
            {
                Console.WriteLine($"Id : {item.Id}");
                Console.WriteLine($"Name : {item.Name}");
                Console.WriteLine($"Filename : {item.Filename}");
            }
            Console.WriteLine($"");
            #endregion

            Console.WriteLine($"Press any key to Exist...{Environment.NewLine}");
            Console.ReadKey();

        }

        private static async Task<APIResult> HttpGetAsync()
        {
            // 呼叫完成 Web API 之後，所得到的處理結果與訊息
            APIResult fooAPIResult;
            #region Http Handler X 需要套用 using 陳述式
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                #region HttpClient 物件 X 需要套用 using 陳述式
                using (HttpClient client = new HttpClient(handler))
                {
                    try
                    {
                        #region 在 HttpClient 內進行操作的過程，都要進行捕捉例外異常
                        #region 呼叫遠端 Web API
                        string FooUrl = $"http://vulcanwebapi.azurewebsites.net/api/values";
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

                        // 使用 HttpClient 提供的方法，進行呼叫 Web API
                        // 這裡是最會有可能產生例外異常的地方
                        response = await client.GetAsync(fooFullUrl);
                        #endregion
                        #endregion

                        #region 處理呼叫完成 Web API 之後的回報結果
                        if (response != null)
                        {
                            if (response.IsSuccessStatusCode == true)
                            {
                                #region HTTP 狀態碼為成功的型態
                                // 取得回傳結果的 Content-Type 內容，透過這個方法，可以知道取得資料的編碼方式，以便進行解碼
                                // 當然，後端 Web API 必須如實回報此一資訊
                                var fooCT = response.Content.Headers.FirstOrDefault(x => x.Key == "Content-Type").Value.FirstOrDefault();

                                // 取得呼叫完成 API 後的回報內容
                                String strResult = await response.Content.ReadAsStringAsync();
                                // 因為呼叫這個範例 Web API 後台服務，一定會回傳 APIResult 的 JSON 編碼內容
                                // 如此，就可以知道此次後端 Web API 對此次呼叫的執行結果
                                fooAPIResult = JsonConvert.DeserializeObject<APIResult>(strResult, new JsonSerializerSettings { MetadataPropertyHandling = MetadataPropertyHandling.Ignore });
                                #endregion
                            }
                            else
                            {
                                // 在這裡將 Web API 回應不正常狀態的資訊，寫入到執行結果 APIResult 物件內
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
                            // 當發生了不可預期的例外異常
                            fooAPIResult = new APIResult
                            {
                                Success = false,
                                Message = "應用程式呼叫 API 發生異常",
                                Payload = null,
                            };
                        }
                        #endregion
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        // 發生了例外異常，將當時的例外異常資訊記錄下來
                        fooAPIResult = new APIResult
                        {
                            Success = false,
                            Message = ex.Message,
                            Payload = ex,
                        };
                    }
                }
                #endregion
            }
            #endregion

            return fooAPIResult;
        }
    }
}
