# Описание

* Проект предназначен для формирования файла заданного формата;
* Произвольные слова для английского языка были взяты со страниц Википедии про Microsoft и Netflix;
* Файл для сортировки каждый раз создаётся как новый (старый удаляется).

# Возможные пути оптимизации

1. Самое очевидное - запускать на SSD;
2. Поэкспериментировать с размером пачки данных, которые пишутся в файл;
3. Создать несколько потоков (Thread) и вести запись через них, не забыв делать блокировку файла
на момет записи. Однако независио от числа потоков узким местом является скорость записи на диск,
поэтому без SSD добиться существенного увеличения скорости записи не выйдет.
