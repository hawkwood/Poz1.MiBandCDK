using Xamarin.Forms;

namespace PozFit.Controls
{
    public class BindableListView : ListView
    {
        public static readonly BindableProperty ItemTappedCommandProperty = BindableProperty.Create("ItemTappedCommand", typeof(Command), typeof(BindableListView), null);

        public Command ItemTappedCommand
        {
            get { return (Command)GetValue(ItemTappedCommandProperty); }
            set { SetValue(ItemTappedCommandProperty, value); }
        }

        public BindableListView()
        {
            ItemTapped += BindableListView_ItemTapped;
        }

        private void BindableListView_ItemTapped(System.Object sender, ItemTappedEventArgs e)
        {
            ItemTappedCommand?.Execute(e.Item);
        }
    }
}
