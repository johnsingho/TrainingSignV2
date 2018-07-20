using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using TrainingSignWeb.Database;

namespace TrainingSignWeb.DAL
{
    /// <summary>
    /// 授权讲师可讲什么课程
    /// </summary>
    public class LectorCourseLinkInfo
    {
        internal static void AddLink(string sCourseNo, string[] lectorWorkIDs)
        {
            using (var context = new TrainingSign_Entities())
            {
                var qp = from p in context.tbl_lector
                             where lectorWorkIDs.Contains(p.lector_workid)
                             select p;
                var qc = from c in context.tbl_course
                             where 0==string.Compare(c.course_no, sCourseNo, StringComparison.InvariantCultureIgnoreCase)
                             select c;
                var people = qp.ToList();
                var course = qc.ToList();
                if (!people.Any() || !course.Any())
                {
                    return;
                }

                foreach (var lec in people)
                {
                    var c = course.First();
                    //关联
                    var entry = new tbl_lector_course_link
                    {
                        ref_course_id = c.id,
                        ref_lector_id = lec.id
                    };

                    if(!context.tbl_lector_course_link.Any(x => x.ref_lector_id == entry.ref_lector_id 
                                                             && x.ref_course_id == entry.ref_course_id))
                    {
                        try
                        {
                            context.tbl_lector_course_link.Add(entry);
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteError(typeof(LectorCourseLinkInfo), ex);
                        }
                    }
                }
            }
        }

        internal static bool AddLinkByCourseID(string courseId, List<string> lectors)
        {
            Guid courID = Guid.Empty;
            if (!Guid.TryParse(courseId, out courID))
            {
                return false;
            }
            using (var context = new TrainingSign_Entities())
            {
                var qc = from c in context.tbl_course
                         where c.id== courID
                         select c;
                if (!qc.Any())
                {
                    return false;
                }

                foreach (var lec in lectors)
                {
                    Guid lecID = Guid.Empty;
                    if (!Guid.TryParse(lec, out lecID))
                    {
                        continue;
                    }

                    //关联
                    var entry = new tbl_lector_course_link
                    {
                        ref_course_id = courID,
                        ref_lector_id = lecID
                    };

                    if (!context.tbl_lector_course_link.Any(x => x.ref_lector_id == entry.ref_lector_id
                                                              && x.ref_course_id == entry.ref_course_id))
                    {
                        try
                        {
                            context.tbl_lector_course_link.Add(entry);
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteError(typeof(LectorCourseLinkInfo), ex);
                        }
                    }
                }
            }
            return true;
        }

        internal static bool AddLinkByLectorID(string slecID, List<string> courses)
        {
            Guid lecID = Guid.Empty;
            if (!Guid.TryParse(slecID, out lecID))
            {
                return false;
            }
            using (var context = new TrainingSign_Entities())
            {
                var ql = from c in context.tbl_lector
                         where c.id == lecID
                         select c;
                if (!ql.Any())
                {
                    return false;
                }

                foreach (var lec in courses)
                {
                    Guid courID = Guid.Empty;
                    if (!Guid.TryParse(lec, out courID))
                    {
                        continue;
                    }

                    //关联
                    var entry = new tbl_lector_course_link
                    {
                        ref_course_id = courID,
                        ref_lector_id = lecID
                    };

                    if (!context.tbl_lector_course_link.Any(x => x.ref_lector_id == entry.ref_lector_id
                                                              && x.ref_course_id == entry.ref_course_id))
                    {
                        try
                        {
                            context.tbl_lector_course_link.Add(entry);
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteError(typeof(LectorCourseLinkInfo), ex);
                        }
                    }
                }
            }
            return true;
        }
    }
}