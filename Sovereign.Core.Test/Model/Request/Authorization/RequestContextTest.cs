using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Sovereign.Core.Model.Request.Authorization;
using Sovereign.Core.Model.Response;

namespace Sovereign.Core.Test.Model.Request.Authorization;

public class RequestContextTest
{
    public static readonly string TestBody = "{\"Status\":\"Test\"}";
    public static readonly List<string> ApiKeys = new List<string>() { "TestApiKey" };
    public static readonly List<string> Secrets = new List<string>() { "TestSecretKey" };
    
    [Test]
    public void TestGetRequest()
    {
        var requestContext = new RequestContext(CreateHttpContext());
        var response = requestContext.GetRequest(SimpleResponseJsonContext.Default.SimpleResponse);
        Assert.That(response!.Status, Is.EqualTo("Test"));
    }

    [Test]
    public void TestIsAuthorizedNullHeader()
    {
        var requestContext = new RequestContext(CreateHttpContext());
        Assert.That(requestContext.IsAuthorized(ApiKeys, Secrets), Is.False);
    }

    [Test]
    public void TestIsAuthorizedIncomplete()
    {
        var requestContext = new RequestContext(CreateHttpContext("ApiKey"));
        Assert.That(requestContext.IsAuthorized(ApiKeys, Secrets), Is.False);
    }

    [Test]
    public void TestIsAuthorizedInvalidApiKey()
    {
        var requestContext = new RequestContext(CreateHttpContext("ApiKey BadKey"));
        Assert.That(requestContext.IsAuthorized(ApiKeys, Secrets), Is.False);
    }

    [Test]
    public void TestIsAuthorizedMissingApiKeys()
    {
        var requestContext = new RequestContext(CreateHttpContext("ApiKey TestApiKey"));
        Assert.That(requestContext.IsAuthorized(null, Secrets), Is.False);
    }

    [Test]
    public void TestIsAuthorizedValidApiKey()
    {
        var requestContext = new RequestContext(CreateHttpContext("ApiKey TestApiKey"));
        Assert.That(requestContext.IsAuthorized(ApiKeys, Secrets), Is.True);
    }

    [Test]
    public void TestIsAuthorizedInvalidSignature()
    {
        var requestContext = new RequestContext(CreateHttpContext("Signature BadSignature"));
        Assert.That(requestContext.IsAuthorized(ApiKeys, Secrets), Is.False);
    }

    [Test]
    public void TestIsAuthorizedMissingSignature()
    {
        using var sha256 = new HMACSHA256(Encoding.UTF8.GetBytes("BadSecretKey"));
        var signature = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(TestBody)));
        var requestContext = new RequestContext(CreateHttpContext($"Signature {signature}"));
        Assert.That(requestContext.IsAuthorized(null, Secrets), Is.False);
    }

    [Test]
    public void TestIsAuthorizedValidSignature()
    {
        using var sha256 = new HMACSHA256(Encoding.UTF8.GetBytes("TestSecretKey"));
        var signature = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(TestBody)));
        var requestContext = new RequestContext(CreateHttpContext($"Signature {signature}"));
        Assert.That(requestContext.IsAuthorized(ApiKeys, Secrets), Is.True);
    }

    public HttpContext CreateHttpContext(string? authorizationHeader = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(TestBody));
        if (authorizationHeader != null)
        {
            context.Request.Headers.Append("Authorization", authorizationHeader);
        }
        return context;
    }
}