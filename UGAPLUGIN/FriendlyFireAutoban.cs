using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendlyFireAutoban
{
    // Finding what the Killer is using LINQ. I'll need to do this sometime. Might not even have to do this at all. 
    internal struct RoleTuple
    {
        public RoleTypeId KillerRole { get; set; }

        public RoleTypeId VictimRole { get; set; }

        public RoleTuple(RoleTypeId killerRole, RoleTypeId victimRole)
        {
            this.KillerRole = killerRole;
            this.VictimRole = victimRole;
        }

        public override string ToString()
        {
            return this.KillerRole.ToString() + "," + (object)this.VictimRole;
        }
    }
}
