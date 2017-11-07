using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    /// <summary>
    /// 模擬使用者登入用到的資料模型
    /// </summary>
    public class LoginInformation
    {
        /// <summary>
        /// 帳號
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// 密碼
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 驗證碼
        /// </summary>
        public string VerifyCode { get; set; }
    }
}
