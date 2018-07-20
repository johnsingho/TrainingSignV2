using System.ComponentModel.DataAnnotations;

namespace TrainingSignWeb.Models
{
    public class TCourseMaintainModel
    {
        [Display(Name = "课程编号 ")]
        public string CourseNO { get; set; }

        [Display(Name = "课程名称 ")]
        public string CourseContext { get; set; }

        [Display(Name = "课程时长 ")]
        public double CourseTime { get; set; }
    }
}