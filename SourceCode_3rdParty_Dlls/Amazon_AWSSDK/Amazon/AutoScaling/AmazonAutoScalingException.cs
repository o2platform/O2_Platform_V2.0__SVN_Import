﻿namespace Amazon.AutoScaling
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class AmazonAutoScalingException : Exception, ISerializable
    {
        private string errorCode;
        private string errorType;
        private string message;
        private string requestId;
        private HttpStatusCode statusCode;
        private string xml;

        public AmazonAutoScalingException()
        {
        }

        public AmazonAutoScalingException(Exception innerException) : this(innerException.Message, innerException)
        {
        }

        public AmazonAutoScalingException(string message)
        {
            this.message = message;
        }

        protected AmazonAutoScalingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.message = info.GetString("message");
            this.statusCode = (HttpStatusCode) info.GetValue("statusCode", typeof(HttpStatusCode));
            this.errorCode = info.GetString("errorCode");
            this.errorType = info.GetString("errorType");
            this.requestId = info.GetString("requestId");
            this.xml = info.GetString("xml");
        }

        public AmazonAutoScalingException(string message, Exception innerException) : base(message, innerException)
        {
            this.message = message;
            AmazonAutoScalingException exception = innerException as AmazonAutoScalingException;
            if (exception != null)
            {
                this.statusCode = exception.StatusCode;
                this.errorCode = exception.ErrorCode;
                this.errorType = exception.ErrorType;
                this.requestId = exception.RequestId;
                this.xml = exception.XML;
            }
        }

        public AmazonAutoScalingException(string message, HttpStatusCode statusCode) : this(message)
        {
            this.statusCode = statusCode;
        }

        public AmazonAutoScalingException(string message, HttpStatusCode statusCode, string errorCode, string errorType, string requestId, string xml) : this(message, statusCode)
        {
            this.errorCode = errorCode;
            this.errorType = errorType;
            this.requestId = requestId;
            this.xml = xml;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("message", this.message, typeof(string));
            info.AddValue("statusCode", this.statusCode, typeof(HttpStatusCode));
            info.AddValue("errorCode", this.errorCode, typeof(string));
            info.AddValue("errorType", this.errorType, typeof(string));
            info.AddValue("requestId", this.requestId, typeof(string));
            info.AddValue("xml", this.xml, typeof(string));
            base.GetObjectData(info, context);
        }

        public string ErrorCode
        {
            get
            {
                return this.errorCode;
            }
        }

        public string ErrorType
        {
            get
            {
                return this.errorType;
            }
        }

        public override string Message
        {
            get
            {
                return this.message;
            }
        }

        public string RequestId
        {
            get
            {
                return this.requestId;
            }
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                return this.statusCode;
            }
        }

        public string XML
        {
            get
            {
                return this.xml;
            }
        }
    }
}

