using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Castle.Core.Internal;

namespace BHelp
{
    public class AppRoutines
    {
        public static int GetAge(DateTime dob, [Optional] DateTime today)
        {
            if (today.ToString(CultureInfo.CurrentCulture).IsNullOrEmpty())
            { today = DateTime.Today;};
            TimeSpan span = today - dob;
            // Because we start at year 1 for the Gregorian
            // calendar, we must subtract a year here.
            int years = (DateTime.MinValue + span).Year - 1;
            return years;
        }
    }
}