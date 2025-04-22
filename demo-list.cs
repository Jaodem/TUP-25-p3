using System;
using System.Collections.Generic;
using System.Linq; // Necesario para Where en Filtrar (alternativa)


class ListaOrdenada<T> where T : IComparable<T> {
    private List<T> lista = new List<T>();

    // Constructor por defecto
    public ListaOrdenada() { }

    // Constructor que inicializa con una colección de elementos
    public ListaOrdenada(IEnumerable<T> elementos) {
        foreach (var elemento in elementos) {
            Agregar(elemento);
        }
    }

    // Propiedad para obtener la cantidad de elementos
    public int Cantidad => lista.Count;

    // Indexador para acceder a los elementos por índice
    public T this[int index] {
        get {
            if (index < 0 || index >= lista.Count) {
                throw new IndexOutOfRangeException("El índice está fuera del rango de la lista.");
            }
            return lista[index];
        }
    }

    // Agrega un elemento manteniendo el orden y sin duplicados
    public void Agregar(T elemento) {
        int index = lista.BinarySearch(elemento);
        // BinarySearch devuelve:
        // - El índice del elemento si se encuentra.
        // - Un número negativo cuyo complemento bit a bit (~) es el índice
        //   del primer elemento mayor que el valor buscado, o Count si todos
        //   los elementos son menores.
        if (index < 0) {
            // El elemento no existe, insertarlo en la posición correcta
            lista.Insert(~index, elemento);
        }
        // Si index >= 0, el elemento ya existe, no hacemos nada.
    }

    // Elimina un elemento de la lista
    public void Eliminar(T elemento) {
        int index = lista.BinarySearch(elemento);
        if (index >= 0) {
            // El elemento fue encontrado, eliminarlo
            lista.RemoveAt(index);
        }
        // Si index < 0, el elemento no existe, no hacemos nada.
    }

    // Verifica si un elemento está contenido en la lista
    public bool Contiene(T elemento) {
        int index = lista.BinarySearch(elemento);
        return index >= 0;
    }

    // Devuelve una nueva ListaOrdenada con los elementos que cumplen el predicado
    public ListaOrdenada<T> Filtrar(Func<T, bool> predicado) {
        var resultado = new ListaOrdenada<T>();
        // Iteramos sobre la lista interna ordenada
        foreach (var elemento in lista) {
            if (predicado(elemento)) {
                // Como la lista original está ordenada y solo estamos filtrando,
                // los elementos que cumplen el predicado ya estarán ordenados
                // entre sí. Podemos agregarlos directamente a la lista interna
                // de la nueva instancia. Esto es más eficiente que llamar a Agregar.
                resultado.lista.Add(elemento);
            }
        }
        return resultado;

        // Alternativa usando LINQ (un poco menos eficiente porque crea una lista intermedia):
        // var elementosFiltrados = lista.Where(predicado);
        // return new ListaOrdenada<T>(elementosFiltrados);
    }
}

class Contacto : IComparable<Contacto> {
    public string Nombre { get; set; }
    public string Telefono { get; set; }

    public Contacto(string nombre, string telefono) {
        Nombre = nombre;
        Telefono = telefono;
    }

    public int CompareTo(Contacto otro) {
        if (otro == null) return 1;
        return string.Compare(this.Nombre, otro.Nombre, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.CompareOptions.IgnoreCase | System.Globalization.CompareOptions.IgnoreNonSpace);
    }

    public override bool Equals(object obj) {
      return obj is Contacto otro && CompareTo(otro) == 0;
    }

    public override int GetHashCode() {
        return HashCode.Combine(Nombre);
    }

    public override string ToString() {
        return $"{Nombre} ({Telefono})";
    }
}

#region 
/// --------------------------------------------------------///
///   Desde aca para abajo no se puede modificar el código  ///
/// --------------------------------------------------------///

/// 
/// PRUEBAS AUTOMATIZADAS
///

// Funcion auxiliar para las pruebas
public static void Assert<T>(T real, T esperado, string mensaje){
    if (!Equals(esperado, real)) throw new Exception($"[ASSERT FALLÓ] {mensaje} → Esperado: {esperado}, Real: {real}");
    Console.WriteLine($"[OK] {mensaje}");
}


/// Pruebas de lista ordenada (con enteros)

var lista = new ListaOrdenada<int>();
lista.Agregar(5);
lista.Agregar(1);
lista.Agregar(3);

Assert(lista[0], 1, "Primer elemento");
Assert(lista[1], 3, "Segundo elemento");
Assert(lista[2], 5, "Tercer elemento");
Assert(lista.Cantidad, 3, "Cantidad de elementos");

Assert(lista.Filtrar(x => x > 2).Cantidad, 2, "Cantidad de elementos filtrados");
Assert(lista.Filtrar(x => x > 2)[0], 3, "Primer elemento filtrado");
Assert(lista.Filtrar(x => x > 2)[1], 5, "Segundo elemento filtrado");

Assert(lista.Contiene(1), true,  "Contiene");
Assert(lista.Contiene(2), false, "No contiene");

lista.Agregar(3);
Assert(lista.Cantidad, 3, "Cantidad de elementos tras agregar un elemento repetido");

lista.Agregar(2);
Assert(lista.Cantidad, 4, "Cantidad de elementos tras agregar 2");
Assert(lista[0], 1, "Primer elemento tras agregar 2");
Assert(lista[1], 2, "Segundo elemento tras agregar 2");
Assert(lista[2], 3, "Tercer elemento tras agregar 2");

lista.Eliminar(2);
Assert(lista.Cantidad, 3, "Cantidad de elementos tras eliminar elemento existente");
Assert(lista[0], 1, "Primer elemento tras eliminar 2");
Assert(lista[1], 3, "Segundo elemento tras eliminar 2");
lista.Eliminar(100);
Assert(lista.Cantidad, 3, "Cantidad de elementos tras eliminar elemento inexistente");



/// Pruebas de lista ordenada (con cadenas)

var nombres = new ListaOrdenada<string>(new string[] { "Juan", "Pedro", "Ana" });
Assert(nombres.Cantidad, 3, "Cantidad de nombres");

Assert(nombres[0], "Ana", "Primer nombre");
Assert(nombres[1], "Juan", "Segundo nombre");
Assert(nombres[2], "Pedro", "Tercer nombre");

Assert(nombres.Filtrar(x => x.StartsWith("A")).Cantidad, 1, "Cantidad de nombres que empiezan con A");
Assert(nombres.Filtrar(x => x.Length > 3).Cantidad, 2, "Cantidad de nombres con más de 3 letras");

Assert(nombres.Contiene("Ana"), true, "Contiene Ana");
Assert(nombres.Contiene("Domingo"), false, "No contiene Domingo");

nombres.Agregar("Pedro");
Assert(nombres.Cantidad, 3, "Cantidad de nombres tras agregar Pedro nuevamente");

nombres.Agregar("Carlos");
Assert(nombres.Cantidad, 4, "Cantidad de nombres tras agregar Carlos");

Assert(nombres[0], "Ana", "Primer nombre tras agregar Carlos");
Assert(nombres[1], "Carlos", "Segundo nombre tras agregar Carlos");

nombres.Eliminar("Carlos");
Assert(nombres.Cantidad, 3, "Cantidad de nombres tras agregar Carlos");

Assert(nombres[0], "Ana", "Primer nombre tras eliminar Carlos");
Assert(nombres[1], "Juan", "Segundo nombre tras eliminar Carlos");

nombres.Eliminar("Domingo");
Assert(nombres.Cantidad, 3, "Cantidad de nombres tras eliminar un elemento inexistente");

Assert(nombres[0], "Ana", "Primer nombre tras eliminar Domingo");
Assert(nombres[1], "Juan", "Segundo nombre tras eliminar Domingo");


/// Pruebas de lista ordenada (con contactos) 

var juan  = new Contacto("Juan",  "123456");
var pedro = new Contacto("Pedro", "654321");
var ana   = new Contacto("Ana",   "789012");
var otro  = new Contacto("Otro",  "345678");

var contactos = new ListaOrdenada<Contacto>(new Contacto[] { juan, pedro, ana });
Assert(contactos.Cantidad, 3, "Cantidad de contactos");
Assert(contactos[0].Nombre, "Ana", "Primer contacto");
Assert(contactos[1].Nombre, "Juan", "Segundo contacto");
Assert(contactos[2].Nombre, "Pedro", "Tercer contacto");

Assert(contactos.Filtrar(x => x.Nombre.StartsWith("A")).Cantidad, 1, "Cantidad de contactos que empiezan con A");
Assert(contactos.Filtrar(x => x.Nombre.Contains("a")).Cantidad, 2, "Cantidad de contactos que contienen a");

Assert(contactos.Contiene(juan), true, "Contiene Juan");
Assert(contactos.Contiene(otro), false, "No contiene Otro");

contactos.Agregar(otro);
Assert(contactos.Cantidad, 4, "Cantidad de contactos tras agregar Otro");
Assert(contactos.Contiene(otro), true, "Contiene Otro");

Assert(contactos[0].Nombre, "Ana", "Primer contacto tras agregar Otro");
Assert(contactos[1].Nombre, "Juan", "Segundo contacto tras agregar Otro");
Assert(contactos[2].Nombre, "Otro", "Tercer contacto tras agregar Otro");
Assert(contactos[3].Nombre, "Pedro", "Cuarto contacto tras agregar Otro");

contactos.Eliminar(otro);
Assert(contactos.Cantidad, 3, "Cantidad de contactos tras eliminar Otro");
Assert(contactos[0].Nombre, "Ana", "Primer contacto tras eliminar Otro");
Assert(contactos[1].Nombre, "Juan", "Segundo contacto tras eliminar Otro");
Assert(contactos[2].Nombre, "Pedro", "Tercer contacto tras eliminar Otro");

contactos.Eliminar(otro);
Assert(contactos.Cantidad, 3, "Cantidad de contactos tras eliminar un elemento inexistente");
Assert(contactos[0].Nombre, "Ana", "Primer contacto tras eliminar Otro");
Assert(contactos[1].Nombre, "Juan", "Segundo contacto tras eliminar Otro");
Assert(contactos[2].Nombre, "Pedro", "Tercer contacto tras eliminar Otro");
#endregion