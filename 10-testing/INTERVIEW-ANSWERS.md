# 10 – Testing: Model Interview Answers

> Reference answers to learn from. Read them, then answer in your own words.

---

## Unit tests and mocking 🔥

**Question:** "How do you unit test a service with dependencies? What do you mock?"

### Short answer

> "I inject dependencies through interfaces, so in a test I pass test doubles,
> using Moq or NSubstitute. I mock things that are **slow, non-deterministic or
> external**: payment gateways, email, external HTTP APIs, the clock (`TimeProvider`).
> Third-party SDKs are wrapped behind **my own interface** (e.g. `IPaymentGateway`)
> and I mock that interface, not the SDK's classes ('don't mock what you don't own').
> I don't mock `DbContext`; for data access I use integration tests with a real
> database. Tests check **behaviour** (results, calls to boundaries), not private
> implementation details. Structure: Arrange / Act / Assert."

```csharp
var gateway = new Mock<IPaymentGateway>();
gateway.Setup(g => g.ChargeAsync(100m)).ReturnsAsync(true);
var sut = new PaymentService(gateway.Object);
await sut.PayAsync(100m);
gateway.Verify(g => g.ChargeAsync(100m), Times.Once);
```

### xUnit

| xUnit | JUnit 5 | PHPUnit |
|---|---|---|
| `[Fact]` | `@Test` | `test...()` |
| `[Theory]` + `[InlineData]` / `[MemberData]` | `@ParameterizedTest` | `@dataProvider` |
| constructor | `@BeforeEach` | `setUp()` |
| `IDisposable.Dispose()` / `IAsyncLifetime` | `@AfterEach` | `tearDown()` |
| `IClassFixture<T>` | `@BeforeAll` (shared setup) | `setUpBeforeClass()` |

A new test-class instance is created **per test**, so tests don't share state.
Tests in the same class run sequentially; different classes run in parallel by default.

---

## Integration tests: `WebApplicationFactory` + Testcontainers 🔥

**Question:** "How do you write integration tests for an ASP.NET Core API?"

### Short answer

> "I use `WebApplicationFactory<Program>`, which boots the real app in memory:
> the real `Program.cs`, DI, middleware, routing, auth and serialization. It gives
> me an `HttpClient` to send real HTTP requests without a network port. I can
> override services in `ConfigureTestServices`, e.g. replace external APIs with
> fakes or add a test authentication handler.
> For the database, I use **Testcontainers** to start the same engine as production
> in Docker, apply migrations, and reset data between tests (e.g. with Respawn).
> I avoid the EF InMemory provider: it isn't relational, so it misses SQL
> translation errors, constraints, transactions and concurrency behaviour."

### What each test level catches

- Unit tests: business logic, fast feedback.
- Integration tests: wiring. DI registrations, middleware order, routing, `[Authorize]`
  (e.g. 401 without a token), JSON shape, real SQL.
  A 401 test must be an integration test, because authentication middleware doesn't run in a unit test.

### Comparisons

Spring `@SpringBootTest` + `MockMvc` / `TestRestTemplate` + Testcontainers.
Laravel feature tests (`$this->getJson(...)->assertStatus(401)`) + `RefreshDatabase`.
