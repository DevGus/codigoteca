using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        [Required]
        [Display (Name = "Nombre del Grupo")]
        public string GroupName { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime GroupDate { get; set; }

        public virtual ICollection<Post> PostGroups { set; get; }

        public ICollection<User> UserGroups{ get; set; }
       
        public int Owner { get; set; }

        [NotMapped]
        [DataType(DataType.EmailAddress, ErrorMessage = "El formato del mail no es válido")]
        public String addMail;
    }
}