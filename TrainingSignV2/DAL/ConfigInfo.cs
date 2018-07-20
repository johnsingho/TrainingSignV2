using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace TrainingSignWeb.DAL
{
    public class ConfigInfo
    {
        public static bool DebugRun
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings["DebugRun"]); }
        }
    }
}