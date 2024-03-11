using System;
using System.Net;
using System.Net.Sockets;

namespace OryxBot.Client.Linux.Services;

public class DiscoverabilityService
{
    public int Port = 5558;

    private HttpListener _listener;

    public void Init()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://*:" + Port.ToString() + "/");
        _listener.Start();
        Receive();
    }

    private void Receive()
    {
        _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
    }

    private void ListenerCallback(IAsyncResult result) {
        if (_listener.IsListening) {
            var context = _listener.EndGetContext(result);
            var request = context.Request;

            // do something with the request
            Console.WriteLine($"{request.Url}");

            Receive();
            
            var response = context.Response;
            response.StatusCode = (int) HttpStatusCode.OK;
            response.ContentType = "text/plain";
            response.OutputStream.Write(new byte[] {}, 0, 0);
            response.OutputStream.Close();
        }
    }
}
