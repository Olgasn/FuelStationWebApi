# FuelStationWebApi
Пример Rest ASP.NET Core сервиса и HTML клиента для работы с информацией, хранящейся в таблицах базы данных. 
Использована схема аутентификации с использование JWT токена. Информация о пользователях хранится в базе данных, управляемой с использованием средств Identity.

Документация по API и работа с ним: http://host:xxxx/swagger/index.html

Вызовы HTML клиента для:

для предварительно аутентификации: http://host:xxxx/index.html

работы с данными с использованием jQuery: http://host:xxxx/jq_operations.html

работы с данными с использованием Fetch API: http://host:xxxx/fetch_operations.html

Структура файла secrets.json, в котором хранятся данные для авторизации:
{
  "Database:login": "",
  "Database:password": ""
}

В нем, в соответствующих местах, требуется указать имя пользователя и пароль для доступа к базе данных, размещенной на удаленном SQL Server. 

[![build and test](https://github.com/Olgasn/FuelStationWebApi/actions/workflows/deployment.yml/badge.svg?branch=master)](https://github.com/Olgasn/FuelStationWebApi/actions/workflows/deployment.yml)
