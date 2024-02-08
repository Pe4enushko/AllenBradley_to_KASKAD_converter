using System.Security.Cryptography.X509Certificates;
using System.Text;
using ABKaskadGUI.ViewModels;
using CommunityToolkit.Maui.Storage;

namespace ABKaskadGUI;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		BindingContext = new MainViewModel();
	}
}

