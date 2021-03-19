using System;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;

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
                SendError(tcpClient, 400);
                return;
            }

            var method = GetMethod(requestMatch.Groups[0].Value);
            if (method != "HEAD" && method != "GET") {
                SendError(tcpClient, 405);
                return;
            }

            var requestURI = Uri.UnescapeDataString(requestMatch.Groups[1].Value);
            if (requestURI.IndexOf("..") >= 0)
            {
                SendError(tcpClient, 400);
                return;
            }

            if (requestURI.EndsWith("/"))
            {
                requestURI += "index.html";
            }

            var path = "www" + requestURI;
            if (!File.Exists(path))
            {
                SendError(tcpClient, 404);
                return;
            }

            var extension = requestURI.Substring(requestURI.LastIndexOf('.'));
            var contentType = GetContentType(extension);

            SendResponse(tcpClient, contentType, path);

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

        private string GetMethod(string firstString) => firstString.Substring(0, firstString.IndexOf(' '));

        private void SendError(TcpClient tcpClient, int code)
        {
            var status = $"{code} {((HttpStatusCode)code).ToString()}";

            var html = $"<html><body><h1>{status}</h1></body></html>";
            var headers = GetHeaders(status, "text/html", html.Length);

            var buffer = Encoding.ASCII.GetBytes(headers + html);
            tcpClient.GetStream().Write(buffer, 0, buffer.Length);
            tcpClient.Close();
        }

        private string GetContentType(string extension)
        {
            switch (extension)
            {
                case ".htm":
                case ".html":
                    return "text/html";
                case ".css":
                    return "text/stylesheet";
                case ".js":
                    return "text/javascript";
                case ".jpg":
                    return "image/jpeg";
                case ".jpeg":
                case ".png":
                case ".gif":
                    return "image/" + extension.Substring(1);
                default:
                    if (extension.Length > 1)
                    {
                        return "application/" + extension.Substring(1);
                    }

                    return "application/unknown";
            }
        }

        private string GetHeaders(string status, string contentType, long contentLength) => $"HTTP/1.1 {status}\nContent-Type: {contentType}\nContent-Length: {contentLength}\nDate: {new DateTime().ToUniversalTime().ToString()}\nConnection: keep-alive\n\n";

        private void SendResponse(TcpClient tcpClient, string contentType, string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var buffer = new byte[1024];
                int count;

                var headers = GetHeaders("200 OK", contentType, fs.Length);
                var headersBuffer = Encoding.ASCII.GetBytes(headers);
                tcpClient.GetStream().Write(headersBuffer, 0, headersBuffer.Length);

                while (fs.Position < fs.Length)
                {
                    count = fs.Read(buffer, 0, buffer.Length);
                    tcpClient.GetStream().Write(buffer, 0, count);
                }
            }
        }
    }
}
