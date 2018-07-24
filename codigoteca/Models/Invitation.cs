using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace codigoteca.Models
{
    public class Invitation
    {

        private CodigotecaDBContext db = new CodigotecaDBContext();

        public int Id { get; set; }
        public int InvitationGroup { get; set; }
        public String From { get; set; }
        public String Invite { get; set; }
        public string InvitationHash { get; set; }

        public DateTime Date { get; set; }
        public String Status { get; set; }
        /*accepted, pending, rejected*/

        public String GroupName()
        {
            var g = db.Groups.Where(a => a.GroupID == this.InvitationGroup).FirstOrDefault();
            return g.GroupName;
        }
    }
}