using Moduli_Rilascio_Core;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using VatService.Structure;
using VatService.View;

namespace VatService.Model
{
    public class DispatcherModel
    {
        private MainViewModel m_mainViewModel { get; set; }

        private DBWriter m_writer;
        private DataSet m_DataFill;
        private Mutex m_mutex;
        private SoapExchange m_soapStruct;
        private IniUtility m_iniUtility;

        public volatile bool disposable;

        public DispatcherModel(MainViewModel l_mainViewModel)
        {
            disposable = false;

            m_iniUtility = new IniUtility();

            #region Default params
            if(!(m_iniUtility.KeyExists("username") && m_iniUtility.KeyExists("password")))
            {
                m_iniUtility.Write("username", "root");
                m_iniUtility.Write("password", "manager");
            }

            #endregion

            Init();

            m_mainViewModel = l_mainViewModel;
            m_mainViewModel.CloseApplicationRequestedFromViewModel += KillAlterEgo;
            m_mainViewModel.SendToModelData += StoreData;
        }

        internal void Init()
        {
            //start dataset
            m_DataFill = new DataSet();

            //start db
            m_writer = new DBWriter(ref m_iniUtility, ref m_DataFill);

            //soapstruct
            m_soapStruct = new SoapExchange();

            //mutex
            m_mutex = new Mutex(false, "Global\\{VatService}");
        }

        private void StoreData(object sender, EventArgs e)
        {
            //ricevo i dati dalla chiama GUI
            //popolo il dataset con i dati da verificare
            //eseguo le chiamate soap
            //archivio nel db
            //ritorno alla grafica i dati utili per riscontro visivo e coferma inserimento db

            VatArg vatArg = (VatArg)e;

            m_mutex.WaitOne(5000, false);
            m_DataFill.Clear();
            m_writer.GetBulk(ref m_DataFill);

            CheckVatResponse response = vatArg.checkVatResponse;
            m_soapStruct.Verify(ref response);

            if (response.Valid)
            {
                // Dataset temp operations
                if (m_DataFill.Tables["StoryVat"].AsEnumerable().Any(x => x["vatNumber"].Equals(response.VatNumber)))
                { //update servirà per le richieste multiple passate come csv o file binario
                    #region Update
                    DataRow rowUpdated = m_DataFill.Tables["StoryVat"].AsEnumerable().FirstOrDefault(x => x["vatNumber"].Equals(response.vatNumber));
                    rowUpdated["requestDate"] = response.RequestDate;
                    rowUpdated["valid"] = response.Valid;
                    rowUpdated["name"] = response.Name;
                    rowUpdated["address"] = response.Address;
                    #endregion
                }
                else
                { //richieste singole avranno un solo ed unico recordo per ogni richiesta/transazione
                    #region insert
                    DataRow newRow = m_DataFill.Tables["StoryVat"].NewRow();
                    newRow["countryCode"] = response.countryCode;
                    newRow["vatNumber"] = response.vatNumber;
                    newRow["requestDate"] = response.RequestDate;
                    newRow["valid"] = response.Valid;
                    newRow["name"] = response.Name;
                    newRow["address"] = response.Address;

                    // Aggiunta del nuovo record al DataSet
                    m_DataFill.Tables["StoryVat"].Rows.Add(newRow);
                    #endregion
                }

                //db operations
                m_writer.WriteOnDB();
            }

            (vatArg.checkVatResponse as Flagger).HasSuccessfull = true;
            //vatArg.checkVatResponse = response;

            //rilascio le risorse applicative
            m_mutex?.ReleaseMutex();


        }


        internal bool CheckKillable()
        {
            if (m_mutex.WaitOne(5000, false))
            {
                m_mutex.ReleaseMutex();

                return true;
            }

            return false;
        }

        private void KillAlterEgo(object sender, EventArgs e)
        {
            do
            {
                //cage model awaiter
            }
            while (!CheckKillable());

            m_writer.Dispose();
            m_mutex.Dispose();
            disposable = true;


            return;
        }
    }
}
