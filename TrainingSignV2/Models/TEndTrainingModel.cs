using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrainingSignWeb.Models
{
    public class TEndTrainingModel
    {
        [Display(Name="待受训人员数 ")]
        public int planReach { get; set; }
        [Display(Name = "签到人数 ")]
        public int actualReach { get; set; }
        [Display(Name = "总培训小时数 ")]
        public double totTrainingTime { get; set; }
        [Display(Name = "合格人数 ")]
        public int pass { get; set; }
        [Display(Name = "讲师刷卡 ")]
        public string endLector { get; set; }

        public Guid curTraining { get; set; }
    }
}