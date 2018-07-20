using System.Web;
using System.Web.Optimization;

namespace TrainingSignWeb
{
    public class BundleConfig
    {
        // 有关 Bundling 的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // 使用 Modernizr 的开发版本进行开发和了解信息。然后，当你做好
            // 生产准备时，请使用 http://modernizr.com 上的生成工具来仅选择所需的测试。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));
            bundles.Add(new ScriptBundle("~/bundles/bs").Include("~/Scripts/bootstrap.js"));
            bundles.Add(new StyleBundle("~/Content/bs")
                        .Include("~/Content/bootstrap.css")
                        .Include("~/Content/bootstrap-theme.css")
                        );
            bundles.Add(new ScriptBundle("~/bundles/my")
                        .Include("~/Scripts/datatables.min.js")
                        .Include("~/Scripts/bootstrap-dialog.min.js")
                        .Include("~/Scripts/jquery.datetimepicker.full.min.js")
                        .Include("~/Scripts/bootstrap-select.js")
                        .Include("~/Scripts/select2.min.js")
                        .Include("~/Scripts/i18n/zh_CN.js")
                        .Include("~/Scripts/moment.min.js")
                        );
            bundles.Add(new StyleBundle("~/Content/my")
                        .Include("~/Content/datatables.min.css")
                        .Include("~/Content/bootstrap-dialog.min.css")
                        .Include("~/Content/font-awesome/css/font-awesome.min.css")
                        .Include("~/Content/bootstrap-select.min.css")
                        .Include("~/Content/select2.min.css")
                        .Include("~/Content/select2-bootstrap.min.css")
                        .Include("~/Content/jquery.datetimepicker.min.css")
                        );

            bundles.Add(new ScriptBundle("~/bundles/main")
                            .Include("~/Scripts/main.js"));

            bundles.Add(new ScriptBundle("~/bundles/My97DatePicker")
                        .Include("~/Scripts/My97DatePicker/WdatePicker.js")
                        .Include("~/Scripts/My97DatePicker/lang/zh-cn.js")
                        );
        }
    }
}