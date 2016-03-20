using System;
using log4net;
using log4net.Config;

[assembly: XmlConfigurator(Watch = true)]

namespace TestLog4Net
{
    public class LogHelper
    {
        /// <summary>
        ///     输出日志到Log4Net
        /// </summary>
        /// <param name="t"></param>
        /// <param name="ex"></param>

        #region static void WriteLog(Type t, Exception ex)
        public static void WriteLog(Type t, Exception ex)
        {
            var log = LogManager.GetLogger(t);
            log.Error("Error", ex);
        }

        #endregion

        /// <summary>
        ///     输出日志到Log4Net
        /// </summary>
        /// <param name="t"></param>
        /// <param name="msg"></param>

        #region static void WriteLog(Type t, string msg)
        public static void WriteLog(Type t, string msg)
        {
            var log = LogManager.GetLogger(t);
            log.Error(msg);
        }

        #endregion
    }
}