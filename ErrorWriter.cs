using System;
using System.IO;
using System.Text;

namespace AutoAddScriptsToJson
{
	public class ConsoleErrorWriterDecorator : TextWriter
	{
		protected readonly TextWriter OriginalTextWriter;

		public ConsoleErrorWriterDecorator(TextWriter consoleTextWriter)
		{
			OriginalTextWriter = consoleTextWriter;
		}

		public override void WriteLine(string value)
		{
			ConsoleColor originalColor = Console.ForegroundColor;
			if (originalColor == ConsoleColor.Red)
			{
				OriginalTextWriter.WriteLine(value);
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				OriginalTextWriter.WriteLine(value);
				Console.ForegroundColor = originalColor;
			}
		}

		public override Encoding Encoding => Encoding.Default; 
	}
}
