# VibraHeka Backend — Instrucciones globales del proyecto

Este fichero es la fuente de verdad de todas las reglas generales del repositorio.
Todos los agentes (`CSharpExpert`, `CSharpQAExpert`, `ProductOwner`) deben seguirlas sin excepción.

---

## 1. Stack tecnológico

- **.NET 8+ / C# 12+**
- **ASP.NET Core** (Web API)
- **MediatR** (CQRS — handlers `IRequestHandler<TRequest, TResponse>`)
- **FluentValidation** (validación de entrada)
- **NUnit** (framework de tests — `[TestFixture]`, `[SetUp]`, `[Test]`, `[TestCase]`, `[DisplayName]`)
- **Moq** (mocking — `Mock<T>`, `.Setup()`, `.Verify()`, `.VerifyNoOtherCalls()`)
- **FluentValidation.TestHelper** (`TestValidate`, `ShouldHaveValidationErrorFor`)
- **Bogus** (datos ficticios en tests)
- **Serilog** (logging estructurado)
- **AWS** (DynamoDB, S3, Cognito) — solo en Infrastructure

---

## 2. Arquitectura de capas (Clean Architecture)

```
src/
  Domain/           # Entidades, value objects, interfaces de repositorio, errores de dominio
  Application/      # Casos de uso, DTOs, interfaces de servicios, validaciones
  Infrastructure/   # Implementaciones: repositorios, servicios externos, AWS
  Web/              # Controllers, middleware, DI setup, appsettings
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

---

## 3. Patrones obligatorios

### Result pattern para errores de dominio
```csharp
// Usar Result<T> o equivalente para errores de dominio; NO lanzar excepciones de negocio
Result<T>.Success(value)
Result<T>.Failure("mensaje de error")
```

### Naming conventions
- Interfaces: `IFeatureRepository`, `IFeatureService`
- Implementaciones: `FeatureRepository`, `FeatureService`, `FeatureController`
- DTOs: `FeatureDto`, `CreateFeatureRequest`, `FeatureResponse`
- Entidades de dominio: inmutables cuando sea posible (`record` o propiedades con `init`)
- Handlers: `CreateFeatureHandler`, `GetFeatureHandler`

### Async/await
- `async/await` obligatorio en todos los métodos de I/O.
- **Prohibido** `.Result` o `.Wait()` en cualquier parte del código (productivo y tests).

### Nullable reference types
- `<Nullable>enable</Nullable>` habilitado en todos los proyectos.
- Sin `!` injustificados.

---

## 4. Convenciones de tests

### Framework y librerías obligatorios
- **NUnit** para todos los tests nuevos o modificados (no xUnit, no MSTest).
- **Moq** exclusivamente para mocking (no NSubstitute).

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
- Para `Result<T>`: usar expresiones condicionales para evitar excepciones eager:
  ```csharp
  // ✅ Seguro
  Assert.That(result.IsFailure, Is.True,
      $"Expected failure but got success with value: '{(result.IsSuccess ? result.Value : "N/A")}'");
  ```

### Verificaciones de Moq
- `It.IsAny<T>()` **solo** en `Setup(...)`, **nunca** en `Verify(...)`.
- `Verify(...)` debe usar argumentos exactos (`It.Is<T>(predicate)`) cuando el valor es conocido.
- Especificar siempre `Times` explícito (`Times.Once`, `Times.Never`, `Times.Exactly(n)`).
- Llamar `mock.VerifyNoOtherCalls()` al final de **cada** test que instancie mocks.

### Naming en clases de test
- **PascalCase obligatorio** en todos los campos de instancia de clases de test:
  ```csharp
  // ✅ OBLIGATORIO
  private Mock<IRecordingStoragePort> StoragePortMock = default!;
  private AdminAddRecordingCommandHandler Handler = default!;
  ```

### Sin `var` en tests
- Tipos declarados explícitamente en todos los ficheros de test.

### Helpers en clases separadas
- Los métodos helper (builders, factories, autenticación) **NO** pueden ser métodos privados inline dentro de la clase de test.
- Scope local a carpeta → clase estática en el mismo directorio.
- Scope compartido → clase estática en directorio raíz del proyecto o en `Helpers/`.

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

Los tests de integración y aceptación se excluyen del quality gate automático (`--filter`). Se ejecutan manualmente cuando hay entorno AWS disponible.

---

## 6. Quality Gate — Ejecución obligatoria al cierre

La tool **`quality_gate`** (definida en `.github/copilot-tools.yml`) ejecuta el gate completo.
El agente `CSharpQAExpert` debe invocarla directamente; los demás agentes pueden referenciarla.

Equivalente manual (solo si la tool no está disponible):
```zsh
chmod +x tool/quality_gate.sh && ./tool/quality_gate.sh
```

El script realiza automáticamente:
1. `dotnet test` con cobertura (`XPlat Code Coverage`)
2. Generación de reporte HTML con `reportgenerator`
3. Verificación del umbral de cobertura de línea **≥ 80%**
4. `dotnet format --verify-no-changes`

**El veredicto LISTO solo puede emitirse si el script termina con código de salida 0.**

- Si falla por cobertura < 80%: añadir tests para cubrir rutas faltantes y re-ejecutar.
- Si falla por `dotnet format`: ejecutar `dotnet format` y confirmar con `--verify-no-changes`.
- **No re-ejecutar** si no hubo cambios desde la última ejecución; reutilizar evidencia previa.

### Umbral de cobertura
- **≥ 80%** de cobertura de línea en los proyectos `Application` e `Infrastructure`.

---

## 7. Ciclo de delegación entre agentes

```
ProductOwner
  └─► CSharpExpert  (implementación)
        └─► CSharpQAExpert  (auditoría + quality gate)
              └─► ProductOwner  (veredicto final)
```

- `CSharpExpert` **no reporta trabajo como completo** al `ProductOwner` hasta recibir veredicto del `CSharpQAExpert`.
- `CSharpQAExpert` **no cierra el ticket**; reporta veredicto al `ProductOwner`.
- `ProductOwner` cierra el ticket solo con veredicto **LISTO** del `CSharpQAExpert`.
- Si veredicto == **NO LISTO**: el `ProductOwner` relanza el ciclo con un nuevo paquete de delegación para `CSharpExpert`.

---

## 8. Restricciones de seguridad y configuración

- Sin secrets ni configuración hardcodeada en código fuente.
- Sin datos sensibles en logs, respuestas HTTP o mensajes de error.
- Sin `dynamic` ni `object` donde se puede tipar con genéricos.
- La estructura de carpetas del proyecto **no puede modificarse**; los tests nuevos van en las carpetas existentes.

