# EducationWebApi


## Требования
* [.NET 10.0 SDK](https://microsoft.com)

## Установка
```bash
git clone https://github.com/amile/education-web-api.git
cd education-web-api
dotnet build
dotnet run

## Проверка работоспособности
http://localhost:5003/health

## Swagger
http://localhost:5003/swagger

## Запуск тестов
dotnet test

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

#### Успешный ответ
* **Code:** 200 Ok
* **Content:**
Guid a4c2c736-e466-49a5-b14d-fd7dc7488417


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
      "endAt": "2026-06-03T16:51:44.236Z"
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
  "endAt": "2026-06-03T16:51:44.236Z"
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
  "endAt": "2026-06-03T16:51:44.236Z"
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

### Получение информации о бронировании
GET /api/booking/{id}

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
