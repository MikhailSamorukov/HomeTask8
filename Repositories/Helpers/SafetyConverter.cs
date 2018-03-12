using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Helpers
{
    internal static class SafetyConverter
    {
        public static int? ToInt(this object value)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return null;
            }
        }
        public static decimal? ToDecimal(this object value)
        {
            try
            {
                return Convert.ToDecimal(value);
            }
            catch
            {
                return null;
            }
        }
        public static double? ToDouble(this object value)
        {
            try
            {
                return Convert.ToDouble(value);
            }
            catch
            {
                return null;
            }
        }
        public static DateTime? ToDateTime(this object value)
        {
            try
            {
                return Convert.ToDateTime(value);
            }
            catch
            {
                return null;
            }
        }
    }
}
