using System;
using System.Threading;
using System.Windows;
using VatService.Structure;
using VatService.View;

namespace VatService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public event EventHandler<VatArg> SendToViewModelData;
        public event EventHandler CloseApplicationRequestedFromView; //evento di chiusura che aspetta che il modello lo sblocchi.

        private CancellationToken m_token;
        private MainViewModel m_mainViewModel;
        
        public MainWindow()
        {
            InitializeComponent();


            try
            {
                m_mainViewModel = new MainViewModel(this, ref m_token);
            }
            catch 
            {
                Thread.CurrentThread.Abort();          
            }

            this.DataContext = this;

            SetBinders();
        }

        private void SetBinders()
        {
            Countrycode.ItemsSource = m_mainViewModel.CountryCodes;
        }

        private void VatCheck(object sender, RoutedEventArgs e)
        {
            if (Countrycode.SelectedItem is null)
                return;

            VatArg vatArg = new VatArg(Countrycode.SelectedItem.ToString(), Vatcode.Text);
            SendToViewModelData?.Invoke(this, vatArg); //gestione della chiamata asincrona

            while(!vatArg.State)
            {
                //Cage awaiter
                //qui si potranno inserire altre attività da fare sulle richieste multiple da file
            }

            //nuova window per mostrare i dati
            DataContainer dataContainer = new DataContainer(vatArg.checkVatResponse);
            dataContainer.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseApplicationRequestedFromView?.Invoke(this, null);

            while (!m_token.IsCancellationRequested)
            {
                m_token.WaitHandle.WaitOne(1000);
            }
        }
    }
}
