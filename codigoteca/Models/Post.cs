using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace codigoteca.Models
{
    public class Post
    {
        public Post()
        {
            this.PostGroups = new HashSet<Group>();
        }
        public int PostId { get; set; }
        [Display(Name = "Nombre")]
        public string PostName { get; set; }

        [Display(Name = "Descripción")]
        public string PostDescrip { get; set; }

        [Display(Name = "Código")]
        public string PostBody { get; set; }

        public User PostOwner { get; set; }

        public DateTime PostDate { get; set; }

        [Display(Name = "Etiquetas")]
        public List<string> PostLabels { get; set; }

        public Boolean PostVisibility { get; set; }

        public virtual ICollection<Group> PostGroups { get; set; }

        [Display(Name = "Lenguaje")]
        public Language PostLanguage { get; set; }
    }
}