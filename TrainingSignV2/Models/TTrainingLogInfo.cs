using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrainingSignWeb.Models
{
    public class TTrainingLogInfo
    {
        public int id { get; set; }
        public string workid { get; set; }
        public string cn_name { get; set; }
        public string dept_name { get; set; }
        public string organ_name { get; set; }
        public string course_no { get; set; }
        public string course_context { get; set; }
        public string training_time { get; set; }
        public string signin_time { get; set; }
    }
}