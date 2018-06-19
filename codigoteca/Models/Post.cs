using System;
using System.Collections.Generic;
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
        public string PostName { get; set; }
        public string PostDescrip { get; set; }
        public string PostBody { get; set; }
        public User PostOwner { get; set; }
        public DateTime PostDate { get; set; }
        public List<string> PostLabels { get; set; }
        public Boolean PostVisibility { get; set; }
        public virtual ICollection<Group> PostGroups { get; set; }
        public Language PostLanguage { get; set; }
    }
}