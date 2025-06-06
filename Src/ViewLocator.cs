using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Tsundoku
{
    public class ViewLocator : IDataTemplate
    {
        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        Control? ITemplate<object?, Control?>.Build(object? param)
        {
            string name = param.GetType().FullName!.Replace("ViewModel", "View");
            Type type = name.GetType();

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            else
            {
                return new TextBlock { Text = "Not Found: " + name };
            }
        }

        bool IDataTemplate.Match(object? data)
        {
            return data is ViewModels.ViewModelBase;
        }
    }
}
