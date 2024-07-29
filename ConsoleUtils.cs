using System;

namespace AutoAddScriptsToJson
{
	internal static class ConsoleUtils
	{
		public static string AskFor(in string text, in Predicate<string> predicate)
		{
			while (true)
			{
				Console.WriteLine(text);
				string value = Console.ReadLine();

				if (predicate.Invoke(value))
				{
					return value;
				}
				else
				{
					WriteDividingLine(10);
				}
			}
		}
		/// <summary>
		/// Ask for confirm or cancel. The answer is given with Enter and Escape.
		/// </summary>
		/// <returns> <see langword="true"/> if confirmed, <see langword="false"/> if canceled. </returns>
		public static bool AskForConfirm()
		{
			Console.WriteLine("Press Enter to confirm or Escape to cancel...");
			while (true)
			{
				ConsoleKey key = Console.ReadKey().Key;
				if (key == ConsoleKey.Enter)
				{
					return true;
				}
				else if (key == ConsoleKey.Escape)
				{
					return false;
				}
			}
		}

		public static void WriteDividingLine(in int length = 30) => Console.WriteLine(new string('-', length));
	}
}
