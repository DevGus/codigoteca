using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre es obligatorio")]
        public string PostName { get; set; }

        [Display(Name = "Descripción")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "La descripción es obligatoria")]
        public string PostDescrip { get; set; }

        [Display(Name = "Código")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Debes ingresar el código")]
        public string PostBody { get; set; }

        public int PostOwner { get; set; }

        public DateTime PostDate { get; set; }

        [Display(Name = "Etiquetas")]
        public List<string> PostLabels { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Debes seleccionar la visibilidad del posteo")]
        public int PostVisibility { get; set; }

        public virtual ICollection<Group> PostGroups { get; set; }

        [Display(Name = "Lenguaje")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "El lenguaje es obligatorio")]
        public Language PostLanguage { get; set; }
        
    }
}