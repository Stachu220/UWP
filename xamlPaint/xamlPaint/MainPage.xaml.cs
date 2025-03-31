using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Timers;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography.Core;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x415

namespace xamlPaint
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Point _startingPoint = new Point();
        private Point _endingPoint = new Point();
        private Point _lastPoint = new Point();
        private SolidColorBrush _brush = new SolidColorBrush(Windows.UI.Colors.LightPink);
        private bool _isDrawing = false;
        private Line _line;
        private Line _lastLine;
        private bool _isProsta = true;
        private int _lineSize;
        Stack<UIElement> _undoStack = new Stack<UIElement>();

        public MainPage()
        {
            this.InitializeComponent();
            _lineSize = (int)brushSizeSlider.Value;

        }

        private void rdbProsta_checked(object sender, RoutedEventArgs e)
        {
            _isProsta = true;
        }

        private void rdbDowolna_checked(object sender, RoutedEventArgs e)
        {
            _isProsta = false;
        }

        private void StartDrawing(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDrawing)
            {
                _lastLine = null;
                _isDrawing = true;
                _startingPoint = e.GetCurrentPoint(poleRysowania).Position;
                _lastPoint = _startingPoint;
            }
        }

        private void StopDrawing(object sender, PointerRoutedEventArgs e)
        {
            _isDrawing = false;
            if (_lastLine != null)
                _undoStack.Push(_lastLine);
            _lastLine = null;
        }

        private void PtrMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isDrawing)
            {
                _endingPoint = e.GetCurrentPoint(poleRysowania).Position;
                _line = new Line();
                _line.Stroke = _brush;
                _line.StrokeStartLineCap = PenLineCap.Round;
                _line.StrokeEndLineCap = PenLineCap.Round;
                if (_isProsta)
                {

                    if (_lastLine != null && _isDrawing)
                    {
                        poleRysowania.Children.Remove(_lastLine);
                    }

                    _line.X1 = _startingPoint.X;
                    _line.Y1 = _startingPoint.Y;
                }
                else
                {
                    _line.X1 = _lastPoint.X;
                    _line.Y1 = _lastPoint.Y;
                }
                _line.X2 = _endingPoint.X;
                _line.Y2 = _endingPoint.Y;
                _line.StrokeThickness = _lineSize;
                poleRysowania.Children.Add(_line);
                if (!_isProsta)
                    _undoStack.Push(_lastLine);
                _lastPoint = _endingPoint;
                _lastLine = _line;
            }
        }

        private void GetColor(object sender, RoutedEventArgs e)
        {

            try
            {
                var color = (Windows.UI.Xaml.Shapes.Rectangle)e.OriginalSource;
                _brush = (SolidColorBrush)color.Fill;

            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                _brush = new SolidColorBrush(Windows.UI.Colors.Red);
            }
            curColor.Fill = _brush;
        }

        private void onBrushSizeChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                _lineSize = (int)brushSizeSlider.Value;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                _lineSize = 2;
            }
        }

        private void onUndoButton(object sender, RoutedEventArgs e)
        {
            if (_undoStack.Count > 0)
            {
                poleRysowania.Children.Remove(_undoStack.Pop());
            }
        }

        private void onClearButton(object sender, RoutedEventArgs e)
        {
            foreach (var element in _undoStack)
            {
                poleRysowania.Children.Remove(element);
            }
        }

        private void poleRysowania_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            poleRysowania.Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, poleRysowania.ActualWidth, poleRysowania.ActualHeight)
            };
        }
        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            _brush = new SolidColorBrush(sender.Color);
            curColor.Fill = _brush;
        }
        private void onExitConfirm()
        {
            CoreApplication.Exit();
        }

        private async void onExitButton(object send, RoutedEventArgs e)
        {
            string contentText = "Do you really want to exit?";
            MediaElement mediaElement = new MediaElement();
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            Windows.Media.SpeechSynthesis.SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(contentText);
            mediaElement.SetSource(stream, stream.ContentType);
            //ContentDialog
            ContentDialog contentDialog = new ContentDialog()
            {
                Title = contentText,
                PrimaryButtonText = "Exit",
                CloseButtonText = "Cancel"
            };
            mediaElement.Play();

            contentDialog.PrimaryButtonClick += (sender, args) =>
            {
                onExitConfirm();
            };

            await contentDialog.ShowAsync();
            
        }


    }
}
