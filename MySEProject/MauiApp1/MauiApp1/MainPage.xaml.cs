//using Android.Graphics;
using MauiApp1.ViewModel;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using DocumentFormat.OpenXml.InkML;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.VisualBasic;

namespace MauiApp1;

public partial class MainPage : ContentPage
{

    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        //// Read the contents of the input text file
        //string filePath = "A:\\SoftwareProject\\Team_VMAUI\\MySEProject\\MauiApp1\\MauiApp1\\sampleOne.txt";
        //string fileContents = File.ReadAllText(filePath);

        //// Display the file contents in a Label control
        //Label fileLabel = new Label
        //{
        //    Text = fileContents
        //};

        //Content = fileLabel;
    }

    private async void OncouterClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "pick a text file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.iOS, new[] { "public.plain-text" } },
            { DevicePlatform.Android, new[] { "text/plain" } },
            { DevicePlatform.UWP, new[] { ".txt" } },
            { DevicePlatform.macOS, new[] { "public.plain-text" } },
        })
        });

        if (result == null)
            return;

        var stream = await result.OpenReadAsync();
        var reader = new StreamReader(stream);
        var fileContents = await reader.ReadToEndAsync();

        myLabel.Text = fileContents;

    }

    private void OnSubmitClicked(object sender, EventArgs e)
    {
        string graphName = graphNameEntry.Text;
        string maxCycles = maxCyclesEntry.Text;
        string highlightTouch = highlightTouchEntry.Text;
        string axis = axisEntry.Text;
        string xAxisTitle = xAxisTitleEntry.Text;
        string yAxisTitle = yAxisTitleEntry.Text;
        string minRange = minRangeEntry.Text;
        string maxRange = maxRangeEntry.Text;
        string subplotTitle = subplotTitleEntry.Text;
        string figureName = figureNameEntry.Text;
        string result = "Graph Name: " + graphName + "\n" +
                "Max Cycles: " + maxCycles + "\n" +
                "Highlight Touch: " + highlightTouch + "\n" +
                "Axis: " + axis + "\n" +
                "X Axis Title: " + xAxisTitle + "\n" +
                "Y Axis Title: " + yAxisTitle + "\n" +
                "Min Range: " + minRange + "\n" +
                "Max Range: " + maxRange + "\n" +
                "Subplot Title: " + subplotTitle + "\n" +
                "Figure Name: " + figureName;

        resultLabel.Text = result;

    }

}


