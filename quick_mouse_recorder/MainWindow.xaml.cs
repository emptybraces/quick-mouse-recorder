using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace quick_mouse_recorder
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		Stopwatch _timer = new Stopwatch();
		Storyboard _sbBlink;
		MainViewModel VM => (MainViewModel)DataContext;
		InterceptInput _interceptInput;
		long _currentRecTime;

		CancellationTokenSource _cts;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			MaxHeight = this.Height;
			MaxWidth = this.Width;
			_sbBlink = (Storyboard)FindResource("Blink");
			_interceptInput = new InterceptInput();
			_interceptInput.AddEvent(HookMouse);
			_interceptInput.AddEvent(HookKey);
			VM.OnFinishCommand += () => {
				//_sbBlink.Stop(button_play);
				//button_play.Content = "開始";
			};
			VM.Init();
			xnSingleUpDownCaptureIntvl.Value = Config.Instance.CaptureInterval;

			void HookMouse(uint mouseId, InterceptInput.Mouse.HookData data)
			{
				// 移動のみ一定時間のインターバルを持たせる
				if (mouseId == InterceptInput.Mouse.WM_MOUSEMOVE && _timer.ElapsedMilliseconds < xnSingleUpDownCaptureIntvl.Value * 1000)
					return;
				// アプリケーション内のイベントは無視する
				if (Left < data.pt.x && data.pt.x <= Left + Width && Top < data.pt.y && data.pt.y < Top + Height)
					return;
				_currentRecTime += _timer.ElapsedMilliseconds;
				_timer.Restart();
				VM.AddCommand(new CommandChunk {
					Time = (int)_currentRecTime,
					Id = mouseId,
					X = data.pt.x,
					Y = data.pt.y,
				});
				var last_index = listViewCommand.Items.Count - 1;
				listViewCommand.ScrollIntoView(listViewCommand.Items[last_index]);
			}

			void HookKey(uint keyId, InterceptInput.Key.HookData data)
			{
				if (!VM.EnableHotKey)
					return;
				if (keyId != InterceptInput.Key.WM_KEYDOWN)
					return;
				var wpfkey = KeyInterop.KeyFromVirtualKey((int)data.vkCode);
				// escape key
				if (wpfkey == Key.Escape) {
					StopRecording();
					StopCommand();
				}
				// space key
				else if (wpfkey == Key.Space) {
					StartCommand();
				}
				// enter key
				else if (wpfkey == Key.Return) {
					SwitchRecording();
				}
				else if (wpfkey == Key.A) {
				}
			}
		}

		private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_interceptInput.Stop();
			// 設定保存
			if (VM.NeedSave) {
				var result = MessageBox.Show("Do you want to save the updated data?", "Confirm", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				if (result == MessageBoxResult.Yes) {
					VM.SaveConfig();
				}
				else if (result == MessageBoxResult.Cancel) {
					e.Cancel = true;
				}
			}
		}
		private void Window_MouseEnter(object sender, MouseEventArgs e)
		{
			//var context = (VM_ContentHotKey)xNameCheckBoxHotKey.DataContext;
			//context.OnMouseEnter(xNameCheckBoxHotKey);
		}

		private void Window_MouseLeave(object sender, MouseEventArgs e)
		{
			//var context = (VM_ContentHotKey)xNameCheckBoxHotKey.DataContext;
			//context.OnMouseLeave(xNameCheckBoxHotKey);
		}

		private void ListboxPlayListAdd_Context(object sender, RoutedEventArgs e)
		{
			var new_name = VM.AddNewPlayList();
			if (new_name != null)
				xnListBoxPlayList.SelectedItem = new_name;
		}
		private void ListboxPlayListRemove_Context(object sender, RoutedEventArgs e)
		{
			VM.RemovePlayList();
		}
		private void ListboxPlayListDuplicate_Context(object sender, RoutedEventArgs e)
		{
			VM.DuplicatePlayList(xnListBoxPlayList.SelectedIndex);
		}
		private void ListboxPlayListRename_Context(object sender, RoutedEventArgs e)
		{
			VM.RenamePlayList();
		}

		private void ListViewCommandItemDelete_Context(object sener, RoutedEventArgs e)
		{
			cn.log(listViewCommand.SelectedIndex);
		}

		float Lerp(float f1, float f2, float by)
		{
			return f1 + (f2 - f1) * by;
		}

		void SwitchRecording()
		{
			if (VM.IsPlaying)
				return;
			if (xnListBoxPlayList.SelectedIndex < 0)
				return;
			if (VM.IsRecording)
				StopRecording();
			else
				StartRecording();
		}

		void StartRecording()
		{
			_timer.Restart();
			//_sbBlink.Begin(button_rec, true);
			//button_rec.Content = "録画中...";
			InterceptInput.EnableMouseInput = true;
			VM.StartRecording();
		}

		void StopRecording()
		{
			_currentRecTime = 0;
			//_sbBlink.Stop(button_rec);
			//button_rec.Content = "録画";
			InterceptInput.EnableMouseInput = false;
			VM.StopRecodring(xnListBoxPlayList.SelectedIndex);
		}

		void StartCommand()
		{
			if (VM.IsRecording) {
				Debug.WriteLine("録画中に再生することはできません。");
				return;
			}
			if (VM.IsPlaying) {
				Debug.WriteLine("既に再生中です。");
				return;
			}
			//_sbBlink.Begin(button_play, true);
			//button_play.Content = "開始中...";
			_cts = new CancellationTokenSource();
			_ = VM.StartCommand(_cts.Token);
		}

		void StopCommand()
		{
			_cts?.Cancel();
			//_sbBlink.Stop(button_play);
			//button_play.Content = "開始";
			VM.StopCommand();
		}

		[DllImport("user32.dll")]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		[DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		private const int GWL_STYLE = -16;
		private const int WS_MAXIMIZEBOX = 0x10000;

		private void Window_SourceInitialized(object sender, EventArgs e)
		{
			var hwnd = new System.Windows.Interop.WindowInteropHelper((Window)sender).Handle;
			var value = GetWindowLong(hwnd, GWL_STYLE);
			SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MAXIMIZEBOX));
		}
	}
}
