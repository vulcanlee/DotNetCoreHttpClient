﻿using DataModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace DownloadImage
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //foreach (Environment.SpecialFolder spfolder in Enum.GetValues(typeof(Environment.SpecialFolder)))
            //{
            //    var ImgFilePath = "";
            //    ImgFilePath = Path.Combine(Environment.GetFolderPath(spfolder), ImgFilePath);
            //    Console.WriteLine($"{spfolder}  ==> {ImgFilePath}");
            //}

            #region 下載圖片檔案到本機儲存空間內
            var fooResult = await DownloadImageAsync("vulcan.png");
            Process myProcess = new Process();
            try
            {
                // true is the default, but it is important not to set it to false
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = fooResult.Payload.ToString();
                myProcess.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine($"Press any key to Exist...{Environment.NewLine}");
            Console.ReadKey();
            #endregion
        }

        private static async Task<APIResult> DownloadImageAsync(string filename)
        {
            string ImgFilePath = $"My_{filename}";
            // 這裡取得的是原始檔案所在的主目錄
            //ImgFilePath = Path.Combine(Environment.CurrentDirectory, ImgFilePath);
            // 這裡取得的是該組件所在目錄
            ImgFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ImgFilePath);

            
                
                APIResult fooAPIResult;
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    try
                    {
                        #region 呼叫遠端 Web API
                        string FooUrl = $"http://vulcanwebapi.azurewebsites.net/Datas/";
                        HttpResponseMessage response = null;

                        #region  設定相關網址內容
                        var fooFullUrl = $"{FooUrl}{filename}";

                        response = await client.GetAsync(fooFullUrl);
                        #endregion
                        #endregion

                        #region 處理呼叫完成 Web API 之後的回報結果
                        if (response != null)
                        {
                            if (response.IsSuccessStatusCode == true)
                            {
                                using (var filestream = File.Open(ImgFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                                {
                                    using (var stream = await response.Content.ReadAsStreamAsync())
                                    {
                                        stream.CopyTo(filestream);
                                    }
                                }
                                fooAPIResult = new APIResult
                                {
                                    Success = true,
                                    Message = string.Format("Error Code:{0}, Error Message:{1}", response.StatusCode, response.Content),
                                    Payload = ImgFilePath,
                                };
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
