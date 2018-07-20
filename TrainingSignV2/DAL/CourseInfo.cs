using Common.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using TrainingSignWeb.Database;

namespace TrainingSignWeb.DAL
{
    public class TCourseEntry
    {
        public Guid id { get; set; }
        public string text { get; set; }
        public double timeLen { get; set; }
    }

    public class CourseInfo
    {
        public static IList<tbl_course> LoadAll()
        {
            using (var context = new TrainingSign_Entities())
            {
                var recs = from p in context.tbl_course
                           orderby p.course_no
                           select p;
                if (recs.Any())
                {
                    return recs.ToList();
                }
                return null;
            }
        }
        internal static IList<tbl_course> LoadByLectorID(string slecID)
        {
            Guid gid = Guid.Empty;
            if (!Guid.TryParse(slecID, out gid))
            {
                return null;
            }
            using (var context = new TrainingSign_Entities())
            {
                var recs = from c in context.tbl_course
                           from l in context.tbl_lector_course_link
                           from lec in context.tbl_lector
                           where c.id == l.ref_course_id
                                && l.ref_lector_id == lec.id
                                && lec.id == gid
                           select c;
                if (recs.Any())
                {
                    return recs.ToList();
                }
                return null;
            }
        }

        public static bool DeleteByID(string id, out string serr)
        {
            var bOk = false;
            Guid gid = Guid.Empty;
            serr = string.Empty;
            if(!Guid.TryParse(id, out gid))
            {
                return false;
            }

            using (var context = new TrainingSign_Entities())
            {
                var its = from p in context.tbl_course
                          where p.id == gid
                          select p;
                if (its.Any())
                {
                    var obj = its.First();
                    //2018-03-04 由于部署到正式库之前，已经有了有用的数据，但在拷贝库的时候导致约束失效
                    // 所以 FK_TBL_LECT_REFERENCE_TBL_COUR 一直没有起作用
                    // 只能手动检查了
                    var qry = from q in context.tbl_lector_course_link
                              where q.ref_course_id == obj.id
                              select q;
                    if (qry.Any())
                    {
                        serr = "课程已授权给讲师，无法删除。<br>请先取消课程授权！";
                        return false;
                    }

                    var qry2 = from q in context.tbl_training
                              where q.ref_course_id==obj.id
                              select q;
                    if (qry2.Any())
                    {
                        serr = "课程已开过课，无法删除。";
                        return false;
                    }

                    context.tbl_course.Remove(obj);
                }
                try
                {
                    context.SaveChanges();
                    bOk = true;
                }
                catch (Exception ex)
                {
                    serr = "课程已授权给讲师，无法删除。<br>请先取消课程授权！";
                }
            }
            return bOk;
        }
        
        internal static bool Insert(string sCourseNo, string sCourseName, float fCourseTime, out string errmsg)
        {
            bool bOk = false;
            using (var context = new TrainingSign_Entities())
            {
                //var items = from x in context.tbl_course
                //            where 0 == x.name.CompareTo(sname)
                //            select x;
                //if (items.Any())
                //{
                //    errmsg = string.Format("{0} already exist", sname);
                //    return false;
                //}

                //insert
                var entity = new tbl_course()
                {
                    id = Guid.NewGuid(),
                    course_no=sCourseNo,
                    course_context=sCourseName,
                    course_time=fCourseTime
                };
                try
                {
                    context.tbl_course.Add(entity);
                    context.SaveChanges();
                    bOk = true;
                    errmsg = string.Empty;
                }
                catch (Exception ex)
                {
                    //errmsg = ex.Message;
                    errmsg = "课程号已存在，无法插入！";
                }
            }
            return bOk;
        }

        internal static tbl_course GetByID(string sid)
        {
            Guid gid = Guid.Empty;
            if (!Guid.TryParse(sid, out gid))
            {
                return null;
            }

            using (var context = new TrainingSign_Entities())
            {
                var items = from x in context.tbl_course
                            where x.id == gid
                            select x;
                if (items.Any())
                {
                    return items.First();
                }
                return null;
            }
        }

        internal static bool Update(string sid, string sCourseNo, string sCourseName, float fCourseTime, out string serr)
        {
            bool bOk = false;
            Guid gid = Guid.Empty;
            serr = string.Empty;
            if (!Guid.TryParse(sid, out gid))
            {
                return false;
            }

            using (var context = new TrainingSign_Entities())
            {
                var items = from x in context.tbl_course
                            where x.id == gid
                            select x;
                if (!items.Any())
                {
                    serr = string.Format("The item not exist!");
                    return false;
                }

                //update
                var entity = items.First();
                entity.course_no = sCourseNo;
                entity.course_context = sCourseName;
                entity.course_time = fCourseTime;

                try
                {
                    context.SaveChanges();
                    bOk = true;
                }
                catch (Exception ex)
                {
                    serr = ex.Message;
                }
            }
            return bOk;
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="serr"></param>
        /// <returns></returns>
        internal static Tuple<DataTable, Dictionary<string,string>> PrepareData(DataTable dt, out string serr)
        {
            serr = string.Empty;
            if (dt==null || dt.Rows.Count == 0)
            {
                serr= "No data,cannot import";
                return null;
            }
            var colMappings = GetMappingCols();
            var cols = dt.Columns;
            var list = new List<string>();
            foreach (KeyValuePair<string, string> mapping in colMappings)
            {
                if (false == cols.Contains(mapping.Value))
                {
                    list.Add(mapping.Value);
                }
            }
            if (list.Count > 0)
            {
                serr = "以下字段未找到：" + string.Join(",", list);
                return null;
            }
            var groups = dt.Rows.Cast<DataRow>().GroupBy(r => r[colMappings["course_no"]]);
            foreach (var g in groups)
            {
                if (g.Count() > 1)
                {                    
                    serr = string.Format("{0}：{1} duplication", colMappings["course_no"], g.Key);
                    return null;
                }
            }
            var resultDt = BuilderTable();
            var dic = new Dictionary<string, string>();
            foreach (DataRow sourceRow in dt.Rows)
            {
                var newRow = resultDt.NewRow();
                var sgid = Guid.NewGuid().ToString("N");
                newRow["ID"] = sgid;
                foreach (KeyValuePair<string, string> kv in colMappings)
                {
                    var key = kv.Key;
                    bool bInner = 0 == key.IndexOf("inner_");
                    if (bInner) {
                        if (!string.IsNullOrEmpty(kv.Value))
                        {
                            var sCour = sourceRow[colMappings["course_no"]] as string;
                            dic[sCour] = sourceRow[kv.Value] as string;
                        }
                        continue;
                    }

                    var val = sourceRow[kv.Value];                    
                    newRow[kv.Key] = val;
                }
                resultDt.Rows.Add(newRow);
            }

            var ret = new Tuple<DataTable, Dictionary<string, string>>(resultDt, dic);
            return ret;
        }

        private static Dictionary<string, string> GetMappingCols()
        {
            var dic = new Dictionary<string, string>();
            dic.Add("course_no", "Job_Code");
            dic.Add("course_context", "Job_Name");
            dic.Add("course_time", "课时");
            dic.Add("inner_ref_lectors", "讲师资格");
            return dic;
        }
        private static DataTable BuilderTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(Guid));
            dt.Columns.Add("course_no", typeof(string));
            dt.Columns.Add("course_context", typeof(string));
            dt.Columns.Add("course_time", typeof(float));
            return dt;
        }

        internal static bool ImportDatatable(DataTable dt, out string serr)
        {
            using (var context = new TrainingSign_Entities())
            {
                var conn = context.Database.Connection;
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                return DatabaseHelper.ImportDatatable(conn, dt, "tbl_course", out serr);
            }
        }

        internal static void ModifyGrant(string courseId, List<string> lectors)
        {
            if(string.IsNullOrEmpty(courseId) /*|| 0 == lectors.Count*/)
            {
                return;
            }

            //1) clear all grant
            RemoveGrant(courseId, string.Empty);
            //2) grant selected lectors
            LectorCourseLinkInfo.AddLinkByCourseID(courseId, lectors);
        }

        internal static bool RemoveGrant(string courseId, string lectorId)
        {
            if (string.IsNullOrEmpty(lectorId))
            {
                return RemoveGrantAll(courseId);
            }
            return RemoveGrantByLectorID(courseId, lectorId);
        }

        public static bool RemoveGrantByLectorID(string courseId, string lectorId)
        {
            var bOk = false;
            Guid courID = Guid.Empty;
            if (!Guid.TryParse(courseId, out courID))
            {
                return false;
            }
            Guid lecID = Guid.Empty;
            if (!Guid.TryParse(lectorId, out lecID))
            {
                return false;
            }

            using (var context = new TrainingSign_Entities())
            {
                var its = from p in context.tbl_lector_course_link
                          where p.ref_course_id == courID
                                && p.ref_lector_id==lecID
                          select p;
                if (its.Any())
                {
                    context.tbl_lector_course_link.RemoveRange(its);
                    try
                    {
                        context.SaveChanges();
                        bOk = true;
                    }
                    catch (Exception ex)
                    {
                        bOk = false;
                    }
                }
            }
            return bOk;
        }

        private static bool RemoveGrantAll(string courseId)
        {
            var bOk = false;
            Guid gid = Guid.Empty;
            if (!Guid.TryParse(courseId, out gid))
            {
                return false;
            }

            using (var context = new TrainingSign_Entities())
            {
                var its = from p in context.tbl_lector_course_link
                          where p.ref_course_id == gid
                          select p;
                if (its.Any())
                {
                    context.tbl_lector_course_link.RemoveRange(its);
                    try
                    {
                        context.SaveChanges();
                        bOk = true;
                    }
                    catch (Exception ex)
                    {
                        bOk = false;
                    }
                }

            }
            return bOk;
        }
    }
}