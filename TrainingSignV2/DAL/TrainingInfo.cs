using Common.Data;
using Common.DotNetUI;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using TrainingSignWeb.Common;
using TrainingSignWeb.Database;
using TrainingSignWeb.Models;

namespace TrainingSignWeb.DAL
{
    public class TTrainingInfo
    {
        public Guid id { get; set; }
        public string text { get; set; }
        public string course_no { get; set; }
        public string course_content { get; set; }
        public string organizer { get; set; }
        public string venue { get; set; }
        public string lectors_str { get; set; }
        public string plan_time_str { get; set; }
    }

    public class TrainingInfo
    {
        internal static Tuple<bool, Guid> Insert(string courID,
                                    List<tbl_lector> lectors,
                                    string sOrganizer,
                                    string sVenue,
                                    DateTime dtStart, DateTime dtEnd,
                                    out string serr)
        {
            var res = new Tuple<bool, Guid>(false, Guid.Empty);

            bool bOk = false;
            serr = string.Empty;
            Guid gid = Guid.Empty;
            if (!Guid.TryParse(courID, out gid))
            {
                serr = "Wrong course ID";
                return res;
            }

            Guid newID = Guid.Empty;
            using (var context = new TrainingSign_Entities())
            {
                using (var dbTran = context.Database.BeginTransaction())
                {
                    //insert
                    var entity = new tbl_training()
                    {
                        id = Guid.NewGuid(),
                        ref_course_id = gid,
                        organizer = sOrganizer,
                        venue = sVenue,
                        plan_start_time = dtStart,
                        plan_end_time = dtEnd
                    };
                    try
                    {
                        context.tbl_training.Add(entity);
                        context.SaveChanges();
                        bOk = true;
                    }
                    catch (Exception ex)
                    {
                        //serr = ex.Message;
                        serr = "创建培训失败!";
                        return res;
                    }

                    newID = entity.id;
                    //link lector
                    bOk = bOk && AddLink(context, entity.id, lectors);
                    if (bOk)
                    {
                        dbTran.Commit();
                    }
                }
            }
            return new Tuple<bool, Guid>(bOk, newID);
        }
        
        internal static Tuple<int, float> CalcSum(string sTrainingID)
        {
            var res = new Tuple<int, float>(0, 0.0f);
            Guid gid = Guid.Empty;
            if (!Guid.TryParse(sTrainingID, out gid))
            {
                return res;
            }

            float fCourse = 0.0f;
            int ncnt = 0;
            using (var context = new TrainingSign_Entities())
            {
                var qry = from t in context.tbl_training
                          from r in context.tbl_trainee
                          where t.id == gid
                             && t.id == r.ref_training_id
                          select r;
                ncnt = qry.Count();

                var qry2 = from tt in context.tbl_training
                           where tt.id == gid
                           select tt;
                if (qry2.Any())
                {
                    var item = qry2.First();
                    //实际计数
                    //var diff = DateTime.Now - item.plan_start_time.Value;
                    var dtEnd = item.plan_end_time.HasValue ? item.plan_end_time.Value : DateTime.Now;
                    var diff = dtEnd - item.plan_start_time.Value;
                    if (diff.TotalHours > 0.0f)
                    {
                        fCourse = (float)diff.TotalHours;
                    }
                    else
                    {
                        // 使用标准时长
                        var citem = CourseInfo.GetByID(item.ref_course_id.ToString());
                        if (citem != null)
                        {
                            fCourse = citem.course_time.HasValue ? (float)citem.course_time.Value : 0.0f;
                        }
                    }
                }
            }

            return new Tuple<int, float>(ncnt, fCourse);
        }

        internal static TCourseEntry GetRefCourse(Guid gTrainingID)
        {
            using (var context = new TrainingSign_Entities())
            {
                var qry = from t in context.tbl_training
                          from c in context.tbl_course
                          where t.id==gTrainingID
                             && t.ref_course_id == c.id
                          select c;
                if (qry.Any())
                {
                    var item = qry.First();
                    return new TCourseEntry
                    {
                        id = item.id,
                        timeLen = item.course_time.Value
                    };
                }
                return null;
            }
        }

        //TODO 结束培训时的讲师工号没有作有效性检查
        internal static bool EndTrain(Guid gid, 
                                        int nPlanReach, int nActualReach, 
                                        DateTime dtEndTime,
                                        double fTotTrainingTime, int nPass, 
                                        string endWorkid, 
                                        out string serr)
        {
            serr = string.Empty;
            //Guid gid = Guid.Empty;
            //if (!Guid.TryParse(sTrainingID, out gid))
            //{
            //    serr = "training ID has problem";
            //    return false;
            //}

            var bOk = false;
            using (var context = new TrainingSign_Entities())
            {
                var qry = from t in context.tbl_training
                          where t.id == gid
                          select t;
                if (!qry.Any())
                {
                    serr = "training ID not found!";
                    return false;
                }
                var item = qry.First();
                item.actual_end_time = dtEndTime;
                item.plan_reach = nPlanReach;
                item.actual_reach = nActualReach;
                item.total_training_time = fTotTrainingTime;
                item.pass = nPass;
                item.end_lector_workid = endWorkid;

                try
                {
                    context.SaveChanges();
                    bOk = true;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError(typeof(TrainingInfo), ex);
                    serr = "Close Training failed";
                }
            }
            return bOk;
        }

        private static bool AddLink(TrainingSign_Entities context, Guid trainingID, List<tbl_lector> lectors)
        {
            //TODO 这里还没有检查lectors id的存在性
            foreach (var lec in lectors)
            {
                //关联
                var entry = new tbl_training_lector_link
                {
                    ref_training_id = trainingID,
                    ref_lector_id = lec.id
                };

                if (!context.tbl_training_lector_link.Any(x => x.ref_lector_id == entry.ref_lector_id
                                                          && x.ref_training_id == entry.ref_training_id))
                {
                    try
                    {
                        context.tbl_training_lector_link.Add(entry);
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteError(typeof(TrainingInfo), ex);
                        return false;
                    }
                }
            }
            return true;
        }

        internal static bool DeleteTrainingLog(Guid gTrainingID, out string serr)
        {
            bool bOk = false;
            serr = string.Empty;
            using (var context = new TrainingSign_Entities())
            {
                //(1) delete tbl_trainee
                var its = from p in context.tbl_trainee
                          where p.ref_training_id == gTrainingID
                          select p;
                if (its.Any())
                {
                    context.tbl_trainee.RemoveRange(its);
                }
                try
                {
                    context.SaveChanges();
                    bOk = true;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError(typeof(TrainingInfo), ex);
                    serr = ex.Message;
                    bOk = false;
                    return bOk;
                }

                //(2) delete tbl_training_lector_link
                var links = from p in context.tbl_training_lector_link
                            where p.ref_training_id == gTrainingID
                            select p;
                if (links.Any())
                {
                    context.tbl_training_lector_link.RemoveRange(links);
                }
                try
                {
                    context.SaveChanges();
                    bOk = true;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError(typeof(TrainingInfo), ex);
                    serr = ex.Message;
                    bOk = false;
                    return bOk;
                }

                //(3) delete tbl_training
                var trainings = from p in context.tbl_training
                                where p.id == gTrainingID
                                select p;
                if (trainings.Any())
                {
                    context.tbl_training.RemoveRange(trainings);
                }
                try
                {
                    context.SaveChanges();
                    bOk = true;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError(typeof(TrainingInfo), ex);
                    serr = ex.Message;
                    bOk = false;
                    return bOk;
                }
                return bOk;
            }
        }
        
        //置空此讲师已经讲过的课
        internal static bool UnlinkLector(Guid gLectorID)
        {
            bool bOk = false;
            using (var context = new TrainingSign_Entities())
            {
                var its = from p in context.tbl_training_lector_link
                          where p.ref_lector_id==gLectorID
                          select p;
                if (its.Any())
                {
                    context.tbl_training_lector_link.RemoveRange(its);
                }
                try
                {
                    context.SaveChanges();
                    bOk = true;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError(typeof(TrainingInfo), ex);
                }
            }
            return bOk;
        }

        public static string MakeTimeRangeStr(DateTime? start, DateTime? end)
        {
            if (!start.HasValue)
            {
                return "";
            }
            var sstart = start.Value.ToString("yyyy-MM-dd HH:mm");
            var send = start.Value.ToString("yyyy-MM-dd HH:mm");
            //var send = end.Value.ToString("HH:mm");
            return sstart + "~" + send;
        }

        public static List<TTrainingInfo> LoadUnfinishTraining()
        {
            using (var context = new TrainingSign_Entities())
            {
                var its = ((from tr in context.tbl_training
                            where false == tr.actual_end_time.HasValue
                            join cour in context.tbl_course on tr.ref_course_id equals cour.id
                            select new
                            {
                                id = tr.id,
                                course_no = cour.course_no,
                                course_content = cour.course_context,
                                organizer = tr.organizer,
                                venue = tr.venue,
                                plan_start_time = tr.plan_start_time,
                                plan_end_time = tr.plan_end_time
                            }).AsEnumerable()
                           .Select(x =>new TTrainingInfo{
                                    id = x.id,
                                    course_no = x.course_no,
                                    course_content = x.course_content,
                                    organizer = x.organizer,
                                    venue = x.venue,
                                    lectors_str = string.Join("/", GetCourseLectors(x.course_no)),
                                    plan_time_str = MakeTimeRangeStr(x.plan_start_time, x.plan_end_time),
                                    text = $"【{x.course_no}】 {x.course_content}({MakeTimeRangeStr(x.plan_start_time, x.plan_end_time)})"
                                })
                           ).ToList();
                return its;
            }
        }

        private static List<string> GetCourseLectors(string course_no)
        {
            using (var context = new TrainingSign_Entities())
            {
                var qry = from c in context.tbl_course
                          from l in context.tbl_lector_course_link
                          from lec in context.tbl_lector
                          where c.course_no.Equals(course_no)
                                && c.id.Equals(l.ref_course_id) && lec.id.Equals(l.ref_lector_id)
                          select lec.lector_en_name;
                return qry.ToList();                                
            }
        }

        public class TTrainingByTime
        {
            public Guid id { get; set; }
            public string text { get; set; }
        };

        internal static List<TTrainingByTime> LoadTrainingDateByCourseID(string sCourID, string sMonth)
        {
            var lst = new List<TTrainingByTime>();
            Guid gid = Guid.Empty;
            if (!Guid.TryParse(sCourID, out gid))
            {
                return lst;
            }

            int nYear = 0;
            int nMon = 0;
            int iPos = sMonth.IndexOf('-');
            if (iPos > 0)
            {
                var str = sMonth.Substring(0, iPos);
                nYear = int.Parse(str);
                str = sMonth.Substring(iPos + 1);
                nMon = int.Parse(str);
            }

            using (var context = new TrainingSign_Entities())
            {
                var its = from t in context.tbl_training.AsEnumerable()
                          where t.ref_course_id == gid
                                && t.plan_start_time.HasValue
                                && (t.plan_end_time.Value.Year == nYear && t.plan_start_time.Value.Month == nMon)
                          orderby t.plan_start_time descending
                          select new TTrainingByTime {
                              id=t.id,
                              text = LocalFormatStr.GetLocalTimeStrHHMM(t.plan_start_time.Value)
                                + (t.plan_end_time.HasValue ? "~" + LocalFormatStr.GetLocalTimeStrHHMM(t.plan_end_time.Value) : "")
                          };
                return its.Any() ? its.ToList() : lst;
            }
        }

        private static string MakeStartEndTimeStr(DateTime dtStart, DateTime? dtEnd)
        {
            return dtStart.ToString("yyyy-MM-dd HH:mm")
                    + (dtEnd.HasValue ? "~" + dtEnd.Value.ToString("HH:mm") : "");
        }

        internal static byte[] GetTrainingLog(string sTrainingID)
        {
            using (var context = new TrainingSign_Entities())
            {
                var conn = context.Database.Connection as SqlConnection;
                var sFmt = @"
                            select ROW_NUMBER() OVER(ORDER BY p.signinTime ASC) AS '#',
                                   p.workid, p.name, p.department, p.organizer, 
	                               c.course_no, c.course_context, 
	                               t.venue, dbo.Get_training_lectors_en(t.id) as Lectors,
	                               dbo.Get_Training_Time_str(t.plan_start_time,t.plan_end_time) as Training_Time, Convert(varchar(30),p.signinTime,120) as Signin_Time 
                            from tbl_trainee p, tbl_training t, tbl_course c
                            where p.ref_training_id=t.id
                            and t.ref_course_id=c.id
                            and t.id='{0}';
                            ";
                var sSql = string.Format(sFmt, sTrainingID);
                var dt = SqlServerHelper.ExecuteQuery(conn, sSql);
                return EPPExcelHelper.BuilderExcel(dt);
            }
        }

        //TODO 也许该与GetTrainingLog整合
        internal static List<TTrainingLogInfo> LoadTrainingLog(Guid gTrainingID)
        {
            var lst = new List<TTrainingLogInfo>();
            using (var context = new TrainingSign_Entities())
            {
                var qry = from p in context.tbl_trainee
                          from t in context.tbl_training
                          from c in context.tbl_course
                          where t.id == gTrainingID
                                && p.ref_training_id == t.id
                                && t.ref_course_id == c.id
                          select new
                          {
                              id = p.id,
                              workid = p.workid,
                              cn_name = p.name,
                              dept_name = p.department,
                              organ_name = p.organizer,
                              course_no = c.course_no,
                              course_context = c.course_context,
                              plan_start_time = t.plan_start_time,
                              plan_end_time = t.plan_end_time,
                              signinTime = p.signinTime
                          };
                var its = from x in qry.AsEnumerable()
                          select new TTrainingLogInfo
                          {
                              id=x.id,
                              workid=x.workid,
                              cn_name=x.cn_name,
                              dept_name=x.dept_name,
                              organ_name=x.organ_name,
                              course_no=x.course_no,
                              course_context=x.course_context,
                              training_time = LocalFormatStr.GetLocalTimeStrHHMM(x.plan_start_time.Value)
                                               + (x.plan_end_time.HasValue ? "~" + LocalFormatStr.GetLocalTimeStrHHMM(x.plan_end_time.Value) : ""),
                              signin_time = x.signinTime.HasValue ? LocalFormatStr.GetLocalTimeStr(x.signinTime.Value) : ""
                          };

                return its.Any() ? its.ToList() : lst;
            }
        }

        //下载此课程本月的记录
        internal static byte[] ExportToExcelCourMon(string course_id, DateTime tim)
        {
            using (var context = new TrainingSign_Entities())
            {
                var conn = context.Database.Connection as SqlConnection;
                var sFmt = @"
                            select  p.workid, p.name, p.department, p.organizer, 
	                                c.course_no, c.course_context, 
	                                t.venue, dbo.Get_training_lectors_en(t.id) as Lectors,
	                                dbo.Get_Training_Time_str(t.plan_start_time,t.plan_end_time) as Training_Time, Convert(varchar(30),p.signinTime,120) as Signin_Time 
                            from tbl_trainee p, tbl_training t, tbl_course c
                            where p.ref_training_id=t.id
                            and t.ref_course_id=c.id
                            and c.id='{0}'
                            and substring(CONVERT(varchar(30), p.signinTime, 120), 0, 8)='{1}'
                            order by p.signinTime
                            ";
                var sSql = string.Format(sFmt, course_id, tim.ToString("yyyy-MM"));
                var dr = SqlServerHelper.ExecuteQueryReader(conn, sSql);
                if (!dr.HasRows)
                {
                    return null;
                }
                return EPPExcelHelper.BuilderExcel(dr);
            }
        }

        //下载所有课程本月的记录
        internal static byte[] ExportToExcelMon(DateTime dt)
        {
            using (var context = new TrainingSign_Entities())
            {
                var conn = context.Database.Connection as SqlConnection;
                var sFmt = @"
                            select  p.workid, p.name, p.department, p.organizer, 
	                                c.course_no, c.course_context, 
	                                t.venue, dbo.Get_training_lectors_en(t.id) as Lectors,
	                                dbo.Get_Training_Time_str(t.plan_start_time,t.plan_end_time) as Training_Time, Convert(varchar(30),p.signinTime,120) as Signin_Time 
                            from tbl_trainee p, tbl_training t, tbl_course c
                            where p.ref_training_id=t.id
                            and t.ref_course_id=c.id
                            and substring(CONVERT(varchar(30), p.signinTime, 120), 0, 8)='{0}'
                            order by c.course_no, p.signinTime
                            ";
                var sSql = string.Format(sFmt, dt.ToString("yyyy-MM"));
                var dr = SqlServerHelper.ExecuteQueryReader(conn, sSql);
                if (!dr.HasRows)
                {
                    return null;
                }
                return EPPExcelHelper.BuilderExcel(dr);
            }
        }

        internal static DataTable QueryMonthSummary(string sYearMon, bool bShowEmpty)
        {
            using (var context = new TrainingSign_Entities())
            {
                var conn = context.Database.Connection as SqlConnection;
                var sFmt = string.Empty;
                if (bShowEmpty)
                {
                    sFmt = @"
                        select c.course_no as '课程号', c.course_context as '课程主题', c.course_time as '课程时长',
                                count(distinct(t.id)) as '开课次数',	                               
                                COUNT(tr.name) AS '受训总人数', 
	                            CASE WHEN COUNT(tr.name)>0 THEN sum(c.course_time) ELSE 0 END as '总培训时长'
                        from tbl_course c
                        LEFT JOIN tbl_training t 
	                        ON c.id=t.ref_course_id
	                        AND substring(CONVERT(varchar(30), t.plan_start_time, 120), 0, 8)='{0}' 
                        LEFT JOIN tbl_trainee tr ON tr.ref_training_id=t.id
                        group by c.course_no, c.course_context, c.course_time
                        order by c.course_no;
                        ";
                }
                else
                {
                    sFmt = @"
                        select c.course_no as '课程号', c.course_context as '课程主题', c.course_time as '课程时长',
                                count(distinct(t.id)) as '开课次数',	                               
                                COUNT(tr.name) AS '受训总人数', 
	                            CASE WHEN COUNT(tr.name)>0 THEN sum(c.course_time) ELSE 0 END as '总培训时长'
                        from tbl_course c
                        JOIN tbl_training t 
	                        ON c.id=t.ref_course_id
	                        AND substring(CONVERT(varchar(30), t.plan_start_time, 120), 0, 8)='{0}' 
                        JOIN tbl_trainee tr ON tr.ref_training_id=t.id
                        group by c.course_no, c.course_context, c.course_time
                        order by c.course_no;
                        ";
                }

                var sSql = string.Format(sFmt, sYearMon);
                return SqlServerHelper.ExecuteQuery(conn, sSql);
            }
        }

        //月度统计
        internal static List<TTrainingLogSummary> LoadMonthSummary(string sYearMon, bool bShowEmpty)
        {
            var lst = new List<TTrainingLogSummary>();
            var dt = QueryMonthSummary(sYearMon, bShowEmpty);
            if (null == dt) { return null; }
            var qry = from r in dt.AsEnumerable()
                      select new TTrainingLogSummary
                      {
                          course_no = r["课程号"] as string,
                          course_context = r["课程主题"] as string,
                          course_time = (double)r["课程时长"],
                          summary_time = (double)r["总培训时长"],
                          open_cnt = (int)r["开课次数"],
                          ntrainee = (int)r["受训总人数"]
                      };
            return qry.ToList();
        }
    }
}