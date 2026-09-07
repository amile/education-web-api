# EducationWebApi


## Требования
* [.NET 10.0 SDK](https://microsoft.com) 
* [Docker](https://www.docker.com) 

# Структура приложения
Приложение разделено на несколько сервисов:
Users - отвечает за создание и аутентификацию пользователей.
Events - работа с событиями.
Bookings - функционал для управления бронированием.

# Взаимодействие с базой данных
Для каждого сервиса необходим доступ к базе данных PostgreSQL 
В сервисе настройки для подключения к базе прописаны в файле appsettings.json, блоке "ConnectionStrings" -> "Default"
В проекте по умолчанию базы разворачивается в docker контейнере

# Создание миграций:
dotnet ef migrations add {MigrationName} --project {ServiceName}.Infrastructure --startup-project {ServiceName}.Presentation

# Применение миграций:
dotnet ef database update

При запуске приложения схема БД создаётся автоматически методом Migrate

## Установка
```bash
git clone https://github.com/amile/education-web-api.git
cd education-web-api
docker compose up -d
```

## Проверка работоспособности
http://localhost:5003/health - users service
http://localhost:5004/health - events service
http://localhost:5005/health - bookings service

## Swagger
{serviceHost}/swagger

## Запуск тестов
dotnet test (производится в директории каждого сервиса отдельно)

Для unit тестов используется In-Memory Provider
Для интеграционных тестов необходимо предварительно запустить docker 

## Формат ошибок
Для описания ошибок используется Problem Details (RFC 7807)

## Аутентификация

В приложении настроена JWT-аутентификация
Конфигурация JWT токена по умолчанию прописана в файле appsettings.json, блоке "token".
В production рекомендуется хранить конфигурацию в переменных окружения,
защищенных конфигурационных файлах, специальных менеджерах секретов.
Секрет для токена (TOKEN__SECRET) необходимо добавить отдельно, например в переменные окружения.

Для работы в Swagger также необходимо ввести токен аутентификации, методы получения токена описаны ниже.

## Кэширование 
В данном проекте реализовано кэширование событий (на базе Redis). Используется для снижения нагрузки на базу данных, ускорения ответа API и оптимизации часто запрашиваемых данных.
Реализован паттерн Cache-Aside (Lazy Loading):
Приложение сначала ищет данные в кэше. Если данных нет (Cache Miss), они запрашиваются из БД и параллельно сохраняются в кэш.
Для хранения данных в кеше установлен TTL (Time-to-Live) — это период времени, в течение которого данные считаются актуальными и хранятся в памяти, прежде чем система запросит их заново из базы данных. 
Информация о событии, TTL - 20 минут.
Информация о 10 наиболее популярных событий, TTL - 10 минут.
При изменении событий в БД (команды POST, PUT, DELETE), связанный кэш удаляется.

## 🚀 API Endpoints

### Регистрация пользователя
POST {usersServiceHost}/auth/register

#### Параметры тела запроса
| Field | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `login` | `string` | Yes | Логин пользователя |
| `password`| `string` | Yes | Пароль пользователя |
| `role` | `UserRole` | No | Роль пользователя |

*Роли пользователя*
UserRole
{
    Admin,
    User
}

#### Успешный ответ
* **Code:** 200 Ok
* **Content:**
```json
{
  "accessToken": "eyJhbGciO...ZVDfcgN1hICLKrKd9FZC7icI"
}
```

### Вход пользователя в систему
POST {usersServiceHost}/auth/login

#### Параметры тела запроса
| Field | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `login` | `string` | Yes | Логин пользователя |
| `password`| `string` | Yes | Пароль пользователя |

#### Успешный ответ
* **Code:** 200 Ok
* **Content:**
```json
{
  "accessToken": "eyJhbGciO...ZVDfcgN1hICLKrKd9FZC7icI"
}
```

### Создание события
POST {eventsServiceHost}/api/events - Auth Required
* **Headers:** `Authorization: Bearer <token>`
* **Allowed Roles:** Admin

#### Параметры тела запроса
| Field | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `title` | `string` | Yes | Наименование события |
| `description`| `string` | No | Описание события |
| `startAt` | `DateTime` | Yes | Дата и время начала события |
| `endAt` | `DateTime` | Yes | Дата и время окончания события |
| `totalSeats` | `int` | Yes | Общее количество мест на событие |

#### Успешный ответ
* **Code:** 200 Ok
* **Content:**
Guid a4c2c736-e466-49a5-b14d-fd7dc7488417

При успешном создании события доступное количество мест для бронирования - availableSeats устанавливается равным totalSeats

### Получение пагинированного списка событий
GET {eventsServiceHost}/api/events - Auth Required
* **Headers:** `Authorization: Bearer <token>`
* **Allowed Roles:** User, Admin

#### Query параметры
Title: string - фильтр по наименованию события
From: DateTime - фильтр по дате начала события
To: DateTime - фильтр по дате окончания события
Page: int - номер страницы пагинированного списка
PageSize: int - размер страницы пагинированного списка

#### Успешный ответ
* **Code:** 200 Ok
* **Content:**
```json
{
  "data": [
    {
      "id": "a4c2c736-e466-49a5-b14d-fd7dc7488417",
      "title": "Test",
      "description": "Test",
      "startAt": "2026-06-02T16:51:44.236Z",
      "endAt": "2026-06-03T16:51:44.236Z",
      "totalSeats": 1,
      "availableSeats": 1
    }
  ],
  "totalCount": 1,
  "currentPage": 1,
  "pageSize": 1
}
```


### Получение списка 10 наиболее популярных событий
GET {eventsServiceHost}/api/events/top - Auth Required
* **Headers:** `Authorization: Bearer <token>`
* **Allowed Roles:** User, Admin

#### Успешный ответ
* **Code:** 200 Ok
* **Content:**
```json
[
  {
    "id": "a4c2c736-e466-49a5-b14d-fd7dc7488417",
    "title": "Test",
    "description": "Test",
    "startAt": "2026-06-02T16:51:44.236Z",
    "endAt": "2026-06-03T16:51:44.236Z",
    "totalSeats": 1,
    "availableSeats": 1
  }
]
```


### Получение информации о событии
GET {eventsServiceHost}/api/events/{id} - Auth Required
* **Headers:** `Authorization: Bearer <token>`
* **Allowed Roles:** User, Admin

#### Параметры запроса из url
id - Уникальный идентификатор события

#### Успешный ответ
* **Code:** 200 Ok
* **Content:**
```json
{
  "id": "a4c2c736-e466-49a5-b14d-fd7dc7488417",
  "title": "Test",
  "description": "Test",
  "startAt": "2026-06-02T16:51:44.236Z",
  "endAt": "2026-06-03T16:51:44.236Z",
  "totalSeats": 1,
  "availableSeats": 1
}
```

#### Error
* **Code:** 404 Not Found
* **Content:**
```json
{
  "status": 404,
  "detail": "Event Id: a4c2c736-e466-49a5-b14d-fd7dc7488418 not found"
}
```


### Редактирование события
PUT {eventsServiceHost}/api/events/{id} - Auth Required
* **Headers:** `Authorization: Bearer <token>`
* **Allowed Roles:** Admin

#### Параметры запроса из url
id - Уникальный идентификатор события

#### Параметры тела запроса
| Field | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `title` | `string` | Yes | Наименование события |
| `description`| `string` | No | Описание события |
| `startAt` | `DateTime` | Yes | Дата и время начала события |
| `endAt` | `DateTime` | Yes | Дата и время окончания события |

#### Успешный ответ
* **Code:** 200 Ok
* **Content:**
```json
{
  "id": "a4c2c736-e466-49a5-b14d-fd7dc7488417",
  "title": "UpdatedEvent",
  "description": "UpdatedEvent",
  "startAt": "2026-06-02T16:51:44.236Z",
  "endAt": "2026-06-03T16:51:44.236Z",
  "totalSeats": 1,
  "availableSeats": 1
}
```

#### Error
* **Code:** 404 Not Found
* **Content:**
```json
{
  "status": 404,
  "detail": "Event Id: a4c2c736-e466-49a5-b14d-fd7dc7488418 not found"
}
```


### Удаление события
DELETE {eventsServiceHost}/api/events/{id} - Auth Required
* **Headers:** `Authorization: Bearer <token>`
* **Allowed Roles:** Admin

#### Параметры запроса из url
id - Уникальный идентификатор события

#### Успешный ответ
* **Code:** 200 Ok

#### Error
* **Code:** 404 Not Found
* **Content:**
```json
{
  "status": 404,
  "detail": "Event Id: a4c2c736-e466-49a5-b14d-fd7dc7488418 not found"
}
```


### Бронирование событий
POST {bookingsServiceHost}/api/events/{id}/book - Auth Required
* **Headers:** `Authorization: Bearer <token>`
* **Allowed Roles:** User, Admin

#### Параметры запроса из url
id - Уникальный идентификатор события

#### Успешный ответ
* **Code:** 202 Accepted
* **Content:**
```json
{
  "id": "31839166-b54c-47bf-89ea-5acb9e6630cf",
  "eventId": "4475f184-04d3-48e8-811e-3d74a0de3bab",
  "status": "Pending",
  "createdAt": "2026-06-02T04:44:39.935285Z",
  "processedAt": null
}
```

#### Error
* **Code:** 404 Not Found
* **Content:**
```json
{
  "status": 404,
  "detail": "Event Id: 31839166-b54c-47bf-89ea-5acb9e6630ca not found"
}
```
* **Code:** 409 Conflict
* **Content:**
```json
{
  "status": 409,
  "detail": "No available seats for this event"
}
```

### Получение информации о бронировании
GET {bookingsServiceHost}/api/bookings/{id} - Auth Required
* **Headers:** `Authorization: Bearer <token>`
* **Allowed Roles:** User (for owner), Admin

#### Параметры запроса из url
id - Уникальный идентификатор бронирования

#### Успешный ответ
* **Code:** 200 Ok
* **Content:**
```json
{
  "id": "31839166-b54c-47bf-89ea-5acb9e6630cf",
  "eventId": "4475f184-04d3-48e8-811e-3d74a0de3bab",
  "status": "Confirmed",
  "createdAt": "2026-06-02T04:44:39.935285Z",
  "processedAt": "2026-06-02T04:44:43.79273Z"
}
```

#### Error
* **Code:** 404 Not Found
* **Content:**
```json
{
  "status": 404,
  "detail": "Booking Id: 31839166-b54c-47bf-89ea-5acb9e6630ca not found"
}
```

### Отмена бронирования
DELETE {bookingsServiceHost}/api/bookings/{id} - Auth Required
* **Headers:** `Authorization: Bearer <token>`
* **Allowed Roles:** User (for owner), Admin

#### Параметры запроса из url
id - Уникальный идентификатор бронирования

#### Успешный ответ
* **Code:** 200 Ok
* **Content:**
```json
{
  "id": "31839166-b54c-47bf-89ea-5acb9e6630cf",
  "eventId": "4475f184-04d3-48e8-811e-3d74a0de3bab",
  "status": "Cancelled",
  "createdAt": "2026-06-02T04:44:39.935285Z",
  "processedAt": "2026-06-02T04:44:43.79273Z"
}
```

#### Error
* **Code:** 404 Not Found
* **Content:**
```json
{
  "status": 404,
  "detail": "Booking Id: 31839166-b54c-47bf-89ea-5acb9e6630ca not found"
}

## Описание бронирования событий

*Модель бронирования*
BookingDto
{
    Guid Id
    Guid EventId
    BookingStatus Status
    DateTime CreatedAt
    DateTime? ProcessedAt
}

*Статусы бронирования*
BookingStatus
{
    Pending,
    Confirmed,
    Rejected
}

Для бронирования необходимо отправить POST запрос на {bookingsServiceHost}/api/events/{id}/book, где id - уникальный идентификатор события (метод описан выше)
Далее информация о бронировании отправляется в брокер сообщений Kafka
В сервисе событий процесс бронирования осуществляется в фоновом сервисе BookingConsumerService. 
Данный сервис читает сообщение из Kafka, проверяет существует ли событие с таким идентификатором, не началось ли событие, наличие свободных мест и при прохождении всех условий резервирует место для бронирования.
Получить актуальную информацию о бронировании можно отправив GET запрос на {bookingsServiceHost}/api/bookings/{id} (метод описан выше)
Если параллельно отправлено количество запросов на бронирование события превышающее количество доступных мест, то успешными пройдут только первые обработанные запросы по количеству доступных мест. Остальные запросы будут отклонены.
