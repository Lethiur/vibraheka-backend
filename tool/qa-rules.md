# Reglas detalladas del agente CSharpQAExpert

> **Reglas globales** (stack, convenciones de test GivenWhenThen, Assert, Moq, PascalCase, proyectos, quality gate, estructura de tests, errores de dominio): `.github/copilot-instructions.md`.
> Este fichero contiene las **reglas específicas de QA** con patrones de código y ejemplos que el agente `CSharpQAExpert` usa como referencia técnica.

---

## 1. Helpers en clases separadas — dónde va cada cosa

| Scope | Ubicación | Ejemplo |
|-------|-----------|---------|
| Setup, mocks, costrucción compartida | Clase base genérica de la suite | `GenericAdminAddRecordingTest.BuildValidCommand()` |
| Local a la carpeta de test | Clase estática en el mismo directorio | `AdminAddRecordingCommandBuilder.cs` |
| Compartido entre suites del proyecto | `Helpers/` en la raíz del proyecto | `UserFakeBuilder.cs` |

```csharp
// ❌ PROHIBIDO — método helper privado inline
private static AdminAddRecordingCommand BuildValidCommand() => new(...);

// ✅ OBLIGATORIO — en la clase base genérica de la suite
// GenericAdminAddRecordingTest.cs
protected static AdminAddRecordingCommand BuildValidCommand() => new(
    Name: "Meditacion test", Description: "Descripcion de test",
    Type: RecordingType.Meditacion,
    FileStream: new MemoryStream(new byte[] { 1, 2, 3 }),
    FileName: "test.mp4");
```

---

## 2. `It.IsAny<T>()` prohibido en `Verify`

`It.IsAny<T>()` está **completamente prohibido** en `Verify(...)`. Usar `It.Is<T>(predicate)` con los valores reales.

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
    "SaveAsync debe llamarse una vez con la entidad correcta");
```

---

## 3. Patrones de helpers

### Entity builder con Bogus
```csharp
public static class UserBuilder
{
    private static readonly Faker Faker = new();

    public static User Valid() => User.Create(Guid.NewGuid(), Faker.Internet.Email(), Faker.Name.FullName());
    public static User WithEmail(string email) => User.Create(Guid.NewGuid(), email, Faker.Name.FullName());
}
```

### Mock factory centralizada
```csharp
public static class UserRepositoryMockFactory
{
    public static Mock<IUserRepository> Empty()
    {
        Mock<IUserRepository> mock = new();
        mock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        return mock;
    }

    public static Mock<IUserRepository> WithUser(User user)
    {
        Mock<IUserRepository> mock = new();
        mock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        return mock;
    }
}
```

---

## 4. Helpers heredados — no duplicar lógica existente

Antes de crear un helper, comprobar si ya existe en las clases base del proyecto.

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

```csharp
// ❌ PROHIBIDO — envolver métodos que ya existen
public static async Task AuthenticateAsAdmin<TApp>(GenericAcceptanceTest<TApp> test) { ... }

// ✅ OBLIGATORIO — usar directamente los métodos heredados
string email = TheFaker.Internet.Email();
await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
AuthenticationResult auth = await AuthenticateUser(email, ThePassword);
Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
```

### Infrastructure.IntegrationTests — `TestBase` ya proporciona

| Método | Descripción |
|--------|-------------|
| `CreateDynamoDBContext()` | Crea un `IDynamoDBContext` con perfil AWS |
| `CreateTestLogger<T>()` | Crea un `ILogger<T>` para tests |
| `CreateTestConfiguration()` | Crea configuración desde `appsettings.Test.json` |
| `CreateValidUser()` | Crea una `UserEntity` válida con Bogus |
| `CleanupUser(userId, context)` | Elimina un usuario de DynamoDB |

Los helpers específicos de cada feature (ej: `CreateValidRecordingEntity`, `CleanupRecording`) se colocan en la clase base genérica de la suite (ej: `GenericRecordingRepositoryTest`).

---

## 5. Checklist de revisión por ticket

### Tests unitarios / funcionales
- [ ] Cada clase bajo test tiene su carpeta con clase base genérica (§10 `copilot-instructions`)
- [ ] Cada método público tiene su propio fichero de test
- [ ] La clase base genérica extiende `TestBase` / `GenericAcceptanceTest<TApp>` si existe en el proyecto
- [ ] Sin tests para clases `[Mapper]` Mapperly
- [ ] Mocks en factories o clase base; no `new Mock<>()` inline en cada test
- [ ] Sin construcción de objetos inline; en builders o clase base genérica
- [ ] Sin `var`; tipos explícitos
- [ ] `// Given`, `// When`, `// Then` — nunca Arrange/Act/Assert
- [ ] `Assert.That(actual, constraint, "mensaje con valores reales")`
- [ ] `Verify` con `It.Is<T>` exacto y `Times` explícito; nunca `It.IsAny` en Verify
- [ ] `VerifyNoOtherCalls()` al final de cada test con mocks
- [ ] PascalCase en campos de instancia de clases de test
- [ ] Formato validado con `dotnet format --verify-no-changes` antes del quality gate

### Tests de integración / aceptación
- [ ] Helpers de `GenericAcceptanceTest<TApp>` o `TestBase` usados sin duplicación
- [ ] Los tests limpian su estado al finalizar (`[TearDown]`)

### Calidad general
- [ ] Sin setup duplicado en más de 2 tests (extraer a clase base)
- [ ] Sin `Thread.Sleep()` ni delays hardcodeados
- [ ] Sin `.Result` o `.Wait()`
- [ ] Todo camino del código productivo cubierto por al menos un test
