using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Deployment.Application;
using System.Data.OleDb;
using System.Text;
using System.Reflection;

namespace Utilities
{
	/// <summary>
	/// Class manages string lines save to log and debug things runtime. Any FileName change should include file extension
	/// </summary>
	public class DebugLogger
	{
		
		private const int NumberOfRetries = 3;
		private const int DelayOnRetry = 1000;

		/// <summary>
		/// Saves line with time stamp into specific file with specific directory
		/// </summary>
		/// <param name="text">Text to write</param>
		/// <param name="pathToFile">Path to File</param>
		/// <param name="fileName">Name of File, default "Debug.CustomConsole.Log.txt"</param>B
		public static void WriteLine(string text, string pathToFile = "", string fileName = "", string extension = ".txt")
		{
			if (fileName == "")
				fileName = GetFileName(FileNames.StandartLogName) + extension;

			if (pathToFile == "")
				pathToFile = GetPath(Paths.ExecutablePath);

			string customPath =  Path.Combine(pathToFile, fileName);
			for (int i = 1; i <= NumberOfRetries; ++i)
			{
				try
				{
					if (!File.Exists(customPath)) using (var stream = File.Create(customPath))
						{
							stream.Close();
						}
					string newline = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " \t" + text + "\n";

					if (!Directory.Exists(customPath)) Directory.CreateDirectory(customPath);

					File.AppendAllText(customPath, newline);

					break;
				}
				catch when (i <= NumberOfRetries)
				{
					System.Threading.Thread.Sleep(DelayOnRetry);
				}
			}
		}

		/// <summary>
		/// Saves file with some filename. If there is already file like this will make a new one with number at the end
		/// </summary>
		/// <param name="text">Text to save</param>
		/// <param name="path">Path to file</param>
		/// <param name="fileName">Name of the file</param>
		public async static void WriteFile(string text, string path = "", string fileName = "", string extension = ".txt")
		{
			if (path == "") path = GetPath(Paths.ExecutablePath);
			if (fileName == "") fileName = GetFileName(FileNames.StandartLogName);
			string defaultName = fileName;
			string finalPath;
			int numberOfRetries = 0;
			while (true)
			{
				try
				{
					if (!Directory.Exists(path)) Directory.CreateDirectory(path);
					if (File.Exists(Path.Combine(path, fileName + extension)))
					{
						numberOfRetries++;
						fileName = defaultName + "_" + numberOfRetries.ToString();
						continue;
					}
					finalPath = Path.Combine(path, fileName + extension);

					File.AppendAllText(finalPath, text);
					break;
				}
				catch when (numberOfRetries <= NumberOfRetries * 5)
				{
					await System.Threading.Tasks.Task.Delay((int)(DelayOnRetry * 0.5));
				}
			}
		}

		/// <summary>
		/// Saves exception into file
		/// </summary>
		/// <param name="exception">Error to save</param>
		/// <param name="path">Path to file</param>
		/// <param name="fileName">Name of the file. Optional. Defoult is [DATE]_ERROR_LOG.txt</param>
		public static void WriteErrorFile(Exception exception, string path = "", string fileName = "", string extension = ".txt")
		{
			if (fileName == "") fileName = GetFileName(FileNames.TimedName) + "_ERROR_LOG" + extension;
			if (path == "") path = Path.Combine(GetPath(Paths.AppDataLocalPath), "Error Logs");

			WriteFile(GetExceptionFullDescription(exception),
				path,
				fileName);
		}
		public static string GetExceptionFullDescription(Exception exception)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append($"{exception.GetType().Name}\n{exception.Message}");
			builder.Append(exception.HelpLink == null ? "" : $"({exception.HResult})");
			builder.Append($"\n\nStackTrace:\n{exception.StackTrace}");
			builder.Append(exception.TargetSite == null ? "" : $"\n\nMethod:\n{ exception.TargetSite}");
			builder.Append(exception.Source == null ? "" : $"\n\nSource:\n{exception.Source}");
			builder.Append(exception.HelpLink == null ? "" : $"\n\nHelpLink:\n{exception.HelpLink}");
			builder.Append(exception.InnerException == null ? "" : $"\n\nInnerException:\n{GetExceptionFullDescription(exception.InnerException)}");

			return builder.ToString();
		}
		/// <summary>
		/// Saves line with time stamp into specific file within [user]\AppData\Local\[AppName]\
		/// </summary>
		/// <param name="text">Text to save</param>
		/// <param name="fileName">Optional file name, default is [AppName]_log.txt</param>
		public static void WriteHiddenLine(string text, string fileName = "", string extension = ".txt")
		{
			if (string.IsNullOrEmpty(fileName))
				fileName = GetFileName(FileNames.StandartLogName);

			fileName += extension;

			string path = GetPath(Paths.AppDataLocalPath);
			string fullFileName = Path.Combine(path, fileName);

			for (int i = 1; i <= NumberOfRetries; ++i)
			{
				try
				{
					if (!Directory.Exists(path)) Directory.CreateDirectory(path);
					if (!File.Exists(fullFileName)) using (var stream = File.Create(fullFileName))
						{
							stream.Close();
						}
					string newline = $"\n{DateTime.Now:dd.MM.yyyy HH:mm:ss}\t{text}";


					File.AppendAllText(fullFileName, newline);

					break;
				}
				catch when (i <= NumberOfRetries)
				{
					System.Threading.Thread.Sleep(DelayOnRetry);
				}
			}
		}

		public static string GetPath(Paths path)
		{
			switch (path)
			{
				case Paths.AppDataLocalPath:
					return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),Application.ProductName.Replace(" ", "_"));
				case Paths.UserDocumentsPath:
					return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Application.ProductName.Replace(" ", "_"));
				case Paths.ExecutablePath:
					return Directory.GetCurrentDirectory();
				default:
					return @"C:\Users\Public\Desktop\";
			}
		}
		public static string GetFileName(FileNames fileName)
		{
			switch (fileName)
			{
				case FileNames.StandartLogName:
					return Application.ProductName.Replace(" ", "_") + "_log";
				case FileNames.TimedName:
					return Application.ProductName.Replace(" ", "_") + DateTime.Now.ToString("yyyyMMddHHmmss");
				default:
					return "bruh";
			}
		}
		public enum Paths
		{
			AppDataLocalPath,
			ExecutablePath,
			UserDocumentsPath
		}
		public enum FileNames
		{
			StandartLogName,
			TimedName
		}
	}

	/// <summary>
	/// Search all public variables of any class or struct.
	/// Error catching uses 'ErrorMessage' private function. If needed specific error catching mechanism only that function could be overwritten
	/// </summary>
	public class GigaSearch
	{
		private static string regexParamPattern = "@[A-Za-z0-9_]+";
		private static string regexGarbagePattern = @"[ \-\(\)\\/]+";
		/// <summary>
		/// Search all public variables. To search specific column write @[ColName] at the end of search string
		/// </summary>
		/// <typeparam name="T">Type. Any works (i hope)</typeparam>
		/// <param name="Search">Search string. Supports [search]@[ColName] for search in specific column. Supports || and && for multi search
		/// Example: ps@name&&52@id. Searches for ps at 'name' column and 52 at 'id' column
		/// </param>
		/// <param name="FullList">Search List</param>
		/// <param name="IsExactSearch">Use strict search. Only case ignored</param>
		/// <returns>Search list of type. Returns base search list on error</returns>
		public static List<T> Search<T>(string Search, List<T> FullList, bool IsExactSearch = false)
		{
			//ну кароч на 10000 записей работает медленно. без параметров работает гораздо быстрее
			//оптимизировал и кароч работает быстрее чем сервер с запросом like '%%'
			//лям записей за 4 секунды ИЩЕТ. гигапоиск, название говорящее и подходящее
			if (string.IsNullOrEmpty(Search)) return FullList;

			List<T> ts = FullList;
			//TODO: (a||b)&&(c||d)
			//не думаю что в текущем состоянии удасться реализовать. идея была в том, чтобы поиск в скобочках осуществлялся перед всем другим
			//и функция была направлена в саму себя. то есть от запроса отделялся бы запрос со скобочками, уже без скобочек передавался в функцию поиска.
			//одна проблема - как потом обратно подсоединить список и второй запрос.
			//со строкой-запросом и при этом операторы (||&&) не просрать 
			//пока что единственная идея - искать в этом массиве, при этом в запросе меняя все выражение в скобочках на "found",
			//и потом из массивов аргументов (foreachStr) удалять все строки "found" и делать поиск "И"(&&) в уже найденном массиве,
			//"ИЛИ"(||) при этом не трогать
			Regex advancedSearch = new Regex(@"\(\)");

			try
			{
				//поиск (...). При нахождении нужно посылать в эту же функцию запрос внутри (...), при этом все что есть в скобочках, от первой до последней,
				//инаже ((a&&b)||(a&&c)) будет находить ((a&&b) и на этом закончится. Поэтому ищется индекс первой "(" и последней ")" - индекс первой "(" -1.
				//Таким образом в строку попадает четко что есть внутри (...) и ничего вне.
				//if (advancedSearch.IsMatch(Search))
				//{
				//	MessageBox.Show(Search.Substring(Search.IndexOf('(') + 1, Search.LastIndexOf(')') - Search.IndexOf('(') - 1));
				//}
				if (advancedSearch.IsMatch(Search))
					MessageBox.Show("Advanced search not supported, normal search initiated");

				string[] forechStr = Search.ToLower().Split(new string[] { "||" }, options: StringSplitOptions.RemoveEmptyEntries);


				List<T> result = new List<T>();
				HashSet<T> resHash = new HashSet<T>();
				foreach (string search in forechStr)
				{
					if (string.IsNullOrEmpty(search)) continue;
					string[] andSearch = search.Split(new string[] { "&&" }, options: StringSplitOptions.RemoveEmptyEntries);
					List<T> localResult = new List<T>();
					for (int i = 0; i < andSearch.Length; i++)
					{
						if (i == 0)
							localResult = SearchIn(andSearch[i], FullList, IsExactSearch);
						else
							localResult = SearchIn(andSearch[i], localResult, IsExactSearch);
					}

					//занесение уникальных записей в результат
					localResult.ForEach(r => resHash.Add(r));
				}
				
				return resHash.ToList();
			}
			catch (Exception exc)
			{
				DebugLogger.WriteErrorFile(exc);
				throw;
			}
		}

		private static List<T> SearchIn<T>(string Search, List<T> FullList, bool IsExactSearch)
		{
			if (string.IsNullOrEmpty(Search)) return FullList;

			//инициализация. Параметры ищутся вида @fowkfaw
			List<T> localResult = new List<T>();
			Regex garbageCleaner = new Regex(regexGarbagePattern);
			string searchStr = garbageCleaner.Replace(Search, "");
			Regex paramRegex = new Regex(regexParamPattern);
			List<string> allParams = paramRegex.Matches(searchStr)
				.Cast<Match>()
				.ToList()
				.Select(s => s.Value.Substring(1)) //string.Concat(s.Value[1].ToString().ToUpper(), s.Value.Substring(2)))
				.Where(p => typeof(T).GetProperties().Where(prop=>prop.Name.ToLower() == p.ToLower()).Count() > 0 )
				.ToList();
			searchStr = paramRegex.Replace(searchStr, "");
			Regex regex = new Regex(searchStr, RegexOptions.IgnoreCase);

			//поиск без параметров
			if (allParams.Count == 0)
				localResult = SearchNoParams(Search, FullList, IsExactSearch);

			//поиск с параметрами
			allParams
				.ForEach(str => localResult.AddRange(FullList
				.Where(s =>
				{
					try
					{
						if (regex.IsMatch(garbageCleaner.Replace(s.GetType().GetProperty(str, System.Reflection.BindingFlags.IgnoreCase
						| System.Reflection.BindingFlags.Public
						| System.Reflection.BindingFlags.Instance)
						.GetValue(s, null).ToString().ToLower(), "")))
						{
							if (IsExactSearch)
							{
								string a = garbageCleaner.Replace(s.GetType().GetProperty(str, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(s, null).ToString().ToLower(), "");
								return a == searchStr;
							}
							else
								return true;
						}
						return false;
					}
					catch { return false; }
				}
				)));

			return localResult;
		}

		/// <summary>
		/// Search all public variables without additional stuff.
		/// </summary>
		/// <typeparam name="T">Type. Any works (i hope)</typeparam>
		/// <param name="Search">Search string. Not supports any || or &&</param>
		/// <param name="FullList">Search List</param>
		/// <param name="IsExactSearch">Use strict search. Only case ignored</param>
		/// <returns>Search list of type. Returns base search list on error</returns>
		public static List<T> SearchNoParams<T>(string Search, List<T> FullList, bool IsExactSearch = false)
		{
			//работает медленнее поиска с параметреми -> почему?
			try
			{
				if (string.IsNullOrEmpty(Search)) return FullList;

				Regex regex = new Regex(Search);
				Regex garbageCleaner = new Regex(regexGarbagePattern);

				List<T> localResult = new List<T>();

				localResult.AddRange(FullList
					.Where(e =>
					{
						return e.GetType().GetProperties().Where(s =>
						{
							if (regex.IsMatch(garbageCleaner.Replace(s.GetValue(e).ToString().ToLower(), "")))
							{
								if (IsExactSearch)
								{
									string a = garbageCleaner.Replace(s.GetValue(e).ToString().ToLower(), "");
									return Search == garbageCleaner.Replace(s.GetValue(e).ToString().ToLower(), "");
								}
								return true;
							}
							return false;
						}).FirstOrDefault() != null;
					}));

				return localResult;
			}
			catch(Exception exc)
			{
				DebugLogger.WriteErrorFile(exc);
				throw;
			}
		}
		/// <summary>
		/// Search, but every {Space} transfers into regex, ordered by "search index" - how many regexes Item match.
		/// Params are enabled, but you need to write them sequencially
		/// example: "abd sft@Name@Param". Searches both "abd" and "sft" int "Name" and "Param" vars, ordered by amount of matches search got
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="Search">Search string</param>
		/// <param name="FullList">List to search</param>
		/// <param name="Include">Should search include non-100% matches. Meaning it will include item if it has at least 1 match, still ordered by search index</param>
		/// <returns>Searched list</returns>
		public static List<T> SearchUniversal<T>(string Search, List<T> FullList, bool Include = false)
		{
			if (string.IsNullOrEmpty(Search)) return FullList;

			List<T> result = new List<T>();

			Regex paramRegex = new Regex(regexParamPattern);

			//по идее работает, перебирает поиск на все параметры и записывает в массив
			string[] param = paramRegex.Matches(Search).Cast<Match>().ToList().Select(m => m.Value.Replace("@","")).ToArray();

			string clearSearch = paramRegex.Replace(Search, "");

			//?? хз тоже если работает. по факту с паматью так делать нежелательно. очень много шарп-сахара
			List<Regex> regices = clearSearch.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(s => new Regex(s)).ToList();

			List<Tuple<T, int>> results = new List<Tuple<T, int>>();

			//магия. через полдня я тут уже не разберусь
			FullList.ForEach(t =>
			{
				int searchIndex = 0;
				if (param.Length > 0)
					param.ToList().ForEach(p =>
					{
						regices.ForEach(r =>
						{
							System.Reflection.PropertyInfo prop = t.GetType().GetProperty(p,
								System.Reflection.BindingFlags.Public |
							System.Reflection.BindingFlags.Instance |
							System.Reflection.BindingFlags.IgnoreCase);
							
							try
							{
								if(prop !=null)
									searchIndex += r.Matches((prop.GetValue(t, null) == null ? "" : prop.GetValue(t, null).ToString().ToLower())).Count;
							}
							catch { }
						});
					});
				else
					regices.ForEach(r =>
					{
						t.GetType().GetProperties().ToList().ForEach(prop =>
						{
							try
							{
								searchIndex += r.Matches((prop.GetValue(t, null) == null ? "" : prop.GetValue(t, null).ToString().ToLower())).Count;
							}
							catch { }
						});
					});

				if (Include)
				{
					if (searchIndex > 0)
						results.Add(new Tuple<T, int>(t, searchIndex));
				}
				else
					if (searchIndex >= regices.Count)
					results.Add(new Tuple<T, int>(t, searchIndex));
			});

			//главное сортировать правильно, надеюсь при селекте сортировака не сбивается.
			result = results.OrderBy(t => t.Item2).Reverse().Select(tu => tu.Item1).ToList();

			return result;
		}
	}

	/// <summary>
	/// Windows calls and WinAPI use cases
	/// </summary>
	namespace Windows
	{
		/// <summary>
		/// A class that manages a global low level keyboard hook (Do something when keyboard key press. even outside the program)
		/// </summary>
		public class GlobalKeyboardHook
		{
			#region Constant, Structure and Delegate Definitions
			/// <summary>
			/// Defines the callback type for the hook
			/// </summary>
			public delegate int KeyboardHookProc(int code, int wParam, ref KeyboardHookStruct lParam);

			public struct KeyboardHookStruct
			{
				public int vkCode;
				public int scanCode;
				public int flags;
				public int time;
				public int dwExtraInfo;
			}

			const int WH_KEYBOARD_LL = 13;
			const int WM_KEYDOWN = 0x100;
			const int WM_KEYUP = 0x101;
			const int WM_SYSKEYDOWN = 0x104;
			const int WM_SYSKEYUP = 0x105;
			#endregion

			#region Instance Variables
			/// <summary>
			/// The collections of keys to watch for
			/// </summary>
			public List<Keys> HookedKeys = new List<Keys>();
			/// <summary>
			/// Handle to the hook, need this to unhook and call the next hook
			/// </summary>
			IntPtr hhook = IntPtr.Zero;
			#endregion

			#region Events
			/// <summary>
			/// Occurs when one of the hooked keys is pressed
			/// </summary>
			public event KeyEventHandler KeyDown;
			/// <summary>
			/// Occurs when one of the hooked keys is released
			/// </summary>
			public event KeyEventHandler KeyUp;
			#endregion

			#region Constructors and Destructors
			/// <summary>
			/// Initializes a new instance of the <see cref="GlobalKeyboardHook"/> class and installs the keyboard hook.
			/// </summary>
			public GlobalKeyboardHook()
			{
				Hook();
			}

			/// <summary>
			/// Releases unmanaged resources and performs other cleanup operations before the
			/// <see cref="GlobalKeyboardHook"/> is reclaimed by garbage collection and uninstalls the keyboard hook.
			/// </summary>
			~GlobalKeyboardHook()
			{
				Unhook();
			}
			#endregion

			#region Public Methods
			private static KeyboardHookProc callbackDelegate;

			/// <summary>
			/// Installs the global hook
			/// </summary>
			public void Hook()
			{
				if (callbackDelegate != null) throw new InvalidOperationException("Can't hook more than once");
				IntPtr hInstance = LoadLibrary("User32");
				callbackDelegate = new KeyboardHookProc(HookProc);
				hhook = SetWindowsHookEx(WH_KEYBOARD_LL, callbackDelegate, hInstance, 0);
				if (hhook == IntPtr.Zero) throw new Exception();
			}

			/// <summary>
			/// Uninstalls the global hook
			/// </summary>
			public void Unhook()
			{
				if (callbackDelegate == null) return;
				bool ok = UnhookWindowsHookEx(hhook);
				if (!ok) throw new Exception();
				callbackDelegate = null;
			}

			/// <summary>
			/// The callback for the keyboard hook
			/// </summary>
			/// <param name="code">The hook code, if it isn't >= 0, the function shouldn't do anyting</param>
			/// <param name="wParam">The event type</param>
			/// <param name="lParam">The keyhook event information</param>
			/// <returns></returns>
			public int HookProc(int code, int wParam, ref KeyboardHookStruct lParam)
			{
				if (code >= 0)
				{
					Keys key = (Keys)lParam.vkCode;
					if (HookedKeys.Contains(key))
					{
						KeyEventArgs kea = new KeyEventArgs(key);
						if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null))
						{
							KeyDown(this, kea);
						}
						else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null))
						{
							KeyUp(this, kea);
						}
						if (kea.Handled)
							return 1;
					}
				}
				return CallNextHookEx(hhook, code, wParam, ref lParam);
			}
			#endregion

			#region DLL imports
			/// <summary>
			/// Sets the windows hook, do the desired event, one of hInstance or threadId must be non-null
			/// </summary>
			/// <param name="idHook">The id of the event you want to hook</param>
			/// <param name="callback">The callback.</param>
			/// <param name="hInstance">The handle you want to attach the event to, can be null</param>
			/// <param name="threadId">The thread you want to attach the event to, can be null</param>
			/// <returns>a handle to the desired hook</returns>
			[DllImport("user32.dll")]
			static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookProc callback, IntPtr hInstance, uint threadId);

			/// <summary>
			/// Unhooks the windows hook.
			/// </summary>
			/// <param name="hInstance">The hook handle that was returned from SetWindowsHookEx</param>
			/// <returns>True if successful, false otherwise</returns>
			[DllImport("user32.dll")]
			static extern bool UnhookWindowsHookEx(IntPtr hInstance);

			/// <summary>
			/// Calls the next hook.
			/// </summary>
			/// <param name="idHook">The hook id</param>
			/// <param name="nCode">The hook code</param>
			/// <param name="wParam">The wparam.</param>
			/// <param name="lParam">The lparam.</param>
			/// <returns></returns>
			[DllImport("user32.dll")]
			static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref KeyboardHookStruct lParam);

			/// <summary>
			/// Loads the library.
			/// </summary>
			/// <param name="lpFileName">Name of the library</param>
			/// <returns>A handle to the library</returns>
			[DllImport("kernel32.dll")]
			static extern IntPtr LoadLibrary(string lpFileName);
			#endregion
		}
		/// <summary>
		/// Class that manages all raw Windows API commands
		/// </summary>
		public class WinAPI
		{
			#region Functions
			/// <summary>
			/// Gets Caret Position
			/// </summary>
			/// <param name="p">Point of caret</param>
			/// <returns>Some integer? idk</returns>
			[DllImport("user32")]
			public extern static int GetCaretPos(out Point p);

			/// <summary>
			/// Get time in seconds since last user input
			/// </summary>
			/// <returns>Time in seconds</returns>
			public static int GetLastInputTime()
			{
				int idleTime = 0;
				LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
				lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
				lastInputInfo.dwTime = 0;

				int envTicks = Environment.TickCount / 1000;

				if (GetLastInputInfo(ref lastInputInfo))
				{
					int lastInputTick = Convert.ToInt32(lastInputInfo.dwTime / 1000);

					idleTime = envTicks - lastInputTick;
				}

				return idleTime;
			}

			/// <summary>
			/// Send Keyboard Key press to Windows API
			/// </summary>
			/// <param name="KeyboardKeyInput">Key</param>
			public static void SendKeyAsKeyboardInput(ScanCodeShort KeyboardKeyInput)
			{
				INPUT[] Inputs = new INPUT[1];
				INPUT Input = new INPUT
				{
					type = 1 // 1 = Keyboard Input
				};
				Input.U.ki.wScan = KeyboardKeyInput;
				Input.U.ki.dwFlags = KEYEVENTF.SCANCODE;
				Inputs[0] = Input;
				SendInput(1, Inputs, INPUT.Size);
			}

			#endregion

			#region Additional stuff
			[StructLayout(LayoutKind.Sequential)]
			struct LASTINPUTINFO
			{
				public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

				[MarshalAs(UnmanagedType.U4)]
				public int cbSize;
				[MarshalAs(UnmanagedType.U4)]
				public UInt32 dwTime;
			}

			[DllImport("user32.dll")]
			private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);


			/// <summary>
			/// Declaration of external SendInput method
			/// </summary>
			[DllImport("user32.dll")]
			internal static extern uint SendInput(
				uint nInputs,
				[MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs,
				int cbSize);


			// Declare the INPUT struct
			[StructLayout(LayoutKind.Sequential)]
			public struct INPUT
			{
				public uint type;
				public InputUnion U;
				public static int Size
				{
					get { return Marshal.SizeOf(typeof(INPUT)); }
				}
			}

			// Declare the InputUnion struct
			[StructLayout(LayoutKind.Explicit)]
			public struct InputUnion
			{
				[FieldOffset(0)]
				internal MOUSEINPUT mi;
				[FieldOffset(0)]
				internal KEYBDINPUT ki;
				[FieldOffset(0)]
				internal HARDWAREINPUT hi;
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct MOUSEINPUT
			{
				internal int dx;
				internal int dy;
				internal MouseEventDataXButtons mouseData;
				internal MOUSEEVENTF dwFlags;
				internal uint time;
				internal UIntPtr dwExtraInfo;
			}

			[Flags]
			public enum MouseEventDataXButtons : uint
			{
				Nothing = 0x00000000,
				XBUTTON1 = 0x00000001,
				XBUTTON2 = 0x00000002
			}

			[Flags]
			public enum MOUSEEVENTF : uint
			{
				ABSOLUTE = 0x8000,
				HWHEEL = 0x01000,
				MOVE = 0x0001,
				MOVE_NOCOALESCE = 0x2000,
				LEFTDOWN = 0x0002,
				LEFTUP = 0x0004,
				RIGHTDOWN = 0x0008,
				RIGHTUP = 0x0010,
				MIDDLEDOWN = 0x0020,
				MIDDLEUP = 0x0040,
				VIRTUALDESK = 0x4000,
				WHEEL = 0x0800,
				XDOWN = 0x0080,
				XUP = 0x0100
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct KEYBDINPUT
			{
				internal VirtualKeyShort wVk;
				internal ScanCodeShort wScan;
				internal KEYEVENTF dwFlags;
				internal int time;
				internal UIntPtr dwExtraInfo;
			}

			[Flags]
			public enum KEYEVENTF : uint
			{
				EXTENDEDKEY = 0x0001,
				KEYUP = 0x0002,
				SCANCODE = 0x0008,
				UNICODE = 0x0004
			}

			public enum VirtualKeyShort : short
			{
				///<summary>
				///Left mouse button
				///</summary>
				LBUTTON = 0x01,
				///<summary>
				///Right mouse button
				///</summary>
				RBUTTON = 0x02,
				///<summary>
				///Control-break processing
				///</summary>
				CANCEL = 0x03,
				///<summary>
				///Middle mouse button (three-button mouse)
				///</summary>
				MBUTTON = 0x04,
				///<summary>
				///Windows 2000/XP: X1 mouse button
				///</summary>
				XBUTTON1 = 0x05,
				///<summary>
				///Windows 2000/XP: X2 mouse button
				///</summary>
				XBUTTON2 = 0x06,
				///<summary>
				///BACKSPACE key
				///</summary>
				BACK = 0x08,
				///<summary>
				///TAB key
				///</summary>
				TAB = 0x09,
				///<summary>
				///CLEAR key
				///</summary>
				CLEAR = 0x0C,
				///<summary>
				///ENTER key
				///</summary>
				RETURN = 0x0D,
				///<summary>
				///SHIFT key
				///</summary>
				SHIFT = 0x10,
				///<summary>
				///CTRL key
				///</summary>
				CONTROL = 0x11,
				///<summary>
				///ALT key
				///</summary>
				MENU = 0x12,
				///<summary>
				///PAUSE key
				///</summary>
				PAUSE = 0x13,
				///<summary>
				///CAPS LOCK key
				///</summary>
				CAPITAL = 0x14,
				///<summary>
				///Input Method Editor (IME) Kana mode
				///</summary>
				KANA = 0x15,
				///<summary>
				///IME Hangul mode
				///</summary>
				HANGUL = 0x15,
				///<summary>
				///IME Junja mode
				///</summary>
				JUNJA = 0x17,
				///<summary>
				///IME final mode
				///</summary>
				FINAL = 0x18,
				///<summary>
				///IME Hanja mode
				///</summary>
				HANJA = 0x19,
				///<summary>
				///IME Kanji mode
				///</summary>
				KANJI = 0x19,
				///<summary>
				///ESC key
				///</summary>
				ESCAPE = 0x1B,
				///<summary>
				///IME convert
				///</summary>
				CONVERT = 0x1C,
				///<summary>
				///IME nonconvert
				///</summary>
				NONCONVERT = 0x1D,
				///<summary>
				///IME accept
				///</summary>
				ACCEPT = 0x1E,
				///<summary>
				///IME mode change request
				///</summary>
				MODECHANGE = 0x1F,
				///<summary>
				///SPACEBAR
				///</summary>
				SPACE = 0x20,
				///<summary>
				///PAGE UP key
				///</summary>
				PRIOR = 0x21,
				///<summary>
				///PAGE DOWN key
				///</summary>
				NEXT = 0x22,
				///<summary>
				///END key
				///</summary>
				END = 0x23,
				///<summary>
				///HOME key
				///</summary>
				HOME = 0x24,
				///<summary>
				///LEFT ARROW key
				///</summary>
				LEFT = 0x25,
				///<summary>
				///UP ARROW key
				///</summary>
				UP = 0x26,
				///<summary>
				///RIGHT ARROW key
				///</summary>
				RIGHT = 0x27,
				///<summary>
				///DOWN ARROW key
				///</summary>
				DOWN = 0x28,
				///<summary>
				///SELECT key
				///</summary>
				SELECT = 0x29,
				///<summary>
				///PRINT key
				///</summary>
				PRINT = 0x2A,
				///<summary>
				///EXECUTE key
				///</summary>
				EXECUTE = 0x2B,
				///<summary>
				///PRINT SCREEN key
				///</summary>
				SNAPSHOT = 0x2C,
				///<summary>
				///INS key
				///</summary>
				INSERT = 0x2D,
				///<summary>
				///DEL key
				///</summary>
				DELETE = 0x2E,
				///<summary>
				///HELP key
				///</summary>
				HELP = 0x2F,
				///<summary>
				///0 key
				///</summary>
				KEY_0 = 0x30,
				///<summary>
				///1 key
				///</summary>
				KEY_1 = 0x31,
				///<summary>
				///2 key
				///</summary>
				KEY_2 = 0x32,
				///<summary>
				///3 key
				///</summary>
				KEY_3 = 0x33,
				///<summary>
				///4 key
				///</summary>
				KEY_4 = 0x34,
				///<summary>
				///5 key
				///</summary>
				KEY_5 = 0x35,
				///<summary>
				///6 key
				///</summary>
				KEY_6 = 0x36,
				///<summary>
				///7 key
				///</summary>
				KEY_7 = 0x37,
				///<summary>
				///8 key
				///</summary>
				KEY_8 = 0x38,
				///<summary>
				///9 key
				///</summary>
				KEY_9 = 0x39,
				///<summary>
				///A key
				///</summary>
				KEY_A = 0x41,
				///<summary>
				///B key
				///</summary>
				KEY_B = 0x42,
				///<summary>
				///C key
				///</summary>
				KEY_C = 0x43,
				///<summary>
				///D key
				///</summary>
				KEY_D = 0x44,
				///<summary>
				///E key
				///</summary>
				KEY_E = 0x45,
				///<summary>
				///F key
				///</summary>
				KEY_F = 0x46,
				///<summary>
				///G key
				///</summary>
				KEY_G = 0x47,
				///<summary>
				///H key
				///</summary>
				KEY_H = 0x48,
				///<summary>
				///I key
				///</summary>
				KEY_I = 0x49,
				///<summary>
				///J key
				///</summary>
				KEY_J = 0x4A,
				///<summary>
				///K key
				///</summary>
				KEY_K = 0x4B,
				///<summary>
				///L key
				///</summary>
				KEY_L = 0x4C,
				///<summary>
				///M key
				///</summary>
				KEY_M = 0x4D,
				///<summary>
				///N key
				///</summary>
				KEY_N = 0x4E,
				///<summary>
				///O key
				///</summary>
				KEY_O = 0x4F,
				///<summary>
				///P key
				///</summary>
				KEY_P = 0x50,
				///<summary>
				///Q key
				///</summary>
				KEY_Q = 0x51,
				///<summary>
				///R key
				///</summary>
				KEY_R = 0x52,
				///<summary>
				///S key
				///</summary>
				KEY_S = 0x53,
				///<summary>
				///T key
				///</summary>
				KEY_T = 0x54,
				///<summary>
				///U key
				///</summary>
				KEY_U = 0x55,
				///<summary>
				///V key
				///</summary>
				KEY_V = 0x56,
				///<summary>
				///W key
				///</summary>
				KEY_W = 0x57,
				///<summary>
				///X key
				///</summary>
				KEY_X = 0x58,
				///<summary>
				///Y key
				///</summary>
				KEY_Y = 0x59,
				///<summary>
				///Z key
				///</summary>
				KEY_Z = 0x5A,
				///<summary>
				///Left Windows key (Microsoft Natural keyboard) 
				///</summary>
				LWIN = 0x5B,
				///<summary>
				///Right Windows key (Natural keyboard)
				///</summary>
				RWIN = 0x5C,
				///<summary>
				///Applications key (Natural keyboard)
				///</summary>
				APPS = 0x5D,
				///<summary>
				///Computer Sleep key
				///</summary>
				SLEEP = 0x5F,
				///<summary>
				///Numeric keypad 0 key
				///</summary>
				NUMPAD0 = 0x60,
				///<summary>
				///Numeric keypad 1 key
				///</summary>
				NUMPAD1 = 0x61,
				///<summary>
				///Numeric keypad 2 key
				///</summary>
				NUMPAD2 = 0x62,
				///<summary>
				///Numeric keypad 3 key
				///</summary>
				NUMPAD3 = 0x63,
				///<summary>
				///Numeric keypad 4 key
				///</summary>
				NUMPAD4 = 0x64,
				///<summary>
				///Numeric keypad 5 key
				///</summary>
				NUMPAD5 = 0x65,
				///<summary>
				///Numeric keypad 6 key
				///</summary>
				NUMPAD6 = 0x66,
				///<summary>
				///Numeric keypad 7 key
				///</summary>
				NUMPAD7 = 0x67,
				///<summary>
				///Numeric keypad 8 key
				///</summary>
				NUMPAD8 = 0x68,
				///<summary>
				///Numeric keypad 9 key
				///</summary>
				NUMPAD9 = 0x69,
				///<summary>
				///Multiply key
				///</summary>
				MULTIPLY = 0x6A,
				///<summary>
				///Add key
				///</summary>
				ADD = 0x6B,
				///<summary>
				///Separator key
				///</summary>
				SEPARATOR = 0x6C,
				///<summary>
				///Subtract key
				///</summary>
				SUBTRACT = 0x6D,
				///<summary>
				///Decimal key
				///</summary>
				DECIMAL = 0x6E,
				///<summary>
				///Divide key
				///</summary>
				DIVIDE = 0x6F,
				///<summary>
				///F1 key
				///</summary>
				F1 = 0x70,
				///<summary>
				///F2 key
				///</summary>
				F2 = 0x71,
				///<summary>
				///F3 key
				///</summary>
				F3 = 0x72,
				///<summary>
				///F4 key
				///</summary>
				F4 = 0x73,
				///<summary>
				///F5 key
				///</summary>
				F5 = 0x74,
				///<summary>
				///F6 key
				///</summary>
				F6 = 0x75,
				///<summary>
				///F7 key
				///</summary>
				F7 = 0x76,
				///<summary>
				///F8 key
				///</summary>
				F8 = 0x77,
				///<summary>
				///F9 key
				///</summary>
				F9 = 0x78,
				///<summary>
				///F10 key
				///</summary>
				F10 = 0x79,
				///<summary>
				///F11 key
				///</summary>
				F11 = 0x7A,
				///<summary>
				///F12 key
				///</summary>
				F12 = 0x7B,
				///<summary>
				///F13 key
				///</summary>
				F13 = 0x7C,
				///<summary>
				///F14 key
				///</summary>
				F14 = 0x7D,
				///<summary>
				///F15 key
				///</summary>
				F15 = 0x7E,
				///<summary>
				///F16 key
				///</summary>
				F16 = 0x7F,
				///<summary>
				///F17 key  
				///</summary>
				F17 = 0x80,
				///<summary>
				///F18 key  
				///</summary>
				F18 = 0x81,
				///<summary>
				///F19 key  
				///</summary>
				F19 = 0x82,
				///<summary>
				///F20 key  
				///</summary>
				F20 = 0x83,
				///<summary>
				///F21 key  
				///</summary>
				F21 = 0x84,
				///<summary>
				///F22 key, (PPC only) Key used to lock device.
				///</summary>
				F22 = 0x85,
				///<summary>
				///F23 key  
				///</summary>
				F23 = 0x86,
				///<summary>
				///F24 key  
				///</summary>
				F24 = 0x87,
				///<summary>
				///NUM LOCK key
				///</summary>
				NUMLOCK = 0x90,
				///<summary>
				///SCROLL LOCK key
				///</summary>
				SCROLL = 0x91,
				///<summary>
				///Left SHIFT key
				///</summary>
				LSHIFT = 0xA0,
				///<summary>
				///Right SHIFT key
				///</summary>
				RSHIFT = 0xA1,
				///<summary>
				///Left CONTROL key
				///</summary>
				LCONTROL = 0xA2,
				///<summary>
				///Right CONTROL key
				///</summary>
				RCONTROL = 0xA3,
				///<summary>
				///Left MENU key
				///</summary>
				LMENU = 0xA4,
				///<summary>
				///Right MENU key
				///</summary>
				RMENU = 0xA5,
				///<summary>
				///Windows 2000/XP: Browser Back key
				///</summary>
				BROWSER_BACK = 0xA6,
				///<summary>
				///Windows 2000/XP: Browser Forward key
				///</summary>
				BROWSER_FORWARD = 0xA7,
				///<summary>
				///Windows 2000/XP: Browser Refresh key
				///</summary>
				BROWSER_REFRESH = 0xA8,
				///<summary>
				///Windows 2000/XP: Browser Stop key
				///</summary>
				BROWSER_STOP = 0xA9,
				///<summary>
				///Windows 2000/XP: Browser Search key 
				///</summary>
				BROWSER_SEARCH = 0xAA,
				///<summary>
				///Windows 2000/XP: Browser Favorites key
				///</summary>
				BROWSER_FAVORITES = 0xAB,
				///<summary>
				///Windows 2000/XP: Browser Start and Home key
				///</summary>
				BROWSER_HOME = 0xAC,
				///<summary>
				///Windows 2000/XP: Volume Mute key
				///</summary>
				VOLUME_MUTE = 0xAD,
				///<summary>
				///Windows 2000/XP: Volume Down key
				///</summary>
				VOLUME_DOWN = 0xAE,
				///<summary>
				///Windows 2000/XP: Volume Up key
				///</summary>
				VOLUME_UP = 0xAF,
				///<summary>
				///Windows 2000/XP: Next Track key
				///</summary>
				MEDIA_NEXT_TRACK = 0xB0,
				///<summary>
				///Windows 2000/XP: Previous Track key
				///</summary>
				MEDIA_PREV_TRACK = 0xB1,
				///<summary>
				///Windows 2000/XP: Stop Media key
				///</summary>
				MEDIA_STOP = 0xB2,
				///<summary>
				///Windows 2000/XP: Play/Pause Media key
				///</summary>
				MEDIA_PLAY_PAUSE = 0xB3,
				///<summary>
				///Windows 2000/XP: Start Mail key
				///</summary>
				LAUNCH_MAIL = 0xB4,
				///<summary>
				///Windows 2000/XP: Select Media key
				///</summary>
				LAUNCH_MEDIA_SELECT = 0xB5,
				///<summary>
				///Windows 2000/XP: Start Application 1 key
				///</summary>
				LAUNCH_APP1 = 0xB6,
				///<summary>
				///Windows 2000/XP: Start Application 2 key
				///</summary>
				LAUNCH_APP2 = 0xB7,
				///<summary>
				///Used for miscellaneous characters; it can vary by keyboard.
				///</summary>
				OEM_1 = 0xBA,
				///<summary>
				///Windows 2000/XP: For any country/region, the '+' key
				///</summary>
				OEM_PLUS = 0xBB,
				///<summary>
				///Windows 2000/XP: For any country/region, the ',' key
				///</summary>
				OEM_COMMA = 0xBC,
				///<summary>
				///Windows 2000/XP: For any country/region, the '-' key
				///</summary>
				OEM_MINUS = 0xBD,
				///<summary>
				///Windows 2000/XP: For any country/region, the '.' key
				///</summary>
				OEM_PERIOD = 0xBE,
				///<summary>
				///Used for miscellaneous characters; it can vary by keyboard.
				///</summary>
				OEM_2 = 0xBF,
				///<summary>
				///Used for miscellaneous characters; it can vary by keyboard. 
				///</summary>
				OEM_3 = 0xC0,
				///<summary>
				///Used for miscellaneous characters; it can vary by keyboard. 
				///</summary>
				OEM_4 = 0xDB,
				///<summary>
				///Used for miscellaneous characters; it can vary by keyboard. 
				///</summary>
				OEM_5 = 0xDC,
				///<summary>
				///Used for miscellaneous characters; it can vary by keyboard. 
				///</summary>
				OEM_6 = 0xDD,
				///<summary>
				///Used for miscellaneous characters; it can vary by keyboard. 
				///</summary>
				OEM_7 = 0xDE,
				///<summary>
				///Used for miscellaneous characters; it can vary by keyboard.
				///</summary>
				OEM_8 = 0xDF,
				///<summary>
				///Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard
				///</summary>
				OEM_102 = 0xE2,
				///<summary>
				///Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
				///</summary>
				PROCESSKEY = 0xE5,
				///<summary>
				///Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes.
				///The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information,
				///see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
				///</summary>
				PACKET = 0xE7,
				///<summary>
				///Attn key
				///</summary>
				ATTN = 0xF6,
				///<summary>
				///CrSel key
				///</summary>
				CRSEL = 0xF7,
				///<summary>
				///ExSel key
				///</summary>
				EXSEL = 0xF8,
				///<summary>
				///Erase EOF key
				///</summary>
				EREOF = 0xF9,
				///<summary>
				///Play key
				///</summary>
				PLAY = 0xFA,
				///<summary>
				///Zoom key
				///</summary>
				ZOOM = 0xFB,
				///<summary>
				///Reserved 
				///</summary>
				NONAME = 0xFC,
				///<summary>
				///PA1 key
				///</summary>
				PA1 = 0xFD,
				///<summary>
				///Clear key
				///</summary>
				OEM_CLEAR = 0xFE
			}

			public enum ScanCodeShort : short
			{
				LBUTTON = 0,
				RBUTTON = 0,
				CANCEL = 70,
				MBUTTON = 0,
				XBUTTON1 = 0,
				XBUTTON2 = 0,
				BACK = 14,
				TAB = 15,
				CLEAR = 76,
				RETURN = 28,
				SHIFT = 42,
				CONTROL = 29,
				MENU = 56,
				PAUSE = 0,
				CAPITAL = 58,
				KANA = 0,
				HANGUL = 0,
				JUNJA = 0,
				FINAL = 0,
				HANJA = 0,
				KANJI = 0,
				ESCAPE = 1,
				CONVERT = 0,
				NONCONVERT = 0,
				ACCEPT = 0,
				MODECHANGE = 0,
				SPACE = 57,
				PRIOR = 73,
				NEXT = 81,
				END = 79,
				HOME = 71,
				LEFT = 75,
				UP = 72,
				RIGHT = 77,
				DOWN = 80,
				SELECT = 0,
				PRINT = 0,
				EXECUTE = 0,
				SNAPSHOT = 84,
				INSERT = 82,
				DELETE = 83,
				HELP = 99,
				KEY_0 = 11,
				KEY_1 = 2,
				KEY_2 = 3,
				KEY_3 = 4,
				KEY_4 = 5,
				KEY_5 = 6,
				KEY_6 = 7,
				KEY_7 = 8,
				KEY_8 = 9,
				KEY_9 = 10,
				KEY_A = 30,
				KEY_B = 48,
				KEY_C = 46,
				KEY_D = 32,
				KEY_E = 18,
				KEY_F = 33,
				KEY_G = 34,
				KEY_H = 35,
				KEY_I = 23,
				KEY_J = 36,
				KEY_K = 37,
				KEY_L = 38,
				KEY_M = 50,
				KEY_N = 49,
				KEY_O = 24,
				KEY_P = 25,
				KEY_Q = 16,
				KEY_R = 19,
				KEY_S = 31,
				KEY_T = 20,
				KEY_U = 22,
				KEY_V = 47,
				KEY_W = 17,
				KEY_X = 45,
				KEY_Y = 21,
				KEY_Z = 44,
				LWIN = 91,
				RWIN = 92,
				APPS = 93,
				SLEEP = 95,
				NUMPAD0 = 82,
				NUMPAD1 = 79,
				NUMPAD2 = 80,
				NUMPAD3 = 81,
				NUMPAD4 = 75,
				NUMPAD5 = 76,
				NUMPAD6 = 77,
				NUMPAD7 = 71,
				NUMPAD8 = 72,
				NUMPAD9 = 73,
				MULTIPLY = 55,
				ADD = 78,
				SEPARATOR = 0,
				SUBTRACT = 74,
				DECIMAL = 83,
				DIVIDE = 53,
				F1 = 59,
				F2 = 60,
				F3 = 61,
				F4 = 62,
				F5 = 63,
				F6 = 64,
				F7 = 65,
				F8 = 66,
				F9 = 67,
				F10 = 68,
				F11 = 87,
				F12 = 88,
				F13 = 100,
				F14 = 101,
				F15 = 102,
				F16 = 103,
				F17 = 104,
				F18 = 105,
				F19 = 106,
				F20 = 107,
				F21 = 108,
				F22 = 109,
				F23 = 110,
				F24 = 118,
				NUMLOCK = 69,
				SCROLL = 70,
				LSHIFT = 42,
				RSHIFT = 54,
				LCONTROL = 29,
				RCONTROL = 29,
				LMENU = 56,
				RMENU = 56,
				BROWSER_BACK = 106,
				BROWSER_FORWARD = 105,
				BROWSER_REFRESH = 103,
				BROWSER_STOP = 104,
				BROWSER_SEARCH = 101,
				BROWSER_FAVORITES = 102,
				BROWSER_HOME = 50,
				VOLUME_MUTE = 32,
				VOLUME_DOWN = 46,
				VOLUME_UP = 48,
				MEDIA_NEXT_TRACK = 25,
				MEDIA_PREV_TRACK = 16,
				MEDIA_STOP = 36,
				MEDIA_PLAY_PAUSE = 34,
				LAUNCH_MAIL = 108,
				LAUNCH_MEDIA_SELECT = 109,
				LAUNCH_APP1 = 107,
				LAUNCH_APP2 = 33,
				OEM_1 = 39,
				OEM_PLUS = 13,
				OEM_COMMA = 51,
				OEM_MINUS = 12,
				OEM_PERIOD = 52,
				OEM_2 = 53,
				OEM_3 = 41,
				OEM_4 = 26,
				OEM_5 = 43,
				OEM_6 = 27,
				OEM_7 = 40,
				OEM_8 = 0,
				OEM_102 = 86,
				PROCESSKEY = 0,
				PACKET = 0,
				ATTN = 0,
				CRSEL = 0,
				EXSEL = 0,
				EREOF = 93,
				PLAY = 0,
				ZOOM = 98,
				NONAME = 0,
				PA1 = 0,
				OEM_CLEAR = 0,
			}

			/// <summary>
			/// Define HARDWAREINPUT struct
			/// </summary>
			[StructLayout(LayoutKind.Sequential)]
			public struct HARDWAREINPUT
			{
				internal int uMsg;
				internal short wParamL;
				internal short wParamH;
			}

			#endregion
		}
	}

	/// <summary>
	/// Base classes extentions
	/// </summary>
	namespace DataBase
	{
		/// <summary>
		/// Get value from DataTableRow with column name. Supposedly works, could fail tho, needs more testing
		/// </summary>
		public static class Extentions
		{
			/// <summary>
			/// Get value from DataTable by column name
			/// </summary>
			/// <param name="dt">DataTable</param>
			/// <param name="row">Current row</param>
			/// <param name="value">Column name</param>
			/// <returns>Value</returns>
			public static string GetValue(this DataTable dt, DataRow row, string value)
			{
				int index = dt.Columns.IndexOf(dt.Columns.Cast<DataColumn>()
					.Where(s => s.ColumnName.ToLower() == value.ToLower()).DefaultIfEmpty(new DataColumn() { ColumnName = "" }).First().ColumnName);
				if (index == -1) return "";

				object result = dt.Rows.Cast<DataRow>().ToList()[dt.Rows.IndexOf(row)].ItemArray[index];

				if (Convert.IsDBNull(result)) return "";

				return result.ToString();
			}
			/// <summary>
			/// Fills entire list from DataTable. Generic, variable name and type should be same as column name and type
			/// </summary>
			/// <typeparam name="T">List type</typeparam>
			/// <param name="dt">DataTable</param>
			/// <returns>Filled list</returns>
			public static List<T> Fill<T>(this DataTable dt)
			{
				List<T> list = new List<T>();
				Type t = typeof(T);

				int[] indexes =new int[dt.Columns.Count];

				t.GetProperties().ToList().ForEach(p =>
				{
					try
					{
						indexes[t.GetProperties().ToList().IndexOf(p)] = dt.Columns.IndexOf(dt.Columns.Cast<DataColumn>()
							.ToList()
							.Where(c => c.ColumnName.ToLower() == p.Name.ToLower() && c.DataType == p.PropertyType).FirstOrDefault());
					} catch {}
				});
				dt.Rows.Cast<DataRow>().ToList().ForEach(row =>
				{
					object def = Activator.CreateInstance(t);
					t.GetProperties().ToList().ForEach(prop =>
					{
						try
						{
							//мне кажется не оптимизировано
							//не нихуя. лям записей (2 столб) за 8 сек
							//						(10 столб) за 46 сек
							// для ляма вполне неплохо, сам ПО от разрабов обрабатывает такой объем за 20 сек.
							//так что вполне вполне. оптимизация возможна, но только для поиска индекса строкой ниже
							//попробовал. 25 сек. и это при том что используется OleDbDataAdapter.Fill(DataTable), который тоже время жрет.
							//как итог готов к использованию
							//лям записей за 25 сек.
							//как оказалось этот метод работает за ~5 сек, а получение записей из GetData(str, str) занимает ~20 сек. так что оптимизация ваще огонь
							int index = indexes[t.GetProperties().ToList().IndexOf(prop)];
							//row.Table.Columns.IndexOf(row.Table.Columns
							//	.Cast<DataColumn>()
							//	.ToList()
							//	.Where(s => s.ColumnName.ToLower() == prop.Name.ToLower() && s.DataType == prop.PropertyType)
							//	.FirstOrDefault());

							if (index != -1)
							{
								object val = row[index];
								if (val != DBNull.Value)
								{
									prop.SetValue(def, val, null);
								}
							}
						}
						catch { return; }
					});

					T c = (T)def;
					list.Add(c);
				});

				return list;
			}

			/// <summary>
			/// Sets all DataPropertyNames in DataGridView to Type's public properties
			/// </summary>
			/// <param name="Table">DataGridView</param>
			/// <param name="Struct">Type with public properties</param>
			public static void DataGridSetPropertyNames(this DataGridView Table, Type Struct)
			{
				if (Table.ColumnCount < Struct.GetProperties().Count())
					Table.ColumnCount = Struct.GetProperties().Count();

				Struct.GetProperties().ToList().ForEach(prop =>
				{
					Table.Columns[Struct.GetProperties().ToList().IndexOf(prop)].DataPropertyName = prop.Name;
				});
			}
		}
		public static class Methods
		{
			/// <summary>
			/// Opens connection, gets data with set querry and returns DataTable with that data
			/// </summary>
			/// <param name="ConnectionString">Connection string</param>
			/// <param name="Querry">Querry. Should return something</param>
			/// <returns>filled DataTable</returns>
			public static DataTable GetData(string ConnectionString, string Querry)
			{
				DataTable dt = new DataTable();
				try
				{
					using (OleDbConnection conn = new OleDbConnection(ConnectionString))
					{
						conn.Open();

						OleDbCommand cmd = new OleDbCommand()
						{
							CommandText = Querry,
							Connection = conn
						};
						OleDbDataAdapter da = new OleDbDataAdapter(cmd);
						da.Fill(dt);

						return dt;
					}
				}
				catch (Exception exc)
				{
					DebugLogger.WriteErrorFile(exc);
					return dt;
				}
			}
			/// <summary>
			/// Gets first row of first column of given connection and querry
			/// </summary>
			/// <param name="ConnectionString">Connection</param>
			/// <param name="Querry">Querry</param>
			/// <returns>Resulting string</returns>
			public static void GetQuickData(string ConnectionString, string Querry, out string result)
			{
				try
				{
					using (OleDbConnection conn = new OleDbConnection(ConnectionString))
					{
						conn.Open();
						OleDbCommand cmd = new OleDbCommand() { Connection = conn, CommandText = Querry };

						object res = cmd.ExecuteScalar();
						if (res == null) result = "";
						else result = res.ToString();
					}
				}
				catch (Exception exc) { DebugLogger.WriteErrorFile(exc); throw; }
			}
			/// <summary>
			/// Test any connection
			/// </summary>
			/// <param name="ConnectionString">Connection string</param>
			/// <returns>Item1 - Test is passed or not, Item2 - Result message</returns>
			public static Tuple<bool, string> TestConnection(string ConnectionString)
			{
				using (OleDbConnection conn = new OleDbConnection(ConnectionString))
				{
					try { conn.Open(); } catch (Exception exc) { return new Tuple<bool, string>(false, exc.Message); }
					return new Tuple<bool, string>(true, "Успешно");
				}
			}
			/// <summary>
			/// Replaces every [properyName] in Text with property value
			/// </summary>
			/// <typeparam name="T">Type from which property name would be search and property type would replace it</typeparam>
			/// <param name="Text">Text to change</param>
			/// <param name="Type">Variable</param>
			/// <returns>Changed string</returns>
			public static string SetUpStringWithVariables<T>(string Text, T Type)
			{
				string result = Text;
				foreach (PropertyInfo prop in Type.GetType().GetProperties())
				{
					Regex regex = new Regex(@"\[" + prop.Name + @"\]");

					foreach (Match match in regex.Matches(Text))
					{
						result = regex.Replace(result, prop.GetValue(Type).ToString());
					}
				}
				return result;
			}
		}
	}


	/// <summary>
	/// More Types
	/// </summary>
	namespace Types
	{
		#region Structs
		public struct User
		{
			public string UserName { get; set; }
			public string UserPassword { get; set; }
		}
		#endregion
		#region EventArgs
		/// <summary>
		/// Transfers all user data in string and current logged in user
		/// </summary>
		public class UserEventArgs : EventArgs
		{
			/// <summary>
			/// string that contains user data
			/// </summary>
			public string UserData { get; set; }
			/// <summary>
			/// User struct that also has all information about user
			/// </summary>
			public User User { get; set; }
		}
		/// <summary>
		/// Event passing only one string
		/// </summary>
		public class StringEventArgs : EventArgs
		{
			/// <summary>
			/// String to pass
			/// </summary>
			public string String { get; set; }
		}
		/// <summary>
		/// Event passing only one integer
		/// </summary>
		public class IntEventArgs :EventArgs
		{
			public int Number { get; set; }
		}
		/// <summary>
		/// Event passing only one float number
		/// </summary>
		public class FloatEventArgs : EventArgs
		{
			public float Number { get; set; }
		}
		/// <summary>
		/// Event passing only one decimal
		/// </summary>
		public class DecimalEventArgs : EventArgs
		{
			public decimal Number { get; set; }
		}
		/// <summary>
		/// Event passing only one boolean
		/// </summary>
		public class BoolEventArgs : EventArgs
		{
			public bool Bool { get; set; }
		}
		#endregion
	}

	/// <summary>
	/// Constructor will give error until compile + constructor reopen
	/// </summary>
	namespace FormControls
	{
		/// <summary>
		/// TextBox with colored borders
		/// </summary>
		class TextBoxColoredBorder : TextBox
		{
			const int WM_NCPAINT = 0x85;
			const uint RDW_INVALIDATE = 0x1;
			const uint RDW_IUPDATENOW = 0x100;
			const uint RDW_FRAME = 0x400;
			[DllImport("user32.dll")]
			static extern IntPtr GetWindowDC(IntPtr hWnd);
			[DllImport("user32.dll")]
			static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
			[DllImport("user32.dll")]
			static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprc, IntPtr hrgn, uint flags);
			Color borderColor = Color.Gray;
			public Color BorderColor
			{
				get { return borderColor; }
				set
				{
					borderColor = value;
					RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero,
						RDW_FRAME | RDW_IUPDATENOW | RDW_INVALIDATE);
				}
			}
			protected override void WndProc(ref Message m)
			{
				base.WndProc(ref m);
				if (m.Msg == WM_NCPAINT && BorderColor != Color.Transparent &&
					BorderStyle == System.Windows.Forms.BorderStyle.Fixed3D)
				{
					var hdc = GetWindowDC(this.Handle);
					using (var g = Graphics.FromHdcInternal(hdc))
					using (var p = new Pen(BorderColor, 5))
						g.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));
					ReleaseDC(this.Handle, hdc);
				}
			}
			protected override void OnSizeChanged(EventArgs e)
			{
				base.OnSizeChanged(e);
				RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero,
					   RDW_FRAME | RDW_IUPDATENOW | RDW_INVALIDATE);
			}

		}

		/// <summary>
		/// Virtual keyboard (with only letters and numbers)
		/// </summary>
		public class KeyBoard : Form
		{
			readonly string keys = "1234567890-←йцукенгшщзхъфывапролджэячсмитьбю␣";
			readonly int numKeys = 12;
			readonly int keysRow1 = 12;
			readonly int keysRow2 = 11;
			readonly int keysRow3 = 10;

			readonly int marginX = 10;
			readonly int marginY = 10;
			readonly int distanceBetweenButtons = 0;
			readonly int buttonSize = 70;
			readonly int offset = 20;
			int PrefferedWidth 
			{
				get
				{
					return 12 * (buttonSize + distanceBetweenButtons) + offset + (marginX * 2);
				}
			}
			int PrefferedHeigth
			{
				get
				{
					return (distanceBetweenButtons + buttonSize) * 4 + marginY*2;
				}
			}

			#region focus
			private const int WM_MOUSEACTIVATE = 0x0021, MA_NOACTIVATE = 0x0003;

			private const int WS_EX_TOOLWINDOW = 0x00000080;
			private const int WS_EX_NOACTIVATE = 0x08000000;
			private const int WS_EX_TOPMOST = 0x00000008;
			private const int CP_NOCLOSE_BUTTON = 0x200;
			protected override CreateParams CreateParams
			{
				get
				{
					CreateParams cp = base.CreateParams;
					cp.ExStyle |= (WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
					cp.Parent = IntPtr.Zero; // Keep this line only if you used UserControl
					cp.ClassStyle |= CP_NOCLOSE_BUTTON;
					cp.X = this.Location.X;
					cp.Y = this.Location.Y;
					return cp;
				}
			}

			protected override void WndProc(ref Message m)
			{
				if (m.Msg == WM_MOUSEACTIVATE)
				{
					m.Result = (IntPtr)MA_NOACTIVATE;
					return;
				}
				base.WndProc(ref m);
			}
			protected override bool ShowWithoutActivation => true;
			#endregion
			public KeyBoard()
			{
				this.TopMost = true;
				this.Size = new Size(PrefferedWidth, PrefferedHeigth);
				this.MaximumSize = new Size(PrefferedWidth, PrefferedHeigth);
				this.MinimumSize = new Size(PrefferedWidth, PrefferedHeigth);
				this.Controls.AddRange(CreateKeyBoardButtons(keys));
				this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - PrefferedWidth - marginX, 200);
				this.ShowInTaskbar = false;
				this.FormBorderStyle = FormBorderStyle.None;
			}
			Button[] CreateKeyBoardButtons(string Keys)
			{
				Button[] buttons = new Button[Keys.Length];

				int[] kostil = { 0, numKeys, keysRow1, keysRow2, keysRow3 };

				for (int j = 0, i = 0, row = 1; j < Keys.Length; j++, i++)
				{
					if (i == kostil[row])
					{
						i = 0;
						row++;
					}
					buttons[j] = GetNewButton(Keys[j].ToString(), new Point
						(
						marginX + ((distanceBetweenButtons + buttonSize) * i) + (offset * (row - 1)),
						marginY + ((distanceBetweenButtons + buttonSize) * (row - 1))
						), new Size(buttonSize, buttonSize));
				}

				return buttons;
			}
			CustomButton GetNewButton(string Text, Point Location, Size Size)
			{
				CustomButton button = new CustomButton
				{
					Size = Size,
					Text = Text,
					Location = Location,
					TabStop = false,
					Font = new Font("Century Gothic", 22, FontStyle.Regular)
				};
				button.Click += (s, e) =>
				{
					switch (button.Text)
					{
						case "←": SendKeys.Send("{BACKSPACE}"); break;
						case "␣": SendKeys.Send(" "); break;

						default: SendKeys.Send("{" + button.Text + "}"); break;
					}
				};

				return button;
			}

			public class CustomButton : Button
			{
				#region focus
				public CustomButton() : base()
				{
					SetStyle(ControlStyles.Selectable, false);
				}
				#endregion
			}
		}

		/// <summary>
		/// Extended button with ContextMenuStrip on left click.
		/// </summary>
		public class MenuButton : System.Windows.Forms.Button
		{
			[DefaultValue(null)]
			public ContextMenuStrip Menu { get; set; }

			[DefaultValue(false)]
			public bool ShowMenuUnderCursor { get; set; }
			[DefaultValue(true)]
			public bool ShowArrow { get; set; }

			protected override void OnMouseDown(MouseEventArgs mevent)
			{
				base.OnMouseDown(mevent);

				if (Menu != null && mevent.Button == MouseButtons.Left)
				{
					Point menuLocation;

					if (ShowMenuUnderCursor)
					{
						menuLocation = mevent.Location;
					}
					else
					{
						menuLocation = new Point(0, Height - 1);
					}

					Menu.Show(this, menuLocation);
				}
			}

			protected override void OnPaint(PaintEventArgs pevent)
			{
				base.OnPaint(pevent);

				if (Menu != null && ShowArrow)
				{
					int arrowX = ClientRectangle.Width - Padding.Right - 14;
					int arrowY = (ClientRectangle.Height / 2) - 1;

					Color color = Enabled ? ForeColor : SystemColors.ControlDark;
					using (Brush brush = new SolidBrush(color))
					{
						Point[] arrows = new Point[] { new Point(arrowX, arrowY), new Point(arrowX + 7, arrowY), new Point(arrowX + 3, arrowY + 4) };
						pevent.Graphics.FillPolygon(brush, arrows);
					}
				}
			}
		}

		/// <summary>
		/// Universal Logining in database form. Only work with OleDb connections.
		/// </summary>
		public partial class FormLogin : Form
		{
			#region Variables
			string UserData;
			public HashSet<Types.User> Users { get; internal set; } = new HashSet<Types.User>();
			/// <summary>
			/// Полная строка подключения
			/// </summary>
			public string GetConnectionString { get => ConnectionString +";"+ CurrentUserConnection; }
			/// <summary>
			/// Только User Id и Password
			/// </summary>
			string CurrentUserConnection { get; set; }
			/// <summary>
			/// Текущий пользователь
			/// </summary>
			public Types.User CurrentUser
			{
				get => currentUser;
				set
				{
					string c = GUCS(value.UserName, value.UserPassword);
					CurrentUserConnection = c;
					currentUser = value;
					textBoxLogin.Text = value.UserName;
					textBoxPassword.Text = value.UserPassword;
				}
			}
			private Types.User currentUser;
			public string ConnectionString { get; set; }
			#endregion Variables

			#region Events
			/// <summary>
			/// When user data preped and ready to be saved in one string
			/// </summary>
			public event EventHandler<Types.UserEventArgs> UserDataReadyToSave;
			/// <summary>
			/// Connection test done successfully and user can be logged in
			/// </summary>
			public event EventHandler<Types.UserEventArgs> ConnectionTestSucces;
			/// <summary>
			/// Connection test failed or canceled
			/// </summary>
			public event EventHandler<Types.StringEventArgs> ConnectionTestFailed;
			#endregion

			#region Contructors
			/// <summary>
			/// Initializes login form. Does not shows it.
			/// </summary>
			/// <param name="UserData">UserData stirng that you can get via UserDataReadyToSave event. Just pass it back in that constructor</param>
			/// <param name="ConnectionString">Connection string without user login info. Provider, ip, port, etc</param>
			public FormLogin(string UserData, string ConnectionString)
			{
				this.UserData = UserData;
				this.ConnectionString = ConnectionString;
				InitializeComponent();
			}
			#endregion

			#region ConnectionAndLoginFunctions

			/// <summary>
			/// Loads connections from given string in constructor
			/// </summary>
			private void LoadConnections()
			{
				string[] conns = UserData.Split(":".ToArray(), StringSplitOptions.RemoveEmptyEntries);

				foreach (string conn in conns)
				{
					string uid = conn.Split(';')[0];
					string pass = conn.Split(';')[1];

					Users.Add(new Types.User() { UserName = uid, UserPassword = pass });
				}
			}
			/// <summary>
			/// Iterate trough all loaded users and write string. No username or password should include ';' or ':'
			/// </summary>
			public void SaveUserData()
			{
				string data = "";
				foreach (Types.User user in Users)
				{
					data += ":" + user.UserName + ";" + user.UserPassword;
				}
				UserDataReadyToSave?.Invoke(this, new Types.UserEventArgs() { UserData = data, User = CurrentUser });
			}
			/// <summary>
			/// Get User Connection String
			/// </summary>
			/// <param name="Uid">User Id</param>
			/// <param name="Password">Password</param>
			/// <returns>"User Id = ;Password = ". literally</returns>
			public static string GUCS(string Uid, string Password)
			{
				return $"User Id={Uid};Password={Password}";
			}

			/// <summary>
			/// Try connection and send event about in success or failure
			/// </summary>
			void TestConnection()
			{
				var t = DataBase.Methods.TestConnection(GetConnectionString);

				if (t.Item1)
				{
					CurrentUser = new Types.User() { UserName = textBoxLogin.Text, UserPassword = textBoxPassword.Text };
					Users.Add(currentUser);

					ConnectionTestSucces?.Invoke(this, new Types.UserEventArgs() { UserData = UserData, User = CurrentUser });
				}
				else
					ConnectionTestFailed?.Invoke(this, new Types.StringEventArgs() { String = t.Item2 });
			}
			//Fill menuButton with userData
			void FillConnectionsContent()
			{
				foreach (Types.User user in Users)
				{
					ContextMenuConnections.Items.Add(user.UserName);
				}
			}
			#endregion

			#region Controll events

			/// <summary>
			/// Ok click
			/// </summary>
			private void ButtonOk_Click(object sender, EventArgs e)
			{
				CurrentUserConnection = GUCS(textBoxLogin.Text, textBoxPassword.Text);
				TestConnection();
			}

			//Cancel click
			private void ButtonCancel_Click(object sender, EventArgs e)
			{
				ConnectionTestFailed?.Invoke(this, new Types.StringEventArgs() { String = "Операция прекращена пользователем" });
				this.Close();
			}
			//loading data
			private void FormLogin_Load(object sender, EventArgs e)
			{
				LoadConnections();
				FillConnectionsContent();
			}
			//fills textboxes and tries to connect outright with any menuButton context menu click
			private void ToolStripItemClick(object sender, ToolStripItemClickedEventArgs e)
			{
				CurrentUser = Users.Where(u => u.UserName == e.ClickedItem.Text).FirstOrDefault();
				TestConnection();
			}
			#endregion
		}
		//Autoganerated stuff. modify if needed
		partial class FormLogin
		{
			//TODO: press Enter to press buttonOk. On any textbox
			/// <summary>
			/// Required designer variable.
			/// </summary>
			private System.ComponentModel.IContainer components = null;

			/// <summary>
			/// Clean up any resources being used.
			/// </summary>
			/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
			protected override void Dispose(bool disposing)
			{
				if (disposing && (components != null))
				{
					components.Dispose();
				}
				base.Dispose(disposing);
			}

			#region Windows Form Designer generated code

			/// <summary>
			/// Required method for Designer support - do not modify
			/// the contents of this method with the code editor.
			/// 
			/// or do modify it - designer no mo :'(
			/// </summary>
			private void InitializeComponent()
			{
				this.components = new System.ComponentModel.Container();
				this.panel1 = new System.Windows.Forms.Panel();
				this.ContextMenuConnections = new System.Windows.Forms.ContextMenuStrip(this.components);
				this.textBoxPassword = new System.Windows.Forms.TextBox();
				this.textBoxLogin = new System.Windows.Forms.TextBox();
				this.label2 = new System.Windows.Forms.Label();
				this.label1 = new System.Windows.Forms.Label();
				this.buttonOk = new System.Windows.Forms.Button();
				this.buttonCancel = new System.Windows.Forms.Button();
				this.menuButtonSavedConnections = new Utilities.FormControls.MenuButton();
				this.panel1.SuspendLayout();
				this.SuspendLayout();
				// 
				// panel1
				// 
				this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
				this.panel1.Controls.Add(this.menuButtonSavedConnections);
				this.panel1.Controls.Add(this.textBoxPassword);
				this.panel1.Controls.Add(this.textBoxLogin);
				this.panel1.Controls.Add(this.label2);
				this.panel1.Controls.Add(this.label1);
				this.panel1.Controls.Add(this.buttonOk);
				this.panel1.Controls.Add(this.buttonCancel);
				this.panel1.Location = new System.Drawing.Point(12, 12);
				this.panel1.Name = "panel1";
				this.panel1.Size = new System.Drawing.Size(386, 138);
				this.panel1.TabIndex = 99;
				// 
				// ContextMenuConnections
				// 
				this.ContextMenuConnections.Name = "ContextMenuConnections";
				this.ContextMenuConnections.Size = new System.Drawing.Size(61, 4);
				this.ContextMenuConnections.ItemClicked += new ToolStripItemClickedEventHandler(this.ToolStripItemClick);
				// 
				// textBoxPasswordTextBox
				// 
				this.textBoxPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
				this.textBoxPassword.Location = new System.Drawing.Point(90, 49);
				this.textBoxPassword.Name = "textBoxPassword";
				this.textBoxPassword.Size = new System.Drawing.Size(251, 23);
				this.textBoxPassword.TabIndex = 2;
				this.textBoxPassword.PasswordChar = '•';
				// 
				// textBoxLogin
				// 
				this.textBoxLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
				this.textBoxLogin.Location = new System.Drawing.Point(90, 21);
				this.textBoxLogin.Name = "textBoxLogin";
				this.textBoxLogin.Size = new System.Drawing.Size(251, 23);
				this.textBoxLogin.TabIndex = 1;
				// 
				// label2
				// 
				this.label2.AutoSize = true;
				this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
				this.label2.Location = new System.Drawing.Point(27, 52);
				this.label2.Name = "label2";
				this.label2.Size = new System.Drawing.Size(57, 17);
				this.label2.TabIndex = 1;
				this.label2.Text = "Пароль";
				// 
				// label1
				// 
				this.label1.AutoSize = true;
				this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
				this.label1.Location = new System.Drawing.Point(27, 24);
				this.label1.Name = "label1";
				this.label1.Size = new System.Drawing.Size(47, 17);
				this.label1.TabIndex = 0;
				this.label1.Text = "Логин";
				// 
				// buttonOK
				// 
				this.buttonOk.Location = new System.Drawing.Point(12, 100);
				this.buttonOk.Name = "buttonOk";
				this.buttonOk.Size = new System.Drawing.Size(75, 23);
				this.buttonOk.TabIndex = 3;
				this.buttonOk.Text = "OK";
				this.buttonOk.UseVisualStyleBackColor = true;
				this.buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
				// 
				// buttonCancel
				// 
				this.buttonCancel.Location = new System.Drawing.Point(93, 100);
				this.buttonCancel.Name = "buttonCancel";
				this.buttonCancel.Size = new System.Drawing.Size(75, 23);
				this.buttonCancel.TabIndex = 4;
				this.buttonCancel.Text = "Закрыть";
				this.buttonCancel.UseVisualStyleBackColor = true;
				this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
				// 
				// menuButtonSavedConnections
				// 
				this.menuButtonSavedConnections.BackgroundImage = global::WastewaterControl.Properties.Resources.три_точки;
				this.menuButtonSavedConnections.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
				this.menuButtonSavedConnections.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
				this.menuButtonSavedConnections.Location = new System.Drawing.Point(347, 20);
				this.menuButtonSavedConnections.Menu = this.ContextMenuConnections;
				this.menuButtonSavedConnections.Name = "menuButtonSavedConnections";
				this.menuButtonSavedConnections.ShowArrow = false;
				this.menuButtonSavedConnections.ShowMenuUnderCursor = true;
				this.menuButtonSavedConnections.Size = new System.Drawing.Size(24, 24);
				this.menuButtonSavedConnections.TabIndex = 99;
				this.menuButtonSavedConnections.UseVisualStyleBackColor = true;
				// 
				// FormLogin
				// 
				this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
				this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
				this.ClientSize = new System.Drawing.Size(406, 167);
				this.Controls.Add(this.panel1);
				this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
				this.Name = "FormLogin";
				this.ShowIcon = false;
				this.Text = "Введите логин и пароль";
				this.Load += new System.EventHandler(this.FormLogin_Load);
				this.panel1.ResumeLayout(false);
				this.panel1.PerformLayout();
				this.ResumeLayout(false);

			}

			#endregion
			#region Controls
			private System.Windows.Forms.Panel panel1;
			private Utilities.FormControls.MenuButton menuButtonSavedConnections;
			private System.Windows.Forms.TextBox textBoxPassword;
			private System.Windows.Forms.TextBox textBoxLogin;
			private System.Windows.Forms.Label label2;
			private System.Windows.Forms.Label label1;
			private System.Windows.Forms.Button buttonOk;
			private System.Windows.Forms.Button buttonCancel;
			private System.Windows.Forms.ContextMenuStrip ContextMenuConnections;
			#endregion
		}
	}

	/// <summary>
	/// Some extra extras
	/// </summary>
	namespace Extras
	{
		class ClickOne
		{
			/// <summary>
			/// Better update check than normal "Chekc for updates" ClickOne feature, still uses ClickOne tho
			/// </summary>
			/// <param name="ManualUpdateCheck">Give some feedback when user tries to update last vervion</param>
			/// <returns>true if updated, better not "Run" Form when it does... or maybe its ok? idk</returns>
			public static bool CheckForUpdates(bool ManualUpdateCheck)
			{
				if (ApplicationDeployment.IsNetworkDeployed)
				{
					ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;

					UpdateCheckInfo info;
					try
					{
						DebugLogger.WriteHiddenLine("Проверка обновления");
						info = ad.CheckForDetailedUpdate();

					}
					catch (DeploymentDownloadException exc)
					{
						if (ManualUpdateCheck)
							MessageBox.Show("Невозможно установить обновление.\n\nПроверьте соединение или попробуйте позже.\n\nОшибка: " + exc.Message, "Ошибка при обновлении", MessageBoxButtons.OK, MessageBoxIcon.Error);
						else
						{
							DebugLogger.WriteErrorFile(exc);
							DebugLogger.WriteHiddenLine("Ошибка при установке обновления", "UpdateLogs.txt");
						}
						return false;
					}
					catch (InvalidDeploymentException exc)
					{
						if (ManualUpdateCheck)
							MessageBox.Show("Ошибка при проверке обновлении\n\nНеобходимо обратиться к специалисту.\n\nОшибка: " + exc.Message, "Ошибка при обновлении", MessageBoxButtons.OK, MessageBoxIcon.Error);
						else
							DebugLogger.WriteErrorFile(exc);
						return false;
					}
					catch (InvalidOperationException exc)
					{
						if (ManualUpdateCheck)
							MessageBox.Show("Приложение не может обновиться. Скорее всего это не ClickOne.\n\nОшибка: " + exc.Message, "Ошибка при обновлении", MessageBoxButtons.OK, MessageBoxIcon.Error);
						else
							DebugLogger.WriteErrorFile(exc);
						return false;
					}

					if (info.UpdateAvailable)
					{
						Boolean doUpdate = true;

						if (!ManualUpdateCheck)
						{
							try
							{

								ad.Update();
								DebugLogger.WriteHiddenLine("Обновление прошло успешно, перезапуск");
								Application.Restart();
								return true;
							}
							catch (Exception exc)
							{
								DebugLogger.WriteErrorFile(exc);
							}
						}

						if (!info.IsUpdateRequired)
						{
							DialogResult dr = MessageBox.Show("Обновление доступно. Приступить к установке обновления?", "Обновление", MessageBoxButtons.OKCancel);
							if (!(DialogResult.OK == dr))
							{
								doUpdate = false;
							}
						}
						else
						{
							// Display a message that the app MUST reboot. Display the minimum required version.
							MessageBox.Show("Обнаружено важное обновление. " +
								"Минимальная версия приложения: " + info.MinimumRequiredVersion.ToString() +
								". Приложение приступит к обновлению и перезапустится.",
								"Обновление", MessageBoxButtons.OK,
								MessageBoxIcon.Information);
						}

						if (doUpdate)
						{
							try
							{
								ad.Update();
								MessageBox.Show("Обновление прошло успешно, перезапуск приложения.", "Обновление успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
								Application.Restart();
								return true;
							}
							catch (DeploymentDownloadException exc)
							{
								MessageBox.Show("Невозможно установить обновление.\n\nПроверьте соединение или попробуйте позже.\n\nОшибка: " + exc.Message, "Ошибка при обновлении", MessageBoxButtons.OK, MessageBoxIcon.Error);
							}
						}
					}
					else
					{
						if (ManualUpdateCheck)
							MessageBox.Show("Приложение использует последнюю версию, обновление не требуется.", "Последнее обновление установлено", MessageBoxButtons.OK, MessageBoxIcon.Information);
						return false;
					}
				}
				return false;
			}

			/// <summary>
			/// Check and update without any asking. Auto update, restart, exception catching
			/// </summary>
			/// <returns>true if updated, false otherwise</returns>
			public static bool CheckAndUpdateInBackground()
			{
				if (ApplicationDeployment.IsNetworkDeployed)
				{
					ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;

					UpdateCheckInfo info;
					try
					{
						info = ad.CheckForDetailedUpdate();
					}
					catch (DeploymentDownloadException exc)
					{
						DebugLogger.WriteErrorFile(exc);
						return false;
					}
					catch (InvalidDeploymentException exc)
					{
						DebugLogger.WriteErrorFile(exc);
						return false;
					}
					catch (InvalidOperationException exc)
					{
						DebugLogger.WriteErrorFile(exc);
						return false;
					}

					if (info.UpdateAvailable)
					{
						try
						{
							ad.Update();
							Application.Restart();
							return true;
						}
						catch (DeploymentDownloadException exc)
						{
							DebugLogger.WriteErrorFile(exc);
						}
						catch (Exception exc)
						{
							DebugLogger.WriteErrorFile(exc);
						}
					}
					else
					{
						return false;
					}
				}
				return false;
			}

			/// <summary>
			/// Run timer to keep updates up. Checkes and updates once a day in specific hour by 24-hour clock
			/// </summary>
			/// <param name="HourToCheck">Hour to check. Could be between 0 and 23, any other value sets to 22 instead</param>
			public static void RunUpdateCheck(int HourToCheck = 22)
			{
				if (HourToCheck > 22 || HourToCheck < 0) HourToCheck = 22;

				Timer timer = new Timer()
				{
					Interval = 360000
				};
				timer.Tick += (s, e) =>
				{
					if (DateTime.Now.Hour >= HourToCheck || DateTime.Now.Hour < HourToCheck + 2)
					{
						CheckForUpdates(false);
					}
				};
				timer.Start();
			}
		}
	}
}
