using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ReactiveUISample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IViewFor<MyViewModel>
    {
        MyViewModel viewModel;
        public MainPage()
        {
            this.InitializeComponent();
            viewModel = new MyViewModel();
            this.DataContext = ViewModel;

            this.Bind(ViewModel, x => x.Name, x => x.name.Text);
            this.WhenAny(x => x.ViewModel.FirstName, x => x.Value ?? "")
               .BindTo(this, x => x.firstName.Text); 
            this.WhenAny(x => x.ViewModel.LastName, x => x.Value ?? "")
                .BindTo(this, x => x.lastName.Text);
            //this.WhenAny(x => x.ViewModel.NameDays, x => x.Value ?? new List<string>() )
            //    .BindTo(this, x => x.nameDays.ItemsSource);
            //this.WhenAny(x => x.ViewModel.Temp, x => x.Value ?? new List<string>())
            //    .BindTo(this, x => x.nameDays.ItemsSource);
            //this.WhenAny(x => x.ViewModel.Temp, x => x.Value ?? new List<string>())
            //    .BindTo(this, x => x.nameDays.ItemsSource);
            //nameDays.ItemsSource = ViewModel.Temp;
        }

        //public MyViewModel ViewModel
        //{
        //    get;
        //    set;
        //    //get { return (MyViewModel)GetValue(ViewModelProperty); }
        //    //set { SetValue(ViewModelProperty, value); }
        //}
        //public static readonly DependencyProperty ViewModelProperty =
        //    DependencyProperty.Register("ViewModel", typeof(MyViewModel), typeof(MainPage), new PropertyMetadata(null));

        public MyViewModel ViewModel
        {
            get { return viewModel; }
            set { viewModel = value; }
        }
        object IViewFor.ViewModel
        {
            get { return viewModel; }
            set { viewModel = (MyViewModel) value; }
        }
    }
}
