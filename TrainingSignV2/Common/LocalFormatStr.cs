using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrainingSignWeb.Common
{
    public class LocalFormatStr
    {
        public static string GetLocalTimeStr(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string GetLocalTimeStrHHMM(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm");
        }

        // 1,123,188
        public static string GetNumDivStr(int num)
        {
            return string.Format("{0:#,0}", num);
        }
    }
}