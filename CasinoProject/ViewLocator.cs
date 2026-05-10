using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CasinoProject.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace CasinoProject;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Nie znaleziono widoku: " + name };
    }
    
    public bool Match(object? data)
    {
        return data is ObservableObject;
    }
}