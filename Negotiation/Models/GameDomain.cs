//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Negotiation.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class GameDomain
    {
        public GameDomain()
        {
            this.DomainVariant = new HashSet<DomainVariant>();
        }
    
        public int Id { get; set; }
        public string DomainXML { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<DomainVariant> DomainVariant { get; set; }
    }
}
