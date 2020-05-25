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

		public bool EnableHotKey => !_isMouseEnter && IsChecked.Value;
		bool _isMouseEnter;

		public VM_ContentHotKey()
		{
		}

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
			_isMouseEnter = true;
			Refresh();
		}

		public void OnMouseLeave(object sender)
		{
			_isMouseEnter = false;
			Refresh();
		}

		public void OnChecked(object sender)
		{
			Refresh();
		}

		public void Refresh()
		{
			DisplayState.Value = IsChecked.Value ? "有効" : "無効";
			if (EnableHotKey) {
				ContentForegroundBrush.Value = Brushes.Red;
				DisplayState.Value += "(マウスが画面内の時は無効)";
			}
			else {
				ContentForegroundBrush.Value = Brushes.Black;
			}
		}

	}
}
