using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrainingSignWeb.Models
{
    public class TPersonInfo
    {
        public System.Guid id { get; set; }
        public string workid { get; set; }        
        public string cn_name { get; set; }
        public string en_name { get; set; }
        public string org_name { get; set; }
        public string oper_time_str { get; set; }
        public object extra { get; set; }
    }
}