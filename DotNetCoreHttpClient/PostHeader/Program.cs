using DataModel;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PostHeader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            #region 使用 Post 方法，搭配 JSON 與 Header，呼叫 Web API 的結果
            #region 呼叫 Web API 得到成功的結果 - 帳號密碼與驗證碼(此透過 Header傳送過去)全都正確
            var fooLoginInformation = new LoginInformation()
            {
                Account = "Vulcan",
                Password = "123",
                VerifyCode = "123"
            };
            var foo = await JsonPostAsync(fooLoginInformation, true);
            Console.WriteLine($"使用 Post 方法，搭配 JSON 與 Header，呼叫 Web API 的結果");
            Console.WriteLine($"結果狀態 : {foo.Success}");
            Console.WriteLine($"結果訊息 : {foo.Message}");
            Console.WriteLine($"Payload : {foo.Payload}");
            Console.WriteLine($"");

            Console.WriteLine($"Press any key to Exist...{Environment.NewLine}");
            Console.ReadKey();
            #endregion

            #region 呼叫 Web API 得到失敗的結果  - 帳號密碼與驗證碼(此透過 Header傳送過去)，其中帳號與密碼不正確
            fooLoginInformation = new LoginInformation()
            {
                Account = "Vuln",
                Password = "13",
                VerifyCode = "123"
            };
            foo = await JsonPostAsync(fooLoginInformation, true);
            Console.WriteLine($"使用 Post 方法，搭配 JSON 與 Header，呼叫 Web API 的結果");
            Console.WriteLine($"結果狀態 : {foo.Success}");
            Console.WriteLine($"結果訊息 : {foo.Message}");
            Console.WriteLine($"Payload : {foo.Payload}");
            Console.WriteLine($"");

            Console.WriteLine($"Press any key to Exist...{Environment.NewLine}");
            Console.ReadKey();
            #endregion

            #region 呼叫 Web API 得到失敗的結果  - 帳號密碼與驗證碼(此透過 Header傳送過去)，其中，驗證碼不正確
            fooLoginInformation = new LoginInformation()
            {
                Account = "Vulcan",
                Password = "123",
                VerifyCode = "888"
            };
            foo = await JsonPostAsync(fooLoginInformation, true);
            Console.WriteLine($"使用 Post 方法，搭配 JSON 與 Header，呼叫 Web API 的結果");
            Console.WriteLine($"結果狀態 : {foo.Success}");
            Console.WriteLine($"結果訊息 : {foo.Message}");
            Console.WriteLine($"Payload : {foo.Payload}");
            Console.WriteLine($"");

            Console.WriteLine($"Press any key to Exist...{Environment.NewLine}");
            Console.ReadKey();
            #endregion

            #region 呼叫 Web API 得到失敗的結果 - 帳號密碼與驗證碼(此資料將沒透過 Header傳送過去)，得到 Header 沒有資料的結果
            fooLoginInformation = new LoginInformation()
            {
                Account = "Vulcan",
                Password = "123",
                VerifyCode = "888"
            };
            foo = await JsonPostAsync(fooLoginInformation, false);
            Console.WriteLine($"使用 Post 方法，搭配 JSON 與 Header，呼叫 Web API 的結果");
            Console.WriteLine($"結果狀態 : {foo.Success}");
            Console.WriteLine($"結果訊息 : {foo.Message}");
            Console.WriteLine($"Payload : {foo.Payload}");
            Console.WriteLine($"");

            Console.WriteLine($"Press any key to Exist...{Environment.NewLine}");
            Console.ReadKey();
            #endregion
            #endregion
        }

        /// <summary>
        /// 模擬使用者登入
        /// </summary>
        /// <param name="loginInformation">使用者登入帳密與驗證碼資訊</param>
        /// <param name="sendHeader">是否要傳送驗證碼 Header</param>
        /// <returns></returns>
        private static async Task<APIResult> JsonPostAsync(LoginInformation loginInformation,
            bool sendHeader)
        {
            APIResult fooAPIResult;
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    try
                    {
                        #region 呼叫遠端 Web API
                        string FooUrl = $"http://vulcanwebapi.azurewebsites.net/api/Values/HeaderPost";
                        HttpResponseMessage response = null;

                        #region  設定相關網址內容
                        var fooFullUrl = $"{FooUrl}";
                        if (sendHeader == true)
                        {
                            // 在這裡將會把驗證碼透過 Header 傳送到後端
                            client.DefaultRequestHeaders.Add("VerifyCode", loginInformation.VerifyCode);
                        }

                        // Accept 用於宣告客戶端要求服務端回應的文件型態 (底下兩種方法皆可任選其一來使用)
                        //client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Content-Type 用於宣告遞送給對方的文件型態
                        //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                        var fooJSON = JsonConvert.SerializeObject(loginInformation);
                        using (var fooContent = new StringContent(fooJSON, Encoding.UTF8, "application/json"))
                        {
                            response = await client.PostAsync(fooFullUrl, fooContent);
                        }
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
