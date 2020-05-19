using Reactive.Bindings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace quick_mouse_recorder
{
	class MainViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<CommandChunk> ListCommand { get; } = new ObservableCollection<CommandChunk>();
		public ObservableCollection<string> ListNames { get; } = new ObservableCollection<string>();
		//public Config Config { get; private set; }
		public bool IsRecording { get; private set; }
		public bool IsPlaying { get; private set; }
		int _selectedIndexListName;
		public int SelectedIndexListName {
			get {
				return _selectedIndexListName;
			}
			set {
				_selectedIndexListName = value;
				RefreshCommandList(ListNames[value]);
				NotifyPropertyChanged();
				NotifyPropertyChanged("CanablePlay");
			}
		}
		int _selectedIndexListCommand = -1;
		public int SelectedIndexListCommand {
			get {
				return _selectedIndexListCommand;
			}
			set {
				_selectedIndexListCommand = value;
				NotifyPropertyChanged();
			}
		}
		public event System.Action OnFinishCommand;
		public ReactiveProperty<float> CaptureInterval { get; } = new ReactiveProperty<float>();
		public ReactiveProperty<string> TryCount { get; } = new ReactiveProperty<string>("1");

		public ReactiveProperty<VM_ContentHotKey> VM_ContentHotkey { get; } = new ReactiveProperty<VM_ContentHotKey>(new VM_ContentHotKey());
		public bool EnableHotKey => VM_ContentHotkey.Value.EnableHotKey;

		public ReactiveProperty<bool> EnableFileList { get; } = new ReactiveProperty<bool>(true);
		public ReactiveProperty<bool> EnableCommandList { get; } = new ReactiveProperty<bool>(true);
		public ReactiveProperty<bool> EnableSettings { get; } = new ReactiveProperty<bool>(true);
		public ReactiveProperty<bool> EnableRecButton { get; } = new ReactiveProperty<bool>(true);
		public bool EnablePlayButton {
			get {
				return _enablePlayButton && ListCommand.Any();
			}
			set {
				_enablePlayButton = value;
				NotifyPropertyChanged();
			}
		}
		bool _enablePlayButton = true;

		public MainViewModel()
		{
		}

		public void Init()
		{
			//Config = Config.ReadConfig();
			//if (Config == null) {
			//	cn.log("make new config");
			//	Config = new Config();
			//	return;
			//}
			if (Config.Instance.CommandList.Any()) {
				foreach (var i in Config.Instance.CommandList.Values.First())
					ListCommand.Add(i);
				foreach (var i in Config.Instance.CommandList.Keys)
					ListNames.Add(i);
				//listBoxEventName.SelectedIndex = 0;
			}
			VM_ContentHotkey.Value.Init();
			NotifyPropertyChanged(nameof(EnablePlayButton));
		}

		public void SaveConfig()
		{
			Config.Instance.IntervalCapture = CaptureInterval.Value;
			cn.log(Config.Instance.IntervalCapture);
			Config.WriteConfig(Config.Instance);
		}

		public void ProcCommand()
		{
			if (IsPlaying) {
				StopCommand();
			}
			else {
				_ = StartCommand();
			}
		}

		public async Task StartCommand()
		{
			EnableCommandList.Value = false;
			EnableFileList.Value = false;
			EnableSettings.Value = false;
			EnableRecButton.Value = false;
			IsPlaying = true;
			//var oldx = 0;
			//var oldy = 0;
			int.TryParse(TryCount.Value, out int try_total);
			try_total = try_total <= 0 ? 1 : try_total;
			cn.log(try_total);
			for (int try_count = 0; try_count < try_total; ++try_count) {
				var prev_wait = 0;
				for (int i1 = 0; i1 < ListCommand.Count; i1++) {
					var cmd = ListCommand[i1];
					if (!IsPlaying) {
						cn.log("中断しました");
						break;
					}
					await Task.Delay(cmd.Time - prev_wait);
					prev_wait = cmd.Time;
					switch (cmd.Id) {
						case InterceptInput.Mouse.WM_MOUSEMOVE:
							SetCursorPos(cmd.X, cmd.Y);
							//await CmdMouseMove(oldx, oldy, i.Point.X, i.Point.Y);
							break;
						case InterceptInput.Mouse.WM_LBUTTONDOWN:
							mouse_event(kLEFT_DOWN, cmd.X, cmd.Y, 0, 0);
							break;
						case InterceptInput.Mouse.WM_LBUTTONUP:
							mouse_event(kLEFT_UP, cmd.X, cmd.Y, 0, 0);
							break;
					}
					//_OnSelectedCommandListItem(i1);
					SelectedIndexListCommand = i1;
				}
			}
			StopCommand();
		}

		public void StopCommand()
		{
			if (!IsPlaying)
				return;
			EnableCommandList.Value = true;
			EnableFileList.Value = true;
			EnableSettings.Value = true;
			EnableRecButton.Value = true;
			//sbBlink.Stop(button_play);
			IsPlaying = false;
			//button_play.Content = "開始";
			cn.log("再生終了");
			OnFinishCommand();
		}

		public void StartRecording()
		{
			EnableCommandList.Value = false;
			EnableFileList.Value = false;
			EnableSettings.Value = false;
			EnablePlayButton = false;
			IsRecording = true;
			ListCommand.Clear();
			cn.log("録画開始");
		}

		public void StopRecodring(int selIndex)
		{
			if (!IsRecording)
				return;
			if (selIndex < 0 || ListNames.Count <= selIndex)
				return;
			EnableCommandList.Value = true;
			EnableFileList.Value = true;
			EnableSettings.Value = true;
			EnablePlayButton = true;
			IsRecording = false;
			Config.Instance.CommandList[ListNames[selIndex]] = ListCommand.ToList();
			cn.log("録画終了");
		}

		public void RefreshCommandList(string eventName)
		{
			ListCommand.Clear();
			if (Config.Instance.CommandList.TryGetValue(eventName, out var commands)) {
				foreach (var i in commands)
					ListCommand.Add(i);
			}
		}

		public void AddCommand(CommandChunk newItem)
		{
			ListCommand.Add(newItem);
		}

		public string AddNewCommandName()
		{
			for (int i = 1; i < 100; ++i) {
				var name = "new_" + i;
				if (!ListNames.Contains(name)) {
					ListNames.Add(name);
					Config.Instance.CommandList[name] = new List<CommandChunk>();
					return name;
				}
			}
			return null;
		}

		public void RemoveEventName(string name)
		{
			ListNames.Remove(name);
			Config.Instance.CommandList.Remove(name);
		}

		public void RenameEventName(string oldName, string newName)
		{
			for (int i = 0; i < ListNames.Count; ++i) {
				if (ListNames[i] == oldName) {
					ListNames[i] = newName;
					break;
				}
			}
			var value = Config.Instance.CommandList[oldName];
			Config.Instance.CommandList.Remove(oldName);
			Config.Instance.CommandList[newName] = value;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		[DllImport("User32.dll")] private static extern bool SetCursorPos(int X, int Y);
		[DllImport("User32.dll")] private static extern bool GetCursorPos();
		[DllImport("user32.dll")] private static extern void mouse_event(uint dwFlags, int dx, int dy, uint cButtons, uint dwExtraInfo);
		const int kLEFT_DOWN = 0x02;
		const int kLEFT_UP = 0x04;
	}
}
