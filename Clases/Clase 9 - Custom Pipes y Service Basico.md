# Custom Pipes y Service Basico

## Custom Pipes: Filtrado en el listado de tareas

Como vimos la clase anterior, Angular provee un conjunto de Pipes que ya vienen integrados y que sirven para transformar los datos antes de mostrarlos en el template (HTML). Ahora veremos como construir nuestros propios, Pipes personalizados, o *Custom Pipes*. 

El código necesario para crearlos es sumamente similar al de los componentes ya creados:

```typescript
import { Pipe, PipeTransform } from '@angular/core'; //0) importamos
import { Homework } from '../models/Homework';

//1) Creamos la clase HomeworksFilterPipe y la decoramos con @Pipe, tal cual como usamos @Component
@Pipe({
  name: 'homeworksFilter'
})
export class HomeworksFilterPipe implements PipeTransform { //2) Implementamos la interfaz PipeTransform

  transform(list: Array<Homework>, arg: string): Array<Homework> { //3) Método de la interfaz a implementar
        //4) Escribimos el código para filtrar las tareas
        // El primer parametro 'list', es el valor que estamos transformando con el pipe (la lista de tareas)
        // El segundo parametro 'arg', es el criterio a utilizar para transfmar el valor (para filtrar las tareas)
        // Es decir, lo que ingresó el usuario
        // El retorno es la lista de tareas filtrada
  }
}
```

Como podemos ver, tenemos que crear una **clase**, y hacerla que implemente la interfaz **PipeTransform**. Dicha interfaz tiene un método **transform** que es el que será encargado de filtrar las tareas. 

A su vez decoramos la clase con un ```@Pipe``` que hace que nuestra clase sea un Pipe. Como notamos, la experiencia a la hora de programar en Angular es bastante consistente, esto es muy similar a cuando creamos componentes.

Tambien podemos utilizar el comando ```ng generate pipe "Nombre de la pipe"``` que nos da como resultado esto mismo.

Luego, para usar este `CustomPipe` en un template, debemos hacer algo así:

```html
<tr *ngFor='let homework of homeworks | HomeworksFilter:listFilter'> </tr>
```

Siendo:

- **HomeworksFilter**: el pipe que acabamos de crear.
- **listFilter**: el string por el cual estaremos filtrando. La notacion `nombreDelPipe:valor` sirve para pasarle un parametro el pipe.

Si quisieramos pasar más argumentos además del `listFilter`, los ponemos separados por ```:```.

También nos falta agregar el Pipe a nuestro módulo. Si queremos que el componente pueda usarlo, entonces debemos decirle a nuestro AppModule que registre a dicho Pipe. Siempre que queremos que un Componente use un Pipe,entonces el módulo del componente debe referenciar al Pipe. 

Lo haremos definiendo al Pipe en el array ```declarations``` del decorador ```ngModule``` de nuestro módulo.

Armemos el Pipe!

### 1) Creamos un archivo para el Pipe

Creamos en la carpeta `app/homeworks-list`, un `homeworks-filter.pipe.ts`, siguiendo nuestras convenciones de nombre.

Tambien podemos usar `ng generate pipe HomeworksFilter` y movemos los archivos a la carpeta.

### 2) Agregamos la lógica del Pipe:


```typescript
import { Pipe, PipeTransform } from '@angular/core';
import { Homework } from '../models/Homework';

@Pipe({
  name: 'homeworksFilter'
})
export class HomeworksFilterPipe implements PipeTransform {

  transform(list: Array<Homework>, arg: string): Array<Homework> {
    return list.filter(
      x => x.description.toLocaleLowerCase()
        .includes(arg.toLocaleLowerCase())
    );
  }
}
```

En nuestro pipe, estamos usando el metodo `filter` de javascript. En el, filtramos los deberes cuya descripcion contenga el string que queremos filtrar. Terminamos retornando el resultado.

### 3) Agregamos el filtrado en el template y sus estilos

Vamos a ```homeworks-list.component.html``` y donde usamos ```*ngFor```, agregamos el filtrado tal cual lo vimos arriba:

```html
<tr *ngFor="let homework of homeworks | homeworksFilter:listFilter">
```

Cuando usamos un `pipe` con un `ngFor`, lo que se hace es:

- Primero se ejecuta el filtro con la lista (en este caso, homeworks)
- Luego, se itera con el resultado del filtro.

### 4) Agregamos el Pipe a nuestro AppModule

Vamos a ```app.module.ts``` y agregamos el pipe:

```typescript
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { AppComponent } from './app.component';
import { HomeworksListComponent } from './homeworks-list/homeworks-list.component';
import { HomeworksFilterPipe } from './homeworks-list/homeworks-filter.pipe';
import { HomeworksService } from './services/homeworks.service';

@NgModule({
  declarations: [
    AppComponent,
    HomeworksListComponent,
    HomeworksFilterPipe
  ],
  imports: [
    FormsModule,
    BrowserModule
  ],
  providers: [
    HomeworksService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
```
**Listo**, ya podemos filtar en nuestra lista!

## Servicios e Inyección de Dependencias

### Servicios

Los componentes nos permiten definir lógica y HTML para una cierta pantalla/vista en particular. Sin embargo, ¿qué hacemos con aquella lógica que no está asociada a una vista en concreto?, o ¿qué hacemos si queremos reusar lógica común a varios componentes (por ejemplo la lógica de conexión contra una API, lógica de manejo de la sesión/autenticación)?

Para lograr eso, construiremos **servicios**. Y a su vez, usaremos **inyección de dependencias** para poder meter/inyectar esos servicios en dichos componentes. 

Los servicios son simplemente clases con un fin en particular. Los usamos para aquellas features que son independientes de un componente en concreto, para reusar lógica o datos a través de componentes o para encapsular interacciones externas. Al cambiar esta responsabilidades y llevarlas a los servicios, nuestro código es más fácil de testear, debuggear y mantener. Tambien nos permiten mantener una unica responsabilidad en nuestro componente: tomar los datos y mostrarlos en la pantalla.

### Inyeccion de dependencias

Como establecer las dependencias siempre es un tema duro de atacar. Como ya vimos en el backend, es importante no hacer dependencias directas entre nuestros modulos, ya que hacen que sea mas dificil hacer pruebas, es mas dificil encapsular nuestros modulos, y muchas otras desventajas. El mismo tema se presenta en el frontend, y Angular provee herramientas para realizarlo.

Angular trae un `Injector` *built-in*, que nos permitirá registrar nuestros servicios en nuestros componentes, y que estos sean `Singleton`. Este `Injector` funciona en base a un contenedor de inyección de dependencias, donde una vez estos se registran, se mantiene una única instancia de cada uno.

Supongamos tenemos 3 servicios: `svc`, `log` y `math`. Una vez un componente utilice uno de dichos servicios en su constructor, el `Angular Injector` le provee la instancia del mismo al componente.


![image](../imgs/angular-clase3/13.png)

### Construyamos un servicio

Para armar nuestro servicio precisamos:
- Crear la clase del servicio.
- Definir la metadata con un `@`, es decir, un decorador en particular.
- Importar lo que precisamos.

¿Familiar? Son los mismos pasos que hemos seguido para construir nuestros componentes y nuestros custom pipes

### 1) Creamos nuestro servicio

Vamos a ```app/services``` y creamos un nuevo archivo: ```homeworks.service.ts```. 
También podemos utilizar el comando ```ng generate service Homeworks``` y lo movemos a la carpeta.

Luego, creamos el siguiente código:

```typescript
import { Injectable } from '@angular/core';
import { Homework } from '../models/Homework';
import { Exercise } from '../models/Exercise';

@Injectable()
export class HomeworksService {

  constructor() { }

  getHomeworks(): Array<Homework> {
    return [
      new Homework('1', 'Una tarea', 0, new Date(), [
        new Exercise('1', 'Un problema', 1),
        new Exercise('2', 'otro problema', 10)
      ]),
      new Homework('2', 'Otra tarea', 0, new Date(), [])
    ];
  }
}
```

Nuestro servicio recien creado es simplemente una clase que tiene un metodo. Este retorna una lista de Homeworks. Podemos ver que tiene el decorador `@Injectable`, el cual indica que este servicio es inyectable mediante inyeccion de dependencias.

### 2) Registramos nuestro servicio a través de un provider

Para poder utilizar nuestro Service recien creado en nuestro componente, debemos registrar un Provider. Un provider es simplemente código que puede crear o retornar un servicio, **típicamente es la clase del servicio mismo**. 

Esto lo logramos definiendolo en el componente, o como metadata en el Angular Module (AppModule).

- Si lo registramos en un componente, podemos inyectar el servicio en el componente y en todos sus hijos. 
- Si lo registramos en el módulo de la aplicación, lo podemos inyectar en toda la aplicación.

En este caso, lo registraremos en el Root Component (```AppModule```). Por ello, vamos a ```app.module.ts``` y reemplazamos todo el código para dejarlo así:

```typescript
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { AppComponent } from './app.component';
import { HomeworksListComponent } from './homeworks-list/homeworks-list.component';
import { HomeworksFilterPipe } from './homeworks-list/homeworks-filter.pipe';
import { HomeworksService } from './services/homeworks.service'; //importamos el servicio

@NgModule({
  declarations: [
    AppComponent,
    HomeworksListComponent,
    HomeworksFilterPipe
  ],
  imports: [
    FormsModule,
    BrowserModule
  ],
  providers: [
    HomeworksService // registramos el servicio para que este disponible en toda nuestra app
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
```

Aquí simplemente lo agregamos a la lista de providers.

Si se hizo mediante el comando, se puede ver como el servicio generado tiene este decorador:

```typescript
@Injectable({
  providedIn: 'root'
})
```

El parametro pasado (`providedIn: 'root'`) lo que hace es decirle a Angular, que utilice el provider por defecto (llamado root). Este lo que hara sera crear una unica instancia de nuestro Servicio en toda la aplicación, y se la inyectara cuando sea necesario.

Si se utiliza el root provider, no es necesario declarar nuestro servicio en el array de providers (ya que no crearemos un provider)

### 3) Inyectamos el servicio en nuestro HomeworksListComponent

La inyección la logramos a través del constructor de la clase, para ello hacemos en ```homeworks-list.component.ts```:

Primero el import:

```typescript
import { HomeworksService } from '../services/homeworks.service';
```
Y luego definimos el constructor que inyecta el servicio a la clase:

```typescript
constructor(private _serviceHomeworks: HomeworksService) { 
   // esta forma de escribir el parametro en el constructor lo que hace es:
   // 1) declara un parametro de tipo HomeworksService en el constructor
   // 2) declara un atributo de clase privado llamado _serviceHomeworks
   // 3) asigna el valor del parámetro al atributo de la clase
}
``` 

Esto inyecta el HomeworksService y lo deja disponible para la clase. Ahí mismo podríamos inicializar nuestras homeworks, llamando al ```getHomeworks``` del servicio, sin embargo, no es prolijo mezclar la lógica de construcción del componente (todo lo que es renderización de la vista), con lo que es la lógica de obtención de datos. Para resover esto usaremos lifecycle Hooks, particularmente, el ```OnInit``` que se ejecuta luego de inicializar el componente.

La diferencia entre el constructor y el `onInit` es que el constructor se ejecuta para crear la clase, sin necesariamente estar todo cargado. Cuando se ejecuta el `onInit`, estamos seguros que nuestra vista ya cargo del todo. Esta diferencia es importante ya puede introducir errores el mezclarlos. Mas info [aqui](https://stackoverflow.com/questions/35763730/difference-between-constructor-and-ngoninit/35763811) y sobretodo [aqui](https://blog.angularindepth.com/the-essential-difference-between-constructor-and-ngoninit-in-angular-c9930c209a42)

Definimos el `onInit`:

```typescript
ngOnInit(): void {
    this.homeworks = this._serviceHomeworks.getHomeworks();
}
```

Quedando, el código del componente algo así:
```typescript
import { Component, OnInit } from '@angular/core';
import { Homework } from '../models/Homework';
import { Exercise } from '../models/Exercise';
import { HomeworksService } from '../services/homeworks.service';

@Component({
  selector: 'app-homeworks-list',
  templateUrl: './homeworks-list.component.html',
  styleUrls: ['./homeworks-list.component.css']
})
export class HomeworksListComponent implements OnInit {

  pageTitle: string = 'HomeworksList';
  homeworks: Array<Homework>;
  showExercises: boolean = false;
  listFilter: string = "";

  constructor(private _serviceHomeworks:HomeworksService) { 
    
  }

  ngOnInit() {
    this.homeworks = this._serviceHomeworks.getHomeworks();
  }

  toogleExercises() {
    this.showExercises = !this.showExercises;
  }
}
```

Ya se pueden ver nuestros Homeworks obtenidos del servicio.


