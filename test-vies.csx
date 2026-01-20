// Тестирование VIES API
#r "System.ServiceModel.dll"
#r "System.ServiceModel.Http.dll"

using System;
using System.ServiceModel;
using System.Threading.Tasks;

var binding = new BasicHttpBinding
{
    MaxReceivedMessageSize = 1024 * 1024,
    Security = { Mode = BasicHttpSecurityMode.None }
};

var endpoint = new EndpointAddress("http://ec.europa.eu/taxation_customs/vies/services/checkVatService");

Console.WriteLine($"Testing VIES API...");
Console.WriteLine($"Endpoint: {endpoint.Uri}");
Console.WriteLine($"Security Mode: {binding.Security.Mode}");

try 
{
    Console.WriteLine("Attempting to connect to VIES...");
    using (var httpClient = new System.Net.Http.HttpClient())
    {
        var response = await httpClient.GetAsync("http://ec.europa.eu/taxation_customs/vies/services/checkVatService?wsdl");
        Console.WriteLine($"HTTP Status: {response.StatusCode}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"WSDL Content Length: {content.Length} characters");
            Console.WriteLine("VIES service is reachable!");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
