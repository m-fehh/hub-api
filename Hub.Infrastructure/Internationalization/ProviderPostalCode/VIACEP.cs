using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using System.Dynamic;
using System.Net;

namespace Hub.Infrastructure.Internationalization.ProviderPostalCode
{
    public class VIACEP : IProviderPostalCode
    {
        public JObject Search(string postalCode)
        {
            RestClient restClient = null;
            RestRequest restRequest = null;
            IRestResponse restResponse = null;

            try
            {
                postalCode = postalCode.PadLeft(8, '0');

                restClient = new RestClient(Engine.AppSettings["viacep-endpoint"]?.ToString());
                restRequest = new RestRequest($"/ws/{postalCode}/json", Method.GET);

                restResponse = restClient.Execute(restRequest);

                if (restResponse.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                dynamic result = JsonConvert.DeserializeObject<dynamic>(restResponse.Content);

                dynamic objReturn = new ExpandoObject();

                objReturn.cep = result.cep.Value;
                objReturn.logradouro = result.logradouro.Value;
                objReturn.complemento = result.complemento.Value;
                objReturn.bairro = result.bairro.Value;
                objReturn.localidade = result.localidade.Value;
                objReturn.uf = result.uf.Value;
                objReturn.estado = result.estado.Value;
                objReturn.ibge = result.ibge.Value;

                return JObject.Parse(JsonConvert.SerializeObject(objReturn));
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                restClient = null;
                restRequest = null;
                restResponse = null;
            }
        }
    }
}
