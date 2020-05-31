using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace quick_mouse_recorder
{
	[Serializable]
	public class CommandChunk
	{
		//public string Name { get; set; }
		public int Time { get; set; }
		public uint Id { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public override string ToString()
		{
			return $"{Time}, {Id}, {X}/{Y}";
		}
	}
	[Serializable]
	public class PlayData
	{
		public string Name { get; set; }
		public CommandChunk[] Commands { get; set; } = new CommandChunk[0];
	}
	[Serializable]
	public class Config
	{
		// members
		public List<PlayData> PlayList { get; set; } = new List<PlayData>();
		public float CaptureInterval { get; set; } = 0.01f;
		//public float RepeatInterval { get; set; }
		public bool EnableHotKey { get; set; }

		static Config _config;
		public static Config Instance {
			get {
				if (_config == null) {
					_config = ReadConfig();
				}
				return _config;
			}
		}

		/// <summary>
		/// 設定ファイルのフルパスを取得
		/// </summary>
		/// <returns>設定ファイルのフルパス</returns>
		public static string GetConfigFilePath()
		{
			// 実行ファイルのフルパスを取得
			string appFilePath = System.Reflection.Assembly.GetEntryAssembly().Location;

			// 実行ファイルのフルパス末尾（拡張子）を変えて返す
			return System.Text.RegularExpressions.Regex.Replace(
				appFilePath,
				".exe",
				".json",
				System.Text.RegularExpressions.RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// 設定読み込み
		/// </summary>
		/// <returns></returns>
		public static Config ReadConfig()
		{
			// 設定ファイルのフルパスを取得
			string configFile = GetConfigFilePath();

			if (File.Exists(configFile) == false) {
				// 設定ファイルなし
				return new Config();
			}

			using (var reader = new StreamReader(configFile, Encoding.UTF8)) {
				// 設定ファイル読み込み
				string buf = reader.ReadToEnd();

				// デシリアライズして返す
				var js = new System.Web.Script.Serialization.JavaScriptSerializer();
				return js.Deserialize<Config>(buf);
			}
		}

		/// <summary>
		/// 設定書き込み
		/// </summary>
		/// <param name="cfg"></param>
		public static void WriteConfig(Config cfg)
		{
			// シリアライズ
			var js = new System.Web.Script.Serialization.JavaScriptSerializer();
			string buf = js.Serialize(cfg);

			// 設定ファイルのフルパス取得
			string configFile = GetConfigFilePath();

			using (var writer = new StreamWriter(configFile, false, Encoding.UTF8)) {
				// 設定ファイルに書き込む
				writer.Write(buf);
			}
		}
	}
}
