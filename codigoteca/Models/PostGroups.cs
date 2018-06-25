using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace codigoteca.Models
{
    public class PostGroups
    {   [Key]
        public int Id { get; set; }
        public int Post_PostId { get; set; }
        public int Group_GroupID { get; set; }
    }
}