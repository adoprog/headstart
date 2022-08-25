﻿using System;
using OrderCloud.Integrations.CardConnect.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.CardConnect.Exceptions
{
    public class CreditCardAuthorizationException : Exception
    {
        public CreditCardAuthorizationException(ApiError error, CardConnectAuthorizationResponse response)
        {
            ApiError = error;
            Response = response;
        }

        public CreditCardAuthorizationException(string errorCode, string message, CardConnectAuthorizationResponse data)
        {
            ApiError = new ApiError()
            {
                Data = data,
                ErrorCode = errorCode,
                Message = message,
            };
            Response = data;
        }

        public ApiError ApiError { get; }

        public CardConnectAuthorizationResponse Response { get; }
    }

    public class CardConnectInquireException : Exception
    {
        public CardConnectInquireException(ApiError error, CardConnectInquireResponse response)
        {
            ApiError = error;
            Response = response;
        }

        public CardConnectInquireException(string errorCode, string message, CardConnectInquireResponse data)
        {
            ApiError = new ApiError()
            {
                Data = data,
                ErrorCode = errorCode,
                Message = message,
            };
            Response = data;
        }

        public ApiError ApiError { get; }

        public CardConnectInquireResponse Response { get; }
    }

    public class CreditCardVoidException : Exception
    {
        public CreditCardVoidException(ApiError error, CardConnectVoidResponse response)
        {
            ApiError = error;
            Response = response;
        }

        public CreditCardVoidException(string errorCode, string message, CardConnectVoidResponse data)
        {
            ApiError = new ApiError()
            {
                Data = data,
                ErrorCode = errorCode,
                Message = message,
            };
            Response = data;
        }

        public ApiError ApiError { get; }

        public CardConnectVoidResponse Response { get; }
    }

    public class CardConnectCaptureException : Exception
    {
        public CardConnectCaptureException(ApiError error, CardConnectCaptureResponse response)
        {
            ApiError = error;
            Response = response;
        }

        public CardConnectCaptureException(string errorCode, string message, CardConnectCaptureResponse data)
        {
            ApiError = new ApiError()
            {
                Data = data,
                ErrorCode = errorCode,
                Message = message,
            };
            Response = data;
        }

        public ApiError ApiError { get; }

        public CardConnectCaptureResponse Response { get; }
    }

    public class CreditCardRefundException : Exception
    {
        public CreditCardRefundException(ApiError error, CardConnectRefundResponse response)
        {
            ApiError = error;
            Response = response;
        }

        public CreditCardRefundException(string errorCode, string message, CardConnectRefundResponse data)
        {
            ApiError = new ApiError()
            {
                Data = data,
                ErrorCode = errorCode,
                Message = message,
            };
            Response = data;
        }

        public ApiError ApiError { get; }

        public CardConnectRefundResponse Response { get; }
    }
}
