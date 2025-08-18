using Avalonia.Controls;

namespace DeeCee.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DasmView.DasmCallback = address => ("xulica", 2);
        DasmView.GotoAddress(0xA000_0000);
    }
    
    
}