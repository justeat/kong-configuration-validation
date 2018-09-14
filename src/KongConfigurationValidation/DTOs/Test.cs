using System.Collections.Generic;

namespace KongConfigurationValidation.DTOs
{
    public class Test
    {
        public string Service { get; set; }
        public string Route { get; set; }
        public string RequestUri { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Method { get; set; }
        public string Payload { get; set; }

        protected bool Equals(Test other)
        {
            return string.Equals(Service, other.Service)
                && string.Equals(Route, other.Route)
                && string.Equals(RequestUri, other.RequestUri)
                && string.Equals(Method, other.Method);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Test) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Service != null ? Service.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Route != null ? Route.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Method != null ? Method.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Payload != null ? Payload.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RequestUri != null ? RequestUri.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
    
}
