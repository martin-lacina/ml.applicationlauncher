---
applyTo: '**/*.cs'
---
# Instructions for AI Copilot

## Purpose
This document provides instructions for AI Copilot to assist in generating and improving C# code. It includes guidelines on coding standards, domain knowledge, and preferences that should be followed.

## Coding Standards
1. **Naming Conventions**:
   - Use PascalCase for class names, method names, and properties.
   - Use camelCase for local variables and method parameters.
   - Use underscores for private fields (e.g., `_privateField`).
   - Use meaningful names that clearly describe the purpose of the variable or method.

2. **Code Structure**:
    - Do not use regions (e.g., `#region Properties`, `#region Methods`).
    - Keep methods short and focused on a single task.
    - Use comments to explain complex logic or important decisions.
    - Use XML documentation comments for public methods and classes to provide clear API documentation.
    - Maintain a consistent indentation style (4 spaces per indentation level).
    - Use blank lines to separate logical sections of code for better readability.
    - Avoid deeply nested code; refactor into smaller methods if necessary.
    - Use `var` for local variable declarations when the type is obvious from the right-hand side (e.g., `var list = new List<int>();`).
fields in classes.
    - Use `readonly` for fields that should not be modified after initialization.
    - Use properties instead of public fields to encapsulate data and provide validation if necessary.
    - Use `async` and `await` for asynchronous methods to improve responsiveness and performance.
    - Put constructors at the top of the class, followed by properties and methods.
    - Put public members before internal and internal members before private members in classes.

3. **Code Comments**:
    - Use inline comments sparingly and only when the code is not self-explanatory.
    - Use XML comments for public APIs to provide documentation that can be used by tools like IntelliSense.
    - Avoid commenting out code; instead, remove it or use version control to track changes.

4. **Error Handling**:
    - Use exceptions for error handling instead of return codes.
    - Catch specific exceptions rather than using a general `catch` block.
    - Use `try-catch-finally` blocks where necessary, ensuring that resources are properly disposed of in the `finally` block if applicable.
    - Log exceptions with sufficient context to aid in debugging.
    - Propage OperationCanceledException for cancellation requests in asynchronous methods.

5. **Performance Considerations**:
    - Avoid unnecessary allocations and use `StringBuilder` for string concatenation in loops.
    - Use `foreach` for iterating over collections unless performance profiling indicates otherwise.
    - Use `IEnumerable<T>` and `IQueryable<T>` appropriately to defer execution until necessary.
    - Be cautious with LINQ queries that may lead to performance issues, especially with large datasets.
    - Prefer using IAsyncEnumerable<T> and IAsyncQueryable<T> for asynchronous data processing.

6. **Unit Testing**:
    - Write unit tests for all public methods and critical private methods.
    - Use a testing framework NUnit.
    - Follow the Arrange-Act-Assert (AAA) pattern for structuring tests.
    - Use mocking framework Moq to isolate dependencies in unit tests.
    - Ensure tests are independent and can run in any order.

7. **Version Control**:
    - Use meaningful commit messages that describe the changes made.
    - Commit code frequently to avoid large, unwieldy commits.
    - Use branches for new features or bug fixes, merging back to the main branch when complete.
    - Avoid committing sensitive information such as API keys or passwords.

## Domain Knowledge
1. **C# Language Features**:
   - Familiarity with C# 10.0 and later features, including records, pattern matching, and nullable reference types.
   - Understanding of LINQ (Language Integrated Query) for querying collections.
   - Knowledge of asynchronous programming patterns using `async` and `await`.
