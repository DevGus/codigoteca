using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace codigoteca.Models
{
    public class UserGroups
    {   [Key]
        public int Id { get; set; }
        public int User_UserID { get; set; }
        public int Group_GroupId { get; set; }
    }
}