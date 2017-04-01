using Xamarin.Forms;

namespace PozFit.Controls
{
    public class BindableContentPage : ContentPage
    {
        public static readonly BindableProperty OnAppearingCommandProperty = BindableProperty.Create(nameof(OnAppearingCommand), typeof(Command), typeof(BindableContentPage), null);
        public static readonly BindableProperty OnDisappearingCommandProperty = BindableProperty.Create(nameof(OnDisappearingCommand), typeof(Command), typeof(BindableContentPage), null);

        public static readonly BindableProperty OnBackPressedCommandProperty = BindableProperty.Create(nameof(OnBackPressedCommand), typeof(Command), typeof(BindableContentPage), null);

        public Command OnAppearingCommand
        {
            get { return (Command)GetValue(OnAppearingCommandProperty); }
            set { SetValue(OnAppearingCommandProperty, value); }
        }

        public Command OnDisappearingCommand
        {
            get { return (Command)GetValue(OnDisappearingCommandProperty); }
            set { SetValue(OnDisappearingCommandProperty, value); }
        }

        public Command OnBackPressedCommand
        {
            get { return (Command)GetValue(OnBackPressedCommandProperty); }
            set { SetValue(OnBackPressedCommandProperty, value); }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            OnAppearingCommand?.Execute(null);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            OnDisappearingCommand?.Execute(null);
        }

        protected override bool OnBackButtonPressed()
        {
            if (OnBackPressedCommand != null)
            {
                OnBackPressedCommand.Execute(null);
                return true;
            }
            else
                return base.OnBackButtonPressed();
        }
    }
}
