using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VatService.Structure;

namespace VatService.View
{
    /// <summary>
    /// Logica di interazione per DataContainer.xaml
    /// </summary>
    public partial class DataContainer : Window
    {
        private CheckVatResponse m_CheckVatResponse;  

        public DataContainer(CheckVatResponse checkVatResponse)
        {
            m_CheckVatResponse = checkVatResponse;
          
            InitializeComponent();
            Setdata();       
        }

        internal void Setdata()
        {
            UserPanel.CountryCode = m_CheckVatResponse.countryCode;
            UserPanel.VatCode = m_CheckVatResponse.vatNumber;
            UserPanel.Address = m_CheckVatResponse.Address;
            UserPanel.DataRQ = m_CheckVatResponse.RequestDate.Date.ToString();
            UserPanel.NameEx = m_CheckVatResponse.Name;
        }
    }
}
