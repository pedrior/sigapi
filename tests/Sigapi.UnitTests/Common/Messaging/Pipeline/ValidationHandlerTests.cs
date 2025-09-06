using FluentValidation;
using FluentValidation.Results;
using Sigapi.Common.Messaging;
using Sigapi.Common.Messaging.Pipeline;
using Sigapi.UnitTests.TestUtils.Requests;

namespace Sigapi.UnitTests.Common.Messaging.Pipeline;

[TestSubject(typeof(ValidationHandler<,>))]
public sealed class ValidationHandlerTests
{
    private readonly IRequestHandler<TestRequest, TestResponse> innerHandler =
        A.Fake<IRequestHandler<TestRequest, TestResponse>>();

    private readonly IValidator<TestRequest> validator1 = A.Fake<IValidator<TestRequest>>();
    private readonly IValidator<TestRequest> validator2 = A.Fake<IValidator<TestRequest>>();

    [Fact]
    public async Task HandleAsync_WhenValidationSucceeds_ShouldCallInnerHandlerAndReturnSuccess()
    {
        // Arrange
        var request = new TestRequest();
        var expectedResponse = new TestResponse();
        var validationResult = new ValidationResult(); // No errors

        A.CallTo(() => validator1.ValidateAsync(A<IValidationContext>._, A<CancellationToken>._))
            .Returns(validationResult);

        A.CallTo(() => innerHandler.HandleAsync(request, A<CancellationToken>._))
            .Returns(expectedResponse);

        var handler = new ValidationHandler<TestRequest, TestResponse>(innerHandler, [validator1]);

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedResponse);

        A.CallTo(() => innerHandler.HandleAsync(request, CancellationToken.None))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => validator1.ValidateAsync(A<ValidationContext<TestRequest>>.That
                .Matches(c => c.InstanceToValidate == request), CancellationToken.None))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnErrorsAndNotCallInnerHandler()
    {
        // Arrange
        var request = new TestRequest();
        var validationFailures = new List<ValidationFailure>
        {
            new("Property1", "Error message 1"),
            new("Property2", "Error message 2")
        };

        var validationResult = new ValidationResult(validationFailures);

        A.CallTo(() => validator1.ValidateAsync(A<IValidationContext>._, A<CancellationToken>._))
            .Returns(Task.FromResult(validationResult));

        var handler = new ValidationHandler<TestRequest, TestResponse>(innerHandler, [validator1]);

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Code == "Property1" &&
                                            e.Description == "Error message 1" &&
                                            e.Type == ErrorType.Validation);

        result.Errors.Should().Contain(e => e.Code == "Property2" &&
                                            e.Description == "Error message 2" &&
                                            e.Type == ErrorType.Validation);

        A.CallTo(() => innerHandler.HandleAsync(A<TestRequest>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task HandleAsync_WithMultipleValidatorsThatFails_ShouldAggregateValidationErrors()
    {
        // Arrange
        var request = new TestRequest();

        var validationFailures1 = new List<ValidationFailure> { new("Property1", "Error message 1") };
        var validationFailures2 = new List<ValidationFailure> { new("Property2", "Error message 2") };

        var validationResult1 = new ValidationResult(validationFailures1);
        var validationResult2 = new ValidationResult(validationFailures2);

        A.CallTo(() => validator1.ValidateAsync(A<IValidationContext>._, A<CancellationToken>._))
            .Returns(Task.FromResult(validationResult1));

        A.CallTo(() => validator2.ValidateAsync(A<IValidationContext>._, A<CancellationToken>._))
            .Returns(Task.FromResult(validationResult2));

        var handler = new ValidationHandler<TestRequest, TestResponse>(innerHandler, [validator1, validator2]);

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().ContainSingle(e => e.Code == "Property1" && e.Description == "Error message 1");
        result.Errors.Should().ContainSingle(e => e.Code == "Property2" && e.Description == "Error message 2");

        A.CallTo(() => innerHandler.HandleAsync(A<TestRequest>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task HandleAsync_WithNoValidators_ShouldCallInnerHandler()
    {
        // Arrange
        var request = new TestRequest();
        var expectedResponse = new TestResponse();

        A.CallTo(() => innerHandler.HandleAsync(request, A<CancellationToken>._))
            .Returns(Task.FromResult<ErrorOr<TestResponse>>(expectedResponse));

        var handler = new ValidationHandler<TestRequest, TestResponse>(innerHandler, []);

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedResponse);

        A.CallTo(() => innerHandler.HandleAsync(request, CancellationToken.None))
            .MustHaveHappenedOnceExactly();
    }
}