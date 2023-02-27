using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiApp1.ViewModel;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        Items = new ObservableCollection<string>();
    }

    [ObservableProperty]
    ObservableCollection <string> items;
         
    [ObservableProperty]
    string text;

    [ICommand]
    void Add()
    {
        if (string.IsNullOrEmpty(Text))
            return; 

        Items.Add(Text);
        Text = string.Empty;
    }
}


