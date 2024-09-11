using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Core.Helpers
{
    public static class CommonUtils
    {
        public static int GCD(int a, int b)
        {
            int remainder;

            while (b != 0)
            {
                remainder = a % b;
                a = b;
                b = remainder;
            }

            return a;
        }
        public static bool IsValidEmail(string email)
        {
			var trimmedEmail = email.Trim();

			if (trimmedEmail.EndsWith("."))
			{
				return false;
			}
			try
			{
				var addr = new System.Net.Mail.MailAddress(email);
				return addr.Address == trimmedEmail;
			}
			catch
			{
				return false;
			}
        }
    }
}
