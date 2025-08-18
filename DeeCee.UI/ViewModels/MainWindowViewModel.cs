using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeeCee.Core;
using DeeCee.UI.Controls;

namespace DeeCee.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public Dreamcast Emulator { get; }
    
    [ObservableProperty]
    private ulong _currentAddress;
    
    public MainWindowViewModel()
    {
        Emulator = new Dreamcast();
    }

    [RelayCommand]
    public unsafe void Step()
    {
        Emulator.Step();
        CurrentAddress = Emulator.Sh4State->PC;
        this.OnPropertyChanged();
    }

    private (string text, int size) Dasm(ulong address)
    {
        var op = Emulator.Mem.Read16(unchecked((uint)address));
        var dasm = Emulator.Sh4Dasm.Disassemble(op);
        return (dasm.FullInstruction, 2);
    }
    
    public DasmView.DasmResultDelegate DasmCallback => Dasm;
}