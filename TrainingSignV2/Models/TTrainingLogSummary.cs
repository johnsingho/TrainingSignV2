using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrainingSignWeb.Models
{
    public class TTrainingLogSummary
    {
        public string course_no { get; set; }
        public string course_context { get; set; }
        public double course_time { get; set; } //课程时长
        public double summary_time { get; set; } //总培训时长
        public int open_cnt { get; set; } //开课次数
        public int ntrainee { get; set; } //受训总人数
    }
}