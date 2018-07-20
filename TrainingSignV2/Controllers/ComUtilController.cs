using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TrainingSignWeb.DAL;
using System;
using DAL;
using Common.DotNetExcel;
using TrainingSignWeb.Models;

namespace TrainingSignWeb.Controllers
{
    public class ComUtilController : Controller
    {
        [HttpPost]
        public ActionResult GetAllCourse()
        {
            var lst = new List<TCourseEntry>();
            var course = CourseInfo.LoadAll();
            if (null == course) { return Json(lst); }

            var qry = from x in course
                      select new TCourseEntry
                      {
                          id = x.id,
                          text = x.course_no + " - " + x.course_context,
                          timeLen = x.course_time.Value
                      };

            return Json(qry.ToList());
        }

        [HttpPost]
        public ActionResult CheckLector(string workid, string courseid)
        {
            var res = new TRes
            {
                data = -1,
                msg = string.Empty
            };
            string serr = string.Empty;
            TPersonInfo lecInfo = null;
            int nRet = LectorInfo.CheckByWorkID(workid, courseid, out lecInfo, out serr);
            res.data = nRet;
            res.msg = serr;
            if (nRet > 0)
            {
                res.extra = lecInfo;
            }
            return Json(res);
        }

        /// <summary>
        /// 学员签到
        /// </summary>
        /// <param name="snr">SNR可能是workID，也可能是SNR码</param>
        /// <param name="trainingid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddTrainee(string snr, string trainingid)
        {
            var res = new TRes
            {
                data = 0,
                msg = string.Empty
            };

            Guid gTrainingID = Guid.Empty;
            if (!Guid.TryParse(trainingid, out gTrainingID))
            {
                res.msg = "培训id无效";
                return Json(res);
            }

            var serr = string.Empty;
            TPersonInfo personInfo = null;
            res.data = TraineeInfo.Add(snr, gTrainingID, out personInfo, out serr);
            res.msg = serr;
            //var cnt = TraineeInfo.GetCountByTrainingID(gTrainingID);
            res.extra = new
            {
                //cnt = cnt,
                personInfo = personInfo
            };

            return Json(res);
        }

        [HttpPost]
        public ActionResult DelTrainee(string sid)
        {
            var res = new TRes
            {
                bok=false,
                msg = string.Empty
            };

            var serr = string.Empty;
            res.bok = TraineeInfo.DeleteByID(sid, out serr);
            res.msg = serr;
            return Json(res);
        }

        //学员列表
        [HttpPost]
        public ActionResult GetTrainees(string training_id)
        {
            var res = new TRes
            {
                bok = false,
                msg = string.Empty
            };

            if (string.IsNullOrEmpty(training_id))
            {
                res.msg = "培训ID必须提供";
                return Json(res);
            }
            Guid gTrainingID = Guid.Empty;
            if (!Guid.TryParse(training_id, out gTrainingID))
            {
                res.msg = "培训ID有问题";
                return Json(res);
            }

            var trainees = TraineeInfo.LoadByTrainingID(gTrainingID, CommonInfo.PAGE_SIZE, -1);
            if (trainees != null)
            {
                //var cnt = TraineeInfo.GetCountByTrainingID(gTrainingID);
                res.data = trainees.Count;
                res.extra = trainees;                
            }
            res.bok = true;
            return Json(res);
        }

        //返回某未完成培训的统计信息
        [HttpPost]
        public ActionResult CalcTrainingInfo(string training_id, bool need_timelen)
        {
            var res = new TRes
            {
                bok = false,
                msg = string.Empty
            };

            if (string.IsNullOrEmpty(training_id))
            {
                res.msg = "培训ID必须提供";
                return Json(res);
            }
            Guid gTrainingID = Guid.Empty;
            if (!Guid.TryParse(training_id, out gTrainingID))
            {
                res.msg = "培训ID有问题";
                return Json(res);
            }

            var nTrainee = TraineeInfo.GetCountByTrainingID(gTrainingID);
            TCourseEntry courInfo = null;
            if (need_timelen)
            {
                courInfo=TrainingInfo.GetRefCourse(gTrainingID);
            }
            res.bok = true;
            res.data = new {
                nTrainee = nTrainee,
                courTimeLen = (null == courInfo) ? 0.0 : courInfo.timeLen
            };
            return Json(res);
        }

        [HttpPost]
        public ActionResult GetUnfinishTrainings()
        {
            var lTrains = TrainingInfo.LoadUnfinishTraining();
            return Json(lTrains);
        }

        [HttpPost]
        public ActionResult GetMonTrainings(string courId, string mon)
        {
            var lTrains = TrainingInfo.LoadTrainingDateByCourseID(courId, mon);
            return Json(lTrains);
        }

        [HttpPost]
        public ActionResult CheckWorkid(string snr)
        {
            var res = new TRes
            {
                bok = false,
                msg = "无效ID"
            };

            var info = WorkIDInfo.GetEmployeeInfo(snr);
            if (null != info)
            {
                res.bok = true;
                res.data = new TPersonInfo
                {
                    workid = info.empID,
                    cn_name = info.cnName,
                    en_name = info.enName,
                    org_name = info.shortDepartment
                };
            }
            return Json(res);
        }

        [HttpPost]
        public ActionResult DelTrainingLog(string training_id)
        {
            var res = new TRes
            {
                bok = false,
                msg = string.Empty
            };            
            Guid gTrainingID = Guid.Empty;
            if (!Guid.TryParse(training_id, out gTrainingID))
            {
                res.msg = "培训ID有问题";
                return Json(res);
            }

            string serr = string.Empty;
            res.bok = TrainingInfo.DeleteTrainingLog(gTrainingID, out serr);
            res.msg = serr;
            return Json(res);
        }

        [HttpPost]
        public ActionResult LoadTrainingLog(string training_id)
        {
            var res = new TRes
            {
                bok = false,
                msg = string.Empty
            };
            Guid gTrainingID = Guid.Empty;
            if (!Guid.TryParse(training_id, out gTrainingID))
            {
                res.msg = "培训ID有问题";
                return Json(res);
            }

            string serr = string.Empty;
            //res.bok = TrainingInfo.DeleteTrainingLog(gTrainingID, out serr);
            res.data = TrainingInfo.LoadTrainingLog(gTrainingID);
            res.bok = true;
            return Json(res);
        }

        #region 下载培训记录
        [HttpGet]
        public ActionResult DownTrainingLog(string training_id, string cour_name, string mon)
        {
            if (string.IsNullOrEmpty(training_id))
            {
                var msg = "培训ID必须提供";
                return View("Error", new Exception(msg));
            }

            if(!string.IsNullOrEmpty(cour_name))
            {
                cour_name = cour_name.Replace(" ", "").Replace("\r", "").Replace("\n", "");
            }
            var fn = "TrainingRec.xlsx";
            var sTrainingTime = mon;
            var dt = DateTime.Now;
            if (DateTime.TryParse(sTrainingTime, out dt))
            {
                sTrainingTime = dt.ToString("yyyyMM");
            }
            fn = string.Format("{0}_{1}.xlsx", 
                string.IsNullOrEmpty(cour_name) ?  "TrainingRec" : cour_name, 
                sTrainingTime);

            var bys = TrainingInfo.GetTrainingLog(training_id);
            return File(bys, ExcelType.XLSX_MIME, fn);
        }

        [HttpGet]
        public ActionResult DownLogCourseMon(string course_id, string cour_name, string mon)
        {
            if (string.IsNullOrEmpty(course_id))
            {
                var msg = "课程ID必须提供";
                return View("Error", new Exception(msg));
            }

            if (!string.IsNullOrEmpty(cour_name))
            {
                cour_name = cour_name.Replace(" ", "").Replace("\r", "").Replace("\n", "");
            }
            var fn = "TrainingRec.xlsx";
            var sTrainingTime = mon;
            var dt = DateTime.Now;
            if (DateTime.TryParse(mon, out dt))
            {
                sTrainingTime = dt.ToString("yyyyMM");
            }
            fn = string.Format("{0}_{1}.xlsx",
                string.IsNullOrEmpty(cour_name) ? "TrainingRec" : cour_name,
                sTrainingTime);

            var bys = TrainingInfo.ExportToExcelCourMon(course_id, dt);
            return File(bys, ExcelType.XLSX_MIME, fn);
        }

        [HttpGet]
        public ActionResult DownLogMon(string mon)
        {
            var fn = "TrainingRec.xlsx";
            var dt = DateTime.Now;
            var sTrainingTime = mon;
            if (!string.IsNullOrEmpty(sTrainingTime))
            {
                if (DateTime.TryParse(sTrainingTime, out dt))
                {
                    sTrainingTime = dt.ToString("yyyyMM");
                }
            }
            fn = string.Format("{0}_{1}.xlsx", sTrainingTime, "AllCourse");
            var bys = TrainingInfo.ExportToExcelMon(dt);
            return File(bys, ExcelType.XLSX_MIME, fn);
        }
        #endregion

        #region 培训记录汇总        
        public ActionResult LoadMonthSummary(string mon, bool showEmpty)
        {
            var res = new TRes
            {
                bok = false,
                msg = string.Empty
            };
            if (string.IsNullOrEmpty(mon))
            {
                res.msg = "汇总月份有问题";
                return Json(res);
            }

            string serr = string.Empty;
            res.data = TrainingInfo.LoadMonthSummary(mon, showEmpty);
            res.bok = true;
            return Json(res);
        }
        #endregion

        #region 课程维护        
        public ActionResult ImportCourse()
        {
            //TODO
        }
        #endregion
    }
}