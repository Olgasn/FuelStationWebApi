# FuelStationWebApi
Пример Rest ASP.NET Core сервиса и HTML клиента для работы с информацией, хранящейся в таблицах базы данных.

Документация по API и работа с ним: http://host:xxxx/swagger/index.html

Вызовы клиента:

с использованием jQuery: http://host:xxxx/jq_operations.html

с использованием Fetch API: http://host:xxxx/fetch_operations.html

Структура файла secrets.json, в котором хранятся данные для авторизации:
{
  "Database:login": "",
  "Database:password": ""
}

В нем, в соответвующих  местах, требуется указать имя пользователя и пароль для доступа к базе данных, размещенной на удаленном SQL Server. 


[![build and test](https://github.com/Olgasn/FuelStationWebApi/actions/workflows/deployment.yml/badge.svg)](https://github.com/Olgasn/FuelStationWebApi/actions/workflows/deployment.yml)