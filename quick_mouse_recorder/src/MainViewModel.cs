using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace quick_mouse_recorder
{
	class MainViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<CommandChunk> ListCurrentSelectedCommands { get; } = new ObservableCollection<CommandChunk>();
		public ObservableCollection<string> ListPlayListNames { get; } = new ObservableCollection<string>();
		public bool IsRecording { get; private set; }
		public bool IsPlaying { get; private set; }
		int _selectedIndexListName;
		public int SelectedIndexPlayName {
			get {
				return _selectedIndexListName;
			}
			set {
				_selectedIndexListName = value;
				if (value < ListPlayListNames.Count && 0 <= value)
					RefreshCommandList(value);
				NotifyPropertyChanged();
				NotifyPropertyChanged("EnablePlayButton");
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
		public ReactiveProperty<int> TryCount { get; } = new ReactiveProperty<int>(1);

		public ReactiveProperty<VM_ContentHotKey> VM_ContentHotkey { get; } = new ReactiveProperty<VM_ContentHotKey>(new VM_ContentHotKey());
		public bool EnableHotKey => ListCurrentSelectedCommands.Any() && VM_ContentHotkey.Value.EnableHotKey;

		public ReactiveProperty<bool> EnableFileList { get; } = new ReactiveProperty<bool>(true);
		public ReactiveProperty<bool> EnableCommandList { get; } = new ReactiveProperty<bool>(true);
		public ReactiveProperty<bool> EnableSettings { get; } = new ReactiveProperty<bool>(true);
		public ReactiveProperty<bool> EnableRecButton { get; } = new ReactiveProperty<bool>(true);
		public bool EnablePlayButton {
			get {
				return _enablePlayButton && ListCurrentSelectedCommands.Any();
			}
			set {
				_enablePlayButton = value;
				NotifyPropertyChanged();
			}
		}
		bool _enablePlayButton = true;
		int _forceDisableCount;

		public MainViewModel()
		{
			// フォーカスイベントの登録
			//System.Windows.EventManager.RegisterClassHandler(typeof(InputBase), Keyboard.PreviewGotKeyboardFocusEvent, (KeyboardFocusChangedEventHandler)OnPreviewGotKeyboardFocus);
			//System.Windows.EventManager.RegisterClassHandler(typeof(InputBase), Keyboard.PreviewLostKeyboardFocusEvent, (KeyboardFocusChangedEventHandler)OnPreviewLostKeyboardFocus);
			System.Windows.EventManager.RegisterClassHandler(typeof(Xceed.Wpf.Toolkit.IntegerUpDown), Keyboard.PreviewGotKeyboardFocusEvent, (KeyboardFocusChangedEventHandler)OnPreviewGotKeyboardFocus);
			System.Windows.EventManager.RegisterClassHandler(typeof(Xceed.Wpf.Toolkit.IntegerUpDown), Keyboard.PreviewLostKeyboardFocusEvent, (KeyboardFocusChangedEventHandler)OnPreviewLostKeyboardFocus);
		}

		public void Init()
		{
			if (Config.Instance.PlayList.Any()) {
				var play_list = Config.Instance.PlayList;
				for (int i = 0; i < play_list.Count; ++i) {
					ListPlayListNames.Add(play_list[i].Name);
					foreach (var jj in play_list[i].Commands)
						ListCurrentSelectedCommands.Add(jj);
				}
			}
			VM_ContentHotkey.Value.Init();
			NotifyPropertyChanged(nameof(EnablePlayButton));
		}

		public void SaveConfig()
		{
			Config.Instance.IntervalCapture = CaptureInterval.Value;
			Config.Instance.EnableHotKey = VM_ContentHotkey.Value.IsChecked.Value;
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
			int try_total = TryCount.Value;
			//int.TryParse(TryCount.Value, out int try_total);
			try_total = try_total <= 0 ? 1 : try_total;
			for (int try_count = 0; try_count < try_total; ++try_count) {
				cn.log($"{try_total+1}回目");
				var prev_wait = 0;
				for (int i1 = 0; i1 < ListCurrentSelectedCommands.Count; i1++) {
					var cmd = ListCurrentSelectedCommands[i1];
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
			ListCurrentSelectedCommands.Clear();
			cn.log("録画開始");
		}

		public void StopRecodring(int selIndex)
		{
			if (!IsRecording)
				return;
			if (selIndex < 0 || ListPlayListNames.Count <= selIndex)
				return;
			EnableCommandList.Value = true;
			EnableFileList.Value = true;
			EnableSettings.Value = true;
			EnablePlayButton = true;
			IsRecording = false;
			Config.Instance.PlayList[selIndex].Commands = ListCurrentSelectedCommands.ToArray();
			cn.log("録画終了");
		}

		public void RefreshCommandList(int idx)
		{
			ListCurrentSelectedCommands.Clear();
			foreach (var i in Config.Instance.PlayList[idx].Commands)
				ListCurrentSelectedCommands.Add(i);
		}

		public void AddCommand(CommandChunk newItem)
		{
			ListCurrentSelectedCommands.Add(newItem);
		}

		public string AddNewPlayList()
		{
			for (int i = 1; i < 100; ++i) {
				var name = "new_" + i;
				if (!ListPlayListNames.Contains(name)) {
					ListPlayListNames.Add(name);
					Config.Instance.PlayList.Add(new PlayData { Name = name });
					return name;
				}
			}
			return null;
		}

		public string DuplicatePlayList(int srcIndex)
		{
			if (0 <= srcIndex) {
				var copy_name = ListPlayListNames[srcIndex] + "_copy";
				Config.Instance.PlayList.Insert(srcIndex, new PlayData { Name = copy_name, Commands = ListCurrentSelectedCommands.ToArray() });
				ListPlayListNames.Insert(srcIndex, copy_name);
				SelectedIndexPlayName = srcIndex;
				return copy_name;
			}
			return null;
		}

		public void RemovePlayList(int idx)
		{
			ListPlayListNames.RemoveAt(idx);
			ListCurrentSelectedCommands.Clear();
			Config.Instance.PlayList.RemoveAt(idx);
		}

		public void RenameEventName(int idx, string newName)
		{
			ListPlayListNames[idx] = newName;
			Config.Instance.PlayList[idx].Name = newName;
		}


		void OnPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			++_forceDisableCount;
			InterceptInput.IsPausedKey = true;
		}

		void OnPreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			--_forceDisableCount;
			if (_forceDisableCount == 0) {
				InterceptInput.IsPausedKey = false;
			}
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
