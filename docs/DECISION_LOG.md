# DECISION LOG
**Volue SmartPulse Forecast Service - Technical Decisions**

**Project:** Forecast Service Microservice  
**Author:** Neslihan Korkmaz  
**Date:** January 2026

---

## Technology Choices & Rationale

### 1. **Runtime: .NET 9**
**Decision:** Use .NET 9 as the primary runtime platform.

**Rationale:**
- **Latest LTS Release:** .NET 9 provides the most recent stable features and performance improvements
- **Performance:** Native AOT compilation support, improved garbage collection
- **Cross-Platform:** Runs on Windows, Linux, macOS - essential for containerization
- **Enterprise Support:** Microsoft's long-term support ensures production stability
- **Ecosystem:** Rich library ecosystem for microservices (ASP.NET Core, EF Core, etc.)

**Alternatives Considered:**
- .NET 8: Stable but lacks newest performance optimizations
- Node.js: Good for real-time but weaker type safety
- Java Spring Boot: Excellent but heavier resource footprint

---

### 2. **Database: PostgreSQL 16**
**Decision:** Use PostgreSQL as the primary data store.

**Rationale:**
- **UPSERT Support:** Native `ON CONFLICT DO UPDATE` clause perfect for forecast updates
- **Array Operations:** Bulk operations with `unnest()` for high-performance batch inserts
- **Open Source:** No licensing costs, production-proven
- **ACID Compliance:** Critical for financial/trading data integrity
- **JSON Support:** Flexible schema evolution if needed
- **Time-Series Optimization:** Excellent for hourly forecast data with proper indexing

**Alternatives Considered:**
- SQL Server: More expensive, Windows-centric
- MySQL: Weaker support for advanced SQL features
- MongoDB: Not ideal for transactional consistency requirements

**Key Implementation:**
```sql
-- Bulk UPSERT with xmax inspection for insert/update detection
INSERT INTO forecasts (...) 
VALUES (unnest(@hours), unnest(@values))
ON CONFLICT (plant_id, hour_utc) DO UPDATE ...
RETURNING (xmax = 0) AS was_inserted
```

---

### 3. **Architecture Pattern: Clean Architecture + CQRS**
**Decision:** Implement Clean Architecture with CQRS repository separation.

**Rationale:**
- **Testability:** Each layer can be tested independently
- **Maintainability:** Clear separation of concerns (API → Services → Repositories)
- **Scalability:** CQRS allows independent optimization of reads vs writes
- **Flexibility:** Easy to swap infrastructure components without touching business logic

**Layer Responsibilities:**
- **API Layer:** HTTP concerns, validation, routing
- **Services Layer:** Business rules, validation logic, orchestration
- **Repositories Layer:** Data access, PostgreSQL-specific optimizations
- **Contracts Layer:** Shared DTOs, interfaces (Dependency Inversion)

**CQRS Implementation:**
- `IForecastWriteRepository`: Optimized for bulk UPSERT operations
- `IForecastReadRepository`: Optimized queries with `AsNoTracking`
- `IPositionReadRepository`: Aggregation-specific queries with `GROUP BY`

---

### 4. **Messaging: RabbitMQ**
**Decision:** Use RabbitMQ for event publishing.

**Rationale:**
- **Simplicity:** Easy to set up and operate for assessment scope
- **Reliability:** Industry-proven message broker
- **Topic Exchange:** Perfect for routing `position.changed` events to multiple consumers
- **Management UI:** Built-in monitoring and debugging tools
- **Docker Support:** Official Alpine images for lightweight containers

**Implementation Strategy:**
- **Graceful Degradation:** Service continues if RabbitMQ unavailable
- **Fire-and-Forget:** Event publishing doesn't block main flow
- **Null Object Pattern:** `NullEventPublisher` when messaging disabled

**Alternatives Considered:**
- Azure Service Bus: Cloud-dependent, assessment requires local deployment
- Apache Kafka: Overkill for simple pub/sub requirements
- Redis Pub/Sub: Less reliable delivery guarantees

---

### 5. **ORM: Entity Framework Core 9**
**Decision:** Use EF Core for data access.

**Rationale:**
- **Code-First Migrations:** Version-controlled database schema
- **LINQ Queries:** Type-safe, compile-time checked queries
- **Change Tracking:** Automatic audit fields (created_at, updated_at)
- **Relationship Management:** Automatic FK constraints and navigation properties

**Performance Optimizations:**
- Used `AsNoTracking()` for read-only queries
- Implemented raw SQL for bulk UPSERT (EF Core limitation)
- Proper indexing strategy with Fluent API configurations

---

### 6. **Error Handling: Result<T> Pattern**
**Decision:** Use Result<T> pattern instead of exception-based control flow.

**Rationale:**
- **Type Safety:** Compiler enforces error handling
- **Performance:** No exception unwinding overhead
- **Explicit Flow:** Clear distinction between success and failure paths
- **Railway-Oriented Programming:** Composable error handling

**Implementation:**
```csharp
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T Value { get; init; }
    public Error Error { get; init; }
}
```

---

### 7. **Containerization: Docker + Multi-Stage Builds**
**Decision:** Use Docker with multi-stage builds and Alpine base images.

**Rationale:**
- **Size Optimization:** Final image ~100MB (Alpine + ASP.NET runtime only)
- **Security:** No build tools in production image
- **Reproducibility:** Consistent environments across dev/test/prod
- **Assessment Requirement:** Docker deployment explicitly required

**Multi-Stage Strategy:**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
# ... build steps ...
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
# ... runtime-only image ...
```

---

### 8. **Observability: Serilog + Correlation IDs**
**Decision:** Implement structured logging with Serilog and distributed tracing.

**Rationale:**
- **Structured Logs:** JSON output for log aggregation systems
- **Correlation IDs:** Track requests across service boundaries
- **Performance:** Efficient async logging with batching
- **Production Ready:** File rotation, console output for containers

**Key Features:**
- `X-Correlation-ID` header extraction/generation
- Log context enrichment (ThreadId, CorrelationId)
- Automatic HTTP request logging
- Multiple sinks (Console + File)

---

### 9. **Validation Strategy: Multi-Layer**
**Decision:** Implement validation at multiple layers.

**Rationale:**
- **Defense in Depth:** Catch errors early, fail fast
- **Business Rules:** Hour-aligned UTC timestamps, non-negative MWh
- **Database Constraints:** UNIQUE constraint on (plant_id, hour_utc)
- **Type Safety:** Non-nullable reference types, strong typing

**Validation Layers:**
1. **API Layer:** ASP.NET Core model binding
2. **Service Layer:** Business rule validation (hour alignment, date ranges)
3. **Database Layer:** UNIQUE constraints, CHECK constraints

---

### 10. **Testing Strategy: Pyramid with Testcontainers**
**Decision:** Use Testcontainers for integration tests.

**Rationale:**
- **Real Database:** Test against actual PostgreSQL, not mocks
- **Isolation:** Each test gets clean database state
- **CI/CD Ready:** Works in containerized build environments
- **Confidence:** Catch integration issues before deployment

**Test Coverage:**
- 15 Unit Tests (business logic, validation)
- 19 Integration Tests (API endpoints, database operations)
- Total: 34 tests with 85%+ coverage

---

## Key Design Principles Applied

1. **SOLID Principles:**
   - Single Responsibility (each class has one reason to change)
   - Open/Closed (extensible without modification)
   - Liskov Substitution (interfaces properly abstracted)
   - Interface Segregation (focused interfaces)
   - Dependency Inversion (depend on abstractions)

2. **12-Factor App:**
   - Configuration via environment variables
   - Stateless processes
   - Port binding for HTTP services
   - Disposable processes (fast startup/shutdown)
   - Dev/prod parity (Docker ensures consistency)

3. **Microservice Patterns:**
   - Database per service (dedicated PostgreSQL schema)
   - Health check API (liveness, readiness)
   - Event-driven communication (optional RabbitMQ)
   - Graceful degradation (service works without messaging)

---

## Trade-offs & Future Considerations

### What We Optimized For:
- **Simplicity:** Easy to understand and maintain
- **Performance:** Bulk operations, proper indexing
- **Reliability:** Graceful degradation, health checks
- **Testability:** Clean architecture enables comprehensive testing

### What We Sacrificed:
- **Caching:** Not implemented (YAGNI for assessment scope)
- **Authentication:** Not required for assessment
- **Rate Limiting:** Not critical for internal service
- **Advanced Monitoring:** Basic health checks sufficient

### Future Enhancements:
- Add Redis for caching frequent queries
- Implement JWT authentication
- Add Prometheus metrics export
- Deploy to Kubernetes with Helm charts

---

**Document Version:** 1.0  
 
