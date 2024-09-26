using System.Collections.Generic;
using NUnit.Framework;
using Sovereign.Core.Model.Response;

namespace Sovereign.Core.Test.Model.Response;

public class ValidationErrorResponseTest
{
    [Test]
    public void TestGetValidationErrorResponse()
    {
        var response = ValidationErrorResponse.GetValidationErrorResponse(new List<ValidationErrorCheck>()
        {
            new ValidationErrorCheck()
            {
                Path = "Path1.Path2",
                Message = "Test message 1",
                IsValid = () => false,
            },
            new ValidationErrorCheck()
            {
                Path = "Path3",
                Message = "Test message 2",
                IsValid = () => true,
            },
            new ValidationErrorCheck()
            {
                Path = "Path4",
                Message = "Test message 3",
                IsValid = () => false,
            },
        });
        
        Assert.That(response!.Status, Is.EqualTo(ResponseStatus.ValidationError));
        Assert.That(response!.Errors[0].Path, Is.EqualTo("Path1.Path2"));
        Assert.That(response!.Errors[0].Message, Is.EqualTo("Test message 1"));
        Assert.That(response!.Errors[1].Path, Is.EqualTo("Path4"));
        Assert.That(response!.Errors[1].Message, Is.EqualTo("Test message 3"));
        Assert.That(response!.Errors.Count, Is.EqualTo(2));
    }

    [Test]
    public void TestGetValidationErrorResponseNoResponse()
    {
        var response = ValidationErrorResponse.GetValidationErrorResponse(new List<ValidationErrorCheck>()
        {
            new ValidationErrorCheck()
            {
                Path = "Path1.Path2",
                Message = "Test message 1",
                IsValid = () => true,
            },
        });
        
        Assert.That(response, Is.Null);
    }
}