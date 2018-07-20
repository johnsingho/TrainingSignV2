using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrainingSignWeb.Database;

namespace TrainingSignWeb.DAL
{
    public class SessionInfo
    {
        private static string KEY_COURSES = "SESSION_COURSES";
        private static string KEY_LECTORS = "SESSION_LECTORS";
        private static string KEY_CUR_TRAINING = "SESSION_CUR_TRAINING";
        //private static string KEY_UNFI_TRAINING = "SESSION_UNFI_TRAINING";

        //临时存放的讲师
        internal static object GetTrainingLectors()
        {
            return SessionHelper.Get(KEY_LECTORS);
        }
        internal static void SetTrainingLectors(Dictionary<string, tbl_lector> dLectors)
        {
            SessionHelper.Set(KEY_LECTORS, dLectors);
        }

        //临时存放所有课程
        internal static object GetCourses()
        {
            return SessionHelper.Get(KEY_COURSES);
        }
        internal static void SetCourses(List<TCourseEntry> lst)
        {
            SessionHelper.Set(KEY_COURSES, lst);
        }

        //临时存放的当前培训ID
        internal static object GetCurTraining()
        {
            return SessionHelper.Get(KEY_CUR_TRAINING);
        }
        internal static void SetCurTraining(string sCurID)
        {
            SessionHelper.Set(KEY_CUR_TRAINING, sCurID);
        }

        //临时存放当前未完成培训
        //internal static object GetUnfinishTraining()
        //{
        //    return SessionHelper.Get(KEY_UNFI_TRAINING);
        //}
        //internal static void SetUnfinishTraining(List<TTrainingInfo> lTrains)
        //{
        //    SessionHelper.Set(KEY_UNFI_TRAINING, lTrains);
        //}
    }
}