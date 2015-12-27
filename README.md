# CanonicalTypes
A C# canonical types library.

Requires Microsoft's [Immutable
Collections](http://www.nuget.org/packages/System.Collections.Immutable).

This is my own private alternative to JSON. It is inspired by Scheme
S-expressions. It supports a wide variety of data types.

It is currently a work in progress, and so is the documentation below.

# Data Types

At runtime, all data types descend from the ``Datum`` class.

All the data types (except for ``MutableBoxDatum``) are immutable, hashable,
and comparable, which means they may be used as items in sets, or keys in
dictionaries. This includes sets and dictionaries themselves. (A
``MutableBoxDatum`` has a mutable *content* part and an immutable *identity.*
Only the identity is used for hashing and comparison.)

All the data types can be serialized as binary and deserialized from
binary. (However, the deserialization code is currently not secure, which
means you should only deserialize trusted byte streams.) With the exception of
*uninterned symbols,* the deserialized value will be equal to the serialized
one.

Here are the data types supported:

## Null
The null datum is written as ``#nil``.

## Boolean
The boolean data are written as ``#t`` for true, or ``#f`` for false.

## Character
Right now a character is only 16 bits, which means that some Unicode code
points have to be encoded with surrogate pairs. The character data type is
only able to represent half of a surrogate pair.

There are three ways to write characters.

* A literal character. For example, ``C`` can be written as ``#\C``.
* A named character, such as ``#\newline`` or ``#\space``. Character names are
case-sensitive.
* A hex character. For example, the escape character would be ``#\x1B``, and
the non-breaking zero-width space (which is used by Unicode as a byte-order
mark) would be ``#\xFEFF``. Hex digits are case-insensitive.

## String
A string is written between double quotes.

* ``""`` is the empty string.
* All the C-style escapes are supported, for example ``\t`` for tab and ``\n``
for newline. So ``"\t"`` is a string consisting of a single tab character.
* You can escape a quote with ``\"`` or a backslash with ``\\``.
* Hexadecimal escapes, such as ``\x1B`` for the escape character, are
supported, but must use exactly two hex digits. Unicode escapes such as
``\UFEFF`` are supported, but must use exactly four hex digits.
* You can escape a newline to omit it from the string and continue the string
on the next line.

## Symbol
A symbol is usually a bare word such as ``symbol``. Symbols are
case-sensitive.

Bare words are limited in what characters they may contain, but a symbol may
contain any characters. Thus, it is also possible to write a "quoted symbol"
between pipe characters, such as ``|has two spaces|``.

* ``||`` is the empty symbol, the symbol whose name is the empty string.
* All the C-style escapes are supported, as with strings.
* You cannot escape a quote, but you can escape a pipe with ``\\|`` or a
backslash with ``\\``.
* Hexadecimal and Unicode escapes are supported, as with strings.
* You can escape a newline just like in a string.

The data model also supports *uninterned symbols.* When an uninterned symbol
is generated, it is guaranteed to be distinct from any other symbol (interned
or uninterned). Once generated, an uninterned symbol can be *used* any number
of times, and is always recognized as being equal to itself.

An uninterned symbol does not have a printed representation, but it can be
serialized in binary. When a ``Datum`` is serialized to binary, the number of
distinct uninterned symbols is counted, and when the ``Datum`` is
deserialized, the same number of new uninterned symbols is generated. The new
symbols correspond one-for-one with the old ones.

## Integer
Integers are arbitrary-precision. Right now only base 10 is
supported. Examples of integers include ``-100``, ``0``, and ``64``.

## Rational
Rational numbers are supported. Examples include ``-1/2`` and ``1/3``. Upon
being read, a rational number is converted to lowest terms.

## Floating-Point
Floating-point numbers are supported. A floating-point number must distinguish
itself from an integer by having a decimal point or an ``E`` (or an ``e``), or
both. Examples of floating-point numbers include ``-1.23`` and ``6.02e23``.

The data model supports infinite and NaN values, but there is currently no way
to write them.

## GUID
Globally-unique identifiers (GUIDs) are supported as a distinct data type. A
GUID must have 32 hex digits, with hyphens in particular places. A GUID always
starts with ``#g{`` and ends with ``}``. An example of a GUID is
``#g{01234567-89AB-CDEF-0123-456789ABCDEF}``. The hex digits are
case-insensitive.

## Byte Array
A byte array is written as ``#y(``, followed by a sequence of hexadecimal
bytes, followed by ``)``. Each hexadecimal byte must consist of exactly two
hex digits. Hex digits are case-insensitive. Whitespace is allowed between
bytes.

As a convenience, it is also possible to put bytes in square brackets; this
causes the order of the bytes in the brackets to be reversed, and is useful
when embedding little-endian multi-byte integers in the byte array. Square
brackets may *not* be nested.

An example of a byte array is ``#y(F4 2C 53 0f [3000])``. This is equivalent
to ``#y([0F53 2CF4] 00 30)``.

## Atomic vs. Compound Types
All the above were *atomic* data types, meaning that they do not contain
unrestricted ``Datum`` values as elements. The *compound* types include lists,
sets, and dictionaries, and they *do* allow unrestricted ``Datum`` values as
elements. Compound types can include other compound types as elements.

Unlike Common Lisp, the empty list, the empty set, and the empty dictionary
are all considered distinct from ``#nil``, from the symbol ``nil``, and from
each other.

## List
A list is written as ``(``, followed by zero or more ``Datum`` values,
followed by ``)``.

A list is considered to be a sequence of zero or more elements. "Dotted lists"
in the style of Lisp and Scheme are not supported. These lists might be more
analogous to the *vectors* in Lisp or Scheme.

## Set
A set is written as ``#s{``, followed by zero or more ``Datum`` values,
followed by ``}``.

Duplicate items in a set are ignored. The data model does not allow them.

## Dictionary
A dictionary is written as ``{``, followed by zero or more key-value pairs,
followed by ``}``. A key-value pair is a ``Datum`` key, followed by ``=>``,
followed by a ``Datum`` value, followed by ``,``. The trailing comma may be
omitted from the last key-value pair.

The result of a duplicate key is undefined. The data model allows only one
value per key.

## Mutable Box
(Coming soon)
