using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Common.Utility;
using Newtonsoft.Json;
using TrainingSignWeb.Database;
using TrainingSignWeb.DAL;
using TrainingSignWeb.Models;
using DAL;

namespace TrainingSignWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("BeginTraining");
        }
        
        [HttpGet]
        public ActionResult BeginTraining()
        {
            ViewBag.Title = "创建培训";
            ViewBag.CurUrl = Url.Action("BeginTraining");
            return View();
        }

        [HttpPost]
        public ActionResult BeginTraining(string txtOrganizer, string txtVenue, 
                                          string txtFromDate, string txtToDate,
                                          string hidCurCourse,
                                          string hidLectors)
        {
            ViewBag.Title = "Begin Training";
            ViewBag.CurUrl = Url.Action("BeginTraining");
            if (!ModelState.IsValid)
            {
                return View();
            }
            var msg = string.Empty;
            if (string.IsNullOrEmpty(txtVenue)
                || string.IsNullOrEmpty(txtFromDate)
                || string.IsNullOrEmpty(txtToDate)
                || string.IsNullOrEmpty(hidCurCourse)
            )
            {
                msg = "课程,培训地点,起始时间,结束时间不能为空";
                ModelState.AddModelError("", msg);
                return View();
            }

            List<tbl_lector> lectors = null;
            try
            {
                lectors = JsonConvert.DeserializeObject<List<tbl_lector>>(hidLectors);
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(typeof(HomeController), ex);
            }
            if (string.IsNullOrEmpty(hidLectors) || lectors==null || lectors.Count==0)
            {
                msg = "没有添加讲师";
                ModelState.AddModelError("", msg);
                return View();
            }

            var courID = hidCurCourse;
            var sTmStart = txtFromDate.Trim();
            var sTmEnd = txtToDate.Trim();

            var dtStart = DateTime.Parse(sTmStart);
            //TODO copy from new change
            //var dtTemp = DateTime.Parse(sTmEnd);
            //var dtEnd = new DateTime(dtStart.Year, dtStart.Month, dtStart.Day, dtTemp.Hour, dtTemp.Minute, dtTemp.Second);
            var dtEnd = DateTime.Parse(sTmEnd);

            var cour = CourseInfo.GetByID(courID);
            if (null == cour)
            {
                msg = "找不到课程信息";
                ModelState.AddModelError("", msg);
                return View();
            }
            

            var serr = string.Empty;
            var res = TrainingInfo.Insert(courID, lectors, txtOrganizer, txtVenue, dtStart, dtEnd, out serr);
            if (res.Item1)
            {
                SessionInfo.SetTrainingLectors(null);
                SessionInfo.SetCurTraining(res.Item2.ToString());//save current training ID
                return RedirectToAction("LiveSigning");
            }
            else
            {
                msg = "创建培训失败! " + serr;
                ModelState.AddModelError("", msg);
                return View();
            }
        }

        
        [HttpGet]
        public ActionResult TrainingLog()
        {
            ViewBag.Title = "培训记录";
            ViewBag.CurUrl = Url.Action("TrainingLog");
            return View();
        }

        [HttpGet]
        public ActionResult LiveSigning()
        {
            ViewBag.Title = "现场签到";
            ViewBag.CurUrl = Url.Action("LiveSigning");
            return View();
        }

        [HttpGet]
        public ActionResult EndTraining()
        {
            ViewBag.Title = "结束培训";
            ViewBag.CurUrl = Url.Action("EndTraining");
            var model = new TEndTrainingModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult EndTraining(TEndTrainingModel model)
        {
            var res = new TRes
            {
                bok = false,
                msg = ""
            };
            if (!ModelState.IsValid || null==model)
            {
                res.msg = "数据无效";
                ModelState.AddModelError("", res.msg);
                return Json(res);
            }
            if (null==model.curTraining || Guid.Empty==model.curTraining)
            {
                res.msg = "没有选择培训或者培训ID无效";
                ModelState.AddModelError("", res.msg);
                return Json(res);
            }
            if (string.IsNullOrEmpty(model.endLector))
            {
                res.msg = "请讲师刷卡结束培训";
                ModelState.AddModelError("", res.msg);
                return Json(res);
            }
            //if (string.IsNullOrEmpty(model.planReach))
            //{
            //    res.msg = "请填写计划到场人数";
            //    ModelState.AddModelError("", res.msg);
            //    return Json(res);
            //}
            //if (string.IsNullOrEmpty(model.actualReach))
            //{
            //    res.msg = "请填写实际到场人数";
            //    ModelState.AddModelError("", res.msg);
            //    return Json(res);
            //}

            int nPlanReach = 0;
            int nActualReach = 0;
            int nPass = 0;
            var fTotTrainingTime = 0.0;

            //double.TryParse(txtTotTrainingTime, out fTotTrainingTime);
            //int.TryParse(txtPlanReach, out nPlanReach);
            //int.TryParse(txtActualReach, out nActualReach);
            //int.TryParse(txtPass, out nPass);
            nPlanReach = model.planReach;
            nActualReach = model.actualReach;
            nPass = model.pass;
            fTotTrainingTime = model.totTrainingTime;

            var serr = string.Empty;
            if (!TrainingInfo.EndTrain(model.curTraining,
                                        nPlanReach, nActualReach,
                                        DateTime.Now,
                                        fTotTrainingTime, nPass,
                                        model.endLector,
                                        out serr))
            {
                res.msg = serr;
                return Json(res);
            }
            else
            {
                res.bok = true;
                return Json(res);
            }
        }

        [HttpGet]
        public ActionResult CourseMaintain()
        {
            if (!CommonInfo.IsLogin())
            {
                return RedirectToAction("Index");
            }
            ViewBag.Title = "课程维护";
            ViewBag.CurUrl = Url.Action("CourseMaintain");
            var model = new TCourseMaintainModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult CourseMaintain(TCourseMaintainModel model)
        {
            //TODO add new
            //if (!CommonInfo.IsLogin())
            //{
            //    return RedirectToAction("Index");
            //}
            //ViewBag.Title = "课程维护";
            //ViewBag.CurUrl = Url.Action("CourseMaintain");
            return View();
        }

        [HttpGet]
        public ActionResult LectorMaintain()
        {
            if (!CommonInfo.IsLogin())
            {
                return RedirectToAction("Index");
            }
            ViewBag.Title = "讲师维护";
            ViewBag.CurUrl = Url.Action("LectorMaintain");
            return View();
        }

        //private static List<TReceivingDiff>  FilterReceivingCmpData(List<TReceivingDiff> lst, TReceivingCmpPara tPara)
        //{
        //    List<TReceivingDiff> res = null;
        //    if (string.IsNullOrEmpty(tPara.sOutgoingNo)
        //      && string.IsNullOrEmpty(tPara.sINV)
        //      && string.IsNullOrEmpty(tPara.sPO)
        //      && string.IsNullOrEmpty(tPara.sPN)
        //      && string.IsNullOrEmpty(tPara.sCDFCODE)
        //      && string.IsNullOrEmpty(tPara.sGRN)
        //      )
        //    {
        //        res = lst;
        //    }
        //    else
        //    {
        //        //过滤
        //        var qry = from x in lst
        //                  where (
        //                      (string.IsNullOrEmpty(tPara.sOutgoingNo) || x.Outgoing.IndexOf(tPara.sOutgoingNo, StringComparison.InvariantCultureIgnoreCase)>=0)
        //                      && (string.IsNullOrEmpty(tPara.sINV) || x.INV.IndexOf(tPara.sINV, StringComparison.InvariantCultureIgnoreCase) >= 0)
        //                      && (string.IsNullOrEmpty(tPara.sPO) || x.PO.IndexOf(tPara.sPO, StringComparison.InvariantCultureIgnoreCase) >= 0)
        //                      && (string.IsNullOrEmpty(tPara.sPN) || x.PN.IndexOf(tPara.sPN, StringComparison.InvariantCultureIgnoreCase) >= 0)
        //                      && (string.IsNullOrEmpty(tPara.sCDFCODE) || x.CDFCODE.IndexOf(tPara.sCDFCODE, StringComparison.InvariantCultureIgnoreCase) >= 0)
        //                      && (string.IsNullOrEmpty(tPara.sGRN) || 
        //                           ((null!=x.GRN) && x.GRN.IndexOf(tPara.sGRN, StringComparison.InvariantCultureIgnoreCase)>=0))
        //                  )
        //                  select x;
        //        res = qry.ToList();
        //    }
        //    return res;
        //}

        //[HttpPost]
        //public JsonResult GetReceivingCmpData(TReceivingCmpPara tPara)
        //{
        //    var res = new TRes
        //    {
        //        bok = false
        //    };
        //    var cacheRes = ReceivingCmpHelper.GetData();
        //    if(null==cacheRes || null == cacheRes.allRecs)
        //    {
        //        return Json(res, JsonRequestBehavior.AllowGet);
        //    }

        //    var lst = FilterReceivingCmpData(cacheRes.allRecs, tPara);
        //    if (lst != null)
        //    {
        //        res.bok = true;
        //        res.data = lst;
        //    }

        //    return Json(res, JsonRequestBehavior.AllowGet);
        //}        

        ///// <summary>
        ///// 用户上传excel文件，然后从BAAN取数据，最后分别填充数据表
        ///// </summary>
        ///// <param name="ctlBaanDate">取Baan数据的起始时间</param>
        ///// <param name="useLastXls">是否使用是一次上传的excel数据</param>
        ///// <param name="file">上传的excel文件</param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult UploadCmpCalc(string ctlBaanDate, bool? useLastXls, HttpPostedFileBase file)
        //{
        //    var res = new TRes
        //    {
        //        bok = false,
        //        msg =""
        //    };

        //    var bUseLastXls = useLastXls.HasValue ? useLastXls.Value : false;
        //    if ((file==null || file.ContentLength==0) && !bUseLastXls)
        //    {
        //        res.msg = "文件有问题";
        //        return Json(res, JsonRequestBehavior.AllowGet);
        //    }

        //    var dtBegin = DateTime.Now.AddDays(-3);
        //    DateTime.TryParse(ctlBaanDate, out dtBegin);

        //    var sErrSync = string.Empty;
        //    var sErrImp = string.Empty;
        //    var tSyncBaan = new Task<bool>(() => ReceivingCmpHelper.SyncBaanData(dtBegin, out sErrSync));
        //    tSyncBaan.Start();

        //    Task<bool> tImpUpload = null;
        //    if (!bUseLastXls)
        //    {
        //        tImpUpload = new Task<bool>(() => ReceivingCmpHelper.ImpUpload(file, out sErrImp));
        //        tImpUpload.Start();
        //    }

        //    tSyncBaan.Wait();
        //    var bImp = true; 
        //    if (!bUseLastXls)
        //    {
        //        tImpUpload.Wait();
        //        bImp = tImpUpload.Result;
        //    }

        //    var bSync = tSyncBaan.Result;            
        //    if(!bImp || !bSync)
        //    {
        //        if (!bImp)
        //        {
        //            res.msg += "\r\n" + sErrImp;
        //        }
        //        if (!bSync)
        //        {
        //            res.msg += "\r\n" + sErrSync;
        //        }
        //        return Json(res, JsonRequestBehavior.AllowGet);
        //    }

        //    res.bok = true;
        //    return Json(res, JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]
        //public ActionResult DownloadRecvCmp(TReceivingCmpPara tPara)
        //{
        //    var res = new TRes
        //    {
        //        bok = false,
        //        msg = "no data",
        //        data = ""
        //    };
        //    var cacheRes = ReceivingCmpHelper.GetData();
        //    if (null == cacheRes || null == cacheRes.allRecs)
        //    {
        //        return Json(res, JsonRequestBehavior.AllowGet);
        //        //return new HttpStatusCodeResult(System.Net.HttpStatusCode.NoContent);
        //    }

        //    var lst = FilterReceivingCmpData(cacheRes.allRecs, tPara);
        //    if (lst != null)
        //    {
        //        var str = string.Empty;
        //        res.bok = ReceivingCmpHelper.SaveTempExcelFile(lst, out str);
        //        if (res.bok) {
        //            res.data = str;
        //        }
        //        else
        //        {
        //            res.msg = str;
        //        }
        //    }
        //    return Json(res, JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet]
        //public ActionResult Download(string file)
        //{
        //    var sPath = ReceivingCmpHelper.MakeTempFilePath(file);
        //    var bys = ReceivingCmpHelper.RetrieveTempFileBytes(sPath);
        //    return File(bys, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file);
        //}

        //[HttpPost]
        //public JsonResult GetCalcTime()
        //{
        //    var dt = default(DateTime);
        //    var res = new TRes
        //    {
        //        bok = true
        //    };
        //    var cacheRes = ReceivingCmpHelper.GetData();
        //    if (null!=cacheRes)
        //    {
        //        dt = cacheRes.calcDate;
        //    }
        //    res.data = LocalFormatStr.GetLocalTimeStr(dt);
        //    return Json(res, JsonRequestBehavior.AllowGet);
        //}
    }
}
