# Общие сведения

* У всех проектов в *.csproj-файле проекта жёстко забита версия языка C# 9.0;
* Решено использовать длину строки в 120 символов, поскольку сейчас почти у всех есть
широкоформатные мониторы и ограничение в 80 символов не имеет смысла, покуда программа не
отправляется на печать;
* Распараллеливание в некоторых местах достигается за счёт использования задач (`Task`),
а не потоков (`Thread`), поскольку задачи являются более высокоуровневой логикой;
* Для каждого метода подбирался наиболее низкий уровень доступа, чтобы не выводить в интерфейс лишнее.
При возможности методы делались статичными локальными функциями, чтобы не попасть даже в
пространство имён класса;
* В папке DAL хранится основная логика работы с данными _(DAL - аббревиатура от Data Access Layer)_;
* Для большинства методов и классов написаны комментарии, даже если из названия очевидны
их предназначения, чтобы избежать предупрежедния CS1591 (нет комментария) _(просто подавить
данное предупреждение в настройках проекта нет желания, т.к. наличие комментариев всё-таки
является хорошим подспорьем при чтении кода)_.;
* В коде встречаются комментарии, описывающие причину использования того или иного
алгоритма/подхода к решению проблемы;
* Не во всех местах есть проверки на валидность параметров: зачастую проверяются самые очевидные
проблемы и в соответствии с ними кидается исключение, которое перехватывается выше;
* Вывод в консоль сделан, чтобы видеть: программа не висит. При желании можно
подключить нормальный логгер и делать записи в консоль/файл/на почту в зависимости от типа
сообщения _(отладка/информация/ошибка...)_;
* Чтобы избежать магических чисел/строк в методах, там, где они используются всего один раз, явно
указывается название параметра;
* При большом числе параметров (в объявлении или при передаче) каждый параметр переносится
на новую строку.

---
В некоторых файлах **README.md** указываются возможные пути улучшения проекта, а также места,
которые могут быть узким или наиболее тяжёлым местом в приложении. В большинстве случаев такие
места оставлены, поскольку в задачу не входило покрыть все возможные ошибки/оптимизировать всё
по максимуму.

---

# Про проекты

## Producer

Формирование файла в заданном формате, передача пути созданного файла через Kafka в **Consumer**.

## Consumer

Сортировка файла, путь к которому получен через Kafka от **Producer**, с записью результата в
новый файл.

## Common

Содержит модели и логику, которые являются общими для проектов **Consumer** и **Producer**. При
желании можно разбить на большее число проектов; в частности, логику работы с Kafka можно вынести
в отдельный проект _(KafkaManager)_.

# Конвенция наименования при написании проектов

Ниже описаны основные способы задания имён объектов с примерами.

1. public и protected объекты, классы, методы, все свойства пишутся через **PascalCase**

* `internal class SomeModel`
* `public void Foo()`
* `protected int IntVar { get; set; }`
* `private bool _IsValid { get; set; }`

2. Параметры методов, локальные переменные, закрытые поля класса (без get/set)
пишутся через **camelCase**

* `public void Foo(int someInt, string someString)`
* `private long _itemsCount`

3. Перед private-объектами стоит нижний прочерк

* `private int _intVar`
* `private void _Process()`
* `private string _TooLongString { get; }`

4. Константы и readonly пишутся CAPS`ом

* `public const int MAX_FILE_SIZE_IN_GB = 10`
* `private readonly string _FILE_NAME = "SomeName.txt";`