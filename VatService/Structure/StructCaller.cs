using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VatService.Structure
{
    public class CheckVatRequest
    {
        [XmlElement(Namespace = "urn:ec.europa.eu:taxud:vies:services:checkVat:types")]
        public string countryCode;

        [XmlElement(Namespace = "urn:ec.europa.eu:taxud:vies:services:checkVat:types")]
        public string vatNumber;

        public void Set(string cCode, string vNum)
        {
            countryCode = cCode;
            vatNumber = vNum;
        }
    }

    public class Flagger : CheckVatRequest //overriding primo livello
    {
        public volatile bool HasSuccessfull = false; //flag risoluzione
        public void Set(bool Succes)
        { 
            HasSuccessfull = Succes;
        }
    }

    public class CheckVatResponse : Flagger //overriding secondo livello
    {
        public string CountryCode { get; set; } //si possono rimuovere
        public string VatNumber { get; set; }   //si possono rimuovere
        public DateTime RequestDate { get; set; }
        public bool Valid { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public void Set(CheckVatResponse checkVatResponse)
        {
            CountryCode = checkVatResponse.countryCode;
            VatNumber = checkVatResponse.VatNumber;
            RequestDate = checkVatResponse.RequestDate;
            Valid = checkVatResponse.Valid;
            Name = checkVatResponse.Name;
            Address = checkVatResponse.Address;
        }
    }

}
