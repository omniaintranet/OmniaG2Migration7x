using Omnia.Fx.Models.Apps;
using Omnia.Migration.Core.Http;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Omnia.Migration.App.Helpers
{
    public static class ConsoleHelper
    {
        public static int PromptForOptions(string message, string[] options)
        {
            Console.WriteLine(message);
            for (int i = 0; i < options.Length; i++)
            {
                Console.WriteLine($"  ({i}) {options[i]}");
            }

            var selectedOptionStr = ReadLine.Read();
            if (!int.TryParse(selectedOptionStr, out int selectedOption))
            {
                Console.WriteLine("Input must be a number.");
                return PromptForOptions(message, options);
            }
            else if (selectedOption >= options.Length || selectedOption < 0)
            {
                Console.WriteLine($"Input must be a number between 0 and {options.Length - 1}.");
                return PromptForOptions(message, options);
            }
            else
            {
                return selectedOption;
            }
        }

        public static int PromptForOptions(string message, Type enumType)
        {
            string[] options = enumType
                .GetEnumNames()
                .Select(x => Regex.Replace(x, "[A-Z]", " $0").Trim())
                .ToArray();

            return PromptForOptions(message, options);
        }

        public static bool Confirm(string message)
        {
            Console.WriteLine($"{message} [y/n]");
            var answer = ReadLine.Read().ToLower();
            switch (answer)
            {
                case "y":
                    return true;
                case "n":
                    return false;
                default:
                    return Confirm(message);
            }
        }
    }
}
