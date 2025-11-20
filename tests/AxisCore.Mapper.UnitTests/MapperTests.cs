using FluentAssertions;
using Xunit;

namespace AxisCore.Mapper.UnitTests;

public class MapperTests
{
    private readonly IMapper _mapper;

    public MapperTests()
    {
        _mapper = new Mapper();
    }

    [Fact]
    public void Map_IntToString_ShouldConvertSuccessfully()
    {
        // Arrange
        var source = new { Age = 25 };

        // Act
        var result = _mapper.Map<DestinationWithString>(source);

        // Assert
        result.Age.Should().Be("25");
    }

    [Fact]
    public void Map_StringToInt_ShouldConvertSuccessfully()
    {
        // Arrange
        var source = new { Age = "30" };

        // Act
        var result = _mapper.Map<DestinationWithInt>(source);

        // Assert
        result.Age.Should().Be(30);
    }

    [Fact]
    public void Map_InvalidStringToInt_ShouldThrowException()
    {
        // Arrange
        var source = new { Age = "hello" };

        // Act
        Action act = () => _mapper.Map<DestinationWithInt>(source);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot convert 'hello' to int*");
    }

    [Fact]
    public void Map_NullableTypes_ShouldHandleCorrectly()
    {
        // Arrange
        var source = new SourceWithNullable { Age = 25, Name = null };

        // Act
        var result = _mapper.Map<SourceWithNullable, DestinationWithNullable>(source);

        // Assert
        result.Age.Should().Be(25);
        result.Name.Should().BeNull();
    }

    [Fact]
    public void Map_ListOfInts_ShouldMapToListOfStrings()
    {
        // Arrange
        var source = new { Numbers = new List<int> { 1, 2, 3, 4, 5 } };

        // Act
        var result = _mapper.Map<DestinationWithStringList>(source);

        // Assert
        result.Numbers.Should().BeEquivalentTo(new List<string> { "1", "2", "3", "4", "5" });
    }

    [Fact]
    public void Map_ArrayOfInts_ShouldMapToArrayOfStrings()
    {
        // Arrange
        var source = new { Numbers = new[] { 1, 2, 3, 4, 5 } };

        // Act
        var result = _mapper.Map<DestinationWithStringArray>(source);

        // Assert
        result.Numbers.Should().BeEquivalentTo(new[] { "1", "2", "3", "4", "5" });
    }

    [Fact]
    public void Map_Dictionary_ShouldMapWithTypeConversion()
    {
        // Arrange
        var source = new { Data = new Dictionary<string, int> { ["one"] = 1, ["two"] = 2 } };

        // Act
        var result = _mapper.Map<DestinationWithDictionary>(source);

        // Assert
        result.Data.Should().ContainKey("one").WhoseValue.Should().Be("1");
        result.Data.Should().ContainKey("two").WhoseValue.Should().Be("2");
    }

    [Fact]
    public void Map_ComplexObject_ShouldMapAllProperties()
    {
        // Arrange
        var source = new PersonSource
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            Email = "john@example.com"
        };

        // Act
        var result = _mapper.Map<PersonSource, PersonDestination>(source);

        // Assert
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Age.Should().Be("30"); // int to string
        result.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void Map_ComplexObjectWithCollections_ShouldMapCorrectly()
    {
        // Arrange
        var source = new OrderSource
        {
            OrderId = 123,
            Items = new List<OrderItemSource>
            {
                new() { ProductId = 1, Quantity = 2, Price = 10.50m },
                new() { ProductId = 2, Quantity = 1, Price = 25.00m }
            }
        };

        // Act
        var result = _mapper.Map<OrderSource, OrderDestination>(source);

        // Assert
        result.OrderId.Should().Be("123"); // int to string
        result.Items.Should().HaveCount(2);
        result.Items[0].ProductId.Should().Be("1");
        result.Items[0].Quantity.Should().Be(2);
        result.Items[1].ProductId.Should().Be("2");
        result.Items[1].Quantity.Should().Be(1);
    }

    [Fact]
    public void Map_ToExistingInstance_ShouldUpdateProperties()
    {
        // Arrange
        var source = new { Name = "Jane", Age = 28 };
        var destination = new DestinationWithString { Name = "Old Name", Age = "0" };

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.Name.Should().Be("Jane");
        destination.Age.Should().Be("28");
    }

    [Fact]
    public void Map_CaseInsensitivePropertyMatching_ShouldWork()
    {
        // Arrange
        var source = new { firstname = "Alice", LASTNAME = "Smith" };

        // Act
        var result = _mapper.Map<PersonDestination>(source);

        // Assert
        result.FirstName.Should().Be("Alice");
        result.LastName.Should().Be("Smith");
    }

    [Fact]
    public void Map_WithCustomMapping_ShouldUseCustomLogic()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<PersonSource, PersonDestination>(source => new PersonDestination
        {
            FirstName = source.FirstName?.ToUpper(),
            LastName = source.LastName?.ToUpper(),
            Age = $"Age: {source.Age}",
            Email = source.Email
        });

        var mapper = new Mapper(config);
        var source = new PersonSource { FirstName = "john", LastName = "doe", Age = 30 };

        // Act
        var result = mapper.Map<PersonSource, PersonDestination>(source);

        // Assert
        result.FirstName.Should().Be("JOHN");
        result.LastName.Should().Be("DOE");
        result.Age.Should().Be("Age: 30");
    }

    [Fact]
    public void Map_SameType_ShouldReturnSameInstance()
    {
        // Arrange
        var source = new PersonSource { FirstName = "John", LastName = "Doe", Age = 30 };

        // Act
        var result = _mapper.Map<PersonSource, PersonSource>(source);

        // Assert
        result.Should().BeSameAs(source);
    }

    [Fact]
    public void Map_DoubleToString_ShouldConvert()
    {
        // Arrange
        var source = new { Value = 123.45 };

        // Act
        var result = _mapper.Map<DestinationWithString>(source);

        // Assert
        result.Value.Should().Be("123.45");
    }

    [Fact]
    public void Map_StringToDouble_ShouldConvert()
    {
        // Arrange
        var source = new { Value = "123.45" };

        // Act
        var result = _mapper.Map<DestinationWithDouble>(source);

        // Assert
        result.Value.Should().Be(123.45);
    }

    [Fact]
    public void Map_BoolToString_ShouldConvert()
    {
        // Arrange
        var source = new { IsActive = true };

        // Act
        var result = _mapper.Map<DestinationWithString>(source);

        // Assert
        result.IsActive.Should().Be("True");
    }

    [Fact]
    public void Map_StringToBool_ShouldConvert()
    {
        // Arrange
        var source = new { IsActive = "true" };

        // Act
        var result = _mapper.Map<DestinationWithBool>(source);

        // Assert
        result.IsActive.Should().BeTrue();
    }

    // Test classes
    private class DestinationWithString
    {
        public string? Name { get; set; }
        public string? Age { get; set; }
        public string? Value { get; set; }
        public string? IsActive { get; set; }
    }

    private class DestinationWithInt
    {
        public int Age { get; set; }
    }

    private class DestinationWithDouble
    {
        public double Value { get; set; }
    }

    private class DestinationWithBool
    {
        public bool IsActive { get; set; }
    }

    private class SourceWithNullable
    {
        public int? Age { get; set; }
        public string? Name { get; set; }
    }

    private class DestinationWithNullable
    {
        public int? Age { get; set; }
        public string? Name { get; set; }
    }

    private class DestinationWithStringList
    {
        public List<string> Numbers { get; set; } = new();
    }

    private class DestinationWithStringArray
    {
        public string[] Numbers { get; set; } = Array.Empty<string>();
    }

    private class DestinationWithDictionary
    {
        public Dictionary<string, string> Data { get; set; } = new();
    }

    private class PersonSource
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int Age { get; set; }
        public string? Email { get; set; }
    }

    private class PersonDestination
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Age { get; set; }
        public string? Email { get; set; }
    }

    private class OrderSource
    {
        public int OrderId { get; set; }
        public List<OrderItemSource> Items { get; set; } = new();
    }

    private class OrderDestination
    {
        public string? OrderId { get; set; }
        public List<OrderItemDestination> Items { get; set; } = new();
    }

    private class OrderItemSource
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    private class OrderItemDestination
    {
        public string? ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
