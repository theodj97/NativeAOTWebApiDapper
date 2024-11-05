using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using WebApiDapperNativeAOT.Models.Requests.Todo;
using WebApiDapperNativeAOT.Models.Responses;
using WebApiDapperNativeAOT.Testing.SeedWork;
using WebApiDapperNativeAOT.Testing.SeedWork.Extension;

namespace WebApiDapperNativeAOT.Testing.FuncionalTesting.API.Todo;

[Collection(nameof(TestServerFixtureCollection))]
public class Post(TestServerFixture fixture)
{
    private readonly TestServerFixture Fixture = fixture;

    [Fact]
    [ResetDatabase]
    [Trait(CategoryTrait.CATEGORY, CategoryTrait.FUNCTIONAL)]
    public async Task CreateTodo_Should_ReturnTodo_Ok()
    {
        // Arrange
        TodoCreateRequest todoCreateRequest = new("title", "description", 0, "assignedTo", DateTime.UtcNow, false);

        // Act
        var response = await Fixture.HttpClient.PostAsync(Routes.Todo.Create(), todoCreateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseModel = await response.Content.ReadFromJsonAsync<TodoResponse>();
        responseModel.Should().NotBeNull();
        responseModel!.Title.Should().Be(todoCreateRequest.Title);
        responseModel.Description.Should().Be(todoCreateRequest.Description);
        responseModel.CreatedBy.Should().Be(todoCreateRequest.CreatedBy);
        responseModel.AssignedTo.Should().Be(todoCreateRequest.AssignedTo);
        responseModel.TargetDate.Should().Be(todoCreateRequest.TargetDate);
        responseModel.IsComplete.Should().Be(todoCreateRequest.IsComplete);
        responseModel.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    [ResetDatabase]
    [Trait(CategoryTrait.CATEGORY, CategoryTrait.FUNCTIONAL)]
    public async Task CreateTodo_TitleAlreadyExists_Should_Return_BadRequest()
    {
        // Arrange
        var defaultTodo = await Fixture.Extension.AddDefaultTodo();
        TodoCreateRequest todoCreateRequest = new(defaultTodo.Title, "description", 0, "assignedTo", DateTime.UtcNow, false);

        // Act
        var response = await Fixture.HttpClient.PostAsync(Routes.Todo.Create(), todoCreateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
