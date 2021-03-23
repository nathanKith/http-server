using System;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Linq;

namespace HTTPServer
{
    class Client
    {
        public Client(TcpClient tcpClient)
        {
            var request = GetRequest(tcpClient);

            var requestMatch = Regex.Match(request.ToString(), @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");
            if (requestMatch == Match.Empty)
            {
                SendError(tcpClient, 400, "GET");
                return;
            }

            var method = GetMethod(requestMatch.Groups[0].Value);
            if (method != "HEAD" && method != "GET")
            {
                SendError(tcpClient, 405, method);
                return;
            }

            var requestURI = Uri.UnescapeDataString(requestMatch.Groups[1].Value);
            if (requestURI.IndexOf("../") >= 0)
            {
                SendError(tcpClient, 400, method);
                return;
            }

            var noFile = false;
            if (requestURI.EndsWith("/"))
            {
                if (requestURI.IndexOf(".") > 0)
                {
                    SendError(tcpClient, 404, method);
                    return;
                }
                requestURI += "index.html";
                noFile = true;
            }

            var path = "../../.." + requestURI;
            // Console.WriteLine(path);
            if (!File.Exists(path))
            {
                if (noFile)
                {
                    SendError(tcpClient, 403, method);
                    return;
                }
                SendError(tcpClient, 404, method);
                return;
            }

            var extension = requestURI.Substring(requestURI.LastIndexOf('.'));
            var contentType = GetContentType(extension);

            SendResponse(tcpClient, contentType, path, method);

            tcpClient.Close();
        }

        private string GetRequest(TcpClient tcpClient)
        {
            var request = new StringBuilder();
            var buffer = new byte[1024];
            int count;

            while ((count = tcpClient.GetStream().Read(buffer, 0, buffer.Length)) > 0)
            {
                request.Append(Encoding.ASCII.GetString(buffer, 0, count));

                if (request.ToString().IndexOf("\r\n\r\n") >= 0)
                {
                    break;
                }
            }

            return request.ToString();
        }

        private string GetMethod(string firstString)
        {
            var space = firstString.IndexOf(' ');
            if (space < 0)
            {
                return "GET";
            }

            return firstString.Substring(0, space);
        }

        private void SendError(TcpClient tcpClient, int code, string method)
        {
            var status = $"{code} {((HttpStatusCode)code).ToString()}";

            if (method == "HEAD") 
            {
                var response = GetHeadersHEAD(status);
                
                var responseBuffer = Encoding.ASCII.GetBytes(response);
                tcpClient.GetStream().Write(responseBuffer, 0, responseBuffer.Length);
                tcpClient.Close();

                return;
            }

            var html = $"<html><body><h1>{status}</h1></body></html>";
            var headers = GetHeaders(status, "text/html", html.Length);

            var buffer = Encoding.ASCII.GetBytes(headers + html);
            tcpClient.GetStream().Write(buffer, 0, buffer.Length);
            tcpClient.Close();
        }

        private string GetHeadersHEAD(string status) => $"HTTP/1.1 {status}\r\nDate: {new DateTime().ToUniversalTime().ToString()}\r\nConnection: keep-alive\r\nServer: superserver/1.0.0 (Ubuntu)\r\n\r\n";

        private string GetContentType(string extension)
        {
            switch (extension)
            {
                case ".htm":
                case ".html":
                    return "text/html";
                case ".css":
                    return "text/css";
                case ".js":
                    return "application/javascript";
                case ".jpg":
                    return "image/jpeg";
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".swf":
                    return "application/x-shockwave-flash";
                default:
                    if (extension.Length > 1)
                    {
                        return "application/" + extension.Substring(1);
                    }

                    return "application/unknown";
            }
        }

        private string GetHeaders(string status, string contentType, long contentLength) => $"HTTP/1.1 {status}\r\nContent-Type: {contentType}\r\nContent-Length: {contentLength}\r\nDate: {new DateTime().ToUniversalTime().ToString()}\r\nConnection: keep-alive\r\nServer: superserver/1.0.0 (Ubuntu)\r\n\r\n";

        private void SendResponse(TcpClient tcpClient, string contentType, string path, string method)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var buffer = new byte[1024];
                int count;

                var headers = GetHeaders("200 OK", contentType, fs.Length);
                var headersBuffer = Encoding.ASCII.GetBytes(headers);
                tcpClient.GetStream().Write(headersBuffer, 0, headersBuffer.Length);

                if (method == "HEAD")
                {
                    return;
                }
                
                while (fs.Position < fs.Length)
                {
                    count = fs.Read(buffer, 0, buffer.Length);
                    tcpClient.GetStream().Write(buffer, 0, count);
                }
            }
        }
    }
}
