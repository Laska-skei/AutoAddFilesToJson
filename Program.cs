using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace AutoAddScriptsToJson
{
	internal class Program
	{
		private string ModFolderPath;
		private string ModJsonPath;

		public const string ModJsonName = "mod.json";
		public const string ModJsonScriptsPropertyName = "Scripts";
		public const string ScriptsExtension = ".cs";

		public const string IgnoredPathsFileName = "IgnoredPaths.txt";
		public string[] IgnoredPaths;

		public static void Main(string[] args)
		{
			new Program();
		}

		public Program()
		{
			LoadIgnoredPaths();
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

		private void LoadIgnoredPaths()
		{
			try
			{
				string ignoredPathsFilePath = Path.Combine(Environment.CurrentDirectory, IgnoredPathsFileName);
				if (File.Exists(ignoredPathsFilePath))
				{
					IgnoredPaths = File.ReadAllLines(ignoredPathsFilePath);
					return;
				}
				else
				{
					File.Create(ignoredPathsFilePath);
				}
			}
			catch (Exception)
			{
			}
			IgnoredPaths = Array.Empty<string>();
		}
		public void WriteInfo()
		{
			Console.WriteLine("Info:");
			FieldInfo[] fields = GetType().GetFields();
			Array.Sort(fields, (FieldInfo x, FieldInfo y) => -x.Name.CompareTo(y.Name));

			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo field = fields[i];
				Console.Write($" {field.Name}: ");

				if (typeof(IList).IsAssignableFrom(field.FieldType))
				{
					Console.WriteLine();
					IList list = (IList)field.GetValue(this);
					for (int j = 0; j < list.Count; j++)
					{
						Console.WriteLine($"  {list[j]?.ToString()}");
					}
				}
				else
				{
					Console.WriteLine(field.GetValue(this));
				}
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