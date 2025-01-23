using System;
using System.Collections.ObjectModel;
using System.Threading;
using VatService;
using VatService.Model;
using VatService.View;

public class MainViewModel
{
    public event EventHandler<VatArg> SendToModelData;
    public event EventHandler CloseApplicationRequestedFromViewModel;
    public ObservableCollection<string> CountryCodes;

    private DispatcherModel m_dispatcherModel;
    private CancellationTokenSource m_source;
    private Thread m_thread; //watchdog sulla chiusura GUI da utente

    public void RaiseDisposable()
    {
        while(!m_dispatcherModel.disposable)
        {
            //watchdog closer
            Thread.Sleep(500);
        }

        m_source.Cancel();
        //unlock view awaiting
    }

    public MainViewModel(MainWindow mainWindow,ref CancellationToken m_token)
    {
        //gestione thread
        m_source = new CancellationTokenSource();
        m_token = m_source.Token;
        m_thread = new Thread(() => { RaiseDisposable(); });

        //gestione modello principale
        m_dispatcherModel = new DispatcherModel(this);

        //reflection verso il modello
        mainWindow.SendToViewModelData += ReflectionData;
        mainWindow.CloseApplicationRequestedFromView += ReflectionClosing;

        //combo
        #region CountryCode
        CountryCodes = new ObservableCollection<string>
        {
            "AT",
            "BE",
            "BG",
            "CY",
            "HR",
            "DK",
            "EE",
            "FI",
            "DE",
            "GB",
            "EL",
            "IE",
            "XI",
            "IT",
            "LV",
            "LT",
            "LU",
            "MT",
            "NL",
            "PL",
            "PT",
            "CZ",
            "SK",
            "RO",
            "SI",
            "ES",
            "SE",
            "HU"
        };
        #endregion

        m_thread.Start();
    }

    //mirroring attivo dei due eventi
    private void ReflectionClosing(object sender, EventArgs e)
    {
        CloseApplicationRequestedFromViewModel?.Invoke(this, EventArgs.Empty);
    }

    private void ReflectionData(object sender, VatArg e)
    {
        SendToModelData?.Invoke(this, e);
    }
}