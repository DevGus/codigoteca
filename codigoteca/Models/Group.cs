using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace codigoteca.Models
{
    public class Group
    {
        public Group()
        {
            this.UserGroups = new HashSet<User>();
            this.PostGroups = new HashSet<Post>();
        }
        public int GroupID { get; set; }
        [Display (Name = "Nombre del Grupo")]
        public string GroupName { get; set; }
        public DateTime GroupDate { get; set; }

        public virtual ICollection<Post> PostGroups { set; get; }
        public ICollection<User> UserGroups{ get; set; }
        public int Owner { get; set; }
    }
}