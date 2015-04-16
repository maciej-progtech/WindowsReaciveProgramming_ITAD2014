using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ReactiveSamples
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            var pointerMove = 
                Observable.FromEventPattern<PointerEventHandler, PointerRoutedEventArgs>
                (h => this.canvas.PointerMoved += h, h => this.canvas.PointerMoved -= h)
            //.Sample( TimeSpan.FromSeconds( 1 ) );
            .Throttle( TimeSpan.FromMilliseconds( 100 ) );

            pointerMove
              .ObserveOn(CoreDispatcherScheduler.Current)
              .Subscribe
              (m => label_mousemove.Text = String.Format("x={0},y={1}", 
                  m.EventArgs.GetCurrentPoint(canvas).Position.X, 
                  m.EventArgs.GetCurrentPoint(canvas).Position.Y));

            var polewo = from ev in 
                             pointerMove.ObserveOn(CoreDispatcherScheduler.Current)
                         where ev.EventArgs.GetCurrentPoint(canvas).Position.X < this.canvas.RenderSize.Width / 2
                         select ev;
            polewo
                .ObserveOn(CoreDispatcherScheduler.Current)
                .Subscribe(m => label_LR.Text = "left");

            var poprawo = from ev in pointerMove
                              .ObserveOn(CoreDispatcherScheduler.Current)
                          where ev.EventArgs.GetCurrentPoint(canvas).Position.X >= this.canvas.RenderSize.Width / 2
                          select ev;
            poprawo
                .ObserveOn(CoreDispatcherScheduler.Current)
                .Subscribe(m => label_LR.Text = "right");




            var mouseDown = from ev in 
                                Observable.FromEventPattern<PointerEventHandler, PointerRoutedEventArgs>
                                (h => this.canvas.PointerPressed += h, h => this.canvas.PointerPressed -= h)
                            select ev.EventArgs.GetCurrentPoint(canvas).Position;
            var mouseUp = from ev in Observable.FromEventPattern
                              <PointerEventHandler, PointerRoutedEventArgs>
                              (h => this.canvas.PointerReleased += h, h => 
                                  this.canvas.PointerReleased -= h)
                          select ev.EventArgs.GetCurrentPoint(canvas).Position;

            var drawline = mouseDown.Zip(mouseUp,
              (down, up) =>
              {
                  return new Point[] { down, up };
              });
            drawline
                .ObserveOn(CoreDispatcherScheduler.Current)
                .Subscribe(line =>
                {
                    var myLine = new Line();
                    myLine.Stroke = new SolidColorBrush(Colors.Black);
                    myLine.X1 = line[0].X;
                    myLine.X2 = line[1].X;
                    myLine.Y1 = line[0].Y;
                    myLine.Y2 = line[1].Y;
                    canvas.Children.Add(myLine);
                });
        }

    }
}
