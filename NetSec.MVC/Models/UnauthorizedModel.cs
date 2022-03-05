using System.Net;

namespace NetSec.MVC.Models;

public class UnauthorizedModel
{
    public UnauthorizedModel(string resource, HttpStatusCode httpStatusCode, string reason)
    {
        Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        HttpStatusCode = httpStatusCode;
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }

    public string Resource { get; }
    public HttpStatusCode HttpStatusCode { get; }
    public string Reason { get; }
}