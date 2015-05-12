using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public static class FormatUtils
    {
        public static String FormatWithSign(this double num)
        {
            return String.Format("{0:+#;-#;0}", num);
        }

        public static String FormatWithSign(this int num)
        {
            return String.Format("{0:+#;-#;0}", num);
        }
    }
}