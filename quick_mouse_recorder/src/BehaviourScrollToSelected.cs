using System.Windows;
using System.Windows.Controls;

namespace quick_mouse_recorder
{
	public static class BehaviorScrollToSelected
	{
		public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.RegisterAttached(
			"DoScroll",
			typeof(object),
			typeof(BehaviorScrollToSelected),
			new PropertyMetadata(null, OnChange));

		public static void SetDoScroll(DependencyObject source, object value) => source.SetValue(SelectedValueProperty, value);
		public static object GetDoScroll(DependencyObject source) => (object)source.GetValue(SelectedValueProperty);
		private static void OnChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ListView)d).ScrollIntoView(e.NewValue);
		}
	}
}
