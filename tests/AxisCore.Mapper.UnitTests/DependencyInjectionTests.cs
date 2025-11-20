using AxisCore.Mapper.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AxisCore.Mapper.UnitTests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddMapper_ShouldRegisterMapperAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMapper();
        var provider = services.BuildServiceProvider();

        // Assert
        var mapper1 = provider.GetRequiredService<IMapper>();
        var mapper2 = provider.GetRequiredService<IMapper>();
        mapper1.Should().BeSameAs(mapper2);
    }

    [Fact]
    public void AddMapper_WithConfiguration_ShouldUseConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMapper(config =>
        {
            config.ThrowOnMappingFailure = false;
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var configuration = provider.GetRequiredService<MapperConfiguration>();
        configuration.ThrowOnMappingFailure.Should().BeFalse();
    }

    [Fact]
    public void AddMapper_WithCustomLifetime_ShouldRespectLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMapper(lifetime: ServiceLifetime.Transient);
        var provider = services.BuildServiceProvider();

        // Assert
        var mapper1 = provider.GetRequiredService<IMapper>();
        var mapper2 = provider.GetRequiredService<IMapper>();
        mapper1.Should().NotBeSameAs(mapper2);
    }

    [Fact]
    public void AddMapper_ShouldAllowMappingInDI()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMapper();
        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        var source = new { Name = "Test", Age = 25 };

        // Act
        var result = mapper.Map<TestDestination>(source);

        // Assert
        result.Name.Should().Be("Test");
        result.Age.Should().Be("25");
    }

    private class TestDestination
    {
        public string? Name { get; set; }
        public string? Age { get; set; }
    }
}
