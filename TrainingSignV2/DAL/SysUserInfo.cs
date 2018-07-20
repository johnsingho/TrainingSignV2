using Common.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrainingSignWeb.Database;
using System.Data.Entity;

namespace DAL
{
    public class SysUserInfo
    {
        internal static List<sys_user> LoadAll()
        {
            using (var context = new TrainingSign_Entities())
            {
                var users = from x in context.sys_user select x;
                return users.ToList();
            }
        }

        public static sys_user GetUserInfoByAd(string sAdName)
        {
            sys_user user = null;
            using (var context = new TrainingSign_Entities())
            {
                var people = from p in context.sys_user
                             where (0 == String.Compare(p.ADAccount, sAdName, StringComparison.InvariantCultureIgnoreCase))
                             select p;
                if (people.Any())
                {
                    user = people.First();
                }
            }
            return user;
        }

        public static bool HasOtherUsers()
        {
            //sys_user user = null;
            using (var context = new TrainingSign_Entities())
            {
                var peoples = context.sys_user;
                if (peoples.Any())
                {
                    return true;
                }
            }
            return false;
        }

        private static DomainUserInfo GetAdInfo(string inputad, out string msg)
        {
            var adh = new Common.Authorization.ActiveDirectoryHelper();
            var adUser = adh.GetDomainUserByAD(inputad, out msg);
            return adUser;
        }

        public static bool InsertUserInfo(string inputad, ref string errmsg)
        {
            bool bOk = false;
            var adUser = GetAdInfo(inputad, out errmsg);
            if (adUser == null)
            {
                errmsg = "AD login failed!";
                return false;
            }
            var adInfo = GetUserInfoByAd(inputad);
            if (adInfo != null)
            {
                errmsg = "You had been registered!";
                return false;
            }
            using (var context = new TrainingSign_Entities())
            {
                var entity = new sys_user()
                {
                    ADAccount = adUser.ADAccount,
                    Email = adUser.Email,
                    FullName = adUser.FirstName + ' ' + adUser.LastName,
                    IsAdmin = false,
                    IsValid = true
                };
                try
                {
                    context.sys_user.Add(entity);
                    context.SaveChanges();
                    bOk = true;
                    errmsg = string.Empty;
                }
                catch (Exception ex)
                {
                    errmsg = ex.Message;
                }
            }
            return bOk;
        }

        public static bool EnableUser(int uid, bool bEnabled, out string errmsg)
        {
            bool bOk = false;
            using (var context = new TrainingSign_Entities())
            {
                var persons = from p in context.sys_user
                              where p.id == uid
                              select p;
                foreach (var obj in persons)
                {
                    obj.IsValid = bEnabled;
                }
                try
                {
                    context.SaveChanges();
                    bOk = true;
                    errmsg = string.Empty;
                }
                catch (Exception ex)
                {
                    errmsg = ex.Message;
                }
            }
            return bOk;
        }

        public static bool DeleteUser(int id, out string errmsg)
        {
            bool bOk = false;
            errmsg = string.Empty;
            using (var mContext = new TrainingSign_Entities())
            {
                var persons = from p in mContext.sys_user
                              where p.id == id
                              select p;
                if (persons.Any())
                {
                    var obj = persons.First();
                    mContext.sys_user.Remove(obj);
                    try
                    {
                        mContext.SaveChanges();
                        bOk = true;
                    }
                    catch (Exception ex)
                    {
                        errmsg = ex.Message;
                    }
                }
            }
            return bOk;
        }
        
        public static void UpdateUserLoginTimeByAd(string sAdName)
        {
            using (var context = new TrainingSign_Entities())
            {
                var people = from p in context.sys_user
                             where (0 == String.Compare(p.ADAccount, sAdName, StringComparison.InvariantCultureIgnoreCase))
                             select p;
                if (people.Any())
                {
                    var user = people.First();
                    user.LastLogon = DateTime.Now;
                    context.Entry(user).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
        }

        public static void Update(sys_user user)
        {
            using (var context = new TrainingSign_Entities())
            {
                //user.LastLogon = DateTime.Now;
                context.Entry(user).State = EntityState.Modified;
                context.SaveChanges();
            }
        }
    }
}
