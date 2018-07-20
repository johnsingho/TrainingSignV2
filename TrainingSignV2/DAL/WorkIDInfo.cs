using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using TrainingSignWeb.Database;

namespace TrainingSignWeb.DAL
{
    public class WorkIDInfo
    {
        public class TEmpInfo
        {
            public string empID { get; set; }
            //public string idCardNum { get; set; }
            public string cnName { get; set; }
            public string enName { get; set; }
            public string shortDepartment { get; set; }
        }

        private static string ConvertICCard(string sICCardNo)
        {
            //转换成16进制
            ulong ICCardNo = 0;
            UInt64.TryParse(sICCardNo, out ICCardNo);
            string ICCard_16 = ICCardNo.ToString("X8");
            //取反
            string ICCard_16Cross = ICCard_16.Substring(6, 2) + ICCard_16.Substring(4, 2) + ICCard_16.Substring(2, 2) + ICCard_16.Substring(0, 2);
            return ICCard_16Cross;
        }


        //用来区分是IC读卡器得到的员工卡内芯片号，还是手输的员工卡号
        private static readonly int SNR_LIMIT = 10;

        //根据工号或员工卡内芯片号在一卡通库找用户信息
        public static Customer GetEmployeeECardInfo(string sICCardNo)
        {
            Customer item = null;
            if (sICCardNo.Length >= SNR_LIMIT)
            {
                // IC卡转换成工号
                var ICCard_16Cross = ConvertICCard(sICCardNo);
                item = GetICInfoBySNR(ICCard_16Cross);
            }
            else if (sICCardNo.Length < 5)
            {
                //少于5位的工号认为是无效的
                return null;
            }
            else
            {
                item = GetICInfoByWorkID(sICCardNo);
            }
            return item;
        }
                
        public static TEmpInfo GetEmployeeInfo(string sICCardNo)
        {
            Customer item = GetEmployeeECardInfo(sICCardNo);
            if (item != null)
            {
                var emp = new TEmpInfo();
                emp.empID = item.OutID;
                emp.cnName = item.Name;
                var empInfo = GetEmployeeInfoByWorkID(item.OutID);
                if (null != empInfo)
                {
                    emp.enName = empInfo.EName;
                    emp.shortDepartment = GetShortDepartment(empInfo.Dept_Name, empInfo.Location);
                }
                return emp;
            }
            return null;
        }
        
        public static EmployeeInfo GetEmployeeInfoByWorkID(string sWorkID)
        {
            int nWorkID = int.Parse(sWorkID);
            using (var context = new FlexDevCommonEntities())
            {
                var people = from p in context.EmployeeInfo
                             where p.WD_EmpNo.HasValue && p.WD_EmpNo.Value == nWorkID
                             select p;
                if (people.Any())
                {
                    return people.First();
                }
                return null;
            }
        }

        public static Customer GetICInfoBySNR(string ICCardNo)
        {
            using (var context = new KQXTEntities())
            {
                var recs = from p in context.Customer
                           where 0==string.Compare(p.SCardSNR, ICCardNo, StringComparison.InvariantCultureIgnoreCase)
                           select p;
                if (recs.Any())
                {
                    return recs.First();
                }
                return null;
            }            
        }
        public static Customer GetICInfoByWorkID(string sWorkID)
        {
            using (var context = new KQXTEntities())
            {
                var recs = from p in context.Customer
                           where 0 == string.Compare(p.OutID, sWorkID, StringComparison.InvariantCultureIgnoreCase)
                           select p;
                if (recs.Any())
                {
                    return recs.First();
                }
                return null;
            }
        }

        //TODO
        //简单取短部门名方法，但还有不够完善，有时会截取得不准确
        #region

        private static string Emp_Get_Sign_str(string sloc)
        {
            if (sloc.IndexOf("Mech-FMA") >= 0)
            {
                return "Mech-FMA";
            }
            else if (sloc.IndexOf("Campus Resource") >= 0)
            {
                return "Campus";
            }
            else if (sloc.IndexOf("Regional Resource") >= 0)
            {
                return "Regional";
            }
            else if (sloc.IndexOf("Multek Corporate") >= 0)
            {
                return "Multek-Corporate";
            }
            else if (sloc.IndexOf("Multek B") >= 0)
            {
                return "Multek-B";
            }
            else if (sloc.IndexOf("PCBA B") >= 0)
            {
                return "PCBA-B";
            }
            else if (sloc.IndexOf("PCBA-B11") >= 0)
            {
                return "PCBA-B11";
            }
            else if (sloc.IndexOf("PCBA-HW") >= 0)
            {
                return "PCBA-HW";
            }
            return string.Empty;
        }

        private static string Emp_Get_Short_Deptname(string sDeptnameLong, string sloc)
        {
            if (string.IsNullOrEmpty(sDeptnameLong)) { return string.Empty; }
            var sret = string.Empty;
            var sSign = Emp_Get_Sign_str(sloc);
            int pos = sDeptnameLong.IndexOf(sSign);
            if (pos >= 0)
            {
                pos = sDeptnameLong.IndexOf('-', pos + sSign.Length);
                if (pos > 0)
                {
                    int iBegin = pos + 1;
                    //search for [
                    pos = sDeptnameLong.IndexOf('[', iBegin + 1);
                    if (pos > 0)
                    {
                        sret = sDeptnameLong.Substring(iBegin, pos - iBegin);
                    }
                }
            }
            return sret.Trim();
        }
        
        private static string GetShortDepartment(string dept_Name, string loc)
        {
            return Emp_Get_Short_Deptname(dept_Name, loc);
        }
        #endregion
    }
}