# Adaptación del proyecto

Luego de avanzar con el proyecto y de desarrollar algunas de las funcionalidades, haremos algunas mejoras a la base de codigo previo a seguir con el curso.

## Repositorios

Luego de haber avanzado en el proyecto, sobre todo en el proyecto `Homeworks.DataAccess`, se puede ver que hay logica bastante repetida. Todas estas clases que son llamadas `XRepository` tienen demasiado comportamiento en comun. Hay un conjunto de funcionalidades que siempre se repiten, lo unico que cambia es la entidad que es usada.

Por ejemplo, todas las clases `Repository` van a tener que tener el metodo `GetAll()` que retorne todos los objetos del tipo que esta manejando ese repositorio.

La solucion ideal seria abstraer esto, y que ese comportamiento repetido no tenga que ser codificado cada vez por nosotros. Podemos crear una clase base que implemente esto, y que reciba el tipo que estamos manejando utilizando Generics.

Esto es lo que haremos. Dentro de `Homeworks.DataAccess`, creamos la clase `Repository` y su interfaz `IRepository`.

**IRepository:**

```c#
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Homeworks.DataAccess
{
    public interface IRepository<T>: IDisposable
    {
        IEnumerable<T> GetAll();
        IEnumerable<T> GetByCondition(Expression<Func<T, bool>> expression);
        T GetFirst(Expression<Func<T, bool>> expression);
        void Add(T entity);
        void Update(T entity);
        void Remove(T entity);
        void Save();
    }
}
```

Como podemos ver, esta interfaz cuenta con todos los metodos que teniamos en nuestros repositorios. Con estos metodos se puede hacer todos los accesos a bases de datos que necesitamos. 

El unico metodo que podemos ver que ahora no tenemos es `Get(Guid id)`, el cual devolvia el primero que tenga el id pasado por parametro. Esto lo podemos sustituir por una llamada a `GetFirst(x => x.Id == id)` cuando usemos el repositorio.

**Repository:**

```c#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Homeworks.DataAccess
{
    public class Repository<T>: IRepository<T> where T : class
    {
        protected DbContext Context { get; set; }

        public Repository(DbContext Context)
        {
            this.Context = Context;
        }
 
        public virtual IEnumerable<T> GetAll()
        {
            return Context.Set<T>();
        }
 
        public virtual IEnumerable<T> GetByCondition(Expression<Func<T, bool>> expression)
        {
            return Context.Set<T>().Where(expression);
        }
 
        public virtual T GetFirst(Expression<Func<T, bool>> expression)
        {
            return Context.Set<T>().First(expression);
        }
 
        public void Add(T entity)
        {
            Context.Set<T>().Add(entity);
        }
 
        public void Update(T entity)
        {
            Context.Set<T>().Update(entity);
        }
 
        public void Remove(T entity)
        {
            Context.Set<T>().Remove(entity);
        }
 
        public void Save()
        {
            Context.SaveChanges();
        }

        // Disposing

        private bool disposedValue = false;

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
  }
}
```

Aca podemos ver como la implementacion del repositorio es bastante directa. Todos los metodos son similares a los que teniamos en los repositorios, solo que usamos el tipo generico `T`. 

---
**Nota:** Este enfoque puede no ser optimo en muchas situaciones, como en casos donde el comportamiento entre cada repositorio varia mucho, o hay muchas dependencias cruzadas. Cada caso necesita un analisis adecuado.

---

Ahora podemos borrar nuestro codigo viejo de los repositorios y utilizar la clase generica. Vamos a tener que adaptar nuestras clases de `Homeworks.BusinessLogic` de la siguiente manera, por ejemplo:

```c#
private IRepository<Homework> homeworksRepository;

public HomeworksLogic() {
    HomeworksContext context = ContextFactory.GetNewContext();
    homeworksRepository = new Repository<Homework>(context);
}
```

Si probamos la api en este momento (por ejemplo, en el endpoint `GET {url}/api/`), vamos a ver que se retornan correctamente todos los `Homework`, pero no se retornan los `Exercise` de su lista. Esto es debido a que no tenemos el `Include` de los `Exercises` cuando pedimos los `Homework` de la DB como si lo teniamos antes.

Como podemos solucionar esto? Haciendo una subclase del `Repository`. Creamos la clase `HomeworksRepository`, que heredara de `Repository` y hara `override` de los metodos de obtencion (`GetAll`, `GetByCondition`, `GetFirst`). Como fueron marcados como virtual, podemos hacer un override simple.

```c#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

using Homeworks.Domain;

namespace Homeworks.DataAccess
{
  public class HomeworksRepository : Repository<Homework>
  {
    public HomeworksRepository(DbContext Context) : base(Context) { }

        public override IEnumerable<Homework> GetAll()
        {
            return Context.Set<Homework>().Include("Exercises");
        }
 
        public override IEnumerable<Homework> GetByCondition(Expression<Func<Homework, bool>> expression)
        {
            return Context.Set<Homework>().Include("Exercises").Where(expression);
        }
 
        public override Homework GetFirst(Expression<Func<Homework, bool>> expression)
        {
            return Context.Set<Homework>().Include("Exercises").First(expression);
        }
  }
}
```

Para mas informacion sobre `eager-loading` (el `Include`) y la carga de datos ver [aqui](https://docs.microsoft.com/en-us/ef/core/querying/related-data). Funciona de manera analoga a como funcionaba en .NET Framework.

Por ultimo, actualizamos nuestro `HomeworksLogic` de `Homeworks.BusinessLogic` a usar este nuevo repositorio.

```c#
private IRepository<Homework> homeworksRepository;

public HomeworksLogic() {
    HomeworksContext context = ContextFactory.GetNewContext();
    homeworksRepository = new Repository<Homework>(context);
}
```

Si probamos de vuelta, vemos que funciona correctamente, trayendonos todos los datos que queriamos!

---

Por ultimo, debemos actualizar los tests que habiamos realizado ya que cambiaron las interfaces que mockeamos. Podemos mockear una interfaz con generics de la siguiente manera:

```c#
var mock = new Mock<IRepository<Homework>>(MockBehavior.Strict);
```

## DTOs

Otra cosa que podemos ver de nuestro codigo actualmente es que el modelo que recibimos en nuestra API es el mismo que el que utilizamos para la base de datos y para la logica del negocio. El cliente habla con la API mediante estos modelos. 

Esta bien esto? Podemos usar las clases del dominio del negocio para esto? Como poder se puede, como ya lo estamos haciendo. Sin embargo, esto tiene varias desventajas:

* **Motivos de cambio del dominio:** Le estamos dando un motivo de cambio mas al dominio. Que pasa si quiero cambiar algunos campos que se reciben, o el formato con el cual se recibe un dato, porque es mejor para el cliente? Tenemos que cambiar el dominio de nuestro sistema, probablemente hacer cambios en la base de datos, etc
* **SRP:** Estamos violando SRP (Single responsibility) ya que nuestro dominio se esta encargando de modelar que recibe y devuelve la API. No debberia cambiar por como esta definida la comunicacion Cliente-Servidor.

La solucion para esto son los DTOs. Un DTO (Data Transfer Object) es un objeto que solo se utiliza para transmitir informacion. Es decir, no tiene una funcionalidad especifica ni nada, solo se utiliza para almacenar informacion. 

En este caso, los objetos que crearemos seran objetos para obtener los datos enviados en las request a la API. Para esto necesitaremos un objeto que represente lo que recibimos en los endpoints. Crearemos uno para `Homework` y otro para `Exercise`. Creamos una carpeta `DTO` dentro de `Homeworks.WebApi` para tener todos estos objetos aqui. 

**ExerciseDTO.cs**

```c#
using System;
using System.Collections.Generic;
using Homeworks.Domain;

namespace Homeworks.WebApi.DTO
{
    public class ExerciseDTO
    {
        public Guid Id { get; set; }
        public string Problem { get; set; }
        public int Score { get; set; }

        public ExerciseDTO() { }

        public ExerciseDTO(Exercise entity)
        {
            SetModel(entity);
        }

        public Exercise ToEntity() => new Exercise()
        {
            Id = this.Id,
            Problem = this.Problem,
            Score = this.Score,
        };

        protected ExerciseDTO SetModel(Exercise entity)
        {
            Id = entity.Id;
            Problem = entity.Problem;
            Score = entity.Score;
            return this;
        }
    }
}
```

**HomeworkDTO.cs**

```c#
using System;
using System.Collections.Generic;
using System.Linq;
using Homeworks.Domain;

namespace Homeworks.WebApi.DTO
{
    public class HomeworkDTO
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public List<ExerciseDTO> Exercises {get; set;}

        public HomeworkDTO()
        {
            Exercises = new List<ExerciseDTO>();
        }

        public HomeworkDTO(Homework entity)
        {
            SetModel(entity);
        }

        public Homework ToEntity() => new Homework()
        {
            Id = this.Id,
            Description = this.Description,
            DueDate = this.DueDate,
            Exercises = this.Exercises.ConvertAll(m => m.ToEntity()),
        };

        protected HomeworkDTO SetModel(Homework entity)
        {
            Id = entity.Id;
            Description = entity.Description;
            DueDate = entity.DueDate;
            Exercises = entity.Exercises.ConvertAll(m => new ExerciseDTO(m));
            return this;
        }

    }
}
```

Como se puede ver, ambas clases tienen atributos similares a los del dominio. Tambien cuentan con metodos que permiten crear un DTO a partir de la clase del dominio, y crear una clase del Dominio a partir del DTO. 

Que ventajas nos trae esto? 

* **Poder cambiar los modelos de la API sin cambiar el dominio:** Imaginense que la API debe recibir 2 campos para la descripcion en vez de 1, uno que se llame `Name` y otro que se llame `Set` o algo similar. De no usar DTOs, tenemos que cambiar la clase entera, hacer una migracion de base de datos, cambiar como se maneja a todo nivel. En cambio, si tenemos el DTO, podemos recibir estos dos parametros y combinarlos como la `Description`.
* **Desacoplamiento de las capas:** Desacoplamos nuestras capas aun mas. El modelo manejado por nuestra API es distinto al modelo manejado por nuestra DB y por nuestra logica de negocio. Cabe aclarar que igual sigue habiendo una relacion, un modelo tiene que poder mapearse al otro. 


Ahora lo unico que queda es cambiar nuestros `Controllers` para que reciban y devuelvan estos DTOs.

Por ejemplo, nuestro `POST {url}/api/homeworks` quedaría asi.

```c#
[HttpPost]
public IActionResult Post([FromBody] HomeworkDTO homeworkDTO)
{
    try {
        Homework homework = homeworkDTO.ToEntity();
        Homework createdHomework = homeworksLogic.Create(homework);

        HomeworkDTO homeworkToReturn = new HomeworkDTO(createdHomework);
        return CreatedAtRoute("Get", new { id = homeworkToReturn.Id }, homeworkToReturn);
    } catch(ArgumentException e) {
        return BadRequest(e.Message);
    }
}
```

Queda como ejercicio cambiar todo el resto de los metodos de los Controllers.