# EducationWebApi


## Требования
* [.NET 10.0 SDK](https://microsoft.com) 
* [Docker](https://www.docker.com) 

Для работы приложения необходим доступ к базе данных PostgreSQL
В проекте по умолчанию база разворачивается в docker контейнере, 
настройки для подключения к базе прописаны в файле appsettings.json, блоке "ConnectionStrings" -> "Default"

# Создание миграций:
dotnet ef migrations add {MigrationName}

# Применение миграций:
dotnet ef database update

При запуске приложения схема БД создаётся автоматически методом Migrate

## Установка
```bash
git clone https://github.com/amile/education-web-api.git
cd education-web-api
docker compose up -d
dotnet build
dotnet run

## Проверка работоспособности
http://localhost:5003/health

## Swagger
http://localhost:5003/swagger

## Запуск тестов
dotnet test

Для unit тестов используется In-Memory Provider
Для нтеграционных тестов необходимо предварительно запустить docker 

## Формат ошибок
Для описания ошибок используется Problem Details (RFC 7807)

## 🚀 API Endpoints


### Создание события
POST /api/events

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
GET /api/events

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


### Получение информации о событии
GET /api/events/{id}

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
PUT /api/events/{id}

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
DELETE /api/events/{id}

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
POST /api/events/{id}/book

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
GET /api/bookings/{id}

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

Для бронирования необходимо отправить POST запрос на /api/events/{id}/book, где id - уникальный идентификатор события (метод описан выше)
Если событие найдено, создается бронирование со статусом Pending
Процесс бронирования осуществляется в фоновом сервисе BookingProcessService. 
Данный сервис каждые четыре секунды запрашивает все бронирования в статусе Pending и далее в цикле отправляет запрос на бронирование.
Если запрос успешно выполнен, статус меняется на Confirmed, в случае ошибки - Rejected. 
Обновленное бронирование сохраняется в репозитории.
Получить актуальную информацию о бронировании можно отправив GET запрос на /api/bookings/{id} (метод описан выше)
Если параллельно отправлено количество запросов на бронирование события превышающее количество доступных мест, то успешными пройдут только первые обработанные запросы по количеству доступных мест. Остальные запросы будут отклонены.
