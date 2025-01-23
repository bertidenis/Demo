using RestSharp;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using VatService.Structure;

namespace VatService.Model
{
    public  class SoapExchange : ICaller
    {
        public void Verify(ref CheckVatResponse response)
        {
            CheckVatRequest request = response;
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDoc.AppendChild(xmlDeclaration);
            XmlElement envelopeElement = xmlDoc.CreateElement("soap", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
            xmlDoc.AppendChild(envelopeElement);
            XmlElement bodyElement = xmlDoc.CreateElement("soap", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
            envelopeElement.AppendChild(bodyElement);
            XmlElement checkVatElement = xmlDoc.CreateElement("tns1", "checkVat", "urn:ec.europa.eu:taxud:vies:services:checkVat:types");
            bodyElement.AppendChild(checkVatElement);
            XmlElement countryCodeElement = xmlDoc.CreateElement("tns1", "countryCode", "urn:ec.europa.eu:taxud:vies:services:checkVat:types");
            countryCodeElement.InnerText = request.countryCode;
            checkVatElement.AppendChild(countryCodeElement);
            XmlElement vatNumberElement = xmlDoc.CreateElement("tns1", "vatNumber", "urn:ec.europa.eu:taxud:vies:services:checkVat:types");
            vatNumberElement.InnerText = request.vatNumber;
            checkVatElement.AppendChild(vatNumberElement);

            string xmlString;

            using (StringWriter stringWriter = new StringWriter())
            {
                xmlDoc.Save(stringWriter);
                xmlString = stringWriter.ToString();
            }

            // Ora posso inviare la richiesta SOAP
             response.Set(SendSoapRequest(xmlString));
        }

        internal  CheckVatResponse SendSoapRequest(string soapXml)
        {
            //rest setup
            var options = new RestClientOptions("http://ec.europa.eu")
            {
                MaxTimeout = -1,
            };

            var client = new RestClient(options);
            var request = new RestRequest("/taxation_customs/vies/services/checkVatService", Method.Post);
            request.AddHeader("Content-Type", "text/xml; charset=utf-8");
            request.AddParameter("application/xml", soapXml, ParameterType.RequestBody);

            RestResponse response =  client.Execute(request);

            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return GetInfo(response.Content);
            }
            else
            {
                return new CheckVatResponse() { VatNumber = string.Empty, CountryCode = string.Empty };
            }          
        }

        internal  CheckVatResponse GetInfo(string xmlString)
        {
            //scompongo xml ricevuto
            CheckVatResponse response = new CheckVatResponse();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("env", "http://schemas.xmlsoap.org/soap/envelope/");
            nsManager.AddNamespace("ns2", "urn:ec.europa.eu:taxud:vies:services:checkVat:types");

            // Seleziono i nodi
            var countryCodeNode = xmlDoc.SelectSingleNode("//ns2:countryCode", nsManager);
            if (countryCodeNode != null)
                response.CountryCode = countryCodeNode.InnerText;

            var vatNumberNode = xmlDoc.SelectSingleNode("//ns2:vatNumber", nsManager);
            if (vatNumberNode != null)
                response.VatNumber = vatNumberNode.InnerText;

            var requestDateNode = xmlDoc.SelectSingleNode("//ns2:requestDate", nsManager);
            if (requestDateNode != null)
                response.RequestDate = DateTime.Parse(requestDateNode.InnerText);

            var validNode = xmlDoc.SelectSingleNode("//ns2:valid", nsManager);
            if (validNode != null)
                response.Valid = Convert.ToBoolean(validNode.InnerText);

            var nameNode = xmlDoc.SelectSingleNode("//ns2:name", nsManager);
            if (nameNode != null)
                response.Name = nameNode.InnerText;

            var addressNode = xmlDoc.SelectSingleNode("//ns2:address", nsManager);
            if (addressNode != null)
                response.Address = addressNode.InnerText;

            return response;
        }
    }
}
