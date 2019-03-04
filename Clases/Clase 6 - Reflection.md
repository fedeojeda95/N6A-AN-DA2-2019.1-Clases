# Reflection

## Introducción a Reflection

**Reflection es la habilidad de un lenguaje de inspeccionar y llamar dinamicamente a clases, metodos, atributos, etc. en tiempo de ejecución**

Mas concretamente para .net, reflection es la habilidad de un programa de autoexaminarse con el objetivo de encontrar ensamblados (`.dll`), módulos, o información de tipos en tiempo de ejecución. En otras palabras, a nivel de código vamos a tener clases y objetos, que nos van a permetir referenciar a ensamblados, y a los tipos que se encuentran contenidos.

Se dice que un programa se refleja en sí mismo (de ahí el termino "reflección"), a partir de extraer metadata de sus assemblies y de usar esa metadata para ciertos fines. Ya sea para informarle al usuario o para modificar su comportamiento.

Al usar Reflection en C#, estamos pudiendo obtener la información detallada de un objeto, sus métodos, e incluso crear objetos e invocar sus métodos en tiempo de ejecución, sin haber tenido que realizar una referencia al ensamblado que contiene la clase y a su namespace.

Específicamente lo que nos permite usar Reflection es el namespace `System.Reflection`, que contiene clases e interfaces que nos permiten manejar todo lo mencionado anteriormente: ensamblados, tipos, métodos, campos, crear objetos, invocar métodos, etc.

## Estructura de un assembly/ensamblado

Primero es importante aclarar que es un assembly. Este es el output del programa luego de ser compilado, generalmente en un .dll. Es la unidad minima en .NET.

Los assemblies contienen módulos, los módulos contienen tipos y los tipos contienen miembros. Reflection provee clases para encapsular estos elementos. 

Como dijimos, es posible utilizar reflection para crear dinámicamente instancias de un tipo, obtener el tipo de un objeto existente e invocarle métodos y acceder a sus atributos de manera dinámica. También se puede obtener toda la informacion de los elementos que vemos en la siguiente imagen.
 
![alt text](http://www.codeproject.com/KB/cs/DLR/structure.JPG)

## ¿Para qué podría servir?

Supongamos por ejemplo, que necesitamos que nuestra aplicación soporte diferentes tipos de loggers como sistema de auditoria del sistema (mecanismos para registrar datos/eventos que van ocurriendo en el flujo del programa). Además, supongamos que hay desarrolladores terceros que nos brindan sus implementaciones en .dll externa que escribe información de logging y la envía a un servidor. En ese caso, tenemos dos opciones:

1) Podemos referenciar al ensamblado directamente y llamar a sus métodos (como hemos hecho siempre) 
2) Podemos usar Reflection para cargar el ensamblado y llamar a sus métodos a partir de sus interfaces.

En el primer caso, cada vez que queremos agregar un nuevo tipo de logger. Tenemos que ir al programa, agregar la referencia, modificar el codigo, y recompilar todo nuestro programa de vuelta. Si tenemos usuarios, todos tienen que instalarse el sistema de vuelta.

Por otro lado, si quisieramos que nuestra aplicación sea lo más desacoplada posible, de manera que otros loggers puedan ser agregados (o 'plugged in' -de ahí el nombre plugin-) de forma sencilla y **SIN RECOMPILAR** la aplicación, es necesario elegir la segunda opción. Lo usuarios podrian simplemente descargarse el .ddl necesario y el programa lo podria utilizar. 

Por ejemplo, podríamos hacer que el usuario elija (a medida que está usando la aplicación), y descargue la .dll de logger para elegir usarla en la aplicación. La única forma de hacer esto es a partir de Reflection. De esta forma, podemos cargar ensamblados externos a nuestra aplicación, y cargar sus tipos en tiempo de ejecución. 

Este es tan solo uno de muchos casos de uso en los que reflection es sumamente util.

## Favoreciendo el desacoplamiento

Lo que es importante para lograr el desacoplamiento de tipos externos, es que nuestro código referencie a una Interfaz, que es la que toda .dll externa va a tener que cumplir. 

Tiene que existir entonces ese contrato previo, de lo contrario, no sería posible saber de antemano qué metodos llamar de las librerías externas que poseen clases para usar loggers.

Por ejemplo, volviendo al caso de los loggers. Podriamos definir que todos los .ddl que utilicemos tengan que tener el metodo `logString(String message)`. Si no definimos esto, es imposible que sepamos que metodos utiliza cada uno, ya que se pueden agregar posteriormente. Para cada .ddl con el cual se interactue, sabremos que podemos llamar al metodo `logString`.

## Ejemplo en ```C#```

Ahora tomaremos el ensamblado (dll) Homeworks.Domain y lo moveremos a una carpeta que sepamos para poder inpecionarlo por ejemplo: `c:\pruebas\` en Windows, o `~/pruebas` en MacOS/Linux.

Lo primero que probaremos será la capacidad de inspección que ofrece reflection sobre los assemblies. Para ello, en el método Main agregaremos el siguiente código. **Nota**: Se deberán agregar algunas sentencias `using` para el funcionamiento.

Primero inspeccionamos el assembly:

```C#
using System;
using System.Reflection;

namespace Reflection
{
    class Program
    {
        static void Main(string[] args)
        {
            // Cargamos el assembly de ejemplo en memoria (Path para windows)
            Assembly miAssembly = Assembly.LoadFile(@"c:\Pruebas\Homeworks.Domain.dll");
            // Podemos ver que Tipos hay dentro del assembly
            foreach (Type tipo in miAssembly.GetTypes())
            {
                Console.WriteLine(string.Format("Clase: {0}", tipo.Name));

                Console.WriteLine("Propiedades");
                foreach (PropertyInfo prop in tipo.GetProperties())
                {
                    Console.WriteLine(string.Format("\t{0} : {1}", prop.Name, prop.PropertyType.Name));
                }
                Console.WriteLine("Constructores");
                foreach (ConstructorInfo con in tipo.GetConstructors())
                {
                    Console.Write("\tConstructor: ");
                    foreach (ParameterInfo param in con.GetParameters())
                    {
                        Console.Write(string.Format("{0} : {1} ", param.Name, param.ParameterType.Name));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.WriteLine("Metodos");
                foreach (MethodInfo met in tipo.GetMethods())
                {
                    Console.Write(string.Format("\t{0} ", met.Name));
                    foreach (ParameterInfo param in met.GetParameters())
                    {
                        Console.Write(string.Format("{0} : {1} ", param.Name, param.ParameterType.Name));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
            Console.ReadLine();
        }
    }
}
```

Analizar detenidamente la salida.

Acabamos de ver como mediante reflection es posible investigar el contenido de un assembly, obtener su información, como conocer las propiedades, los constructores y los métodos de cada clase. 

![alt text](../imgs/reflectionDomain.PNG)

## Instanciando tipos dinámicamente

Como ya hemos mencionado, otra de las pricipales ventajas de reflection es que además de poder conocer información sobre los tipos dentro de un assembly, permite trabajar con ellos de manera dinámica. Para ejemplificarlo, vamos a crear un objeto de la clase User utilizando un constructor con parámetros, le vamos a cambiar el valor de una de sus propiedades y luego le invocaremos un método. Todo esto desde nuestra aplicación de consola, que NO tiene una referencia a la dll con las clases, por lo que todo se hará de manera dinámica.

Cambiemos el contenido del Main por el siguiente:

```C#
using System;
using System.Reflection;

namespace Reflection
{
    class Program
    {
        static void Main(string[] args)
        {
            // Cargamos el assembly de ejemplo en memoria
            Assembly testAssembly = Assembly.LoadFile(@"/Users/federicoojeda/Desktop/Homeworks.Domain.dll");
   
            // Obtenemos el tipo que representa a User
            Type userType = testAssembly.GetType("Homeworks.Domain.User");
            
            // Creamos una instancia de User
            object userInstance = Activator.CreateInstance(userType);
            
            // Lo mostramos
            Console.WriteLine(userInstance.ToString());
            
            //Invocamos al método
            MethodInfo met = userType.GetMethod("IsValid");
            Console.WriteLine(string.Format("Es Valido: {0}", met.Invoke(userInstance, null)));
            
            //También podemos cambiar su nombre
            PropertyInfo prop = userType.GetProperty("Name");
            prop.SetValue(userInstance, "Manuel", null);
            
            Console.WriteLine(prop.GetValue(userInstance));

            Console.ReadLine();  
        }
    }
}
```

**IMPORTANTE:** aquí estamos asumiendo los nombres de los métodos y llamandolos directamente pasandole Strings como parámetros. Esto en un caso más real no sería correcto, ya que primero deberíamos asegurarnos de que el tipo que queremos instanciar cumple con la interfaz (es decir, tiene los métodos), que queremos usar.

Esto se puede hacer preguntando de la siguiente forma:

```C#

typeof(IMyInterface).IsAssignableFrom(typeof(MyType))
typeof(MyType).GetInterfaces().Contains(typeof(IMyInterface))

```

Es importante destacar que como estamos manejando tipos dinamicos, hay que tener un excelente manejo de excepciones para manejar los errores. Estos metodos (como `CreateInstance` por ejemplo) lanzan excepciones ante distintos errores. Por ejemplo, prueben pasarle parametros incorrectos a `CreateInstance` y veran que se lanza: 

```
Unhandled Exception: System.MissingMethodException: Constructor on type 'Homeworks.Domain.User' not found
```

Es importante leer la documentacion del framework y pataforma para ver las excepciones que son lanzadas. 

# Inyección de Dependencias (ID)

## ¿Qué es una dependencia?

En software, cuando hablamos de que dos piezas, componentes, librerías, módulos, clases, funciones (o lo que se nos pueda ocurrir relacionado al área), son dependientes entre sí, nos estamos refiriendo a que uno requiere del otro para funcionar. A nivel de clases, significa que una cierta **'Clase A'** tiene algún tipo de relación con una **'Clase B'**, delegándole el flujo de ejecución a la misma en cierta lógica.
Ej: **UserLogic** *depende de* **UserRepository**

##### Business Logic -> Repository
 
 ```c#
  public class UsersLogic : IUsersLogic
  {
        public IRepository<User> users;

        public UsersLogic() {
            HomeworksContext context = ContextFactory.GetNewContext();
            repository = new Repository<User>(context);
        }
  }
 ```
 
**¿Notaron el problema (común entre ambas porciones de código) que existe?**
 
El problema reside en que ambas piezas de código tiene la responsabilidad de la instanciación de sus dependencias. Nuestras capas no deberían estar tan fuertemente acopladas y no deberían ser tan dependientes entre sí. Si bien el acoplamiento es a nivel de interfaz (tenemos IUsersLogic e IRepository), la tarea de creación/instanciación/"hacer el new" de los objetos debería ser asignada a alguien más. Nuestras capas no deberían preocuparse sobre la creación de sus dependencias.

**¿Por qué? ¿Qué tiene esto de malo?**:-1:

1. Si queremos **reemplazar** por ejemplo nuestro `UsersLogic` **por una implementación diferente**, debemos modificar nuestro controller. Si queremos reemplazar nuestro `UserRepository` por otro, tenemos que modificar nuestra clase `UserLogic`.

2. Si la UserLogic tiene sus propias dependencias, **debemos configurarlas dentro del controller**. Para un proyecto grande con muchos controllers, el código de configuración empieza a esparcirse a lo largo de toda la solución.

3. **Es muy difícil de testear, ya que las dependencias 'estan hardcodeadas'.** Nuestro controller siempre llama a la misma lógica de negocio, y nuestra lógica de negocio siempre llama al mismo repositorio para interactuar con la base de datos. En una prueba unitaria, se necesitaría realizar un mock/stub las clases dependientes, para evitar probar las dependencias. Por ejemplo: si queremos probar la lógica de `UserLogic` sin tener que depender de la lógica de la base de datos, podemos hacer un mock de `UserRepository`. Sin embargo, con nuestro diseño actual, al estar las dependencias 'hardcodeadas', esto no es posible. Lo solucionamos (en parte) previamente teniendo varios constructores. 

Una forma de resolver esto es a partir de lo que se llama, **Inyeccion de Dependencias**. Vamos a inyectar la dependencia de la lógica de negocio en nuestro controller, y vamos a inyectar la dependencia del repositorio de datos en nuestra lógica de negocio. **Inyectar dependencias es entonces pasarle la referencia de un objeto a un cliente, al objeto dependiente (el que tiene la dependencia)**. Significa simplemente que la dependencia es encajada/empujada en la clase desde afuera. Esto significa que no debemos instanciar (hacer new), dependencias, dentro de la clase.

Esto lo haremos a partir de un parámetro en el constructor, o de un setter. Por ejemplo:

 ```c#
  public class UserLogic : IUserLogic
  {
        public IRepository<User> users;

        public UserLogic(IRepository<User> users)
        {
            this.users = users;
        }
  }
 ```
Esto es fácil lograrlo usando interfaces o clases abstractas en C#. Siempre que una clase satisfaga la interfaz,voy a poder sustituirla e inyectarla.

## Ventajas de ID

Logramos resolver lo que antes habíamos descrito como desventajas o problemas.

1. **Código más limpio:** El código es más fácil de leer y de usar.
2. **Testeabilidad:** Nuestro software termina siendo más fácil de Testear ya que es mas facil mockear las dependencias. 
3. **Es más fácil de modificar:** Nuestros módulos son flexibles a usar otras implementaciones. Desacoplamos nuestras capas.
4. **Permite NO Violar SRP:** Permite que sea más fácil romper la funcionalidad coherente en cada interfaz. Ahora nuestra lógica de creación de objetos no va a estar relacionada a la lógica de cada módulo. Cada módulo solo usa sus dependencias, no se encarga de inicializarlas ni conocer cada una de forma particular.
5. **Permite NO Violar OCP:** Por todo lo anterior, nuestro código es abierto a la extensión y cerrado a la modificación. El acoplamiento entre módulos o clases es siempre a nivel de interfaz.

## Problema de la construcción de dependencias:

Vimos como inyectar dependencias a través del constructor. Sin embargo, ahora tenemos un problema, el cuál es dónde construir nuestras dependencias (dónde hacer el **new**).

Para resolver resolver este problema utilizaremos el ServiceProvider (DependencyInjection) que nos brinda WebApi que nos permite inyectar dependencias.

## Adaptación del proyecto

### Ubicación de las interfaces

En las clases anteriores, tuvimos que crear interfaces para cada una de las clases que tenemos. Por ejemplo, nuestro `HomeworksLogic` implementa una interfaz llamada `IHomeworksLogic`. Cuando se crea el controlador, se recibe una instancia de esta interfaz y es la que se utiliza. 

Sin embargo, cuando lo creamos, nunca nos paramos a pensar donde deberian estar ubicadas estas interfaces. Hasta ahora las ubicamos en el mismo proyecto que las implementaciones, pero es esto correcto? 

La respuesta es que no. De poco nos sirve si estamos agregando una referencia directa al modulo en el cual esta la implementacion. Nuestro objetivo tiene que ser no conocer la implementacion de ninguna manera, solo conocer el "contrato" que tiene que cumplir la dependencia mediante la interfaz.

Como podemos hacer esto? La solución es mover las interfaces a otro proyecto. Por ejemplo, las interfaces de las clases de `Homeworks.BusinessLogic` estaran en un paquete llamado `Homeworks.BusinessLogic.Interfaces`. 

El objetivo de esto es que `Homeworks.WebApi` nunca tenga que importar `Homeworks.BusinessLogic`, si no que solo `Homeworks.BusinessLogic.Interfaces` (el "contrato"). 

Ahora debemos hacer eso tanto para `BusinessLogic` como para `DataAccess`, moviendo las interfaces y agregando las referencias correspondientes. Todavia no podemos remover todas las referencias a los proyectos ya que aun seguimos creando instancias en los constructores. Despues de lo que haremos en la siguiente parte se podran remover.

### Preparando el proyecto de WebApi

Lo primero que vamos a hacer es modificar nuestros constructores del las clases DataAccess, Controllers y Logic, para permitir la ID. Esto significa agregar constructores que reciban como parametro la interfaz. En caso de no haberlo hecho (lo hicimos en clases previas) debemos realizarlo ahora si o si.

## Agregando inyeccion de dependencias a nuestro proyecto

***En Nuestro StartUp***

En el metodo `ConfigureServices`, vamos a decirle a nuestra api que implementacion tiene que usar para cada interfaz. Este metodo se encuentra en la clase `Startup` dentro de nuestro proyecto `Homeworks.WebApi`. Esta clase esta encargada de hacer todo el setup y configurar el proyecto antes de arrancar. Se ejecuta en un periodo de configuracion del proyecto previo a empezar a recibir requests `HTTP`. En particular este metodo, sirve para configurar servicios, como su nombre lo indica. 

`ASP .NET Core` automaticamente sabra como crearlas, y las pasara como parametros del constructor. Este es el mecanismo de inyeccion de dependencias ya provisto por el framework.

Nuestro metodo `ConfigureServices` quedará de esta manera:

```C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

    services.AddDbContext<DbContext, HomeworksContext>(
        o => o.UseSqlServer(@"Server=127.0.0.1,1401;Database=HomeworksDB;User Id=sa;Password=YourStrong!Passw0rd;")
    );
    // services.AddDbContext<DbContext, HomeworksContext>(
    //     options => options.UseInMemoryDatabase("HomeworksDB"));
    services.AddScoped<IUsersLogic, UsersLogic>();
    services.AddScoped<IHomeworksLogic, HomeworksLogic>();
    services.AddScoped<IExerciseLogic, ExerciseLogic>();
    services.AddScoped<ISessionsLogic, SessionsLogic>();
    services.AddScoped<IRepository<Homework>, HomeworksRepository>();
    services.AddScoped<IRepository<Session>, SessionsRepository>();
    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
}
```
Se pueden ver algunas cosas a destacar aca:

***Para EF Core:*** 

```C#
services.AddDbContext<DbContext, HomeworksContext>(o => o.UseSqlServer(Configuration.GetConnectionString("HomeworksDB")));
```

Cada vez que un constructor reciba un DbContext, necesitamos que se inyecte un `HomeworksContext`. Con esto le indicamos que cuando se necesite un DbContext se inyecte un HomeworksContext y le va a pasar por el constructor un `DbContextOptions` configurado para usar el SqlServer con un connection string que se encuentra en el archivo de configuracion. Analogamente, se puede ver comentada la opcion para usar un `in-memory` database.

***Para las clases concretas:***

```c#
services.AddScoped<IUserLogic, UserLogic>();
```

Aca le estamos diciendo a nuestra WebApi, que cada vez que se presente `IUserLogic` como dependencia (parametro de un constructor en este caso), se cree una instancia de `UserLogic` y la inyecte.

***Para las clases genericas:***

```c#
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

Por ultimo, en el caso del repository, necesitamos decirle que cada vez que vea un `IRepository<T>` generico, inyecte un `Repository<T>` en esta instancia. Esto se va a hacer para todos los que no definimos previamente. Por ejemplo, como definimos que para `IRepository<Session>` se inyecte `SessionsRepository`, esto toma "precedencia" y se hace, **no** se inyecta un `Repository<Session>`.

***Archivo de Configuracion (appsettings.json)*** encontramos la siguiente linea:

```
"ConnectionStrings": {
    "HomeworksDB": "Server=.\\SQLEXPRESS;Database=HomeworksDB;Trusted_Connection=True;MultipleActiveResultSets=True;"
 },
```

### Obtener un servicio para un filtro

Para obtener un servicio dentro de un filtro se lo vamos a tener que pedir directamente al `httpcontext`, ya que no podemos inyectar servicio en los constructores de un filtro, especialmente si lo usamos como atributo.

Entonces si teniamos en nuestro filtro:

```c#
public void OnActionExecuting(ActionExecutingContext context)
{
    //CODIGO ..
    using (var sessions = new SessionLogic()) {
        //CODIGO ...
    }
    //CODIGO ...
}
```
Ahora para para pedir un servicio invocamos el siguiente metodo ```context.HttpContext.RequestServices.GetService(TIPO_DEL_SERVICIO_QUE_BUSCAMOS_INYECTAR)``` que nos retorna un object que es del tipo del servicio. Entonces pasamos a tener:

```c#
private void VerifyToken(Guid token, ActionExecutingContext context)
{
    using (var sessions = GetSessionLogic(context)) {
        // Code .....
    }
}

private ISessionsLogic GetSessionLogic(ActionExecutingContext context) {
    var typeOfSessionsLogic = typeof(ISessionsLogic);
    return context.HttpContext.RequestServices.GetService(typeOfSessionsLogic) as ISessionsLogic;
}
```

## Mejorando la inyeccion de dependencias utilizando `Reflection`

A pesar de que avanzamos mucho en desacoplar las capas del sistema, aun falta. El problema que tenemos es que aun tenemos una dependencia directa a todas las capaz desde `WebApi`. Es necesario conocer todas estas implementaciones ya que es desde ese proyecto donde las registramos. Idealmente, no deberia conocerlas, si no que cada capa sea completamente independiente de las restantes. 

Para resolver esto vamos a crear un nuevo paquete (`Homeworks.Factory`) que se encargara de realizar estas inyecciones. Con lo que sabemos de `Reflection`, podemos ir a buscar cada una de las `.dll`s de las capas e instanciar cada una de las implementaciones de sus interfaces.  

Crearemos dos clases: `BusinessLogicFactory` para inyectar las dependencias de `Homeworks.BusinessLogic` y `RepositoryFactory` para inyectar las dependencias de `Homeworks.DataAccess`.

**BusinessLogicFactory:**

```C#
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Homeworks.BusinessLogic.Interface;
using Homeworks.DataAccess.Interface;
using Homeworks.DataAccess;

namespace Homeworks.ServiceFactory
{
    public class BusinessLogicFactory
    {
        private string assemblyPath;

        public BusinessLogicFactory()
        {
            assemblyPath = @"PATH_TO_DLL";
        }

        public Type GetImplementation<T>() where T : class
        {
            var typeOfInterface = typeof(T);
            Type implementationType = GetInstanceOfInterface<T>();
            return implementationType;
        }

        private Type GetInstanceOfInterface<Interface>(params object[] args)
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(assemblyPath);
                IEnumerable<Type> implementations = GetTypesInAssembly<Interface>(assembly);
                if (implementations.Count() <= 0)
                {
                    throw new NullReferenceException(assemblyPath + " don't contains Types that extend from " + nameof(Interface));
                }

                return implementations.First();
            }
            catch (Exception e)
            {
                throw new Exception("Can't load assembly " + assemblyPath, e);
            }
        }

        private static IEnumerable<Type> GetTypesInAssembly<Interface>(Assembly assembly)
        {
            List<Type> types = new List<Type>();
            foreach (var type in assembly.GetTypes())
            {
                var interfaceType = typeof(Interface);
                if (typeof(Interface).IsAssignableFrom(type))
                {
                    types.Add(type);
                }
            }
            return types;
        }
    }
}
```

**RepositoryFactory:**

```c#
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Homeworks.BusinessLogic.Interface;
using Homeworks.DataAccess.Interface;
using Homeworks.DataAccess;

namespace Homeworks.ServiceFactory
{
    public class RepositoryFactory
    {
        private string assemblyPath;

        public RepositoryFactory()
        {
            assemblyPath = @"PATH_TO_DLL";
        }

        public Type GetImplementation<T>() where T : class
        {
            Type implementationType = GetInstanceOfInterface<T>();
            return implementationType;
        }

        public Type GetImplementation(Type typeOfInterface)
        {
            Type implementationType = GetInstanceOfInterface(typeOfInterface);
            return implementationType;
        }

        private Type GetInstanceOfInterface<Interface>()
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(assemblyPath);
                IEnumerable<Type> implementations = GetTypesInAssembly<Interface>(assembly);
                if (implementations.Count() <= 0)
                {
                    throw new NullReferenceException(assemblyPath + " don't contains Types that extend from " + nameof(Interface));
                }

                return implementations.First();
            }
            catch (Exception e)
            {
                throw new Exception("Can't load assembly " + assemblyPath, e);
            }
        }

        private Type GetInstanceOfInterface(Type interfaceToRegister)
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(assemblyPath);
                IEnumerable<Type> implementations = GetTypesInAssembly(interfaceToRegister, assembly);
                if (implementations.Count() <= 0)
                {
                    throw new NullReferenceException(assemblyPath + " don't contains Types that extend from " + nameof(interfaceToRegister));
                }

                return implementations.First();
            }
            catch (Exception e)
            {
                throw new Exception("Can't load assembly " + assemblyPath, e);
            }
        }

        private static IEnumerable<Type> GetTypesInAssembly<Interface>(Assembly assembly)
        {
            List<Type> types = new List<Type>();
            foreach (var type in assembly.GetTypes())
            {
                var interfaceType = typeof(Interface);
                if (typeof(Interface).IsAssignableFrom(type))
                {
                    types.Add(type);
                }
            }
            return types;
        }

        private static IEnumerable<Type> GetTypesInAssembly(Type interfaceToRegister, Assembly assembly)
        {
            List<Type> types = new List<Type>();
            foreach (var type in assembly.GetTypes())
            {
                if (IsAssignableToGenericType(type, interfaceToRegister))
                {
                    types.Add(type);
                }
            }
            return types;
        }

        private static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            Type baseType = givenType.BaseType;
            Type[] interfaceTypes = givenType.GetInterfaces();

            // Checkeamos que haya una interfaz del tipo que sea generica y que sea top level (herede directamente de Object).
            // De esta manera nos evitamos que las subclases tambien cumplan la condición.
            foreach (Type it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType && baseType == typeof(Object))
                {
                    return true;
                }
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            return false;
        }
    }
}
```

La logica que llevamos a cabo en estas clases es la siguiente:

Como recordamos, al hacer `services.AddScoped<IUsersLogic, UsersLogic>();` estamos diciendole que cada vez que haya un `IUsersLogic`, instancie un `UsersLogic` y lo utilice en el constructor. Nuestro objetivo es que solo diciendole *registra `IUsersLogic`*, nuestro sistema puede ir a buscar al `.dll` correspondiente la implementación de de `IUsersLogic` y registrarla. Los pasos son los siguientes:

1- Llamar a un metodo con la interfaz que queremos registrar como "parametro" (parametro mismo o *generics*)
2- Ir al `.dll` correspondiente y obtener sus clases
3- Buscar que clase implementa la interfaz pasada como parametro
4- Utilizar esa clase para registrarla como servicio utilizando `services.AddScoped(Type1, Type2);` de `IServiceCollection`.

En las clases anteriores (`BusinessLogicFactory` y `RepositoryFactory`) se implementan los puntos 2 y 3. El metodo `GetImplementation` recibe un tipo por parametro y devuelve quien lo implementa de un `.dll` especifico.

Ahora debemos implementar los pasos 1 y 4. Esto lo haremos mediante el mecanismo de c# conocido como metodos de extensión. Estos haran que agregar los servicios sea mucho mas comodo. Creamos una clase llamada `ServiceExtension` y agregamos nuestros metodos de extension ahi. *Extenderemos* la funcionalidad de la clase `IServiceCollection`, agregandole metodos.

```c#
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Homeworks.ServiceFactory
{
    public static class BLServiceCollectionServiceExtensions
    {
        public static IServiceCollection AddLogic<T>(this IServiceCollection service)
            where T : class
        {
            BusinessLogicFactory businessLogicFactory = new BusinessLogicFactory();
            Type typeToRegister = businessLogicFactory.GetImplementation<T>();
            return service.AddScoped(typeof(T), typeToRegister);
        }

        public static IServiceCollection AddRepository<T>(this IServiceCollection service)
            where T : class
        {
            RepositoryFactory repositoryFactory = new RepositoryFactory();
            Type typeToRegister = repositoryFactory.GetImplementation<T>();
            return service.AddScoped(typeof(T), typeToRegister);
        }

        public static IServiceCollection AddRepository(this IServiceCollection service, Type type)
        {
            RepositoryFactory repositoryFactory = new RepositoryFactory();
            Type typeToRegister = repositoryFactory.GetImplementation(type);
            return service.AddScoped(type, typeToRegister);
        }
    }
}
```

Para mas informacion sobre los metodos de extension, leer [aqui](https://www.tutorialsteacher.com/csharp/csharp-extension-method) o [aqui](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods).

Estos metodos podran ser llamados en el `service`. Como podemos ver, estos usan las clases `XFactory` que creamos antes, y registran el servicio utilizando el metodo que ya conociamos `service.AddScoped(type1, type2)`.

Volviendo a nuestra clase ***StartUp***, la actualizamos para usar nuestros metodos de extension.

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

    services.AddDbContext<DbContext, HomeworksContext>(
        o => o.UseSqlServer(@"connectionString")
    );

    // services.AddDbContext<DbContext, HomeworksContext>(
    //     options => options.UseInMemoryDatabase("HomeworksDB"));

    services.AddLogic<IUsersLogic>();
    services.AddLogic<IHomeworksLogic>();
    services.AddLogic<IExerciseLogic>();
    services.AddLogic<ISessionsLogic>();

    services.AddRepository<IRepository<Homework>>();
    services.AddRepository<IRepository<Session>>();
    services.AddRepository(typeof(IRepository<>));
}
```

## Mas Info

* [StartUp](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup?view=aspnetcore-2.1)
* [Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1)
