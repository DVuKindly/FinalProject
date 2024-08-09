using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Model
{
    public class UpdateIsActiveViewModel
    {
        public Guid UserId { get; set; }
        public bool IsActive { get; set; }
    }

}
