using System;
using System.Diagnostics;
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

		public MainWindow()
		{
			InitializeComponent();
			_sbBlink = (Storyboard)FindResource("Blink");
			_interceptInput = new InterceptInput();
			_interceptInput.AddEvent(hookMouse);
			InterceptInput.IsPausedMouse = true;
			_interceptInput.AddEvent(hookKey);
			//KeyboardHook.AddEvent(hookKeyboardTest);
			//KeyboardHook.Start();
			VM.OnFinishCommand += () => {
				_sbBlink.Stop(button_play);
				button_play.Content = "開始";
			};
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			slider_captureIval.Value = VM.Config.IntervalCapture;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			_interceptInput.Stop();
			// 設定保存
			VM.SaveConfig();
		}

		private void button_play_Click(object sender, RoutedEventArgs e)
		{
			SwitchCommand();
		}
		private void button_rec_Click(object sender, RoutedEventArgs e)
		{
			SwitchRecording();
		}

		private void listboxCommandNameAdd_Context(object sender, RoutedEventArgs e)
		{
			var new_name = VM.AddNewCommandName();
			if (new_name != null)
				listBoxCommandName.SelectedItem = new_name;
		}
		private void listboxCommandNameDelete_Context(object sender, RoutedEventArgs e)
		{
			if (listBoxCommandName.SelectedIndex == -1) return;
			VM.RemoveEventName(listBoxCommandName.SelectedItem.ToString());
		}
		private void listboxCommandNameRename_Context(object sender, RoutedEventArgs e)
		{
			if (listBoxCommandName.SelectedIndex == -1) return;
			var dialogue = new DialogueRename();
			var old_name = listBoxCommandName.SelectedItem.ToString();
			dialogue.textBox.Text = old_name;
			var res = dialogue.ShowDialog();
			if (res != true)
				return;
			var new_name = dialogue.textBox.Text;
			VM.RenameEventName(old_name, new_name);
		}

		private void listViewCommandItemDelete_Context(object sener, RoutedEventArgs e)
		{
			cn.log(listViewCommand.SelectedIndex);
		}

		void hookMouse(uint mouseId, InterceptInput.Mouse.HookData data)
		{
			// 移動のみ一定時間のインターバルを持たせる
			if (mouseId == InterceptInput.Mouse.WM_MOUSEMOVE && _timer.ElapsedMilliseconds < slider_captureIval.Value * 1000)
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

		void hookKey(uint keyId, InterceptInput.Key.HookData data)
		{
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
				SwitchCommand();
			}
			// enter key
			else if (wpfkey == Key.Return) {
				SwitchRecording();
			}
		}

		float Lerp(float f1, float f2, float by)
		{
			return f1 + (f2 - f1) * by;
		}

		void SwitchRecording()
		{
			if (VM.IsPlaying)
				return;
			if (listBoxCommandName.SelectedIndex < 0)
				return;
			if (VM.IsRecording)
				StopRecording();
			else
				StartRecording();
		}

		void StartRecording()
		{
			_timer.Restart();
			_sbBlink.Begin(button_rec, true);
			button_rec.Content = "録画中...";
			InterceptInput.IsPausedMouse = false;
			VM.StartRecording();
		}

		void StopRecording()
		{
			_currentRecTime = 0;
			_sbBlink.Stop(button_rec);
			button_rec.Content = "録画";
			InterceptInput.IsPausedMouse = true;
			VM.StopRecodring(listBoxCommandName.SelectedIndex);
		}

		void SwitchCommand()
		{
			if (VM.IsRecording)
				return;
			if (VM.IsPlaying)
				StopCommand();
			else
				StartCommand();
		}

		void StartCommand()
		{
			if (VM.IsRecording)
				return;
			_sbBlink.Begin(button_play, true);
			button_play.Content = "開始中...";
			_ = VM.StartCommand();
		}

		void StopCommand()
		{
			_sbBlink.Stop(button_play);
			button_play.Content = "開始";
			VM.StopCommand();
		}
	}
}
