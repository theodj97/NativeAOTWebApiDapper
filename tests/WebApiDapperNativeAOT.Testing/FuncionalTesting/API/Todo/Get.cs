using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using WebApiDapperNativeAOT.Models.Responses;
using WebApiDapperNativeAOT.Testing.SeedWork;

namespace WebApiDapperNativeAOT.Testing.FuncionalTesting.API.Todo;

[Collection(nameof(TestServerFixtureCollection))]
public class Get(TestServerFixture fixture)
{
    private readonly TestServerFixture Fixture = fixture;

    [Fact]
    [ResetDatabase]
    [Trait("Category", "Functional")]
    public async Task GetTodoById_Should_ReturnTodo_Ok()
    {
        // Arrange
        var defaultTodo = await Fixture.Extension.AddDefaultTodo();

        // Act
        var response = await Fixture.HttpClient.GetAsync(ApiRoutes.Todo.GetById(defaultTodo.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseModel = await response.Content.ReadFromJsonAsync<TodoResponse>();
        responseModel.Should().NotBeNull();
        responseModel!.Id.Should().Be(defaultTodo.Id);
        responseModel.Title.Should().Be(defaultTodo.Title);
        responseModel.Description.Should().Be(defaultTodo.Description);
        responseModel.CreatedBy.Should().Be(defaultTodo.CreatedBy);
        responseModel.AssignedTo.Should().Be(defaultTodo.AssignedTo);
        responseModel.TargetDate.Should().Be(defaultTodo.TargetDate);
        responseModel.IsComplete.Should().Be(defaultTodo.IsComplete);
    }

    [Fact]
    [ResetDatabase]
    [Trait("Category", "Functional")]
    public async Task GetTodoById_Should_ReturnNotFound_NotFound()
    {
        // Arrange
        var id = 1;

        // Act
        var response = await Fixture.HttpClient.GetAsync(ApiRoutes.Todo.GetById(id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //[Fact]
    //[ResetDatabase]
    //[Trait("Category", "Functional")]
    //public async Task SearchTodo_Should_ReturnTodos_Ok()
    //{
    //    // Arrange
    //    var firstTodo = await Fixture.Extension.AddDefaultTodo(title: "titleOne");
    //    var secondTodo = await Fixture.Extension.AddDefaultTodo(title: "titleTwo", createdBy: 5);

    //    // Act
    //    var response = await Fixture.HttpClient.GetAsync(ApiRoutes.Todo.Search(title: [firstTodo.Title, secondTodo.Title], createdBy: secondTodo.CreatedBy));

    //    // Assert
    //    response.StatusCode.Should().Be(HttpStatusCode.OK);
    //    var responseModel = await response.Content.ReadFromJsonAsync<IEnumerable<TodoResponse>>();
    //    responseModel.Should().NotBeNull();
    //    responseModel!.Should().HaveCount(1);
    //    responseModel!.First().Id.Should().Be(secondTodo.Id);
    //    responseModel!.First().Title.Should().Be(secondTodo.Title);
    //    responseModel!.First().Description.Should().Be(secondTodo.Description);
    //    responseModel!.First().CreatedBy.Should().Be(secondTodo.CreatedBy);
    //    responseModel!.First().AssignedTo.Should().Be(secondTodo.AssignedTo);
    //    responseModel!.First().TargetDate.Should().Be(secondTodo.TargetDate);
    //    responseModel!.First().IsComplete.Should().Be(secondTodo.IsComplete);
    //}
}
