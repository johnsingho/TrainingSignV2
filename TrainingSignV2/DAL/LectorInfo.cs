using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using TrainingSignWeb.Database;
using TrainingSignWeb.Models;

namespace TrainingSignWeb.DAL
{
    /// <summary>
    /// 讲师信息维护
    /// </summary>
    public class LectorInfo
    {
        internal static bool ImportLector_right(Dictionary<string, string> mapCourseLector, out string serr)
        {
            serr = string.Empty;
            try
            {
                foreach (var entry in mapCourseLector)
                {
                    var lectors = entry.Value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    var sCourseNo = entry.Key;
                    if (string.IsNullOrEmpty(sCourseNo) || null==lectors || !lectors.Any())
                    {
                        continue;
                    }
                    foreach (var slec in lectors)
                    {
                        AddByWorkID(slec.Trim());
                    }
                    LectorCourseLinkInfo.AddLink(sCourseNo, lectors);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteInfo(typeof(LectorInfo), ex.Message);
                serr = ex.Message;
                return false;
            }            
            return true;
        }

        //此处逻辑与 CourseInfo.DeleteByID 相似
        internal static bool DeleteByID(string id, out string serr)
        {
            var bOk = false;
            Guid gid = Guid.Empty;
            serr = string.Empty;
            if (!Guid.TryParse(id, out gid))
            {
                return false;
            }

            using (var context = new TrainingSign_Entities())
            {
                var its = from p in context.tbl_lector
                          where p.id == gid
                          select p;
                if (its.Any())
                {
                    var obj = its.First();
                    //2018-03-04 由于部署到正式库之前，已经有了有用的数据，但在拷贝库的时候导致约束失效
                    // 所以 FK_TBL_LECT_REFERENCE_TBL_COUR 一直没有起作用
                    // 只能手动检查了
                    var qry = from q in context.tbl_lector_course_link
                              where q.ref_lector_id == obj.id
                              select q;
                    if (qry.Any())
                    {
                        serr = "讲师已分配课程，无法删除。<br/>请先取消课程授权！";
                        return false;
                    }

                    context.tbl_lector.Remove(obj);
                }
                try
                {
                    TrainingInfo.UnlinkLector(gid); //置空此讲师已经讲过的课
                    context.SaveChanges();
                    bOk = true;
                }
                catch (Exception ex)
                {
                    serr = "讲师已分配课程，无法删除。<br/>请先取消课程授权！";
                }
            }
            return bOk;
        }

        internal static IList<tbl_lector> LoadAll()
        {
            using (var context = new TrainingSign_Entities())
            {
                var recs = from p in context.tbl_lector
                           orderby p.lector_workid
                           select p;
                if (recs.Any())
                {
                    return recs.ToList();
                }
                return null;
            }
        }
        internal static IList<tbl_lector> LoadByCourseID(string sCourID)
        {
            Guid gid = Guid.Empty;
            if (!Guid.TryParse(sCourID, out gid))
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
                                && c.id == gid
                           select lec;
                if (recs.Any())
                {
                    return recs.ToList();
                }
                return null;
            }
        }

        internal static bool AddByWorkID(string sWorkID)
        {
            int nWorkID = int.Parse(sWorkID);
            if (0==nWorkID) { return false; }
            if (null!=GetByWorkID(sWorkID))
            {
                return true;
            }
            using (var context = new FlexDevCommonEntities())
            {
                var people = from p in context.EmployeeInfo
                             where p.WD_EmpNo.HasValue && p.WD_EmpNo.Value==nWorkID
                             select p;
                if (!people.Any())
                {
                    return false;
                }

                var item = people.First();
                if (!item.WD_EmpNo.HasValue) { return false; }
                var serr = string.Empty;
                return InsertLector(item.WD_EmpNo.Value.ToString(), item.EName, item.CName, out serr);
            }
        }

        internal static bool Insert(string sWorkID, string sLectorEn, string sLectorCn, out string errmsg)
        {
            return InsertLector(sWorkID, sLectorEn, sLectorCn, out errmsg);
        }

        private static bool InsertLector(string sWorkID, string eName, string cName, out string serr)
        {
            bool bOk = false;
            serr = string.Empty;
            using (var context = new TrainingSign_Entities())
            {
                //insert
                var entity = new tbl_lector()
                {
                    id = Guid.NewGuid(),
                    lector_workid= sWorkID,
                    lector_en_name=eName,
                    lector_cn_name=cName
                };
                try
                {
                    context.tbl_lector.Add(entity);
                    context.SaveChanges();
                    bOk = true;
                }
                catch (Exception ex)
                {
                    //serr = ex.Message;
                    serr = "此工号已存在，不能重复添加";
                }
            }
            return bOk;
        }
        internal static bool Update(string sid, string sWorkID, string sLectorEn, string sLectorCn, out string serr)
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
                var items = from x in context.tbl_lector
                            where x.id == gid
                            select x;
                if (!items.Any())
                {
                    serr = "The item not exist!";
                    return false;
                }

                //update
                var entity = items.First();
                entity.lector_workid = sWorkID;
                entity.lector_en_name = sLectorEn;
                entity.lector_cn_name = sLectorCn;
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
        
        internal static tbl_lector GetByID(string sid)
        {
            Guid gid = Guid.Empty;
            if (!Guid.TryParse(sid, out gid))
            {
                return null;
            }

            using (var context = new TrainingSign_Entities())
            {
                var items = from x in context.tbl_lector
                            where x.id == gid
                            select x;
                if (items.Any())
                {
                    return items.First();
                }
                return null;
            }
        }

        internal static tbl_lector GetByWorkID(string sWorkID)
        {
            using (var context = new TrainingSign_Entities())
            {
                var people = from p in context.tbl_lector
                             where 0 == string.Compare(p.lector_workid, sWorkID, StringComparison.InvariantCultureIgnoreCase)
                             select p;
                if (people.Any())
                {
                    return people.First();
                }
                return null;
            }
        }

        internal static void ModifyGrant(string slecID, List<string> lSels)
        {
            if (string.IsNullOrEmpty(slecID) /*|| 0 == lSels.Count*/)
            {
                return;
            }

            //1) clear all grant
            RemoveGrant(slecID, string.Empty);
            //2) grant selected lectors
            LectorCourseLinkInfo.AddLinkByLectorID(slecID, lSels);
        }

        internal static bool RemoveGrant(string lectorId, string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                return RemoveGrantAll(lectorId);
            }
            return RemoveGrantByCourseID(lectorId, courseId);
        }

        private static bool RemoveGrantByCourseID(string lectorId, string courseId)
        {
            return CourseInfo.RemoveGrantByLectorID(courseId, lectorId);
        }

        private static bool RemoveGrantAll(string lectorId)
        {
            var bOk = false;
            Guid gid = Guid.Empty;
            if (!Guid.TryParse(lectorId, out gid))
            {
                return false;
            }

            using (var context = new TrainingSign_Entities())
            {
                var its = from p in context.tbl_lector_course_link
                          where p.ref_lector_id == gid
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
        

        /// <summary>
        /// 检查是否讲师或是否已经有授权
        /// </summary>
        /// <returns></returns>
        internal static int CheckByWorkID(string sWorkID, string courseid, out TPersonInfo lecInfo, out string serr)
        {
            lecInfo = null;
            serr = string.Empty;
            var custom = WorkIDInfo.GetEmployeeECardInfo(sWorkID);
            if (null == custom)
            {
                serr = "没有找到员工信息";
                return -1;
            }

            var workid = custom.OutID;
            if (null == GetByWorkID(workid))
            {
                serr = "不是讲师";
                return -1;
            }

            Guid gid = Guid.Empty;
            if (!Guid.TryParse(courseid, out gid))
            {
                serr = "无效课程ID";
                return -1;
            }
            //check for course privilege
            using (var context = new TrainingSign_Entities())
            {
                var people = from p in context.tbl_lector
                             from q in context.tbl_lector_course_link
                             from c in context.tbl_course
                             where string.Compare(p.lector_workid, workid, StringComparison.InvariantCultureIgnoreCase) == 0
                                    && c.id == gid
                                    && q.ref_lector_id == p.id && q.ref_course_id == c.id
                             select new TPersonInfo
                             {
                                 id = p.id,
                                 workid = p.lector_workid,
                                 cn_name = p.lector_cn_name,
                                 en_name = p.lector_en_name
                             };
                if (people.Any())
                {
                    lecInfo = people.First();
                    return 1;
                }
                else
                {
                    serr = "讲师没有此课程的授权";
                    return 0;
                }
            }
        }
    }
}