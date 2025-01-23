using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VatService.Structure;

namespace VatService.View
{
    public class VatArg : EventArgs
    {
        public CheckVatResponse checkVatResponse {  get; set; }

        public VatArg(string code, string vat) 
        {
            checkVatResponse = new CheckVatResponse();

            #region BULKING 1 Livello
            ((CheckVatRequest)checkVatResponse).Set(code, vat);
            #endregion

            #region BULKING 2 Livello
            ((Flagger)checkVatResponse).Set(false);
            #endregion
        }

        public bool State //serve alla view
        {
            get
            {
                return ((Flagger)checkVatResponse).HasSuccessfull;
            }
        }
    }
}
