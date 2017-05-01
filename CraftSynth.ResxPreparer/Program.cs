using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace CraftSynth.ResxPreparer
{
	/// <summary>
	/// Copies missing items beetween .resx and localized pairs (like .resx.en-us). 
	/// To use it put this in pre-build event in project where your .resx files are:
	/// $(SolutionDir)CraftSynth.ResxPreparer\bin\Debug\CraftSynth.ResxPreparer.exe "$(ProjectDir)App_GlobalResources"
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			try{
				MainInner(args);
			}
			catch (Exception exception)
			{
				try
				{
					string exeFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "");
					string content = exception.Message + "\r\n\r\n" + exception.StackTrace;
					File.WriteAllText(exeFolderPath + "\\LastError.txt", content);
				}
				catch (Exception)
				{
				}
				throw exception;
			}
		}

		private static void MainInner(string[] args)
		{
			if (args.Length != 1)
			{
				throw new Exception("App must be run with one argument (path to folder with .resx files).");
			}

			string folderPath = args[0];
			if (!Directory.Exists(folderPath))
			{
				throw new Exception("Folder not found:" + folderPath);
			}

			folderPath = folderPath.TrimEnd('\\');

			List<string> resxFilesPaths = GetFilePaths(folderPath, "*.resx");
			if (resxFilesPaths == null || resxFilesPaths.Count == 0)
			{
				throw new Exception("No .resx files in folder:" +folderPath);
			}

			string defaultResxFilePath = resxFilesPaths.FirstOrDefault(r=>!Path.GetFileName(r.ToLower()).Replace(".resx",string.Empty).Contains("."));
			if (defaultResxFilePath == null)
			{
				throw new Exception("Can not find default .resx file - one without culture name in filename. Folder:" + folderPath);
			}

			//TODO: consider special logic if we have this for example: .it-IT.resx and .it.resx

			//first copy items that exist in localized files but missing in default resx
			foreach (var resxFilePath in resxFilesPaths)
			{
				if (string.Compare(resxFilePath, defaultResxFilePath, StringComparison.InvariantCultureIgnoreCase) != 0)
				{
					CopyUnexistingItems(resxFilePath,defaultResxFilePath);
				}
			}

			//secondly copy missing items from default resx to localized files
			foreach (var resxFilePath in resxFilesPaths)
			{
				if (string.Compare(resxFilePath, defaultResxFilePath, StringComparison.InvariantCultureIgnoreCase) != 0)
				{
					CopyUnexistingItems(defaultResxFilePath, resxFilePath);
				}
			}
		}

		private static void CopyUnexistingItems(string sourceResxfith, string destinationResxFilePath)
		{
			XmlDocument sourceXmlDoc = new XmlDocument();
			sourceXmlDoc.Load(sourceResxfith);

			XmlDocument destinationXmlDoc = new XmlDocument();
			destinationXmlDoc.Load(destinationResxFilePath);

			bool fileChanged = false;

			XmlNodeList nodes = sourceXmlDoc.SelectNodes("/root/data");
			foreach (XmlNode node in nodes)
			{
				XmlNode pairInDestinationXml = destinationXmlDoc.SelectSingleNode("/root/data[@name='" + node.Attributes["name"].Value + "']");
				if (pairInDestinationXml == null)
				{
					//copy item
					var importedNode = destinationXmlDoc.ImportNode(node, true);
					//and add for example it-IT prefix to values in order for translator to spot and translate them next time.
					importedNode["value"].InnerText = GetCultureName(destinationResxFilePath) + importedNode["value"].InnerText;
					if (importedNode["comment"] != null && !string.IsNullOrWhiteSpace(importedNode["comment"].InnerText))
					{
						importedNode["comment"].InnerText = RemoveTimestampsFromComment(importedNode["comment"].InnerText);
					}
					destinationXmlDoc.SelectSingleNode("/root").AppendChild(importedNode);
					fileChanged = true;
				}
			}

			if (fileChanged)
			{
				destinationXmlDoc.Save(destinationResxFilePath);
			}
		}

		/// <summary>
		/// From d:\dir\filename.en-EN.resx returns E_.
		/// For default resx (without culture name) returns empty string.
		/// </summary>
		/// <param name="resxFilePath"></param>
		/// <returns></returns>
		private static string GetCultureName(string resxFilePath)
		{
			string cultureName = string.Empty;
			string filename = Path.GetFileName(resxFilePath.ToLower()).Replace(".resx", string.Empty);
			if (filename.Contains("."))
			{
				cultureName = filename.Substring(filename.LastIndexOf('.') + 1, 1).ToUpper();
				cultureName = cultureName + "_";
			}
			return cultureName;
		}



		#region Helper methods
		/// <summary>
		/// Gets list of strings where each is full path to file including filename (for example: <example>c:\dir\filename.ext</example>.
		/// </summary>
		/// <param name="folder">Full path of folder that should be searched. For example: <example>c:\dir</example>.</param>
		/// <param name="searchPatern">Filter that should be used. For example: <example>*.txt</example></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">Thrown when parameter is null or empty.</exception>
		public static List<string> GetFilePaths(string folderPath, string searchPatern)
		{
			if (string.IsNullOrEmpty(folderPath)) throw new ArgumentException("Value must be non-empty string.", "folderPath");
			if (string.IsNullOrEmpty(searchPatern)) throw new ArgumentException("Value must be non-empty string.", "searchPatern");

			List<string> filePaths = new List<string>();
			string[] filePathStrings = Directory.GetFiles(folderPath, searchPatern);
			if (filePathStrings != null)
			{
				filePaths.AddRange(filePathStrings);
			}

			return filePaths;
		}

		private static string RemoveTimestampsFromComment(string comment)
		{
			if (comment.IsNOTNullOrWhiteSpace())
			{
				string timestamps = comment.GetSubstring("[", "]");
				if (timestamps.IsNOTNullOrWhiteSpace())
				{
					//example LastSync=GMT2005.12.14 13:15:50,LastChange=GMT2005.10.11 03:33:34
					string patern = @"^LastSync=GMT(?<year>\d{4}).(?<month>\d{2}).(?<day>\d{2}) (?<hour>\d{2}):(?<minute>\d{2}):(?<second>\d{2}),LastChange=GMT(?<year2>\d{4}).(?<month2>\d{2}).(?<day2>\d{2}) (?<hour2>\d{2}):(?<minute2>\d{2}):(?<second2>\d{2})";
					Regex regex = new Regex(patern, RegexOptions.IgnoreCase);
					Match match = regex.Match(timestamps);
					if (match.Success)
					{
						comment =comment.RemoveSubstring("[", "]").TrimStart('[').TrimStart(']').TrimStart(' ');
					}
				}
			}
			return comment;
		}
		#endregion
	}
}
