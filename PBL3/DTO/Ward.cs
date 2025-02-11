﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBL3.DTO
{
    [Table("Ward")]
    public class Ward
    {
        [Key]
        [Required]
        public int WardID { get; set; }

        [Required]
        public string WardName { get; set; }

        public int DistrictID { get; set; }
        [ForeignKey("DistrictID")]
        public virtual District District { get; set; }

        public virtual ICollection<University> Universities { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }

        public Ward()
        {
            this.Universities = new HashSet<University>();
            this.Addresses = new HashSet<Address>();
        }
    }
}
