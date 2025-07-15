# GoodMaster Architecture Overview

This document describes the code structure, data flow, and design principles of the GoodMaster Blazor WebAssembly project.

---

## Project Layers & Directory Responsibilities

- **Components/**: Reusable Blazor UI components (e.g., debt cards, modals, charts, summaries).
- **Layout/**: Layout components for consistent page structure (e.g., navigation, main layout).
- **Models/**: Data models (e.g., `Debt`, `Payment`) used throughout the app.
- **Pages/**: Top-level Blazor pages (e.g., dashboard, not found) that compose components and manage page-level state.
- **Properties/**: Project and launch settings.
- **Services/**: Application services (e.g., `LocalStorageService`) for cross-cutting concerns and browser interop.
- **Utils/**: Utility and calculation logic (e.g., `DebtCalculations.cs` for all payoff and financial math).
- **Tests/**: Unit tests for backend logic, organized by feature or utility.
- **wwwroot/**: Static assets (CSS, images, icons, etc.).

---

## Data Flow & State Management

- **State is managed at the page/component level** using Blazor’s built-in mechanisms (`@code` blocks, parameters, cascading values).
- **LocalStorageService** is used for persisting user data (debts, settings) in the browser’s local storage.
- **All business logic and calculations** (e.g., payoff projections, minimum payment logic) are centralized in `Utils/DebtCalculations.cs` for maintainability and testability.
- **No backend API**: All logic runs client-side in the browser; there is no server-side database or REST API.

---

## Key Design Principles

- **Separation of Concerns**: UI, business logic, and data models are kept in separate directories/files.
- **Reusability**: Components are designed to be reusable and composable.
- **Testability**: All core logic is covered by unit tests in the `Tests/` directory.
- **Extensibility**: New features (e.g., additional payoff strategies, new UI components) can be added with minimal changes to existing code.
- **Accessibility**: UI components are built with accessibility in mind (ARIA roles, keyboard navigation, color contrast).
- **Performance**: Heavy calculations are optimized and memoized where appropriate; only necessary data is stored in local storage.

---

## Extending the Project

- **To add a new feature**: Create new components in `Components/`, update or add models in `Models/`, and add any new business logic to `Utils/`.
- **To add a new page**: Add a new `.razor` file in `Pages/` and register the route.
- **To persist new data**: Extend `LocalStorageService` or add new services in `Services/`.
- **To add tests**: Place new test files in `Tests/`, following the existing structure and naming conventions.

---

For questions or contributions, see the main `README.md` and open an issue or pull request. 