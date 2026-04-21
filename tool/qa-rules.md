# Reglas del agente CSharpQAExpert

Este fichero es la fuente de verdad de todas las reglas que el agente `CSharpQAExpert` debe seguir al crear, revisar o corregir tests en este repositorio.

---

## 1. Convenciones de nomenclatura

### PascalCase obligatorio en todos los campos de clase de test

```csharp
// ❌ PROHIBIDO
private Mock<IRecordingStoragePort> _storagePortMock = default!;
private AdminAddRecordingCommandHandler _handler = default!;
private string? _lastCreatedRecordingId;

// ✅ OBLIGATORIO
private Mock<IRecordingStoragePort> StoragePortMock = default!;
private AdminAddRecordingCommandHandler Handler = default!;
private string? LastCreatedRecordingId;
```

Aplica a **todos** los campos de instancia en clases de test, incluyendo campos de teardown, flags de limpieza, etc.

---

## 2. Helpers en clases separadas

Los métodos helper (builders, factories, autenticación, construcción de formularios, etc.) **NO** pueden ser métodos privados dentro de la clase de test. Deben estar en clases separadas:

- **Scope local a la carpeta de test** → clase estática en el mismo directorio (ej: `AdminAddRecordingCommandBuilder.cs`)
- **Scope compartido entre suites** → clase estática en el directorio raíz del proyecto de test o en una carpeta `Helpers/` (ej: `RecordingAcceptanceHelpers.cs`)

```csharp
// ❌ PROHIBIDO — método helper privado inline
private static AdminAddRecordingCommand BuildValidCommand() => new(...);
private async Task AuthenticateAsAdmin() { ... }

// ✅ OBLIGATORIO — clase separada
// AdminAddRecordingCommandBuilder.cs
public static class AdminAddRecordingCommandBuilder {
    public static AdminAddRecordingCommand BuildValid() => new(...);
}

// RecordingAcceptanceHelpers.cs
public static class RecordingAcceptanceHelpers {
    public static async Task AuthenticateAsAdmin<TApp>(...) { ... }
}
```

---

## 3. `It.IsAny<T>()` prohibido en `Verify`

`It.IsAny<T>()` está **completamente prohibido** dentro de llamadas `Verify(...)`. Todos los `Verify` deben usar `It.Is<T>(predicate)` que valide los datos reales pasados al mock.

```csharp
// ❌ PROHIBIDO
RegistryPortMock.Verify(
    x => x.SaveAsync(It.IsAny<RecordingEntity>(), It.IsAny<CancellationToken>()),
    Times.Once);

// ✅ OBLIGATORIO
RegistryPortMock.Verify(
    x => x.SaveAsync(
        It.Is<RecordingEntity>(e =>
            e.Name == command.Name &&
            e.StorageKey == expectedStorageKey &&
            !string.IsNullOrEmpty(e.Id)),
        It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
    Times.Once,
    "Mensaje descriptivo del verify");
```

- `It.IsAny<T>()` **solo** está permitido en `Setup(...)`, nunca en `Verify(...)`.
- Un `Verify` con `It.IsAny` se considera **test inválido** y debe corregirse antes de cerrar el ciclo.

---

## 4. Quality Gate — Ejecución automática obligatoria

Al finalizar cualquier ciclo de trabajo, el QA **debe ejecutar** el script de quality gate:

```zsh
chmod +x tool/quality_gate.sh && ./tool/quality_gate.sh
```

El script realiza automáticamente:
1. `dotnet test` con cobertura (`XPlat Code Coverage`)
2. Generación de reporte HTML con `reportgenerator`
3. Verificación del umbral de cobertura de línea **≥ 80%**
4. `dotnet format --verify-no-changes`

**El veredicto LISTO solo puede emitirse si el script termina con código de salida 0.**

Si el script falla por cobertura < 80%:
- Identificar qué código de producción no está cubierto
- Crear tests adicionales para cubrir las rutas faltantes
- Volver a ejecutar el script hasta que pase

Si el script falla por `dotnet format`:
- Ejecutar `dotnet format` sobre los ficheros con errores
- Volver a ejecutar `dotnet format --verify-no-changes` para confirmar

---

## 5. Patrones de tests obligatorios

### Framework y librerías
- **NUnit**: `[TestFixture]`, `[SetUp]`, `[Test]`, `[TestCase]`, `[DisplayName]`, `[Description]`
- **Mocks**: Moq — `Mock<T>`, `.Setup()`, `.Verify()`, `.VerifyNoOtherCalls()`
- **Validators**: `FluentValidation.TestHelper` — `TestValidate`, `ShouldHaveValidationErrorFor`, `ShouldNotHaveValidationErrorFor`

### Estructura de cada test
```csharp
[Test]
[DisplayName("Descripción legible del comportamiento")]
public async Task NombreDescriptivoCamelCase()
{
    // Given: descripción del estado inicial

    // When: acción ejecutada

    // Then: verificación del resultado esperado
}
```

### Asserts
- Siempre `Assert.That(actual, constraint, "mensaje descriptivo")`
- El mensaje debe explicar qué se esperaba y qué se obtuvo
- Para `Result<T>`: usar expresiones condicionales para evitar excepciones eager:
  ```csharp
  // ❌ Lanza excepción si IsSuccess == true
  Assert.That(result.IsFailure, Is.True, $"Error: {result.Error}");

  // ✅ Seguro
  Assert.That(result.IsFailure, Is.True,
      $"Expected failure but got success with value: '{(result.IsSuccess ? result.Value : "N/A")}'");
  ```

---

## 6. Proyectos de test y su propósito

| Proyecto | Propósito | Requiere AWS |
|----------|-----------|--------------|
| `Domain.UnitTests` | Tests de entidades y lógica de dominio | No |
| `Application.UnitTests` | Tests de handlers, validators con mocks | No |
| `Application.FunctionalTests` | Tests funcionales de handlers con mocks más completos | No |
| `Infrastructure.UnitTests` | Tests de repositorios y servicios con mocks | No |
| `Infrastructure.IntegrationTests` | Tests de integración real con DynamoDB/S3/Cognito | **Sí** |
| `Web.AcceptanceTests` | Tests E2E del API con `WebApplicationFactory` | **Sí** |

Los tests de integración y aceptación se excluyen del quality gate automático (`--filter`). Se ejecutan manualmente cuando hay entorno AWS disponible.

---

## 7. Cobertura mínima

- **Umbral**: ≥ 80% de cobertura de línea en los proyectos `Application` e `Infrastructure`
- Medida con `XPlat Code Coverage` + `reportgenerator`
- Script: `./tool/quality_gate.sh`

---

## 8. Reutilización de helpers heredados — No duplicar código existente

Antes de crear un helper nuevo, **comprobar siempre** si ya existe en las clases base del proyecto de test.

### Web.AcceptanceTests — `GenericAcceptanceTest<TApp>` ya proporciona

| Método | Descripción |
|--------|-------------|
| `RegisterUser(username, email, password)` | Registra un usuario |
| `RegisterAndConfirmUser(username, email, password)` | Registra y confirma un usuario |
| `RegisterAndConfirmAdmin(username, email, password)` | Registra, confirma y promueve a admin |
| `AuthenticateUser(email, password)` | Autentica y devuelve `AuthenticationResult` |
| `WaitForVerificationCode(itemId, timeout)` | Espera código de verificación en DynamoDB |
| `CheckForUser(userId)` | Recupera un usuario del repositorio |
| `GetObjectFromFactory<T>()` | Resuelve un servicio del DI container |
| `CreateValidMultipartForm(templateName, fileName, content)` | Construye un formulario multipart genérico |

### Regla

```csharp
// ❌ PROHIBIDO — duplicar lógica de autenticación ya existente en GenericAcceptanceTest
public static async Task AuthenticateAsAdmin<TApp>(GenericAcceptanceTest<TApp> test) where TApp : class
{
    string email = test.TheFaker.Internet.Email();
    await test.RegisterAndConfirmAdmin(...);
    AuthenticationResult auth = await test.AuthenticateUser(...);
    test.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
}

// ✅ OBLIGATORIO — llamar directamente a los métodos heredados en el test
string email = TheFaker.Internet.Email();
await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
```

Los helpers en clases separadas **solo** deben contener lógica que no exista ya en la clase base:
- Construcción de objetos de dominio específicos del contexto (builders)
- Construcción de formularios multipart específicos del endpoint
- Limpieza de datos de test (teardown helpers)

**Nunca** wrappear métodos que ya existen en `GenericAcceptanceTest` u otras clases base.

### Infrastructure.IntegrationTests — `TestBase` ya proporciona

| Método | Descripción |
|--------|-------------|
| `CreateDynamoDBContext()` | Crea un `IDynamoDBContext` con perfil AWS |
| `CreateTestLogger<T>()` | Crea un `ILogger<T>` para tests |
| `CreateTestConfiguration()` | Crea la configuración desde `appsettings.Test.json` |
| `CreateValidUser()` | Crea una `UserEntity` válida con Bogus |
| `CleanupUser(userId, context)` | Elimina un usuario de DynamoDB |

Los helpers específicos de cada feature (ej: `CreateValidRecordingEntity`, `CleanupRecording`) se colocan en la clase abstracta genérica del propio módulo (ej: `GenericRecordingRepositoryTest`).


