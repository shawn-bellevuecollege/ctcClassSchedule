//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CTCClassSchedule.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SubjectsCoursePrefix
    {
        public int SubjectID { get; set; }
        public string CoursePrefixID { get; set; }
    
        public virtual Subject Subject { get; set; }
    }
}