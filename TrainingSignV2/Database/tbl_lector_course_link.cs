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
    
    public partial class tbl_lector_course_link
    {
        public System.Guid ref_lector_id { get; set; }
        public System.Guid ref_course_id { get; set; }
        public int id { get; set; }
    
        public virtual tbl_lector tbl_lector { get; set; }
    }
}
