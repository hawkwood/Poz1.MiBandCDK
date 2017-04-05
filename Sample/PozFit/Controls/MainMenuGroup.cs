using System.Collections.ObjectModel;

namespace PozFit.Controls
{
    public class MainMenuGroup : ObservableCollection<MainMenuItem>
    {
        public string Title { get; private set; }

        public MainMenuGroup(string title)
        {
            this.Title = title;
        }
    }
}
