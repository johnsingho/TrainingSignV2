//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TrainingSignWeb.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_delete_trainee_log
    {
        public int id { get; set; }
        public System.Guid ref_training_id { get; set; }
        public string workid { get; set; }
        public string name { get; set; }
        public Nullable<System.DateTime> signinTime { get; set; }
        public Nullable<System.DateTime> deleteTime { get; set; }
    }
}
