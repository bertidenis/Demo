using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VatService.View
{
    /// <summary>
    /// Logica di interazione per UserPanel.xaml
    /// </summary>
    public partial class UserPanel : UserControl
    {
        public UserPanel()
        {
            InitializeComponent();
        }

        public string CountryCode
        {
            set
            {
                COUNTRYCODE.Text = value;
            }
        }

        public string VatCode
        {
            set
            {
                VATCODE.Text = value;
            }
        }

        public string Address
        {
            set
            {
                ADDRESS.Text = value;
            }
        }

        public string DataRQ
        {
            set
            {
                DATARQ.Text = value;
            }
        }

        public string NameEx
        {
            set
            {
                NAME.Text = value;
            }
        }
    }
}
