# GoodMaster Backend Tests

This directory contains unit and integration tests for the GoodMaster Blazor/C# backend.

## Prerequisites

- [.NET 7 SDK or later](https://dotnet.microsoft.com/download)
- Visual Studio 2022+ (Enterprise recommended) or VS Code with C# extension

## Running Tests

### In Visual Studio
1. Open the solution in Visual Studio.
2. Build the solution to restore dependencies.
3. Open the **Test Explorer** (`Test > Test Explorer`).
4. Click **Run All** to execute all tests.
5. View results and debug failing tests directly from the Test Explorer.

### Using .NET CLI

From the root of the solution (where the `.sln` file is):

```sh
dotnet test
```

This will build the test project and run all tests, outputting results to the console.

## Writing Tests

- Place all test files in this `Tests/` directory or subfolders by feature (e.g., `Tests/Utils/`, `Tests/Services/`).
- Use [xUnit](https://xunit.net/) for all test classes and methods.
- Name test classes after the class being tested, suffixed with `Tests` (e.g., `DebtCalculationsTests`).
- Use descriptive method names: `MethodName_Scenario_ExpectedResult`.
- Cover edge cases (e.g., zero balance, negative interest, minimum payment too low).
- Use mocks for dependencies (e.g., services, repositories) with [Moq](https://github.com/moq/moq4) or similar.

## Best Practices

- Ensure each test is independent and does not rely on shared state.
- Use `[Fact]` for single-case tests, `[Theory]` with `[InlineData]` for parameterized tests.
- Prefer **Arrange-Act-Assert** structure in each test method.
- Add XML documentation to public methods in test classes for clarity.
- Run all tests before submitting a pull request.
- Aim for high coverage of all calculation logic, especially in `Utils/DebtCalculations.cs`.

## Extending Test Coverage

- Add new test files for new features or bug fixes.
- Update existing tests if calculation logic changes.
- For integration tests, use in-memory databases or test doubles.

## Troubleshooting

- If tests are not discovered, ensure the test project references `xunit` and is included in the solution.
- For CLI issues, try `dotnet restore` then `dotnet test` again.

---

For questions or issues, contact the project maintainer or open an issue in the repository. 