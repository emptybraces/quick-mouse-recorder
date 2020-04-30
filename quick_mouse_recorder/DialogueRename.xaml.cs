using System.Windows;

namespace quick_mouse_recorder
{
	/// <summary>
	/// Dialogue.xaml の相互作用ロジック
	/// </summary>
	public partial class DialogueRename : Window
	{
		public DialogueRename()
		{
			InitializeComponent();
		}
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}
	}

}
