using PlayerDePobre.ViewModel;
using System.Runtime.CompilerServices;

namespace PlayerDePobre;

public partial class MainPage : ContentPage
{
	public MainPage(MusicasVM viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}

