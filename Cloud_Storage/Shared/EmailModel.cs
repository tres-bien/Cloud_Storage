using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudService.Shared
{
    public class EmailModel
    {
        public string To { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }
}
