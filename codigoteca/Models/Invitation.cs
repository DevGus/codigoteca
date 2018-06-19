using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace codigoteca.Models
{
    public class Invitation
    {
        public int Id { get; set; }
        public Group InvitationGroup { get; set; }
        public User Invite { get; set; }
        public string InvitationHash { get; set; }
    }
}