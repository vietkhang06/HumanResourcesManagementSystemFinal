using System.Windows;
using System.Windows.Media.Animation;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Sidebar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var anim = (Storyboard)this.Resources["OpenMenuAnimation"];
            anim.Begin();
        }

        private void Sidebar_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (BtnUserProfile.ContextMenu.IsOpen) return;

            var anim = (Storyboard)this.Resources["CloseMenuAnimation"];
            anim.Begin();
        }

        private void UserProfile_Click(object sender, RoutedEventArgs e)
        {
            if (BtnUserProfile.ContextMenu != null)
            {
                BtnUserProfile.ContextMenu.PlacementTarget = BtnUserProfile;
                BtnUserProfile.ContextMenu.IsOpen = true;
            }
        }

        private void UserContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            if (!SidebarBorder.IsMouseOver)
            {
                var anim = (Storyboard)this.Resources["CloseMenuAnimation"];
                anim.Begin();
            }
        }
    }
}