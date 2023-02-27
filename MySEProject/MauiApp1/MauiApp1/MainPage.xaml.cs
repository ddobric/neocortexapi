using MauiApp1.ViewModel;
using Microsoft.Maui.Storage;

namespace MauiApp1;

public partial class MainPage : ContentPage
{

	public MainPage(MainViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

}
/*public async Task<FileResult> PickAndShow(PickOptions options)
{
    try
    {
        var result = await FilePicker.Default.PickAsync(options);
        if (result != null)
        {
            if (result.FileName.EndsWith("txt", StringComparison.OrdinalIgnoreCase))
            {
                using var stream = await result.OpenReadAsync();
                var image = ImageSource.FromStream(() => stream);
            }
        }

        return result;
    }
    catch (Exception ex)
    {
        // The user canceled or something went wrong
    }

    return null;
}*/
