using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace DeeCee.UI.Controls;

public class DasmView : UserControl
{
    public delegate (string text, int size) DasmResultDelegate(ulong address);

    private Typeface _typeface = new Typeface("JetBrains Mono");
    private double _fontSize = 15;

    private double _lineHeight;
    private double _ascent;
    private double _descent;

    private ulong _lowerAddress = 0;
    private ulong _higherAddress = 0x100000000;
    private ulong _viewFirstAddr = 0;
    private ulong _currentAddr = 0;
    private ulong _selectedAddr = 0;

    private readonly List<ulong> _displayList = new();

    private ScrollBar _vScroll;
    private Grid _grid;

    private int _addressWidth;
    private int _dasmDisplace = 8;

    private SolidColorBrush _currentAddrColor = new(Color.FromRgb(120, 200, 255));
    private SolidColorBrush _selectedAddrColor = new(Color.FromArgb(200, 255, 200, 200));
    private SolidColorBrush _dasmBgColor = new(Color.FromRgb(255, 255, 220));
    private SolidColorBrush _addressBgColor = new(Color.FromRgb(220, 255, 220));

    private DasmResultDelegate? _dasmCallback;
    public static readonly DirectProperty<DasmView, DasmResultDelegate?> DasmCallbackProperty =
        AvaloniaProperty.RegisterDirect<DasmView, DasmResultDelegate?>(
            nameof(DasmCallback),
            o => o.DasmCallback,
            (o, v) => o.DasmCallback = v);
    public DasmResultDelegate? DasmCallback
    {
        get => _dasmCallback;
        set => SetAndRaise(DasmCallbackProperty, ref _dasmCallback, value);
    }
    
    public DasmView()
    {
        Focusable = true;
        
        // Criar o grid para organizar o layout
        _grid = new Grid();
        _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star)); // Conteúdo
        _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // ScrollBar

        // Criar a scrollbar
        _vScroll = new ScrollBar 
        { 
            Orientation = Orientation.Vertical,
            Width = 20,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        _vScroll.Minimum = 0;
        _vScroll.ValueChanged += OnScrollValueChanged;

        // Adicionar scrollbar ao grid
        Grid.SetColumn(_vScroll, 1);
        _grid.Children.Add(_vScroll);

        // Definir o grid como conteúdo do UserControl
        Content = _grid;

        CalculateFontMetrics();
    }

    private void OnScrollValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        _viewFirstAddr = (ulong)e.NewValue;
        InvalidateVisual();
    }

    private void CalculateFontMetrics()
    {
        var glyph = _typeface.GlyphTypeface;
        var metrics = glyph.Metrics;
        double scale = _fontSize / metrics.DesignEmHeight;

        _lineHeight = Math.Ceiling(metrics.LineSpacing * scale);
        _ascent = metrics.Ascent * scale;
        _descent = metrics.Descent * scale;

        var ft = new FormattedText("00000000", CultureInfo.InvariantCulture,
                                   FlowDirection.LeftToRight, _typeface,
                                   _fontSize, Brushes.Black);
        _addressWidth = (int)Math.Ceiling(ft.Width);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateScrollRange();
    }

    private void UpdateScrollRange()
    {
        if (Bounds.Height > 0 && _lineHeight > 0)
        {
            int visibleLines = (int)(Bounds.Height / _lineHeight);
            
            _vScroll.Minimum = _lowerAddress;
            _vScroll.Maximum = Math.Max(_lowerAddress, _higherAddress - (ulong)visibleLines);
            _vScroll.ViewportSize = visibleLines;
            _vScroll.SmallChange = 1;
            _vScroll.LargeChange = visibleLines;
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Ajustar a largura do conteúdo para considerar a scrollbar
        double contentWidth = Bounds.Width - (_vScroll.IsVisible ? _vScroll.Width : 0);
        
        var rect = new Rect(0, 0, contentWidth, Bounds.Height);
        context.FillRectangle(_dasmBgColor, rect);

        if (_lineHeight <= 0) return;

        int linesPerView = (int)(Bounds.Height / _lineHeight) + 2;
        ulong address = _viewFirstAddr;

        _displayList.Clear();

        int addressPadding = 3;
        int dasmPosX = _addressWidth + _dasmDisplace + addressPadding * 2;

        // fundo da coluna de endereços
        context.FillRectangle(_addressBgColor, new Rect(0, 0, _addressWidth + addressPadding * 2, Bounds.Height));

        for (int i = 0; i < linesPerView && address < _higherAddress; i++)
        {
            if (DasmCallback == null) break;

            var (text, size) = DasmCallback(address);
            if (size <= 0) size = 1; // Evitar loop infinito
            
            _displayList.Add(address);

            double lineTop = i * _lineHeight;

            if (address == _currentAddr)
                context.FillRectangle(_currentAddrColor, new Rect(0, lineTop, contentWidth, _lineHeight));

            if (address == _selectedAddr)
                context.FillRectangle(_selectedAddrColor, new Rect(0, lineTop, contentWidth, _lineHeight));

            // endereço
            var addrText = new FormattedText(address.ToString("X8"),
                                             CultureInfo.InvariantCulture,
                                             FlowDirection.LeftToRight,
                                             _typeface, _fontSize, Brushes.Black);
            context.DrawText(addrText, new Point(addressPadding, lineTop + _ascent));

            // instrução
            var instrText = new FormattedText(text ?? "",
                                              CultureInfo.InvariantCulture,
                                              FlowDirection.LeftToRight,
                                              _typeface, _fontSize, Brushes.Black);
            context.DrawText(instrText, new Point(dasmPosX, lineTop + _ascent));

            address += (ulong)size;
        }

        // linha divisória
        context.DrawLine(new Pen(Brushes.Black, 1),
                         new Point(_addressWidth + addressPadding * 2, 0),
                         new Point(_addressWidth + addressPadding * 2, Bounds.Height));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
        
        var pos = e.GetPosition(this);
        int line = (int)(pos.Y / _lineHeight);
        if (line >= 0 && line < _displayList.Count)
        {
            _selectedAddr = _displayList[line];
            InvalidateVisual();
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        
        // Suporte para rolagem com mouse wheel
        double delta = e.Delta.Y * 3; // Multiplicar para tornar mais sensível
        double newValue = _vScroll.Value - delta;
        newValue = Math.Max(_vScroll.Minimum, Math.Min(_vScroll.Maximum, newValue));
        _vScroll.Value = newValue;
        
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        switch (e.Key)
        {
            case Key.Space:
                NextLineRequested?.Invoke(this, EventArgs.Empty);
                break;
                
            case Key.Up:
                if (_vScroll.Value > _vScroll.Minimum)
                    _vScroll.Value -= 1;
                e.Handled = true;
                break;
                
            case Key.Down:
                if (_vScroll.Value < _vScroll.Maximum)
                    _vScroll.Value += 1;
                e.Handled = true;
                break;
                
            case Key.PageUp:
                _vScroll.Value = Math.Max(_vScroll.Minimum, _vScroll.Value - _vScroll.LargeChange);
                e.Handled = true;
                break;
                
            case Key.PageDown:
                _vScroll.Value = Math.Min(_vScroll.Maximum, _vScroll.Value + _vScroll.LargeChange);
                e.Handled = true;
                break;
        }
    }

    public event EventHandler? NextLineRequested;
    
    // Propriedade bindável para MVVM: sincroniza com GotoAddress
    public static readonly DirectProperty<DasmView, ulong> CurrentAddressProperty =
        AvaloniaProperty.RegisterDirect<DasmView, ulong>(
            nameof(CurrentAddress),
            o => o.CurrentAddress,
            (o, v) => o.CurrentAddress = v);

    public ulong CurrentAddress
    {
        get => _currentAddr;
        set
        {
            if (_currentAddr == value) return;
            // Usa a navegação para manter o comportamento (scroll + redraw)
            GotoAddress(value);
        }
    }


    public void GotoAddress(ulong addr)
    {
        // Atualiza o backing field e notifica o binding
        SetAndRaise(CurrentAddressProperty, ref _currentAddr, addr);

        if (!_displayList.Contains(addr))
        {
            _vScroll.Value = addr;
        }
        InvalidateVisual();
    }

    public ulong SelectedAddress => _selectedAddr;
    public ulong CursorAddress => _selectedAddr;

    public void SetAddressLimits(ulong low, ulong hi)
    {
        _lowerAddress = low;
        _higherAddress = hi;
        UpdateScrollRange();
        InvalidateVisual();
    }
}