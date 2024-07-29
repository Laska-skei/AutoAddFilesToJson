using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AutoAddScriptsToJson
{
	internal class Program
	{
		public string ModFolderPath;
		public string ModJsonPath;

		public const string ModJsonName = "mod.json";
		public const string ModJsonScriptsPropertyName = "Scripts";
		public const string ScriptsExtension = ".cs";

		public static readonly string[] IgnoredPaths = new string[]
		{
			"obj/",
		};

		public static void Main(string[] args)
		{
			new Program();
		}
		
		public Program()
		{
			WriteInfo();
			Console.SetError(new ConsoleErrorWriterDecorator(Console.Error));

			ModFolderPath = ConsoleUtils.AskFor("Enter full mod folder path:", (string value) =>
			{
				if (value != null && Directory.Exists(value))
				{
					ModJsonPath = Path.Combine(value, ModJsonName);
					if (File.Exists(ModJsonPath))
					{
						return true;
					}
					else
					{
						Console.Error.WriteLine($"{ModJsonName} does not exist in this folder.");
					}
				}
				else
				{
					Console.Error.WriteLine("Folder does not exist.");
				}
				return false;
			});

			string json = File.ReadAllText(ModJsonPath);
			string[] scriptsInJson = JsonUtils.JsonReadPropertyValue<string[]>(ModJsonScriptsPropertyName, json);
			FormatPaths(scriptsInJson);

			string[] scriptsInFolder = GetScriptsInModFolder().ToArray();

			Console.WriteLine("Scripts difference:");
			WriteDifference(scriptsInJson, scriptsInFolder);
			ConsoleUtils.WriteDividingLine();

			Console.WriteLine($"Update {ModJsonName} \"{ModJsonScriptsPropertyName}\" ?");
			if (ConsoleUtils.AskForConfirm())
			{
				json = JsonUtils.JsonWritePropertyValue(ModJsonScriptsPropertyName, scriptsInFolder, json);
				File.WriteAllText(ModJsonPath, json);
				Console.WriteLine("All scripts are written in json.");
			}
			else
			{
				Console.WriteLine("Canceled.");
			}

			Thread.Sleep(2500);
			Environment.Exit(0);
		}

		public void WriteInfo()
		{
			Console.WriteLine("Info:");
			Console.WriteLine($" {nameof(ModJsonName)}: {ModJsonName}");
			Console.WriteLine($" {nameof(ModJsonScriptsPropertyName)}: {ModJsonScriptsPropertyName}");
			Console.WriteLine($" {nameof(ScriptsExtension)}: {ScriptsExtension}");

			Console.WriteLine($" {nameof(IgnoredPaths)}:");
			for (int i = 0; i < IgnoredPaths.Length; i++)
			{
				Console.WriteLine("\t" + IgnoredPaths[i]);
			}
			ConsoleUtils.WriteDividingLine();
		}

		/// <returns> Formatted local mod paths to files with <see cref="ScriptsExtension"/>. </returns>
		public IEnumerable<string> GetScriptsInModFolder()
		{
			return Directory.EnumerateFiles(ModFolderPath, "*" + ScriptsExtension, SearchOption.AllDirectories)
				.Select((string filePath) => filePath.Remove(0, ModFolderPath.Length + 1).Replace('\\', '/'))
				.Where((string filePath) => !IgnoredPaths.Any(filePath.StartsWith));
		}
		public static void WriteDifference(in string[] oldCollection, in string[] newCollection)
		{
			if (oldCollection == null)
			{
				throw new ArgumentNullException(nameof(oldCollection));
			}
			else if (newCollection == null)
			{
				throw new ArgumentNullException(nameof(newCollection));
			}

			int removedValuesCount = 0;
			int addedValuesCount = 0;

			string[] differentValues = newCollection.Except(oldCollection).ToArray();

			string[] concatedCollection = new string[oldCollection.Length + differentValues.Length];
			oldCollection.CopyTo(concatedCollection, 0);
			differentValues.CopyTo(concatedCollection, oldCollection.Length);

			Array.Sort(concatedCollection);

			ConsoleColor originalColor = Console.ForegroundColor;
			ConsoleColor lastColor = originalColor;
			for (int i = 0; i < concatedCollection.Length; i++)
			{
				string filePath = concatedCollection[i];
				if (oldCollection != null && oldCollection.Contains(filePath))
				{
					if (newCollection.Contains(filePath))
					{
						setConsoleColor(ConsoleColor.Gray);
					}
					else
					{
						setConsoleColor(ConsoleColor.Red);
						removedValuesCount++;
					}
				}
				else
				{
					setConsoleColor(ConsoleColor.Green);
					addedValuesCount++;
				}
				Console.WriteLine("- " + filePath);
			}
			setConsoleColor(originalColor);
			Console.WriteLine($"Total: {addedValuesCount} added, {removedValuesCount} removed, {concatedCollection.Length - addedValuesCount - removedValuesCount} remained.");

			void setConsoleColor(in ConsoleColor color)
			{
				if (lastColor != color)
				{
					Console.ForegroundColor = color;
					lastColor = color;
				}
			}
		}
		/// <summary>
		/// Replace all <b>\</b> to <b>/</b> in <paramref name="paths"/>.
		/// </summary>
		public static void FormatPaths(string[] paths)
		{
			for (int i = 0; i < paths.Length; i++)
			{
				paths[i] = paths[i].Replace('\\', '/');
			}
		}
	}
}