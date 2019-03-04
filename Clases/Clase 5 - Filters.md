# Clase 4 - Filters

Los filtros en ASP.NET Core permiten ejecutar código antes o después de etapas específicas en el pipeline de una request. Funcionan muy similar a como funcionan los middlewares en otros lenguajes/frameworks (Node.js con Express/Koa por ejemplo).

## Como funcionan los filtros:

Conceptualmente, una vez que al servidor le llega una request y se elige una accion, se ejecutan una serie de pasos definidos aparte de las acciones que definimos en los controladores. Cada uno de estos pasos se pueden ejecutar antes o despues de ejecutar una acción.

Cada uno de estos pasos recibe los datos del paso anterior (solo los de la request en caso de ser el primero) y le pasa datos al siguiente paso, formando una cadena.

El conjunto completo de estos pasos es conocido como el **pipeline de la request**.

Los filters en particular, corren entre la MVC Action invocation pipeline o filter pipeliine. La filter pipeline corre después de que la API selecciona una acción que ejecutar.

![FILTERS-PIPELINE](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters/_static/filter-pipeline-1.png?view=aspnetcore-2.1)

## Algunos filtros ya construidos:

* **Authorization:** prevenir acceso a una ruta la cual el usuario no está autorizado
* **Protocol:** Asegurarse que todas las request usen HTTPS
* **Response caching:** Short-circuiting de la request pipeline para retornar una respuesta cacheada. Si existe un cache, se retorna eso y no se ejecuta la accion entera. 

Filters pueden ser creados para manejar 'preocupaciones' o tareas transversales (no especificamente de una acción). 

*Ej*: Manejo de excepciones la pueden realizar los filtros, si un método lanza una excepción es atrapado por un filtro y este retorna un 404, entonces con los filtros consolidamos el manejo de este error.

## Tipos de filtros:

Los filtros se pueden categorizar en distintas categorías:

Tipo | Descripción
------------ | -------------
[Authorization filters](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-2.1#authorization-filters)| Se ejecutan primero y son usados para determinar si el usuario actual es autorizado para acceder al recurso actual.
[Resource filters](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-2.1#resource-filters)| Se ejecutan luego de la autorización. Y sirven para ejecutar código antes y después de que el pipeline termine. Son útiles para caching o shot-circuit la filter pipeline y así mejorar la performance.
[Action filters](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-2.1#action-filters)| Sirven para ejecutar código antes y después de una acción (método) es invocado. Son útiles para manipular argumentos pasados en la acción en particular.
[Exception filters](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-2.1#exception-filters)| Son usados para aplicar 'políticas' globales para manejar excepciones ocurridas antes de que cualquier cosa sea escrita en el body de la response.
[Result filters](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-2.1#result-filters)| Se ejecutan antes y después de la ejecución de un action results. Solo se ejecutan cuando un action method ha sido ejecutado exitosamente. Son útiles para crear formateadores.

![FILTERS-PIPELINE2](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters/_static/filter-pipeline-2.png?view=aspnetcore-2.1)

## Implementación de un Filtro

Vamos a crear ActionFilter para manejar la autenticación de nuestra api.

Esto no es óptimo, ya que idealmente se debería de encargar un [Authorization filters](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/index?view=aspnetcore-2.1). 

Para este ejemplo no lo utilizaremos ya que todo lo relacionado a la autorización de usuarios ya se encuentra todo muy digerido e implica otros temas como la generación de tokens con [jwt](https://jwt.io/). En terminos practicos, es mejor mostrar un poco como funciona un authorization filter por detrás y simplificar los tokens, pero son bienvenidos a usarlo para el obligatorio :smile:

## Preparando nuestro proyecto

Debido a que para manejar autenticación debemos tener el concepto de usuarios y sesiones, debemos agregar varios recursos a nuestro sistema. Los usuarios y sesiones estan implementados en el codigo de la carpeta `clase5-base`.

Primero, crearemos nuestro controller que se utilice para el manejo de usuarios:

**UsersController**

```c#
using System;
using Microsoft.AspNetCore.Mvc;
using Homeworks.WebApi.DTO;
using Homeworks.BusinessLogic;

namespace Homeworks.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : ControllerBase, IDisposable
    {
        private IUsersLogic usersLogic;

        public UsersController(IUsersLogic usersLogic = null)
        {
            if (usersLogic == null) {
                this.usersLogic = new UsersLogic();
            } else {
                this.usersLogic = usersLogic;
            }
        }


        [HttpGet]
        public IActionResult Get()
        {
            IEnumerable<User> users = usersLogic.GetUsers();
            IEnumerable<UserDTO> usersToReturn = users.Select(x => new UserDTO(x));
            return Ok(usersToReturn);
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            User user = usersLogic.Get(id);
            if (user == null) {
                return NotFound();
            }
            UserDTO userToReturn = new UserDTO(user);
            return Ok(userToReturn);
        }

        [HttpPost]
        public IActionResult Post([FromBody] UserDTO userDTO)
        {
            try {
                User userToCreate = userDTO.ToEntity();
                User createdUser = usersLogic.Create(userToCreate);
                UserDTO userToReturn = new UserDTO(createdUser);

                return CreatedAtRoute("Get", new { id = userToReturn.Id }, userToReturn);
            } catch(ArgumentException e) {
                return BadRequest(e.Message);
            }
        }

        public void Dispose()
        {
            usersLogic.Dispose();
        }
    }
}
```

Luego de agregar esta clase, el codigo no nos va a andar aun. Principalmente porque no vamos a tener creadas tres clases criticas para el funcionamiento: `IUsersLogic` con su implementación, `UserDTO` y `User`. Ahora crearemos estas:

**UserDTO:**

```c#
using System;
using System.Collections.Generic;
using System.Linq;
using Homeworks.Domain;

namespace Homeworks.WebApi.DTO
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        public UserDTO() { }

        public UserDTO(User entity)
        {
            SetModel(entity);
        }

        public User ToEntity() => new User()
        {
            Id = this.Id,
            Name = this.Name,
            UserName = this.UserName,
            Password = this.Password,
            Role = this.Role,
        };

        protected UserDTO SetModel(User entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            UserName = entity.UserName;
            Role = entity.Role;
            return this;
        }

    }
}
```

**User:**

```c#
using System;
using System.Collections.Generic;

namespace Homeworks.Domain
{
    public class User
    {       
        public Guid Id {get; set;}
        public string Name {get; set;}
        public string UserName {get; set;}
        public string Password {get; set;}
        public string Role {get; set;}

        public bool IsValid()
        {
            return true;
        }

        public User Update(User entity)
        {
            if (entity.Name != null)
                Name = entity.Name;
            if (entity.Password != null)
                Password = entity.Password;
            return this;
        }
    }
}
```

**IUserLogic:**

```c#
using System;
using System.Collections.Generic;
using Homeworks.Domain;

namespace Homeworks.BusinessLogic
{
    public interface IUsersLogic : IDisposable
    {
        User Create(User user);

        User Get(Guid id);

        IEnumerable<User> GetUsers();
    }
}
```

**UserLogic:**

```c#
using System;
using System.Collections.Generic;
using Homeworks.DataAccess;
using Homeworks.Domain;

namespace Homeworks.BusinessLogic
{
    public class UsersLogic : IUsersLogic
    {
        private IRepository<User> repository;

        public UsersLogic(IRepository<User> repository) {
            this.repository = repository;
        }

        public UsersLogic() {
            HomeworksContext context = ContextFactory.GetNewContext();
            repository = new Repository<User>(context);
        }

        public User Create(User user) 
        {
            ValidateUser(user);
            repository.Add(user);
            repository.Save();
            return user;
        }

        public User Get(Guid id) {
            return repository.GetFirst(x => x.Id == id);
        }

        public IEnumerable<User> GetUsers() 
        {
            return repository.GetAll();
        }

        public void Dispose()
        {
            repository.Dispose();
        }

        private void ValidateUser(User user) 
        {
            // No es correcto del todo. Estas validaciones podrían estar en otro lugar
            if (user == null || !user.IsValid()) 
            {
                throw new ArgumentException("User not valid");
            }
        }
    }
}
```

Este codigo es bastante directo. Analogamente a los otros recursos que tenemos, simplemente agregamos las clases correspondientes para poder manipular los usuarios de nuestro sistema. 

Una vez que tenemos usuarios en nuestro sistema, podemos pasar a hacer la autenticacion. Para esto, manejaremos la `Session` como un recurso. Una sesión representara que el usuario inicio sesión y mantiene una sesion activa. Al momento de crear una sesion (Login), este recibira un usuario y contraseña, y devolvera un token autogenerado. Este token autogenerado sera usado por el usuario para indicar que efectivamente es el y tiene una sesión activa.

Primero crearemos el codigo que nos permitira manejar estas sesiones. Como cualquier otro recurso, debemos crear un `SessionController`, un `SessionLogic` y un `SessionRepository`. Tambien debemos crear modelos que representen esto. Para recibir el inicio de sesión, recibiremos un `SessionDTO`. Para guardar en la base de datos, crearemos un modelo `Session`.

**SessionController:**

```c#
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

using Homeworks.Domain;
using Homeworks.WebApi.DTO;
using Homeworks.BusinessLogic;

namespace Homeworks.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class SessionController : ControllerBase, IDisposable
    {
        private ISessionsLogic sessionsLogic;

        public SessionController(ISessionsLogic sessionsLogic = null)
        {
            if (sessionsLogic == null) {
                this.sessionsLogic = new SessionsLogic();
            } else {
                this.sessionsLogic = sessionsLogic;
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO login) {
            try {
                Guid token = sessionsLogic.Login(login.UserName, login.Password);
                if (token == null) 
                {
                    return BadRequest("Invalid user/password");
                }
                return Ok(token);
            } catch(ArgumentException exception) {
                return BadRequest(exception.Message);
            }
        }

        public void Dispose()
        {
            sessionsLogic.Dispose();
        }
    }
}
```

El Controller es bastante directo. Por ahora, tenemos un solo endpoint (`api/session/login`) el cual recibira usuario y contraseña (Modelado en `LoginDTO`) y crearea la session mediante `ISessionsLogic`. Esto retornara un GUID, el cual retornaremos al usuario de la API.

**LoginDTO**

```c#
using System;
using System.Collections.Generic;
using System.Linq;
using Homeworks.Domain;

namespace Homeworks.WebApi.DTO
{
    public class LoginDTO
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public LoginDTO() { }
    }
}
```

`LoginDTO` no puede ser mas simple. Simplemente cuenta con el usuario y contraseña del usuario que esta intentando loguearse. 

**ISessionsLogic:**

```c#
using System;

namespace Homeworks.BusinessLogic
{
    public interface ISessionsLogic: IDisposable
    {
        Guid Login(string username, string password);
    }
}
```

**SessionsLogic:**

```c#
using System;

using Homeworks.Domain;
using Homeworks.DataAccess;

namespace Homeworks.BusinessLogic
{
  public class SessionsLogic : ISessionsLogic
  {
    private IRepository<Session> sessionRepository;
    private IRepository<User> userRepository;

    public SessionsLogic(SessionsRepository sessionRepository, IRepository<User> userRepository) {
        this.sessionRepository = sessionRepository;
        this.userRepository = userRepository;
    }

    public SessionsLogic() {
        HomeworksContext context = ContextFactory.GetNewContext();
        userRepository = new Repository<User>(context);
        sessionRepository = new SessionsRepository(context);
    }

    public Guid Login(string username, string password)
    {
        Guid sessionToken = Guid.NewGuid();
        User user = userRepository.GetFirst(u => u.UserName == username && u.Password == password);
        if (user == null) {
            throw new ArgumentException("Username/Password not valid");
        }

        Session session = new Session() { token = sessionToken, user = user };
        sessionRepository.Add(session);
        sessionRepository.Save();

        return sessionToken;
    }

    public void Dispose()
    {
        sessionRepository.Dispose();
        userRepository.Dispose();
    }

  }
}
```

Por ahora, `SessionLogic` cuenta con un solo metodo, el cual inicia la sesión de un usuario apartir de un usuario y contraseña. En el caso que no exista un usuario con ese username y contraseña, se tira una exception. Es importante notar que tenemos 2 repositorios en esta clase de la logica, ya que debemos obtener tanto usuarios como repositorios para la logica. 

```c#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

using Homeworks.Domain;

namespace Homeworks.DataAccess
{
  public class SessionsRepository : Repository<Session>
  {
    public SessionsRepository(DbContext Context) : base(Context) { }

    public override IEnumerable<Session> GetAll()
    {
        return Context.Set<Session>().Include("Exercises");
    }

    public override IEnumerable<Session> GetByCondition(Expression<Func<Session, bool>> expression)
    {
        return Context.Set<Session>().Include("User").Where(expression);
    }

    public override Session GetFirst(Expression<Func<Session, bool>> expression)
    {
        return Context.Set<Session>().Include("User").First(expression);
    }
  }
}
```

Debemos crear una repositorio para la sesión. Como se puede recordar, al tener una relacion con un `User`, debemos utilizar `Include` para que efectivamente lo traiga de la base de datos. Si no, el usuario de la sesión que obtengamos de la base de datos sera siempre `null`.

**Session:**

```c#
using System;

namespace Homeworks.Domain
{
    public class Session
    {
        public Guid Id {get; set;}
        public Guid token {get; set;}
        public User user {get; set;}

        public Session()
        {
            Id = Guid.NewGuid();
        }

        public bool IsValid()
        {
            return true;
        }
    }
}
```

La clase del dominio guarda una referencia a un usuario y a un token. Tambien cuenta un Id para identificar la sesión en la base de datos.

Por ultimo, hicimos muchos cambios que afectan la base de datos. Creamos dos modelos nuevos del dominio (`User` y `Session`) que seran guardados en tablas distintas en la base de datos. Por esto, debemos modificar nuestro contexto de base de datos y crear una nueva migración.

**HomeworksContext:**

```c#
using Microsoft.EntityFrameworkCore;
using Homeworks.Domain;

namespace Homeworks.DataAccess
{
    public class HomeworksContext : DbContext
    {
        public DbSet<Homework> Homeworks { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }

        public HomeworksContext(DbContextOptions options) : base(options) { }
    }
}
```

AHora debemos crear una nueva migracion. Como recuerdan de clases anteriores, debemos ir al proyecto `Homeworks.WebApi` y correr los siguientes comandos en la consola:

```
dotnet ef migrations add AddUsersAndSessions -p ../Homeworks.DataAccess
dotnet ef database update -p ../Homeworks.DataAccess
```

Esto nos creara una nueva migracion `AddUsersAndSession` dentro de la carpeta `Migrations` en el proyecto `Homeworks.DataAccess`. Luego corremos el comando update para que efectivamente se ejecute la migración.

## Creación del Filtro

Nuestro ActionFilter va a implementar la interfaz **IActionFilter** que tiene los siguientes métodos: **OnActionExecuting** (Se ejecuta antes del action method y **OnActionExecuted** (Se ejecuta después del action method), y también va a heredar de **Attribute** que nos permitirá usarlo como **tag** en C# (arriba de la accion con el formato [....])

El constructor va a recibir el rol del usuario que tiene permitido ejecutar el action method. Los parametros del constructor son los que pasamos en la anotación (`[Anotation(paramA)]`). En este caso solo implementaremos **OnActionExecuting** ya que solo nos interesa controlar si impedir o permitir el acceso al action method antes de que se ejecute.

Creamos una nueva carpeta Filters donde pondremos nuestro nuevo filtro:

```c#
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Homeworks.BusinessLogic;

namespace Homeworks.WebApi.Filters {
    public class ProtectFilter : Attribute, IActionFilter
    {
        private readonly string role;

        public ProtectFilter(string role) 
        {
            this.role = role;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Obtenemos el token del header HTTP `authorization`
            string headerToken = context.HttpContext.Request.Headers["Authorization"];

            // Si el token es null, el usuario no se esta autenticado. Por eso cortamos
            // el pipeline. Si no envio un token, no es necesario seguir ejecutando el resto
            // de la aplicación.

            if (headerToken == null) {
                context.Result = new ContentResult()
                {
                    Content = "Token is required",
                };
            } else {
                try {
                    Guid token = Guid.Parse(headerToken);
                    VerifyToken(token, context);
                } catch (FormatException exception) {
                    context.Result = new ContentResult()
                    {
                        Content = "Invalid Token format",
                    };
                }
            }
        }

        private void VerifyToken(Guid token, ActionExecutingContext context)
        {
            // Usamos using asi nos aseguramos que se llame el Dispose de este `sessions` enseguida salgamos del bloque
            using (var sessions = new SessionsLogic()) {
                // Verificamos que el token sea valido
                if (!sessions.IsValidToken(token)) {
                    context.Result = new ContentResult()
                    {
                        Content = "Invalid Token",
                    };
                }
                // Verificamos que el rol del usuario sea correcto
                if (!sessions.HasLevel(token, role)) {
                    context.Result = new ContentResult()
                    {
                        Content = "The user isn't " + role,
                    };   
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Vacio, ya que no queremos hacer nada despues de la request
        }
    }
}
```

Como podemos ver, hay dos metodos de `SessionLogic` que no todavia no implementamos: `IsValidToken` y `HasLevel`. Estos dos son muy simples:

```c#
public bool IsValidToken(Guid token)
{
    Session sessionForToken = sessionRepository.GetFirst(s => s.Token == token);
    return sessionForToken != null;
}

public bool HasLevel(Guid token, string role)
{
    Session sessionForToken = sessionRepository.GetFirst(s => s.Token == token);
    User sessionUser = sessionForToken.User;
    
    if (sessionUser == null) {
        return false;
    }

    if (sessionUser.Role != role) {
        return false;
    }

    return true;
}
```

Recordar que tambien hay que agregarlos a la interfaz `ISessionsLogic`.

## Uso del filtro

Para usar el filtro simplemente debemos agregar el nombre del filtro como atributo encima de un metodo si quiere que se ejecute cuando ese metodo es invocado.

```c#
[ProtectFilter("Admin")]
[HttpGet("Check")]
public IActionResult CheckLogin() {
    //..
}
```

O encima de un controller si quieren que se ejecute para cada uno de los metodos de este.

```c#
[ProtectFilter("Admin")]
[Route("api/[controller]")]
public class HomeworksController : Controller
{
    //...
}
```

Para probar que este filtro anda, agregaremos un metodo a nuestro SessionController:

```c#
    [ProtectFilter("Admin")]
    [HttpGet("Check")]
    public IActionResult CheckLogin() {
        return Ok("it's allright!!");
    }
```

Al agregar este endpoint, y pegarle a la URL con `GET /api/session/check` sin nada mas, podemos ver como el endpoint devuelve algo distinto, frenando la ejecución de la accion:

```
Token is required
```

Sin embargo, al agregar un token valido como header `"Authorization"`, la accion si es ejecutada. Si agregamos un string cualquiera (`aaaaa`) vemos como nos dice que el token no tiene el formato adecuado

```
Invalid Token format
```

Para esto, agregaremos un par de `User` que tengan el role `Admin` y uno que tenga el role `Tester`. Si enviamos el token del `Admin`, recibimos lo siguiente:

```
it's allright!!
```

A pesar de que aun no cuenta con mucha logica nuestro sistema, podemos ver los filtros en accion.

## Que falta hacer? (Como deberes)

* Cuando se agrega un nuevo homework, deberia agregarse un homework para un usuario especifico. Este usuario puede obtenerse a partir del token que es enviado.

# Por que usar Tokens?

La forma preferida hoy en día para autenticarse desde el front-end ya sea web o mobile es la de tokens por las siguientes razones:

**Escalabilidad de servidores**: El token que se envía al servidor es independiente, contiene toda la información necesaria para la autenticación del usuario, por lo que añadir más servidores a la granja es una tarea fácil ya que no depende de una sesión compartida.

**Bajo acoplamiento**: Su aplicación front-end no se acopla con el mecanismo de autenticación específico, el token se genera desde el servidor y su API se construye de una manera que se pueda entender y hacer la autenticación.

**Móvil amigable**: Al tener una forma estándar para autenticar a los usuarios va a simplificar nuestra vida si decidimos consumir la API de servicios de fondo desde aplicaciones nativas como IOS, Android y Windows Phone.

# Mas Info

* [Filters](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-2.1)
* [Authorization in Core](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/index?view=aspnetcore-2.1)
* [JWT Framework](http://enmilocalfunciona.io/construyendo-una-web-api-rest-segura-con-json-web-token-en-net-parte-i/)
* [JWT Framework - 2](http://codigoenpuntonet.blogspot.com/2016/09/inicio-de-sesion-basado-en-tokens-con.html)
