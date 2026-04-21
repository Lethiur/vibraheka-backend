# VibraHeka Backend — Instrucciones globales del proyecto

Este fichero es la fuente de verdad de todas las reglas generales del repositorio.
Todos los agentes (`CSharpExpert`, `CSharpQAExpert`, `ProductOwner`) deben seguirlas sin excepción.

---

## 1. Stack tecnológico

- **.NET 8+ / C# 12+**
- **ASP.NET Core** (Web API)
- **MediatR** (CQRS — Commands/Queries con `IRequest<TResponse>` + `IRequestHandler<TRequest, TResponse>`)
- **FluentValidation** (validación de entrada — `AbstractValidator<T>`)
- **NUnit** (tests — `[TestFixture]`, `[SetUp]`, `[Test]`, `[TestCase]`, `[DisplayName]`)
- **Moq** (mocking — `Mock<T>`, `.Setup()`, `.Verify()`, `.VerifyNoOtherCalls()`)
- **FluentValidation.TestHelper** (`TestValidate`, `ShouldHaveValidationErrorFor`)
- **Bogus** (datos ficticios en tests)
- **Serilog** (logging estructurado)
- **AWS** (DynamoDB via `IDynamoDBContext`, S3 via `IAmazonS3`, Cognito) — solo en Infrastructure
- **Mapperly** (`[Mapper]` + clases `partial`) — solo en Infrastructure para domain↔DTO

---

## 2. Arquitectura de capas (Clean Architecture)

```
src/
  Domain/           # Entidades, value objects, interfaces de repositorio (Ports/Out/), errores de dominio
  Application/      # Commands, Queries, Handlers (MediatR), validators (FluentValidation), DTOs
  Infrastructure/   # Repositorios, servicios AWS, mappers Mapperly, middleware
  Web/              # Controllers (solo MediatR), DI setup, appsettings
tests/
  Domain.UnitTests/
  Application.UnitTests/
  Application.FunctionalTests/
  Infrastructure.UnitTests/
  Infrastructure.IntegrationTests/   # Requiere AWS
  Web.AcceptanceTests/               # Requiere AWS — WebApplicationFactory
```

### Restricciones de capas
- Sin dependencias de Infrastructure en Domain ni Application.
- Side effects (DB, HTTP, filesystem) **solo** en Infrastructure; nunca en Domain o Application.
- Sin lógica de negocio en Controllers ni en Infrastructure.
- Inyección de dependencias vía constructor; sin service locator en lógica de negocio.

### Clases base de Infrastructure — OBLIGATORIO extender

| Clase base | Cuándo extender | Métodos clave |
|-----------|----------------|---------------|
| `GenericDynamoRepository<T>` | Todo repositorio DynamoDB | `FindByID`, `FindByIdAndRangeKey`, `FindOneByIndex`, `Save`, `Delete`, `GetAll` |
| `GenericS3Repository` | Todo repositorio S3 | `UploadAsync`, `FileExistsAsync`, `GetFileContents`, `StreamToFile`, `GetDownloadPreSignedUrl` |

---

## 3. Patrones obligatorios

### CQRS con MediatR
- Escritura: `Command` + `CommandHandler` + `CommandValidator`
- Lectura: `Query` + `QueryHandler`
- Cada caso de uso en su propia subcarpeta: `Application/<Feature>/Commands/<CasoDeUso>/`
- **Validación vía pipeline de MediatR**: los `AbstractValidator<TRequest>` se ejecutan mediante behavior/pipeline; **no inyectar validators directamente en handlers**.

### Result pattern para errores de dominio
```csharp
// Usar Result<T> para errores de dominio; NO lanzar excepciones de negocio
Result<T>.Success(value)
Result<T>.Failure("PREFIX-NNN")
```

### Naming conventions
- Interfaces de repositorios/servicios: `IFeatureRegistryPort`, `IFeatureStoragePort`
- Implementaciones: `FeatureRepository`, `FeatureStorageRepository`
- Controllers: `FeatureController` (delgado — solo IMediator)
- DTOs: `FeatureDto`, `CreateFeatureRequest`, `FeatureResponse`
- Entidades de dominio: inmutables (`record` o propiedades `init`)
- Handlers: `CreateFeatureCommandHandler`, `GetFeatureQueryHandler`

### Async/await
- `async/await` obligatorio en todos los métodos de I/O.
- **Prohibido** `.Result` o `.Wait()` en cualquier parte del código.

### Nullable reference types
- `<Nullable>enable</Nullable>` habilitado en todos los proyectos.
- Sin `!` injustificados.

---

## 4. Convenciones de tests

> **Patrones detallados, ejemplos de código, checklist y helpers heredados** para el agente `CSharpQAExpert` → `tool/qa-rules.md`.

### Framework y librerías obligatorios
- **NUnit** para todos los tests (no xUnit, no MSTest).
- **Moq** exclusivamente para mocking (no NSubstitute).
- Sin `var`; tipos declarados explícitamente en todos los ficheros de test.

### Alcance de pruebas de validación (OBLIGATORIO)
- Los escenarios de datos inválidos se prueban en los tests del `Validator` y en tests de aceptación.
- En tests de `CommandHandler`/`QueryHandler`, no duplicar tests de validación de datos del `Validator`; enfocarse en lógica de negocio, puertos, side effects y manejo de resultados.
- Si un caso de uso requiere validación, debe existir al menos:
  1. Suite del validator (`<CasoDeUso>CommandValidatorTest.cs` o equivalente).
  2. Cobertura de extremo a extremo en aceptación del rechazo por datos inválidos.

### Formato GivenWhenThen obligatorio
```csharp
[Test]
[DisplayName("Descripción legible del comportamiento")]
public async Task ShouldReturnFailureWhenUserDoesNotExist()
{
    // Given: un userId que no existe en el repositorio

    // When: se ejecuta el handler con el userId

    // Then: se espera un resultado de failure
    Assert.That(result.IsFailure, Is.True,
        $"Expected failure but got success with value: '{(result.IsSuccess ? result.Value : "N/A")}'");
}
```
- Comentarios de sección: `// Given`, `// When`, `// Then` — **nunca** Arrange/Act/Assert.
- Nombres de test: `Should<ExpectedBehavior>[When<Scenario>]`.

### Reglas de Assert con NUnit
- Siempre `Assert.That(actual, constraint, "mensaje descriptivo")`.
- **Prohibido** `Assert.IsTrue/IsFalse/AreEqual` sin mensaje.
- El mensaje debe incluir los valores reales: `$"Expected X but got {actual}"`.
- Para `Result<T>`: expresiones condicionales para evitar excepciones eager (ver ejemplo arriba).

### Verificaciones de Moq
- `It.IsAny<T>()` **solo** en `Setup(...)`, **nunca** en `Verify(...)`.
- `Verify(...)` debe usar `It.Is<T>(predicate)` con valores concretos cuando son conocidos.
- Especificar siempre `Times` explícito (`Times.Once`, `Times.Never`, `Times.Exactly(n)`).
- Llamar `mock.VerifyNoOtherCalls()` al final de **cada** test que instancie mocks.

### Naming en clases de test — PascalCase
```csharp
// ✅ OBLIGATORIO
private Mock<IRecordingStoragePort> StoragePortMock = default!;
private AdminAddRecordingCommandHandler Handler = default!;
```

---

## 5. Proyectos de test y su propósito

| Proyecto | Propósito | Requiere AWS |
|----------|-----------|--------------|
| `Domain.UnitTests` | Entidades y lógica de dominio | No |
| `Application.UnitTests` | Handlers, validators con mocks | No |
| `Application.FunctionalTests` | Tests funcionales de handlers con mocks más completos | No |
| `Infrastructure.UnitTests` | Repositorios y servicios con mocks | No |
| `Infrastructure.IntegrationTests` | Integración real con DynamoDB/S3/Cognito | **Sí** |
| `Web.AcceptanceTests` | Tests E2E del API con `WebApplicationFactory` | **Sí** |

Los tests de integración y aceptación se excluyen del quality gate automático. Se ejecutan manualmente con entorno AWS disponible.

---

## 6. Quality Gate — Ejecución obligatoria al cierre

Solo `CSharpQAExpert` ejecuta el quality gate. Seleccionar script según SO:

| SO | Comando |
|----|---------|
| **Windows (PowerShell)** | `.\tool\quality_gate.ps1` |
| **Linux / macOS (bash)** | `chmod +x tool/quality_gate.sh && ./tool/quality_gate.sh` |

El script realiza automáticamente:
1. `dotnet test` con cobertura (`XPlat Code Coverage`)
2. Generación de reporte HTML con `reportgenerator` en `coverage/report/`
3. Verificación umbral **≥ 80%** de cobertura de línea en Application e Infrastructure
4. `dotnet format --verify-no-changes`

### Regla de formateo preventivo — OBLIGATORIA para agentes técnicos
- Todo cambio de código debe escribirse conforme al estilo de `dotnet format`.
- Antes de delegar o cerrar una fase, el agente técnico debe validar formato (`dotnet format --verify-no-changes`) y corregir con `dotnet format` si falla.
- No dejar deuda de formato para el siguiente agente; el código entregado debe estar listo para pasar la validación de formato del quality gate.

**El veredicto LISTO solo puede emitirse si el script termina con código de salida 0.**
- Si falla por cobertura: añadir tests, re-ejecutar.
- Si falla por formato: `dotnet format`, confirmar con `--verify-no-changes`.
- **No re-ejecutar** si no hubo cambios; reutilizar evidencia previa.

---

## 7. Ciclo de delegación entre agentes

### Flujo normal (feature con implementación)
```
ProductOwner
  └─► CSharpExpert  (implementación — SIN tests)
        └─► CSharpQAExpert  (tests + auditoría + quality gate)
              └─► ProductOwner  (veredicto final)
```

### Flujo directo (petición solo de tests)
```
ProductOwner
  └─► CSharpQAExpert  (tests + quality gate)
        └─► ProductOwner  (veredicto final)
```

**Reglas:**
- `CSharpExpert` **nunca escribe tests**.
- `CSharpQAExpert` es el **único agente que escribe tests** y el único que ejecuta el quality gate.
- `CSharpQAExpert` **no cierra el ticket**; reporta veredicto al `ProductOwner`.
- `ProductOwner` cierra el ticket solo con veredicto **LISTO** del `CSharpQAExpert`.

---

## 8. Convención de errores de dominio

Cada feature tiene `Domain/<Feature>/Errors/<Feature>Errors.cs`. Errores numerados con prefijo:

| Feature | Prefijo | Ejemplo |
|---------|---------|---------|
| Users | `US` | `US-001`, `US-002` |
| Recordings | `REC` | `REC-001`, `REC-002` |
| Subscriptions | `SUB` | `SUB-001`, `SUB-002` |
| Settings | `SET` | `SET-001`, `SET-002` |

```csharp
public static class UserErrors
{
    public const string NotFound      = "US-001";
    public const string AlreadyExists = "US-002";
}
```

- Respetar numeración existente; nunca reutilizar un número ya definido.
- Nuevos errores al final con el siguiente número disponible.
- Nunca crear constantes de error fuera de `Errors/` del módulo.
- Errores transversales de Infrastructure (p.ej. `GenericPersistenceErrors`) viven en `Infrastructure/Exceptions/`.

---

## 9. Mappers en Infrastructure

### Dominio ↔ DTO — Mapperly (obligatorio)
```csharp
// Infrastructure/Mappers/RecordingMapper.cs
[Mapper]
public partial class RecordingMapper
{
    public partial RecordingDto ToDto(RecordingEntity entity);
}
```
- `[Mapper]` + `partial` siempre. Sin lógica manual; conversiones especiales con métodos `partial`.
- **No crear mappers manuales** (sin Mapperly).
- Los mappers Mapperly son auto-generados: **no se crean tests para ellos**.

### Entidades DynamoDB — IDynamoDBContext (anotaciones)
Las entidades que persisten en DynamoDB se mapean mediante las anotaciones del SDK (`[DynamoDBHashKey]`, `[DynamoDBProperty]`, etc.) sobre la clase de entidad. `IDynamoDBContext` gestiona la conversión automáticamente; no se construyen `Dictionary<string, AttributeValue>` manualmente en los repositorios.

---

## 10. Estructura de tests — 1 carpeta por clase, 1 fichero por método

Cada clase bajo test tiene su **propia carpeta**. Dentro de esa carpeta:

1. **Clase base genérica** (`Generic<ClassName>Test.cs`) — setup, campos compartidos, helpers de construcción/seeding.
2. **Un fichero por método público** bajo prueba (`<Método>Test.cs`), cuya clase hereda de la clase base genérica.
3. Si el proyecto tiene `TestBase` o `GenericAcceptanceTest<TApp>`, la clase base genérica **debe** extender de ella.

```
tests/
  Infrastructure.UnitTests/
    Persistence/Repository/DynamoRepositoryTest/
      GenericDynamoRepositoryTest.cs   ← clase base (ya existe)
      FindByIDAsyncTest.cs             ← [TestFixture] : GenericDynamoRepositoryTest
      SaveAsyncTest.cs                 ← [TestFixture] : GenericDynamoRepositoryTest
  Application.UnitTests/
    Recordings/Commands/AdminAddRecording/
      GenericAdminAddRecordingTest.cs  ← clase base abstracta propia
      AdminAddRecordingCommandHandlerTest.cs
      AdminAddRecordingCommandValidatorTest.cs
  Infrastructure.IntegrationTests/
    Repositories/RecordingRepository/
      GenericRecordingRepositoryTest.cs  ← extiende TestBase
      SaveTest.cs
      GetByIdTest.cs
  Web.AcceptanceTests/
    Recordings/
      GenericRecordingAcceptanceTest.cs  ← extiende GenericAcceptanceTest<TApp>
      AddRecordingTest.cs
```

### Integración con clases base de proyectos de test

| Proyecto | Clase base del proyecto | La suite debe extender |
|----------|------------------------|------------------------|
| `Infrastructure.IntegrationTests` | `TestBase` | `GenericXxxTest : TestBase` |
| `Web.AcceptanceTests` | `GenericAcceptanceTest<TApp>` | `GenericXxxTest<TApp> : GenericAcceptanceTest<TApp>` |
| `Application.UnitTests` | _(sin clase base — abstracta propia)_ | `abstract class GenericXxxTest` |
| `Infrastructure.UnitTests` | _(sin clase base — abstracta propia)_ | `abstract class GenericXxxTest` |

### Scope de helpers y builders

| Scope | Ubicación |
|-------|-----------|
| Local a la suite (setup, mocks) | Clase base genérica de la suite |
| Local a la carpeta | Clase estática en el mismo directorio |
| Compartido en el proyecto | `Helpers/` en la raíz del proyecto de test |

---

## 11. Flujo de errores — Infraestructura → Dominio

### Regla fundamental
Todo `Result<T>` en fallo **siempre** lleva un código de error constante. Nunca se retornan `Result.Failure<T>("mensaje libre")`.

### Tres capas de error

```
Third-party exception (AWS, Stripe…)
        │
        ▼
GenericDynamoRepository / GenericS3Repository
        │  catch + mapea a GenericPersistenceErrors (GPE-xxx)
        ▼
Repository adapter (RecordingRepository, SubscriptionRepository…)
        │  .MapError(): GPE-xxx → error de dominio (REC-xxx, SUB-xxx…)
        ▼
Application handler  ←  solo ve errores de dominio
```

### Capa 1 — Conectores genéricos (GenericDynamoRepository, GenericS3Repository)
- Capturan **todas** las excepciones del SDK y las mapean a `GenericPersistenceErrors` (`GPE-xxx`).
- **Nunca** dejan escapar excepciones crudas ni retornan mensajes libres.
- Los códigos `GPE-xxx` son agnósticos de dominio; nunca contienen lógica de negocio.

```csharp
// GenericDynamoRepository — HandleError centralizado
private string HandleError(Exception ex) => ex switch
{
    ProvisionedThroughputExceededException => GenericPersistenceErrors.ProvisionedThroughputExceeded,
    ResourceNotFoundException              => GenericPersistenceErrors.ResourceNotFound,
    ConditionalCheckFailedException        => GenericPersistenceErrors.ConditionalCheckFailed,
    _                                      => GenericPersistenceErrors.GeneralError
};
```

### Capa 2 — Repository adapters
- Mapean `GPE-xxx` a errores de dominio específicos con `.MapError()`.
- Solo los errores con semántica de negocio se traducen; los errores genéricos (`GPE-999`, etc.) se propagan tal cual.
- **Nunca** se mapea a un string literal; siempre se usan constantes de `*Errors`.

```csharp
// RecordingRepository — mapeo selectivo
public async Task<Result<RecordingEntity>> GetByIdAsync(string id, CancellationToken ct)
{
    return await FindByID(id, ct)
        .MapError(error => error == GenericPersistenceErrors.NoRecordsFound
            ? RecordingErrors.NotFound   // GPE-000 → REC-001
            : error)                     // resto de GPE-xxx se propagan
        .Map(mapper.FromDbModel);
}
```

### Capa 3 — Application handlers
- Reciben exclusivamente errores de dominio o `GPE-xxx` no mapeados.
- No conocen la existencia de `GenericPersistenceErrors`; delegan el mapeo al adapter.

### Reglas de codificación
| Regla | ✅ Correcto | ❌ Incorrecto |
|-------|------------|--------------|
| Errores en conectores | `GenericPersistenceErrors.GeneralError` | `"Unexpected error"` |
| Errores en adapters | `RecordingErrors.NotFound` | `"GPE-000"` literal |
| Errores en dominio | `UserErrors.AlreadyExists` | `"US-002"` literal |
| Resultados de fallo | `Result.Failure<T>(SomeErrors.Code)` | `Result.Failure<T>("texto libre")` |

---

## 12. Restricciones de seguridad y configuración

- Sin secrets ni configuración hardcodeada en código fuente.
- Sin datos sensibles en logs, respuestas HTTP o mensajes de error.
- Sin `dynamic` ni `object` donde se puede tipar con genéricos.
- La estructura de carpetas del proyecto **no puede modificarse**; los tests nuevos van en las carpetas existentes.

