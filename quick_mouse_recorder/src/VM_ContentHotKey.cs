using Reactive.Bindings;
using System;
using System.ComponentModel;
using System.Windows.Media;

namespace quick_mouse_recorder
{
	class VM_ContentHotKey : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public ReactiveProperty<bool> IsChecked { get; } = new ReactiveProperty<bool>();
		public ReactiveProperty<string> DisplayState { get; } = new ReactiveProperty<string>();
		public ReactiveProperty<Brush> ContentForegroundBrush { get; } = new ReactiveProperty<Brush>(Brushes.Gray);

		public bool EnableHotKey => !_forceDisable && IsChecked.Value;
		bool _forceDisable;

		public void Init()
		{
			IsChecked.Value = Config.Instance.EnableHotKey;
			IsChecked.Subscribe(e => {
				Refresh();
				Config.Instance.EnableHotKey = e;
			});
		}

		public void OnMouseEnter(object sender)
		{
			_forceDisable = true;
			Refresh();
		}

		public void OnMouseLeave(object sender)
		{
			_forceDisable = false;
			Refresh();
		}

		public void OnChecked(object sender)
		{
			Refresh();
		}

		public void Refresh()
		{
			DisplayState.Value = IsChecked.Value ? "有効" : "無効";
			if (_forceDisable && IsChecked.Value) {
				ContentForegroundBrush.Value = Brushes.Red;
				DisplayState.Value += "(マウスが画面内の時は無効)";
			}
			else {
				ContentForegroundBrush.Value = Brushes.Black;
			}
		}
	}
}
