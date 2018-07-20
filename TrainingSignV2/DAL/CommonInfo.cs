using Common.Authorization;
using System.Web;
using System;
using System.Security.Policy;

namespace DAL
{
    public class CommonInfo
    {
        public static string SiteTitle = "Activity Sign in System";

        private static UserBasicInfo _currentUser = null;
        private static CookieKey _cook = new CookieKey()
        {
            skUser = "FLEXUSERKEY_2017TrainingSign",
            skSession = "FLEXSESSION_2017TrainingSign",
            skBrowser = "FLEXBK_2017TrainingSign"
        };

        public static int PAGE_SIZE = 10; //分页查询时的大小

        /// <summary>
        /// 获取当前登录的用户, 该值可能为 null
        /// </summary>
        public static UserBasicInfo CurrentUser
        {
            get
            {   
                if (!IsLogin())
                    return null;
                else
                {
                    if (_currentUser == null)
                    {
                        var vUserState = UserState.GetInstance(_cook);
                        _currentUser = vUserState.GetLoginUser();
                    }
                    return _currentUser;
                }
            }
        }

        public static bool IsLogin()
        {
            var vUserState = UserState.GetInstance(_cook);
            return vUserState.IsLogin;
        }

        public static void Logout()
        {
            var vUserState = UserState.GetInstance(_cook);
            vUserState.Logout();
        }

        internal static void Login(UserBasicInfo domainUser)
        {
            var vUserState = UserState.GetInstance(_cook);
            vUserState.Login(domainUser);
        }
    }
}
