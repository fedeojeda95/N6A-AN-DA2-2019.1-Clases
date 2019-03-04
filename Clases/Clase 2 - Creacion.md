# Clase 2

## Instalacion del ambiente y dependencias


* [.NET Core 2.2 downloads](https://www.microsoft.com/net/download/dotnet-core/2.2)
* [Visual Studio Code](https://code.visualstudio.com/Download)
  * [Extensiones para .Net Core/C#](https://code.visualstudio.com/docs/languages/csharp)
* [Postman](https://www.getpostman.com/apps)

Cualquiera de estos SDKs/Herramientas funciona para cualquier sistema operativo (Windows/MacOS/Linux)

## Cheat sheet (Lista de comandos desde la consola)

Commando | Resultado
------------ | -------------
`dotnet new sln` | Creamos solucion (principalmente útil para VisualStudio, cuando queremos abrir la solución y levantar los proyectos asociados)
`dotnet new webapi -n "Nombre del Proyecto"` | Crear un nuevo Proyecto del template WebApi
`dotnet sln add` | Asociamos el proyecto creado al .sln
`dotnet sln list` | Vemos todos los proyectos asociados a la solución
`dotnet new classlib -n "Nombre del Proyecto"` | Crear un nueva libreria (standard)
`dotnet add "Nombre del Proyecto 1".csproj reference "Nombre del Proyecto 2".csproj` | Agrega una referencia al Proyecto 1 del Proyecto 2
`dotnet add package "Nombre del Package"` | Instala el Package al proyecto actual. Similar a cuando se agregaban paquetes de Nuget en .NET Framework.
`dotnet build` | Compilar y generar los archivos prontos para ser deplegados (_production build_)
`dotnet run` | Compilar y correr el proyecto

Siempre que se necesite ayuda con el comando o no se sabe cual usar, la mejor ayuda es correr `dotnet [COMANDO] -h` para una explicacion de los parametros del comando o cual es su funcionalidad. Tambien se puede correr `dotnet -h` para poder ver los comandos disponibles

### Es necesario crear una solucion? (SLN)

No, no es necesario. Sin embargo, tener una trae varias utilidades:
* Si usamos Visual Studio 2017, es necesario crearla para levantar todos los proyectos dentro del IDE
* Aunque no usemos VS2017, tener una solucion permite crear/compilar/manejar todos los proyectos involucrados juntos, sin tener que correr cada uno, por ejemplo. Se maneja todo como una unica unidad.

Para mas informacion, se puede leer [aquí](https://stackoverflow.com/questions/42730877/net-core-when-to-use-dotnet-new-sln)

## Creacion de proyecto HomeworkWebApi

A continuación crearemos un proyecto de ejemplo, sobre el cual seguiremos trabajando y seguiremos agregnadole funcionalidad.

### Creamos el sln para poder abrirlo en vs2017 y otras utilidades (opcional)

```
dotnet new sln
```

### Creamos el proyecto webapi y lo agregamos al sln
```
dotnet new webapi -au none -n Homeworks.WebApi
dotnet sln add Homeworks.WebApi
```

### Creamos la libreria businesslogic y la agregamos al sln

```
dotnet new classlib -n Homeworks.BusinessLogic
dotnet sln add Homeworks.BusinessLogic
```

### Creamos la libreria dataaccess y la agregamos al sln

```
dotnet new classlib -n Homeworks.DataAccess
dotnet sln add Homeworks.DataAccess
```

### Creamos la libreria domain y la agregamos al sln

```
dotnet new classlib -n Homeworks.Domain
dotnet sln add Homeworks.Domain
```

### Agregamos referencias de los proyectos a la webapi

```
dotnet add Homeworks.WebApi reference Homeworks.DataAccess
dotnet add Homeworks.WebApi reference Homeworks.Domain
dotnet add Homeworks.WebApi reference Homeworks.BusinessLogic
```

### Agregamos la referencia del domain al dataaccess

```
dotnet add Homeworks.DataAccess reference Homeworks.Domain
```

### Agregamos las referencias de domain y dataaccess a businesslogic

```
dotnet add Homeworks.BusinessLogic reference Homeworks.Domain
dotnet add Homeworks.BusinessLogic reference Homeworks.DataAccess
```

### Descargamos Entity Framework Core

Nos movemos a la carpeta WebApi (`cd Homeworks.WebApi`)

```
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

Nos movemos a la carpeta dataaccess (`cd Homeworks.DataAccess`)

```
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

### Estructura de los proyectos

Antes de comenzar a progamar, es importante saber como esta formado cada uno de los proyectos. Tenemos 3 grandes elementos (o conjuntos de) en el proyecto ademas de nuestro codigo.

**X.csproj**

Este es el archivo de configuracion del proyecto. Aqui se definen varias cosas como: 

* Version del framework usado (netcore2.2 por ejemplo)
* Dependencias a otros proyectos dentro de una solucion y el path a ellos (por ejemplo `<ProjectReference Include="..\Homeworks.Domain\Homeworks.Domain.csproj" />` indica que hay una diferencia el proyecto `Homeworks.Domain`)
* Dependencias de paquetes externos de nuget. (Por ejemplo: `<PackageReference Include="Microsoft.EntityFrameworkCore" Version="2.2.2" />` indica que este proyecto utiliza `EntityFrameworkCore`). Cuando se toma el proyecto nuevo, se utiliza esta informacion para bajar los archivos necesarios de nuget packages. 

Este archivo vendria a tener una funcionalidad similar a la que tienen archivos similares en otros lenguajes/plataformas, como Javascript con el `package.json`

**/bin** 

Aca se encuentran todos los archivos compilados. Cada vez que se hace `dotnet run` o `dotnet build`, se compila el proyecto y se generan los `.dll` correspondientes. Lo mejor es ignorar de git esta carpeta

**/obj**

Son varios archivos que utiliza despues el compilador para compilar el proyecto. Son una especie de "archivos intermedios". Tambien conviene ignorar en git esta carpeta.

### Codigo basico

Agregaremos un poco de funcionalidad muy basica al sistema que tenemos para poder probar que este funcionando correctamente. 

**En Homeworks.WebApi**

Este proyecto sera donde tengamos los controllers. Estos tendran la responsabilidad de:

* Definir las rutas
* Obtener los datos que son enviados en las requests (ya sea por la URL, por headers, por el body, etc)


Primero borraremos la clase `ValuesController` de adentro de `Controllers` y crearemos la clase `HomeworksController`. Dentro de esta clase, agregaremos una unica ruta:

```c#
namespace Homeworks.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeworksController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult Get()
        {
            return Ok();
        }
    }
}
```

Esta ruta lo unico que hace es retornar `Ok()`. Esto es un metodo que hace que la respuesta sea de un codigo `200`. Como no le pasamos ningun parametro, la respuesta es vacia. 

Para probarlo, podemos utilizar el browser. Si corremos `dotnet run` dentro de `Homeworks.WebApi`, el proyecto se inicia. Si vamos a `https://localhost:5001/api/homeworks` podemos ver que *no* falla, solo queda en blanco, lo cual es exitoso.

**Dentro de Homeworks.Domain**

Crearemos la clase que representa a nuestros deberes, `Homeworks`.

```c#
using System;
using System.Collections.Generic;

namespace Homeworks.Domain
{
    public class Homework
    {       
        public Guid Id { get; set; }
        public DateTime DueDate { get; set; }
        public string Description { get; set; }
        public List<Exercise> Exercises { get; set; }

        public Homework()
        {
            Id = Guid.NewGuid();
            Exercises = new List<Exercise>();
        }
    }
}
```

Nuestros "Deberes" (`Homework`) tienen una lista de ejercicios. Debido a esto, tambien debemos crear la clase `Exercise`

```c#
using System;

namespace Homeworks.Domain
{
    public class Exercise
    {
        public Guid Id {get; set;}
        public string Problem {get; set;}
        public int Score {get; set;}

        public Exercise() {
            Id = Guid.NewGuid();
        }
    }
}
```

Ambas clases son muy simples, asi que no necesitan explicacion alguna. Lo unico a explicar es el `Id`, el cual es una instancia de Guid. Recomendamos buscar que es un Guid si no lo saben, ya que es la mejor opcion para los identificadores de una entidad.


**Dentro de Homeworks.BusinessLogic**

Crearemos una clase llamada `HomeworksLogic`, la cual tiene la logica de nuestros "Deberes". Esta clase no tendra ninguna logica aun.

```c#
using System;

namespace Homeworks.BusinessLogic
{
    public class HomeworksLogic
    {
    }
}
```

**Dentro de Homeworks.DataAccess**

Dentro del proyecto de DataAccess, crearemos `HomeworksRepository`. Este seria el encargado de devolvernos los datos necesarios que tengamos guardados. Como no tenemos logica alguna, simplemente crearemos un par de objetos *dummy* y los devolveremos en una lista. El codigo es bastante directo, simplemente creamos dos `Exercise` y dos `Homework`

```c#
using System;
using System.Collections;
using System.Collections.Generic;
using Homeworks.Domain;

namespace Homeworks.DataAccess
{
    public class HomeworksRepository
    {
        public List<Homework> GetHomeworks() {
            List<Homework> homeworks = new List<Homework>();
            
            Exercise firstExercise = CreateExercise("firstProblem", 5);
            Exercise secondExercise = CreateExercise("secondProblem", 6);

            Homework firstHomework = CreateHomework("firstDescription", firstExercise);
            homeworks.Add(firstHomework);

            Homework secondHomework = CreateHomework("secondDescription", secondExercise);
            homeworks.Add(secondHomework);

            return homeworks;
        }

        private Exercise CreateExercise(String problem, int score) {
            Exercise exercise = new Exercise();
            exercise.Problem = problem;
            exercise.Score = score;
            return exercise;
        }

        private Homework CreateHomework(String description, Exercise exercise) {
            Homework homework = new Homework();
            homework.Exercises.Add(exercise);
            homework.DueDate = DateTime.Now;
            homework.Description = description;
            return homework;
        }
    }
}
```

**Conectando todo**

Ahora es hora de conectar todo. Haremos que `HomeworksController` tenga una instancia de `HomeworksLogic`, y que esta ultima tenga una instancia de `HomeworksRepository`. Cada una llamara al metodo de cada clase.

*Controller:*

```c#
[Route("api/[controller]")]
[ApiController]
public class HomeworksController : ControllerBase
{
    private HomeworksLogic homeworksLogic;

    public HomeworksController() {
        homeworksLogic = new HomeworksLogic();
    }

    // GET api/homeworks
    [HttpGet]
    public ActionResult Get()
    {
        List<Homework> homeworks = homeworksLogic.GetHomeworks();
        return Ok(homeworks);
    }
}
```

*Logic:*

```c#
    public class HomeworksLogic
    {
        private HomeworksRepository homeworksRepository;

        public HomeworksLogic() {
            homeworksRepository = new HomeworksRepository();
        }

        public List<Homework> GetHomeworks() {
            return homeworksRepository.GetHomeworks();
        }
    }
```

Ahora, si ejecutamos el codigo y vamos al browser (a la misma URL que usamos previamente), podemos ver como nos devuelve la informacion de 2 instancias de `Homework`:

```json
[
  {
    "id": "e23658f2-af27-4fb8-b94f-63bdd023c724",
    "dueDate": "2019-03-06T18:42:41.837-03:00",
    "description": "firstDescription",
    "exercises": [
      {
        "id": "88e28b6c-3b6e-44d4-8ae9-10d856890319",
        "problem": "firstProblem",
        "score": 5
      }
    ]
  },
  {
    "id": "dc874eb4-ee5c-4ac7-a133-db87f857e415",
    "dueDate": "2019-03-06T18:42:41.837094-03:00",
    "description": "secondDescription",
    "exercises": [
      {
        "id": "6b8848fb-cade-45e3-8635-d0f6651de1b4",
        "problem": "secondProblem",
        "score": 6
      }
    ]
  }
]
```

Ya tenemos el esqueleto de nuestro sistema andando.






