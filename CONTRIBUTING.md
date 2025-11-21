# Contributing to AxisCore.Mediator

Thank you for your interest in contributing to AxisCore.Mediator! This document provides guidelines and instructions for contributing.

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment for all contributors.

## How to Contribute

### Reporting Bugs

Before creating bug reports, please check existing issues to avoid duplicates. When creating a bug report, include:

- A clear, descriptive title
- Detailed steps to reproduce the issue
- Expected vs actual behavior
- Code samples (if applicable)
- Environment details (.NET version, OS, etc.)

### Suggesting Enhancements

Enhancement suggestions are welcome! Please:

- Use a clear, descriptive title
- Provide a detailed description of the proposed enhancement
- Explain why this enhancement would be useful
- Include code examples if applicable

### Pull Requests

1. **Fork the repository** and create your branch from `main`
2. **Follow the coding standards** outlined below
3. **Write tests** for your changes
4. **Update documentation** as needed
5. **Ensure all tests pass**
6. **Submit a pull request**

## Development Setup

### Prerequisites

- .NET SDK 6.0 or later
- Git
- Your favorite IDE (Visual Studio, Rider, or VS Code)

### Getting Started

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/AxisCore.git
cd AxisCore

# Build the solution
dotnet build

# Run tests
dotnet test

# Run benchmarks
dotnet run --project benchmarks/AxisCore.Mediator.Benchmarks -c Release
```

## Coding Standards

### General Guidelines

- Follow C# coding conventions
- Use meaningful variable and method names
- Keep methods focused and concise
- Write XML documentation comments for public APIs
- Use nullable reference types appropriately

### Code Style

```csharp
// ✅ Good
public sealed class MyHandler : IRequestHandler<MyRequest, MyResponse>
{
    private readonly ILogger<MyHandler> _logger;

    public MyHandler(ILogger<MyHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MyResponse> Handle(
        MyRequest request,
        CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### Performance Considerations

- Use `Task<T>` for async operations
- Avoid unnecessary allocations
- Use `ConfigureAwait(false)` in library code
- Cache expensive operations
- Use `AggressiveInlining` for hot path methods

### Testing

- Write unit tests for all new functionality
- Maintain or improve code coverage
- Use descriptive test names: `Method_Scenario_ExpectedBehavior`
- Follow AAA pattern (Arrange, Act, Assert)

Example:
```csharp
[Fact]
public async Task Send_WithValidRequest_ReturnsResponse()
{
    // Arrange
    var mediator = CreateMediator();
    var request = new TestRequest { Value = "test" };

    // Act
    var result = await mediator.Send(request);

    // Assert
    result.Should().Be("Expected");
}
```

## Project Structure

```
AxisCore/
├── src/
│   └── AxisCore.Mediator/          # Main library
├── tests/
│   ├── AxisCore.Mediator.Tests/           # Unit tests
│   └── AxisCore.Mediator.IntegrationTests/ # Integration tests
├── benchmarks/
│   └── AxisCore.Mediator.Benchmarks/      # Performance benchmarks
├── samples/
│   └── BasicUsage/                        # Sample applications
└── docs/                                  # Documentation
```

## Commit Messages

Use clear, descriptive commit messages:

```
Add support for custom handler factories

- Implement IHandlerFactory interface
- Add factory registration extension methods
- Update documentation
- Add unit tests
```

Format:
- Use imperative mood ("Add feature" not "Added feature")
- First line: brief summary (50 chars or less)
- Blank line
- Detailed description if needed

## Pull Request Process

1. **Update the README.md** with details of changes if applicable
2. **Update documentation** in the docs/ folder
3. **Add tests** to cover your changes
4. **Ensure CI passes** - all tests and builds must succeed
5. **Request review** from maintainers
6. **Address feedback** promptly

### PR Checklist

- [ ] Code follows project style guidelines
- [ ] Self-review of code completed
- [ ] Comments added for complex logic
- [ ] Documentation updated
- [ ] Tests added/updated
- [ ] All tests pass locally
- [ ] No new warnings introduced
- [ ] Benchmarks run (for performance changes)

## Areas for Contribution

Looking for ways to contribute? Consider:

### High Priority
- Performance optimizations
- Additional built-in behaviors
- Documentation improvements
- Bug fixes

### Medium Priority
- Additional examples
- Integration with other frameworks
- Tooling improvements

### Ideas Welcome
- New features (discuss first via issues)
- API improvements (discuss first)

## Performance Testing

When making performance-related changes:

1. **Run benchmarks before** your changes
2. **Run benchmarks after** your changes
3. **Compare results** and include in PR description
4. **Explain any regressions**

```bash
dotnet run --project benchmarks/AxisCore.Mediator.Benchmarks -c Release
```

## Documentation

### XML Documentation

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Sends a request to a single handler.
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
/// <param name="request">Request object</param>
/// <param name="cancellationToken">Optional cancellation token</param>
/// <returns>A task representing the send operation</returns>
public Task<TResponse> Send<TResponse>(
    IRequest<TResponse> request,
    CancellationToken cancellationToken = default);
```

### Markdown Documentation

Update relevant .md files in docs/ when:
- Adding new features
- Changing existing behavior
- Adding examples

## Security

If you discover a security vulnerability:
1. **Do NOT** create a public issue
2. Email security details to [security contact - to be determined]
3. Wait for response before public disclosure

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

## Questions?

- Open a discussion on GitHub
- Check existing issues and discussions
- Review documentation

## Recognition

Contributors will be:
- Listed in release notes
- Credited in the README (for significant contributions)
- Thanked in commit messages

Thank you for contributing to AxisCore.Mediator!
