using DataModel;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ProgressHandler
{
    delegate void HttpProgressDelegate(object request, HttpProgressEventArgs e);

    class Program
    {
        //public static event EventHandler<HttpProgressEventArgs> HttpReceiveProgress;
        //public static event EventHandler<HttpProgressEventArgs> HttpSendProgress;

        static async Task Main(string[] args)
        {
            //HttpReceiveProgress += Program_HttpReceiveProgress;
            //HttpSendProgress += Program_HttpSendProgress; ;

            #region 訂閱請求與回應的資料傳輸事件
            #region 上傳圖片且有進度回報
            Console.WriteLine($"上傳圖片且有進度回報");
            await UploadImageAsync("vulcan.png", Program_HttpSendProgress, Program_HttpReceiveProgress);
            Console.WriteLine($"Press any key to Exist...{Environment.NewLine}");
            Console.ReadKey();
            #endregion

            #region 下載圖片且有進度回報
            Console.WriteLine($"下載圖片且有進度回報");
            await DownloadImageAsync("vulcan.png", Program_HttpSendProgress, Program_HttpReceiveProgress);
            Console.WriteLine($"Press any key to Exist...{Environment.NewLine}");
            Console.ReadKey();
            #endregion
            #endregion

        }

        private static void Program_HttpSendProgress(object sender, HttpProgressEventArgs e)
        {
            Console.WriteLine($"Send : {e.ProgressPercentage}");
        }

        private static void Program_HttpReceiveProgress(object sender, HttpProgressEventArgs e)
        {
            Console.WriteLine($"Receive : {e.ProgressPercentage}");
        }

        public static async Task<APIResult> UploadImageAsync(string filename,
            HttpProgressDelegate onHttpRequestProgress,
            HttpProgressDelegate onHttpResponseProgress)
        {
            APIResult fooAPIResult;
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                // System.Net.Http.Formatting.Extension
                ProgressMessageHandler progressMessageHandler = new ProgressMessageHandler();
                progressMessageHandler.InnerHandler = handler;
                progressMessageHandler.HttpReceiveProgress += new EventHandler<HttpProgressEventArgs>(onHttpResponseProgress);
                progressMessageHandler.HttpSendProgress += new EventHandler<HttpProgressEventArgs>(onHttpRequestProgress);

                //progressMessageHandler.HttpReceiveProgress += (s, e) =>
                //{
                //    Console.WriteLine($"Receive : {e.ProgressPercentage}");
                //};
                //progressMessageHandler.HttpSendProgress += (s, e) =>
                //{
                //    Console.WriteLine($"Send : {e.ProgressPercentage}");
                //};

                //progressMessageHandler.HttpReceiveProgress += HttpReceiveProgress;
                //progressMessageHandler.HttpSendProgress += HttpSendProgress;
                using (HttpClient client = new HttpClient(progressMessageHandler))
                {
                    try
                    {
                        #region 呼叫遠端 Web API
                        string FooUrl = $"http://vulcanwebapi.azurewebsites.net/api/Upload";
                        HttpResponseMessage response = null;


                        #region  設定相關網址內容
                        var fooFullUrl = $"{FooUrl}";

                        // Accept 用於宣告客戶端要求服務端回應的文件型態 (底下兩種方法皆可任選其一來使用)
                        //client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Content-Type 用於宣告遞送給對方的文件型態
                        //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                        #region 將檔案上傳到網路伺服器上(使用 Multipart 的規範)
                        // 規格說明請參考 https://www.w3.org/Protocols/rfc1341/7_2_Multipart.html
                        using (var content = new MultipartFormDataContent())
                        {
                            var rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                            // 取得這個圖片檔案的完整路徑
                            var path = Path.Combine(rootPath, filename);

                            // 開啟這個圖片檔案，並且讀取其內容
                            using (var fs = File.Open(path, FileMode.Open))
                            {
                                var fooSt = $"My{filename}";
                                var streamContent = new StreamContent(fs);
                                streamContent.Headers.Add("Content-Type", "application/octet-stream");
                                streamContent.Headers.Add("Content-Disposition", "form-data; name=\"files\"; filename=\"" + fooSt + "\"");
                                content.Add(streamContent, "file", filename);

                                // 上傳到遠端伺服器上
                                response = await client.PostAsync(fooFullUrl, content);
                            }
                        }
                        #endregion
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


        private static async Task<APIResult> DownloadImageAsync(string filename,
            HttpProgressDelegate onHttpRequestProgress,
            HttpProgressDelegate onHttpResponseProgress)
        {
            string ImgFilePath = $"My_{filename}";
            ImgFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ImgFilePath);
            APIResult fooAPIResult;
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                ProgressMessageHandler progressMessageHandler = new ProgressMessageHandler();
                progressMessageHandler.InnerHandler = handler;
                progressMessageHandler.HttpReceiveProgress += new EventHandler<HttpProgressEventArgs>(onHttpResponseProgress);
                progressMessageHandler.HttpSendProgress += new EventHandler<HttpProgressEventArgs>(onHttpRequestProgress);

                //progressMessageHandler.HttpReceiveProgress += (s, e) =>
                //{
                //    Console.WriteLine($"Receive : {e.ProgressPercentage}");
                //};
                //progressMessageHandler.HttpSendProgress += (s, e) =>
                //{
                //    Console.WriteLine($"Send : {e.ProgressPercentage}");
                //};
                using (HttpClient client = new HttpClient(progressMessageHandler))
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
                                //byte[] foo = await response.Content.ReadAsByteArrayAsync();
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

















    #region 從 System.Net.Http.Formatting.Extension 套件中抽取出來的原始碼
    // http://aspnetwebstack.codeplex.com/SourceControl/latest#src/System.Net.Http.Formatting/

    /// <summary>
    /// The <see cref="ProgressMessageHandler"/> provides a mechanism for getting progress event notifications
    /// when sending and receiving data in connection with exchanging HTTP requests and responses.
    /// Register event handlers for the events <see cref="HttpSendProgress"/> and <see cref="HttpReceiveProgress"/>
    /// to see events for data being sent and received.
    /// </summary>
    public class ProgressMessageHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressMessageHandler"/> class.
        /// </summary>
        public ProgressMessageHandler()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressMessageHandler"/> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler to which this handler submits requests.</param>
        public ProgressMessageHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        /// <summary>
        /// Occurs every time the client sending data is making progress.
        /// </summary>
        public event EventHandler<HttpProgressEventArgs> HttpSendProgress;

        /// <summary>
        /// Occurs every time the client receiving data is making progress.
        /// </summary>
        public event EventHandler<HttpProgressEventArgs> HttpReceiveProgress;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AddRequestProgress(request);
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (HttpReceiveProgress != null && response != null && response.Content != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await AddResponseProgressAsync(request, response);
            }

            return response;
        }

        /// <summary>
        /// Raises the <see cref="HttpSendProgress"/> event.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="e">The <see cref="HttpProgressEventArgs"/> instance containing the event data.</param>
        protected internal virtual void OnHttpRequestProgress(HttpRequestMessage request, HttpProgressEventArgs e)
        {
            if (HttpSendProgress != null)
            {
                HttpSendProgress(request, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="HttpReceiveProgress"/> event.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="e">The <see cref="HttpProgressEventArgs"/> instance containing the event data.</param>
        protected internal virtual void OnHttpResponseProgress(HttpRequestMessage request, HttpProgressEventArgs e)
        {
            if (HttpReceiveProgress != null)
            {
                HttpReceiveProgress(request, e);
            }
        }

        private void AddRequestProgress(HttpRequestMessage request)
        {
            if (HttpSendProgress != null && request != null && request.Content != null)
            {
                HttpContent progressContent = new ProgressContent(request.Content, this, request);
                request.Content = progressContent;
            }
        }

        private async Task<HttpResponseMessage> AddResponseProgressAsync(HttpRequestMessage request, HttpResponseMessage response)
        {
            Stream stream = await response.Content.ReadAsStreamAsync();
            ProgressStream progressStream = new ProgressStream(stream, this, request, response);
            HttpContent progressContent = new StreamContent(progressStream);
            //response.Content.Headers.CopyTo(progressContent.Headers);
            response.Content = progressContent;
            return response;
        }
    }

    /// <summary>
    /// Provides data for the events generated by <see cref="ProgressMessageHandler"/>.
    /// </summary>
    public class HttpProgressEventArgs : ProgressChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpProgressEventArgs"/> with the parameters given.
        /// </summary>
        /// <param name="progressPercentage">The percent completed of the overall exchange.</param>
        /// <param name="userToken">Any user state provided as part of reading or writing the data.</param>
        /// <param name="bytesTransferred">The current number of bytes either received or sent.</param>
        /// <param name="totalBytes">The total number of bytes expected to be received or sent.</param>
        public HttpProgressEventArgs(int progressPercentage, object userToken, long bytesTransferred, long? totalBytes)
            : base(progressPercentage, userToken)
        {
            BytesTransferred = bytesTransferred;
            TotalBytes = totalBytes;
        }

        /// <summary>
        /// Gets the current number of bytes transferred.
        /// </summary>
        public long BytesTransferred { get; private set; }

        /// <summary>
        /// Gets the total number of expected bytes to be sent or received. If the number is not known then this is null.
        /// </summary>
        public long? TotalBytes { get; private set; }
    }

    /// <summary>
    /// Wraps an inner <see cref="HttpContent"/> in order to insert a <see cref="ProgressStream"/> on writing data.
    /// </summary>
    internal class ProgressContent : HttpContent
    {
        private readonly HttpContent _innerContent;
        private readonly ProgressMessageHandler _handler;
        private readonly HttpRequestMessage _request;

        public ProgressContent(HttpContent innerContent, ProgressMessageHandler handler, HttpRequestMessage request)
        {
            Contract.Assert(innerContent != null);
            Contract.Assert(handler != null);
            Contract.Assert(request != null);

            _innerContent = innerContent;
            _handler = handler;
            _request = request;

            //innerContent.Headers.CopyTo(Headers);
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            ProgressStream progressStream = new ProgressStream(stream, _handler, _request, response: null);
            return _innerContent.CopyToAsync(progressStream);
        }

        protected override bool TryComputeLength(out long length)
        {
            long? contentLength = _innerContent.Headers.ContentLength;
            if (contentLength.HasValue)
            {
                length = contentLength.Value;
                return true;
            }

            length = -1;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _innerContent.Dispose();
            }
        }
    }

    /// <summary>
    /// This implementation of <see cref="DelegatingStream"/> registers how much data has been 
    /// read (received) versus written (sent) for a particular HTTP operation. The implementation
    /// is client side in that the total bytes to send is taken from the request and the total
    /// bytes to read is taken from the response. In a server side scenario, it would be the
    /// other way around (reading the request and writing the response).
    /// </summary>
    internal class ProgressStream : DelegatingStream
    {
        private readonly ProgressMessageHandler _handler;
        private readonly HttpRequestMessage _request;

        private long _bytesReceived;
        private long? _totalBytesToReceive;

        private long _bytesSent;
        private long? _totalBytesToSend;

        public ProgressStream(Stream innerStream, ProgressMessageHandler handler, HttpRequestMessage request, HttpResponseMessage response)
            : base(innerStream)
        {
            Contract.Assert(handler != null);
            Contract.Assert(request != null);

            if (request.Content != null)
            {
                _totalBytesToSend = request.Content.Headers.ContentLength;
            }

            if (response != null && response.Content != null)
            {
                _totalBytesToReceive = response.Content.Headers.ContentLength;
            }

            _handler = handler;
            _request = request;
        }

        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[bufferSize];
            int readCount = 0;
            //InnerStream.Seek(0, SeekOrigin.Begin);
            while ((readCount = await InnerStream.ReadAsync(buffer, 0, bufferSize, cancellationToken)) > 0)
            {
                await destination.WriteAsync(buffer, 0, readCount);
                ReportBytesReceived(readCount, userState: null);
            }
            //await base.CopyToAsync(destination, bufferSize, cancellationToken);
            return;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = InnerStream.Read(buffer, offset, count);
            ReportBytesReceived(bytesRead, userState: null);
            return bytesRead;
        }

        public override int ReadByte()
        {
            int byteRead = InnerStream.ReadByte();
            ReportBytesReceived(byteRead == -1 ? 0 : 1, userState: null);
            return byteRead;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int readCount = await InnerStream.ReadAsync(buffer, offset, count, cancellationToken);
            ReportBytesReceived(readCount, userState: null);
            return readCount;
        }
#if !NETFX_CORE // BeginX and EndX are not supported on streams in portable libraries
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return InnerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            int bytesRead = InnerStream.EndRead(asyncResult);
            ReportBytesReceived(bytesRead, asyncResult.AsyncState);
            return bytesRead;
        }
#endif

        public override void Write(byte[] buffer, int offset, int count)
        {
            InnerStream.Write(buffer, offset, count);
            ReportBytesSent(count, userState: null);
        }

        public override void WriteByte(byte value)
        {
            InnerStream.WriteByte(value);
            ReportBytesSent(1, userState: null);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await InnerStream.WriteAsync(buffer, offset, count, cancellationToken);
            ReportBytesSent(count, userState: null);
        }

#if !NETFX_CORE // BeginX and EndX are not supported on streams in portable libraries
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return new ProgressWriteAsyncResult(InnerStream, this, buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            ProgressWriteAsyncResult.End(asyncResult);
        }
#endif

        internal void ReportBytesSent(int bytesSent, object userState)
        {
            if (bytesSent > 0)
            {
                _bytesSent += bytesSent;
                int percentage = 0;
                if (_totalBytesToSend.HasValue && _totalBytesToSend != 0)
                {
                    percentage = (int)((100L * _bytesSent) / _totalBytesToSend);
                }

                // We only pass the request as it is guaranteed to be non-null (the response may be null)
                _handler.OnHttpRequestProgress(_request, new HttpProgressEventArgs(percentage, userState, _bytesSent, _totalBytesToSend));
            }
        }

        private void ReportBytesReceived(int bytesReceived, object userState)
        {
            if (bytesReceived > 0)
            {
                _bytesReceived += bytesReceived;
                int percentage = 0;
                if (_totalBytesToReceive.HasValue && _totalBytesToReceive != 0)
                {
                    percentage = (int)((100L * _bytesReceived) / _totalBytesToReceive);
                }

                // We only pass the request as it is guaranteed to be non-null (the response may be null)
                _handler.OnHttpResponseProgress(_request, new HttpProgressEventArgs(percentage, userState, _bytesReceived, _totalBytesToReceive));
            }
        }
    }

    /// <summary>
    /// Stream that delegates to inner stream. 
    /// This is taken from System.Net.Http
    /// </summary>
    internal abstract class DelegatingStream : Stream
    {
        private Stream _innerStream;

        protected DelegatingStream(Stream innerStream)
        {
            if (innerStream == null)
            {
                throw new Exception("innerStream");
            }
            _innerStream = innerStream;
        }

        protected Stream InnerStream
        {
            get { return _innerStream; }
        }

        public override bool CanRead
        {
            get { return _innerStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _innerStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _innerStream.CanWrite; }
        }

        public override long Length
        {
            get { return _innerStream.Length; }
        }

        public override long Position
        {
            get { return _innerStream.Position; }
            set { _innerStream.Position = value; }
        }

        public override int ReadTimeout
        {
            get { return _innerStream.ReadTimeout; }
            set { _innerStream.ReadTimeout = value; }
        }

        public override bool CanTimeout
        {
            get { return _innerStream.CanTimeout; }
        }

        public override int WriteTimeout
        {
            get { return _innerStream.WriteTimeout; }
            set { _innerStream.WriteTimeout = value; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _innerStream.Dispose();
            }
            base.Dispose(disposing);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

#if !NETFX_CORE // BeginX and EndX not supported on Streams in portable libraries
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _innerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return _innerStream.EndRead(asyncResult);
        }
#endif

        public override int ReadByte()
        {
            return _innerStream.ReadByte();
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return _innerStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _innerStream.FlushAsync(cancellationToken);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

#if !NETFX_CORE // BeginX and EndX not supported on Streams in portable libraries
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _innerStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _innerStream.EndWrite(asyncResult);
        }
#endif

        public override void WriteByte(byte value)
        {
            _innerStream.WriteByte(value);
        }
    }

    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "_manualResetEvent is disposed in End<TAsyncResult>")]
    internal abstract class AsyncResult : IAsyncResult
    {
        private AsyncCallback _callback;
        private object _state;

        private bool _isCompleted;
        private bool _completedSynchronously;
        private bool _endCalled;

        private Exception _exception;

        protected AsyncResult(AsyncCallback callback, object state)
        {
            _callback = callback;
            _state = state;
        }

        public object AsyncState
        {
            get { return _state; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                Contract.Assert(false, "AsyncWaitHandle is not supported -- use callbacks instead.");
                return null;
            }
        }

        public bool CompletedSynchronously
        {
            get { return _completedSynchronously; }
        }

        public bool HasCallback
        {
            get { return _callback != null; }
        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
        }

        protected void Complete(bool completedSynchronously)
        {
            if (_isCompleted)
            {
                throw new Exception(" Error.InvalidOperation(Properties.Resources.AsyncResult_MultipleCompletes, GetType().Name)");
            }

            _completedSynchronously = completedSynchronously;
            _isCompleted = true;

            if (_callback != null)
            {
                try
                {
                    _callback(this);
                }
                catch (Exception)
                {
                    throw new Exception("Error.InvalidOperation(e, Properties.Resources.AsyncResult_CallbackThrewException)");
                }
            }
        }

        protected void Complete(bool completedSynchronously, Exception exception)
        {
            _exception = exception;
            Complete(completedSynchronously);
        }

        protected static TAsyncResult End<TAsyncResult>(IAsyncResult result) where TAsyncResult : AsyncResult
        {
            if (result == null)
            {
                throw new Exception("Error.ArgumentNull");
            }

            TAsyncResult thisPtr = result as TAsyncResult;

            if (thisPtr == null)
            {
                throw new Exception("Error.Argument(");
            }

            if (!thisPtr._isCompleted)
            {
                thisPtr.AsyncWaitHandle.WaitOne();
            }

            if (thisPtr._endCalled)
            {
                throw new Exception("Error.InvalidOperation(Properties.Resources.AsyncResult_MultipleEnds)");
            }

            thisPtr._endCalled = true;

            if (thisPtr._exception != null)
            {
                throw thisPtr._exception;
            }

            return thisPtr;
        }
    }

    internal class ProgressWriteAsyncResult : AsyncResult
    {
        private static readonly AsyncCallback _writeCompletedCallback = WriteCompletedCallback;

        private readonly Stream _innerStream;
        private readonly ProgressStream _progressStream;
        private readonly int _count;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is handled as part of IAsyncResult completion.")]
        public ProgressWriteAsyncResult(Stream innerStream, ProgressStream progressStream, byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            : base(callback, state)
        {
            Contract.Assert(innerStream != null);
            Contract.Assert(progressStream != null);
            Contract.Assert(buffer != null);

            _innerStream = innerStream;
            _progressStream = progressStream;
            _count = count;

            try
            {
                IAsyncResult result = innerStream.BeginWrite(buffer, offset, count, _writeCompletedCallback, this);
                if (result.CompletedSynchronously)
                {
                    WriteCompleted(result);
                }
            }
            catch (Exception e)
            {
                Complete(true, e);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is handled as part of IAsyncResult completion.")]
        private static void WriteCompletedCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            ProgressWriteAsyncResult thisPtr = (ProgressWriteAsyncResult)result.AsyncState;
            try
            {
                thisPtr.WriteCompleted(result);
            }
            catch (Exception e)
            {
                thisPtr.Complete(false, e);
            }
        }

        private void WriteCompleted(IAsyncResult result)
        {
            _innerStream.EndWrite(result);
            _progressStream.ReportBytesSent(_count, AsyncState);
            Complete(result.CompletedSynchronously);
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ProgressWriteAsyncResult>(result);
        }
    }
    #endregion
}
