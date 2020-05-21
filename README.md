## NOT TESTED VERY MUCH

Fixed size collection whose elements are preallocated

`Pool<IPoolable>` is a fixed size collection whose elements are preallocated on creation.
Getting and releasing therefore does not heap alloc every time and call the garbage collector.
Objects become "alive" with `Get()` or "die" with `Release()`;

It is explicitely fixed-size to prevent inadvertent memory leaks, 
so you cannot get more objects than the pool's capacity, otherwise an exception will be thrown.

Pool requires an object that inherits `IPoolable`.
When Pool is constructed, it creates the element and calls `Allocate()`.
When you `Get()` an element, it calls Obtain() on it, and `Release()` when object is released.
Therefore use `Release()` or `Obtain()` on `IPoolable` to its data.

`Clear()` releases all objects, but does not actually destroy them (they stay in memory).
Objects will be garbage collected only when the Pool itself gets unreferenced.

The package also contains an example of a GameObjectPool, which shows how pool can be used with objects that should not be created with `new`, but via a proxy.
