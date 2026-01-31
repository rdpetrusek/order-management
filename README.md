# Project Overview
An order management system for loading orders from two backend sources and displaying them to the user with optional filtering by ID.

# Technology Stack
- API: C# dotnet core
    - Fast, requires minimal setup, strongly typed which reduced design-time issues and well supported by Microsoft
- Frontend: Angular
    - Well documented and full featured SPA frontend framework. Works well for forms applications and being highly opinionated allows easy onboarding.

# Setup Instructions
1. Install nodejs
2. Install dotnet SDK

- Navigate to repo root
    - Install dependencies: `npm run install:all`

- Execute each in a different terminal
    - Start Backend: `npm run backend`
    - Start Frontend: `npm run frontend`

- Navigate to: `http://localhost:4200/`

# API Documentation

## Base URL
`http://localhost:5013` (when running locally)

## Endpoints

### GET /api/health
Health check endpoint to verify the API is running.

**Response:** `200 OK`

---

### GET /api/orders
Retrieves all orders from both System A and System B data sources.

**Parameters:** None

**Response:** `200 OK`

**Example Response:**
```json
[
  {
    "sourceSystem": "SystemA",
    "orderId": "A12345",
    "customerName": "John Doe",
    "orderDate": "2024-01-15T00:00:00",
    "totalAmount": 150.75,
    "status": "Completed"
  },
  {
    "sourceSystem": "SystemB",
    "orderId": "B67890",
    "customerName": "Jane Smith",
    "orderDate": "2024-01-20T00:00:00",
    "totalAmount": 299.99,
    "status": "Shipped"
  }
]
```

---

### GET /api/orders/search?status={status}
Filters orders by their normalized status.

**Parameters:**
- `status` (string, query parameter, required) - The order status to filter by. Valid values: "Pending", "Processing", "Shipped", "Completed", "Cancelled" (case-insensitive)

**Response:**
- `200 OK` - Returns filtered orders
- `400 Bad Request` - Status parameter missing or invalid

**Example Request:**
```
GET /api/orders/search?status=Processing
```

**Example Response (200 OK):**
```json
[
  {
    "sourceSystem": "SystemA",
    "orderId": "A12345",
    "customerName": "John Doe",
    "orderDate": "2024-01-15T00:00:00",
    "totalAmount": 150.75,
    "status": "Processing"
  },
  {
    "sourceSystem": "SystemB",
    "orderId": "B54321",
    "customerName": "Bob Johnson",
    "orderDate": "2024-01-18T00:00:00",
    "totalAmount": 89.50,
    "status": "Processing"
  }
]
```

**Example Response (400 Bad Request):**
```json
{
  "error": "Invalid status value. Valid values are: Pending, Processing, Shipped, Completed, Cancelled"
}
```

---

### GET /api/orders/{orderId}
Retrieves a specific order by its ID.

**Parameters:**
- `orderId` (string, path parameter) - The unique identifier for the order

**Response:**
- `200 OK` - Order found
- `404 Not Found` - Order not found

**Example Response (200 OK):**
```json
{
  "sourceSystem": "SystemA",
  "orderId": "A12345",
  "customerName": "John Doe",
  "orderDate": "2024-01-15T00:00:00",
  "totalAmount": 150.75,
  "status": "Completed"
}
```

## Data Model

### Order
| Field | Type | Description |
|-------|------|-------------|
| sourceSystem | string | The source system ("SystemA" or "SystemB") |
| orderId | string | Unique identifier for the order |
| customerName | string | Name of the customer |
| orderDate | DateTime | Date the order was placed (ISO 8601 format) |
| totalAmount | decimal | Total order amount |
| status | string | Order status (see OrderStatus enum) |

### OrderStatus Enum
- `Pending` (1)
- `Processing` (2)
- `Shipped` (3)
- `Completed` (4)
- `Cancelled` (5)

# Approach & Decisions
- Data normalization
    - Date
        - Parsed the individual date formats using their expected format strings then converted the orders to a common internal domain type.
        - System A date format is supported and serialized to JSON without specifying the format.
    - Status code
        - SystemA: Used a normalized string (convert source to uppercase to handle PEND, Pend, pEND, etc) to convert to the enum type internally
        - SystemB: Used the int code as the value of the enum, this allowed it to be serialized without special logic
- Date format
    - I chose to return the date in ISO 8601 format as the YYYY-MM-DD format is the most reliable for ordering

# Time Management
### Priorities

1. Set up scaffolded API and frontend
    - Forms the foundation of the project
2. Add DTOs to API that represent the shape of the data coming from the data sources
    - Ensures that the data will be properly serialized from the sources
3. Add domain types for Orders to be returned from the API request
    - Ensures we are fulfilling the requested API contract (property names, data types)
4. Add GetAllOrders endpoint logic
    - Get all is the meat of the project. Once this is done it's simple to filter in memory (though suboptimal).
5. Add GetById endpoint logic
    - Easy addition after GetAll is done
6. Add healthcheck endpoint logic
    - Lowest priority endpoint
7. Add bare minimum frontend for displaying data
    - Frontend is there to validate data
8. Add search box and button to filter orders

# Known Limitations
- Large JSON files will cause the request to slow down and eventually hang if it takes longer than the configured timeout.
- Files are loaded and parsed on each request. 
    - This is simple to cache internally, but I wanted to simulate a scenario where these were not static files but instead a data source that is constantly updated (database, downstream api, etc)
- UI input not sanitized or validated

# Future Improvements
- Thorough unit tests and integration tests on frontend and backend
- Stream processing for large JSON datasets
- Push ID filtering logic down into the get logic instead of loading all orders and then filtering afterwards. With large datasets, this can be a huge performance issue.
- Proper styling on the frontend
    - Design an Order card to display each Order
    - Format the date in a more user friendly format
- Breaking out frontend into components instead of one large component
- Trimming whitespace from frontend input and other input validations
- Robust search capabilities
    - Match partial IDs