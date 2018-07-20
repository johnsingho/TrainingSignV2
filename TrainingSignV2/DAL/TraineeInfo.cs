using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using TrainingSignWeb.Common;
using TrainingSignWeb.Database;
using TrainingSignWeb.Models;

namespace TrainingSignWeb.DAL
{
    public class TraineeInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="snr"></param>
        /// <param name="gTrainingID"></param>
        /// <param name="empInfo"></param>
        /// <param name="serr"></param>
        /// <returns>
        ///    0 -- 失败
        ///    1 -- 签到成功
        ///    -1 -- 已经签到过
        /// </returns>
        internal static int Add(string snr, Guid gTrainingID, out TPersonInfo empInfo, out string serr)
        {
            empInfo = null;
            serr = string.Empty;
            var info = WorkIDInfo.GetEmployeeInfo(snr);
            if (null == info)
            {
                serr = "没有找到此员工信息";
                return 0;
            }

            //insert
            int nRet = -1;
            using (var context = new TrainingSign_Entities())
            {
                //由于部门解释还是有问题，用来避免由于部门名称过长而插入失败，
                //这里的限制与 tbl_trainee的department列一致
                var sDept = StringUtility.LimitStr(info.shortDepartment, 80);
                //insert
                var entity = new tbl_trainee()
                {
                    ref_training_id = gTrainingID
                    ,workid = info.empID
                    ,name = info.cnName
                    ,department = sDept
                    ,signinTime = DateTime.Now
                    //,organizer
                    //,memo
                };

                if (!context.tbl_trainee.Any(x => x.ref_training_id == entity.ref_training_id
                                            && 0==string.Compare(x.workid, entity.workid, StringComparison.InvariantCultureIgnoreCase)))
                {
                    try
                    {
                        context.tbl_trainee.Add(entity);
                        context.SaveChanges();
                        nRet = 1;
                        empInfo = new TPersonInfo{
                            extra = entity.id, //!注意
                            workid = entity.workid,
                            cn_name = entity.name,
                            org_name = entity.department,
                            oper_time_str = LocalFormatStr.GetLocalTimeStr(entity.signinTime.Value)
                        };
                    }
                    catch (Exception ex)
                    {
                        //serr = ex.Message;
                        serr = "添加员工签到失败";
                    }
                }
                else
                {
                    //已经签到过
                    empInfo = new TPersonInfo
                    {
                        workid = entity.workid,
                        cn_name = entity.name,
                        org_name = entity.department,
                        oper_time_str = ""
                    };
                }
            }
            return nRet;
        }

        //取记录数
        internal static int GetCountByTrainingID(Guid gTrainingID)
        {
            using (var context = new TrainingSign_Entities())
            {
                var cnt = context.tbl_trainee.Where(x => x.ref_training_id == gTrainingID).Count();
                return cnt;
            }
        }

        internal static IList<TPersonInfo> LoadByTrainingID(Guid gTrainingID, int pageSize, int iPage=-1)
        {
            using (var context = new TrainingSign_Entities())
            {                
                IQueryable<tbl_trainee> qry = null;
                if (iPage <= 0)
                {
                    qry = from x in context.tbl_trainee
                          where x.ref_training_id == gTrainingID
                          orderby x.signinTime descending
                          select x;
                }
                else
                {
                    qry = (from x in context.tbl_trainee
                              where x.ref_training_id == gTrainingID
                              orderby x.signinTime descending
                              select x
                             ).Skip((iPage - 1) * pageSize).Take(pageSize);
                }
                
                if (qry.Any())
                {
                    var items = from p in qry.AsEnumerable()
                                select new TPersonInfo
                                {
                                    extra = p.id, //注意
                                    workid = p.workid,
                                    cn_name = p.name,
                                    org_name = p.department,
                                    oper_time_str = !p.signinTime.HasValue ? "" : LocalFormatStr.GetLocalTimeStr(p.signinTime.Value)
                                };                    
                    return items.ToList();
                }
                return null;
            }
        }

        public static bool DeleteByID(string sid, out string serr)
        {
            var bOk = false;
            serr = string.Empty;
            int id = -1;            
            if(!int.TryParse(sid, out id))
            {
                return false;
            }

            using (var context = new TrainingSign_Entities())
            {
                var its = from p in context.tbl_trainee
                          where p.id == id
                          select p;
                if (its.Any())
                {
                    var obj = its.First();
                    context.tbl_trainee.Remove(obj);
                    LogDeleteTrainee(obj);
                }
                try
                {
                    context.SaveChanges();
                    bOk = true;
                }
                catch (Exception ex)
                {
                    serr = "删除学员失败！";
                }
            }
            return bOk;
        }

        private static void LogDeleteTrainee(tbl_trainee tr)
        {
            using (var context = new TrainingSign_Entities())
            {

                var entity = new tbl_delete_trainee_log()
                {
                    ref_training_id = tr.ref_training_id,
                    workid = tr.workid,
                    name = tr.name,
                    signinTime = tr.signinTime,
                    deleteTime = DateTime.Now
                };

                try
                {
                    context.tbl_delete_trainee_log.Add(entity);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}