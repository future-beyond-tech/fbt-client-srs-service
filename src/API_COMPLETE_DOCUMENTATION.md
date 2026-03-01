# Complete API Documentation - Sales & Revenue System (SRS)

## Table of Contents
1. [Authentication](#authentication)
2. [Customers](#customers)
3. [Vehicles](#vehicles)
4. [Sales](#sales)
5. [Manual Billing](#manual-billing)
6. [Purchases](#purchases)
7. [Purchase Expenses](#purchase-expenses)
8. [Finance Companies](#finance-companies)
9. [Dashboard](#dashboard)
10. [Search](#search)
11. [Upload](#upload)
12. [Delivery Note Settings](#delivery-note-settings)

---

## Authentication

### POST Login

**Endpoint Name:** Login  
**HTTP Method:** POST  
**Route URL:** `/api/auth/login`  
**Short Description:** Authenticate user and receive JWT token  
**Detailed Description:** Validates user credentials (username and password) against the database. Upon successful validation, generates and returns a JWT bearer token that must be included in subsequent authenticated requests.

**Authentication Requirement:** No

---

#### Request

**Content-Type:** `application/json`

**Request Body Example:**
```json
{
  "username": "admin",
  "password": "securePassword123"
}
```

**Request Fields:**

| Field Name | Data Type | Required | Default | Validation Rules | Description |
|-----------|-----------|----------|---------|------------------|-------------|
| username | String (C#: `string`, JSON: `string`) | Mandatory | - | No null/empty validation enforced | The username of the user account |
| password | String (C#: `string`, JSON: `string`) | Mandatory | - | No length validation enforced | The plaintext password of the user account |

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6ImFkbWluIiwicm9sZSI6IkFkbWluIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| token | String (C#: `string`, JSON: `string`) | No | JWT Bearer token for authentication. Valid for API requests. |

---

**Status Code: 401 Unauthorized**

**Response Example:**
```json
{
  "message": "Invalid credentials"
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| message | String | No | Error message indicating invalid username or password |

---

#### Error Response Examples

**Validation Error (Invalid Credentials):**
```json
{
  "message": "Invalid credentials"
}
```

**Exception-Based Error:**
```json
{
  "message": "An error occurred while processing your request"
}
```

#### Edge Cases
- Username or password is null or empty
- User account does not exist
- Password hash verification fails
- Database connection timeout

#### Business Rules
- User credentials must match exactly (case-sensitive for username)
- Only users with Admin role can log in
- Token is valid for a configured duration (typically 24 hours)
- Multiple login attempts may trigger rate limiting (if configured)

#### Side Effects
- None (read-only operation)

#### Idempotent
No - Multiple identical requests will generate different tokens

---

## Customers

### POST Create Customer

**Endpoint Name:** Create Customer  
**HTTP Method:** POST  
**Route URL:** `/api/customers`  
**Short Description:** Create a new customer record  
**Detailed Description:** Creates a new customer in the system with name, phone number, and optional address. Automatically assigns a unique GUID identifier and records the creation timestamp.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** `application/json`

**Request Body Example:**
```json
{
  "name": "Rajesh Kumar",
  "phone": "9876543210",
  "address": "123 Main Street, New Delhi"
}
```

**Request Fields:**

| Field Name | Data Type | Required | Default | Validation Rules | Description |
|-----------|-----------|----------|---------|------------------|-------------|
| name | String (C#: `string`, JSON: `string`) | Mandatory | - | Must not be null or empty | Customer full name |
| phone | String (C#: `string`, JSON: `string`) | Mandatory | - | Must not be null or empty. Format: 10-digit phone number | Customer phone number (mobile or landline) |
| address | String (C#: `string?`, JSON: `string` or `null`) | Optional | null | None | Customer residential or commercial address |

---

#### Response

**Status Code: 201 Created**

**Response Example:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Rajesh Kumar",
  "phone": "9876543210",
  "address": "123 Main Street, New Delhi",
  "photoUrl": null,
  "createdAt": "2026-02-20T10:30:00Z"
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| id | GUID (C#: `Guid`, JSON: `string`) | No | Unique identifier for the customer |
| name | String | No | Customer name |
| phone | String | No | Customer phone number |
| address | String | Yes | Customer address |
| photoUrl | String | Yes | URL to customer photo if uploaded |
| createdAt | DateTime (C#: `DateTime`, JSON: `ISO 8601 string`) | No | Timestamp when customer was created |

---

**Status Code: 400 Bad Request**

**Response Example:**
```json
{
  "message": "Customer with this phone number already exists"
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| message | String | No | Error message explaining the validation failure |

---

#### Error Response Examples

**Validation Error (Duplicate Phone):**
```json
{
  "message": "Customer with this phone number already exists"
}
```

**Exception-Based Error:**
```json
{
  "message": "An unexpected error occurred while creating customer"
}
```

#### Edge Cases
- Phone number already exists in system
- Name contains special characters
- Address is very long (>500 characters)
- Null or empty phone number
- Null or empty name

#### Business Rules
- Phone number must be unique across the system
- Phone number format must be valid (10-15 digits)
- Name must be non-empty
- Customer is in "active" status by default
- Photo URL is initially null until a photo is uploaded

#### Side Effects
- Database INSERT operation
- Customer record created and persisted
- Unique GUID generated
- Creation timestamp set to current UTC time

#### Idempotent
No - Multiple identical requests create multiple customer records

---

### GET All Customers

**Endpoint Name:** Get All Customers  
**HTTP Method:** GET  
**Route URL:** `/api/customers`  
**Short Description:** Retrieve all customers  
**Detailed Description:** Fetches a complete list of all customers in the system with their details. No pagination applied.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (GET request)

**Query Parameters:** None

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Rajesh Kumar",
    "phone": "9876543210",
    "address": "123 Main Street, New Delhi",
    "photoUrl": null,
    "createdAt": "2026-02-20T10:30:00Z"
  },
  {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "name": "Priya Singh",
    "phone": "9876543211",
    "address": "456 Oak Avenue, Mumbai",
    "photoUrl": "https://storage.example.com/priya_photo.jpg",
    "createdAt": "2026-02-19T15:45:00Z"
  }
]
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| id | GUID | No | Unique identifier for the customer |
| name | String | No | Customer name |
| phone | String | No | Customer phone number |
| address | String | Yes | Customer address |
| photoUrl | String | Yes | URL to customer photo |
| createdAt | DateTime | No | Timestamp when customer was created |

---

**Status Code: 200 OK (Empty List)**

**Response Example:**
```json
[]
```

---

#### Edge Cases
- No customers exist in the system (empty array returned)
- Database contains thousands of customers (consider pagination)
- Customer phone number is not unique (data integrity issue)

#### Business Rules
- Returns all active customers
- Results are not paginated
- Customers are sorted by creation date (descending) or name (ascending) - depends on implementation

#### Side Effects
- None (read-only operation)

#### Idempotent
Yes - Multiple identical requests return the same data

---

### GET Customer by ID

**Endpoint Name:** Get Customer by ID  
**HTTP Method:** GET  
**Route URL:** `/api/customers/{id:guid}`  
**Short Description:** Retrieve a specific customer by ID  
**Detailed Description:** Fetches a single customer's complete information using their unique GUID identifier.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (GET request)

**Route Parameters:**

| Parameter Name | Data Type | Required | Format | Description |
|---|---|---|---|---|
| id | GUID (C#: `Guid`, JSON: `string`) | Mandatory | UUID | Unique customer identifier |

**Example:** `/api/customers/550e8400-e29b-41d4-a716-446655440000`

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Rajesh Kumar",
  "phone": "9876543210",
  "address": "123 Main Street, New Delhi",
  "photoUrl": null,
  "createdAt": "2026-02-20T10:30:00Z"
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| id | GUID | No | Unique identifier for the customer |
| name | String | No | Customer name |
| phone | String | No | Customer phone number |
| address | String | Yes | Customer address |
| photoUrl | String | Yes | URL to customer photo |
| createdAt | DateTime | No | Timestamp when customer was created |

---

**Status Code: 404 Not Found**

**Response Example:**
```json
{
  "message": "Customer not found"
}
```

---

#### Edge Cases
- Invalid GUID format (malformed UUID)
- Customer ID does not exist
- Customer is soft-deleted

#### Business Rules
- Only returns active (non-deleted) customers
- GUID must be valid format

#### Side Effects
- None (read-only operation)

#### Idempotent
Yes - Multiple identical requests return the same data

---

### GET Search Customers by Phone

**Endpoint Name:** Search Customers  
**HTTP Method:** GET  
**Route URL:** `/api/customers/search`  
**Short Description:** Search customers by phone number  
**Detailed Description:** Searches and returns customers matching the provided phone number or phone number pattern. Useful for quick customer lookup before creating a sale.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (GET request)

**Query Parameters:**

| Parameter Name | Data Type | Required | Default | Format | Description |
|---|---|---|---|---|---|
| phone | String | Mandatory | - | Numeric digits | Phone number to search for (partial or full match allowed) |

**Example:** `/api/customers/search?phone=9876543210`

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Rajesh Kumar",
    "phone": "9876543210",
    "address": "123 Main Street, New Delhi",
    "photoUrl": null,
    "createdAt": "2026-02-20T10:30:00Z"
  }
]
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| id | GUID | No | Unique identifier for the customer |
| name | String | No | Customer name |
| phone | String | No | Customer phone number |
| address | String | Yes | Customer address |
| photoUrl | String | Yes | URL to customer photo |
| createdAt | DateTime | No | Timestamp when customer was created |

---

**Status Code: 200 OK (No Results)**

**Response Example:**
```json
[]
```

---

#### Edge Cases
- Phone number contains non-numeric characters
- Empty phone number string
- Phone number is partial (e.g., "98765") - may return multiple results
- No matches found

#### Business Rules
- Search performs partial string matching
- Returns all matching customers
- Case-insensitive search
- Only searches active (non-deleted) customers

#### Side Effects
- None (read-only operation)

#### Idempotent
Yes - Multiple identical requests return the same data

---

## Vehicles

### GET All Vehicles

**Endpoint Name:** Get All Vehicles  
**HTTP Method:** GET  
**Route URL:** `/api/vehicles`  
**Short Description:** Retrieve all vehicles  
**Detailed Description:** Fetches a complete list of all vehicles in inventory with their full details including purchase prices, selling prices, and current status.

**Authentication Requirement:** No

---

#### Request

**Content-Type:** N/A (GET request)

**Query Parameters:** None

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
[
  {
    "id": 1,
    "brand": "Hyundai",
    "model": "Creta",
    "year": 2022,
    "registrationNumber": "DL01AB1234",
    "chassisNumber": "MA1FA7YA2M2123456",
    "engineNumber": "G4LA987654",
    "colour": "Silver",
    "sellingPrice": 850000,
    "status": 1,
    "createdAt": "2026-02-15T08:00:00Z"
  },
  {
    "id": 2,
    "brand": "Maruti",
    "model": "Swift",
    "year": 2021,
    "registrationNumber": "HR26EF5678",
    "chassisNumber": "MA1DL7FA2L1234567",
    "engineNumber": "M13A234567",
    "colour": "White",
    "sellingPrice": 450000,
    "status": 1,
    "createdAt": "2026-02-18T12:30:00Z"
  }
]
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| id | Int32 | No | Unique vehicle identifier |
| brand | String | No | Vehicle manufacturer/brand (e.g., Hyundai, Maruti) |
| model | String | No | Vehicle model name (e.g., Creta, Swift) |
| year | Int32 | No | Manufacturing year of vehicle |
| registrationNumber | String | No | Unique registration/license plate number |
| chassisNumber | String | Yes | Vehicle chassis/VIN number |
| engineNumber | String | Yes | Engine identification number |
| colour | String | Yes | Vehicle color |
| sellingPrice | Decimal (C#: `decimal`, JSON: `number`) | No | Asking/selling price in currency units |
| status | Int32 | No | Vehicle status (1=Available, 2=Sold) |
| createdAt | DateTime | No | Timestamp when vehicle was added to inventory |

---

**Status Code: 200 OK (Empty List)**

**Response Example:**
```json
[]
```

---

#### Edge Cases
- No vehicles in inventory
- Vehicle with null optional fields (chassisNumber, engineNumber, colour)
- Large inventory (thousands of vehicles)

#### Business Rules
- Returns all vehicles regardless of status
- Vehicles are typically sorted by creation date (newest first)
- Selling price should be >= purchasing cost + expenses

#### Side Effects
- None (read-only operation)

#### Idempotent
Yes - Multiple identical requests return the same data

---

### GET Available Vehicles

**Endpoint Name:** Get Available Vehicles  
**HTTP Method:** GET  
**Route URL:** `/api/vehicles/available`  
**Short Description:** Retrieve only available vehicles for sale  
**Detailed Description:** Fetches a list of vehicles with status "Available" (not yet sold). These are the vehicles that can be displayed in the sales interface.

**Authentication Requirement:** No

---

#### Request

**Content-Type:** N/A (GET request)

**Query Parameters:** None

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
[
  {
    "id": 1,
    "brand": "Hyundai",
    "model": "Creta",
    "year": 2022,
    "registrationNumber": "DL01AB1234",
    "chassisNumber": "MA1FA7YA2M2123456",
    "engineNumber": "G4LA987654",
    "colour": "Silver",
    "sellingPrice": 850000,
    "status": 1,
    "createdAt": "2026-02-15T08:00:00Z"
  }
]
```

**Response Fields:** (Same as GET All Vehicles)

---

#### Edge Cases
- No vehicles available (all sold)
- Vehicle status is neither Available (1) nor Sold (2)

#### Business Rules
- Only returns vehicles with status = 1 (Available)
- Typically used in sales form to populate vehicle selection dropdown

#### Side Effects
- None (read-only operation)

#### Idempotent
Yes - Multiple identical requests return the same data

---

### PUT Update Vehicle

**Endpoint Name:** Update Vehicle  
**HTTP Method:** PUT  
**Route URL:** `/api/vehicles/{id:int}`  
**Short Description:** Update vehicle details  
**Detailed Description:** Updates specific vehicle properties such as selling price, color, and registration number. Soft-deleted vehicles cannot be updated.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** `application/json`

**Route Parameters:**

| Parameter Name | Data Type | Required | Format | Description |
|---|---|---|---|---|
| id | Int32 | Mandatory | Integer | Unique vehicle identifier |

**Request Body Example:**
```json
{
  "sellingPrice": 900000,
  "colour": "Pearl White",
  "registrationNumber": "DL01AB1234"
}
```

**Request Fields:**

| Field Name | Data Type | Required | Default | Validation Rules | Description |
|-----------|-----------|----------|---------|------------------|-------------|
| sellingPrice | Decimal (C#: `decimal`, JSON: `number`) | Mandatory | - | Must be >= 0. Typically >= purchasing cost + expenses | Updated selling price for the vehicle |
| colour | String (C#: `string?`, JSON: `string` or `null`) | Optional | null | None | Vehicle color (e.g., Silver, White, Black) |
| registrationNumber | String (C#: `string?`, JSON: `string` or `null`) | Optional | null | Must be unique if provided | Updated registration/license plate number |

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
{
  "id": 1,
  "brand": "Hyundai",
  "model": "Creta",
  "year": 2022,
  "registrationNumber": "DL01AB1234",
  "chassisNumber": "MA1FA7YA2M2123456",
  "engineNumber": "G4LA987654",
  "colour": "Pearl White",
  "sellingPrice": 900000,
  "status": 1,
  "createdAt": "2026-02-15T08:00:00Z"
}
```

**Response Fields:** (Same as GET All Vehicles)

---

**Status Code: 404 Not Found**

**Response Example:**
```json
{
  "message": "Vehicle not found"
}
```

---

**Status Code: 400 Bad Request**

**Response Example:**
```json
{
  "message": "Registration number already exists"
}
```

---

**Status Code: 409 Conflict**

**Response Example:**
```json
{
  "message": "Cannot update a sold vehicle"
}
```

---

#### Error Response Examples

**Vehicle Not Found:**
```json
{
  "message": "Vehicle not found"
}
```

**Duplicate Registration Number:**
```json
{
  "message": "Registration number already exists"
}
```

**Cannot Update Sold Vehicle:**
```json
{
  "message": "Cannot update a sold vehicle"
}
```

**Invalid Selling Price:**
```json
{
  "message": "Selling price must be greater than or equal to 0"
}
```

#### Edge Cases
- Vehicle ID does not exist
- Vehicle is already sold (status = Sold)
- Vehicle is soft-deleted
- Registration number conflicts with another vehicle
- Selling price is negative
- Required field is null

#### Business Rules
- Cannot update a sold vehicle
- Registration number must be unique
- Selling price must be >= 0
- Only Admin users can update vehicles
- Cannot update vehicle if it's in a sale transaction

#### Side Effects
- Database UPDATE operation
- Vehicle record modified with new values
- Modification timestamp updated (if tracked)

#### Idempotent
No - Multiple identical requests may have side effects if validation rules change

---

### DELETE Soft Delete Vehicle

**Endpoint Name:** Soft Delete Vehicle  
**HTTP Method:** DELETE  
**Route URL:** `/api/vehicles/{id:int}`  
**Short Description:** Soft delete a vehicle  
**Detailed Description:** Marks a vehicle as deleted without removing it from the database. Soft-deleted vehicles are excluded from inventory lists but retain historical data for audit purposes.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (DELETE request)

**Route Parameters:**

| Parameter Name | Data Type | Required | Format | Description |
|---|---|---|---|---|
| id | Int32 | Mandatory | Integer | Unique vehicle identifier |

**Example:** `/api/vehicles/1`

---

#### Response

**Status Code: 204 No Content**

*No response body is returned. The request was successful.*

---

**Status Code: 404 Not Found**

**Response Example:**
```json
{
  "message": "Vehicle not found"
}
```

---

**Status Code: 409 Conflict**

**Response Example:**
```json
{
  "message": "Cannot delete a sold vehicle"
}
```

---

#### Error Response Examples

**Vehicle Not Found:**
```json
{
  "message": "Vehicle not found"
}
```

**Cannot Delete Sold Vehicle:**
```json
{
  "message": "Cannot delete a sold vehicle"
}
```

**Already Deleted:**
```json
{
  "message": "Vehicle is already deleted"
}
```

#### Edge Cases
- Vehicle ID does not exist
- Vehicle is already soft-deleted
- Vehicle is sold (status = Sold)
- Vehicle is involved in an active transaction

#### Business Rules
- Cannot soft-delete a sold vehicle
- Soft-deleted vehicles are excluded from GET /api/vehicles and /api/vehicles/available
- Soft-deleted vehicles retain all data for audit trail
- Operation is not reversible via API (would require admin database access)

#### Side Effects
- Database UPDATE operation
- Vehicle is marked as deleted (IsDeleted = true or similar flag)
- Vehicle excluded from inventory lists
- Historical data preserved

#### Idempotent
No - Second delete attempt returns 409 Conflict if already deleted

---

### PATCH Update Vehicle Status

**Endpoint Name:** Update Vehicle Status  
**HTTP Method:** PATCH  
**Route URL:** `/api/vehicles/{id:int}/status`  
**Short Description:** Update vehicle status (Available/Sold)  
**Detailed Description:** Changes the status of a vehicle between Available (1) and Sold (2). Typically called when a sale is created to mark a vehicle as sold.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** `application/json`

**Route Parameters:**

| Parameter Name | Data Type | Required | Format | Description |
|---|---|---|---|---|
| id | Int32 | Mandatory | Integer | Unique vehicle identifier |

**Request Body Example:**
```json
{
  "status": 2
}
```

**Request Fields:**

| Field Name | Data Type | Required | Default | Validation Rules | Description |
|-----------|-----------|----------|---------|------------------|-------------|
| status | Int32 (Enum: VehicleStatus) | Mandatory | - | Must be 1 (Available) or 2 (Sold) | New status for the vehicle |

**Status Enum Values:**
- `1` = Available
- `2` = Sold

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
{
  "id": 1,
  "brand": "Hyundai",
  "model": "Creta",
  "year": 2022,
  "registrationNumber": "DL01AB1234",
  "chassisNumber": "MA1FA7YA2M2123456",
  "engineNumber": "G4LA987654",
  "colour": "Silver",
  "sellingPrice": 850000,
  "status": 2,
  "createdAt": "2026-02-15T08:00:00Z"
}
```

**Response Fields:** (Same as GET All Vehicles)

---

**Status Code: 404 Not Found**

**Response Example:**
```json
{
  "message": "Vehicle not found"
}
```

---

**Status Code: 409 Conflict**

**Response Example:**
```json
{
  "message": "Vehicle is already marked as sold"
}
```

---

#### Error Response Examples

**Vehicle Not Found:**
```json
{
  "message": "Vehicle not found"
}
```

**Already in Desired State:**
```json
{
  "message": "Vehicle is already marked as sold"
}
```

**Invalid Status:**
```json
{
  "message": "Invalid vehicle status. Allowed values: 1 (Available), 2 (Sold)"
}
```

#### Edge Cases
- Vehicle does not exist
- Vehicle is already in the target status
- Status value is not 1 or 2
- Vehicle is soft-deleted

#### Business Rules
- Status can only be 1 (Available) or 2 (Sold)
- Cannot change status of a sold vehicle to available
- Status is automatically updated when a sale is created
- Only Admin users can manually update status

#### Side Effects
- Database UPDATE operation
- Vehicle status changed
- May affect vehicle availability in dropdown lists

#### Idempotent
No - Changing status from Available to Sold is non-idempotent

---

## Sales

### POST Create Sale

**Endpoint Name:** Create Sale  
**HTTP Method:** POST  
**Route URL:** `/api/sales`  
**Short Description:** Create a new sale transaction  
**Detailed Description:** Records a complete sale transaction with vehicle, customer, and payment information. Automatically calculates profit, updates vehicle status to Sold, and generates a unique bill number.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** `application/json`

**Request Body Example:**
```json
{
  "vehicleId": 1,
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "customerName": "Rajesh Kumar",
  "customerPhone": "9876543210",
  "customerAddress": "123 Main Street, New Delhi",
  "customerPhotoUrl": "https://storage.example.com/photo.jpg",
  "paymentMode": 1,
  "cashAmount": 500000,
  "upiAmount": 350000,
  "financeAmount": null,
  "financeCompany": null,
  "rcBookReceived": true,
  "ownershipTransferAccepted": true,
  "vehicleAcceptedInAsIsCondition": true,
  "saleDate": "2026-02-20T10:00:00Z"
}
```

**Request Fields:**

| Field Name | Data Type | Required | Default | Validation Rules | Description |
|-----------|-----------|----------|---------|------------------|-------------|
| vehicleId | Int32 | Mandatory | - | Must exist and be Available (status=1) | ID of the vehicle being sold |
| customerId | GUID (C#: `Guid?`, JSON: `string` or `null`) | Optional | null | Valid GUID if provided | Existing customer ID (if customer is in system) |
| customerName | String (C#: `string?`, JSON: `string` or `null`) | Optional | null | Required if customerId is null | Customer full name |
| customerPhone | String (C#: `string?`, JSON: `string` or `null`) | Optional | null | Valid phone format | Customer phone number |
| customerAddress | String (C#: `string?`, JSON: `string` or `null`) | Optional | null | None | Customer address |
| customerPhotoUrl | String | Mandatory | - | Valid URL format | URL to customer photo |
| paymentMode | Int32 (Enum: PaymentMode) | Mandatory | - | Must be 1, 2, or 3 | Payment method (1=Cash, 2=UPI, 3=Finance) |
| cashAmount | Decimal (C#: `decimal?`, JSON: `number` or `null`) | Optional | null | >= 0 if provided | Amount paid in cash |
| upiAmount | Decimal (C#: `decimal?`, JSON: `number` or `null`) | Optional | null | >= 0 if provided | Amount paid via UPI |
| financeAmount | Decimal (C#: `decimal?`, JSON: `number` or `null`) | Optional | null | >= 0 if provided | Amount financed |
| financeCompany | String (C#: `string?`, JSON: `string` or `null`) | Optional | null | Must exist in finance companies if provided | Name of financing company |
| rcBookReceived | Boolean | Mandatory | - | true/false | Whether RC book was received from customer |
| ownershipTransferAccepted | Boolean | Mandatory | - | true/false | Whether customer accepted ownership transfer terms |
| vehicleAcceptedInAsIsCondition | Boolean | Mandatory | - | true/false | Whether customer accepted vehicle in as-is condition |
| saleDate | DateTime (C#: `DateTime`, JSON: `ISO 8601 string`) | Mandatory | - | Must be <= current date/time | Date and time of the sale transaction |

**Payment Mode Enum Values:**
- `1` = Cash
- `2` = UPI
- `3` = Finance

---

#### Response

**Status Code: 201 Created**

**Response Example:**
```json
{
  "billNumber": 10001,
  "vehicleId": 1,
  "vehicle": "Hyundai Creta 2022",
  "customerName": "Rajesh Kumar",
  "totalReceived": 850000,
  "profit": 150000,
  "saleDate": "2026-02-20T10:00:00Z"
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| billNumber | Int32 | No | Unique bill/invoice number for this sale |
| vehicleId | Int32 | No | ID of the sold vehicle |
| vehicle | String | No | Vehicle description (Brand Model Year) |
| customerName | String | No | Customer name |
| totalReceived | Decimal | No | Total payment received (cashAmount + upiAmount + financeAmount) |
| profit | Decimal | No | Profit = (totalReceived - sellingPrice) - (buyingCost + expenses) |
| saleDate | DateTime | No | Date and time of the sale |

---

**Status Code: 404 Not Found**

**Response Example:**
```json
{
  "message": "Vehicle not found"
}
```

---

**Status Code: 400 Bad Request**

**Response Example:**
```json
{
  "message": "Invalid payment configuration. Total of all payment modes must equal vehicle selling price."
}
```

---

**Status Code: 409 Conflict**

**Response Example:**
```json
{
  "message": "Vehicle is already sold"
}
```

---

**Status Code: 422 Unprocessable Entity**

**Response Example:**
```json
{
  "message": "Sale validation failed: All payment amounts must sum to vehicle price"
}
```

---

#### Error Response Examples

**Vehicle Not Found:**
```json
{
  "message": "Vehicle not found or is not available"
}
```

**Invalid Payment Configuration:**
```json
{
  "message": "Total payment amount must equal vehicle selling price"
}
```

**Missing Customer Information:**
```json
{
  "message": "Either customerId or customerName must be provided"
}
```

**Vehicle Already Sold:**
```json
{
  "message": "Vehicle is already sold"
}
```

**Invalid Finance Company:**
```json
{
  "message": "Finance company not found or inactive"
}
```

#### Edge Cases
- Vehicle does not exist or is already sold
- Payment amounts don't sum to vehicle selling price
- Neither customerId nor customerName is provided
- Invalid customerId format (malformed GUID)
- Finance amount provided but financeCompany is null
- Sale date is in the future
- Customer photo URL is invalid
- Finance company does not exist
- Finance company is inactive

#### Business Rules
- Vehicle must be Available (status = 1) to be sold
- Total payment (cash + UPI + finance) must equal vehicle selling price
- At least one payment mode must be used
- If customerPhotoUrl is null, validation fails
- Profit is calculated as: Total Received - (Buying Cost + Expenses)
- Only existing and active finance companies can be used
- Bill number is auto-generated (sequential)
- Vehicle status is automatically changed to Sold (2)
- Sale date cannot be in the future

#### Side Effects
- Database INSERT operation (creates Sale record)
- Database UPDATE operation (changes vehicle status to Sold)
- Bill number generated (unique sequence)
- BillDetail record created with comprehensive sale information
- Vehicle is removed from available inventory
- Profit calculated and stored

#### Idempotent
No - Multiple identical requests create multiple sales with different bill numbers

---

### GET Sale History

**Endpoint Name:** Get Sale History  
**HTTP Method:** GET  
**Route URL:** `/api/sales`  
**Short Description:** Retrieve paginated list of sales with filtering  
**Detailed Description:** Fetches sales history with support for pagination, keyword search, and date range filtering. Useful for generating sales reports and viewing transaction history.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (GET request)

**Query Parameters:**

| Parameter Name | Data Type | Required | Default | Format | Description |
|---|---|---|---|---|---|
| pageNumber | Int32 | Optional | 1 | Integer >= 1 | Page number for pagination |
| pageSize | Int32 | Optional | 10 | Integer >= 1 | Number of records per page |
| search | String | Optional | null | Text string | Search keyword (matches customer name, phone, vehicle model, bill number) |
| fromDate | DateTime | Optional | null | ISO 8601 string | Start date for filtering sales |
| toDate | DateTime | Optional | null | ISO 8601 string | End date for filtering sales |

**Example:**  
`/api/sales?pageNumber=1&pageSize=10&search=Rajesh&fromDate=2026-02-01&toDate=2026-02-28`

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
{
  "items": [
    {
      "billNumber": 10001,
      "saleDate": "2026-02-20T10:00:00Z",
      "customerName": "Rajesh Kumar",
      "phone": "9876543210",
      "vehicleModel": "Creta",
      "registrationNumber": "DL01AB1234",
      "profit": 150000
    },
    {
      "billNumber": 10002,
      "saleDate": "2026-02-19T14:30:00Z",
      "customerName": "Priya Singh",
      "phone": "9876543211",
      "vehicleModel": "Swift",
      "registrationNumber": "HR26EF5678",
      "profit": 75000
    }
  ],
  "totalCount": 2,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| items | Array | No | List of sales records |
| items[].billNumber | Int32 | No | Unique bill number |
| items[].saleDate | DateTime | No | Date of sale |
| items[].customerName | String | No | Customer name |
| items[].phone | String | No | Customer phone |
| items[].vehicleModel | String | No | Vehicle model name |
| items[].registrationNumber | String | No | Vehicle registration number |
| items[].profit | Decimal | No | Profit from the sale |
| totalCount | Int32 | No | Total number of sales matching criteria |
| pageNumber | Int32 | No | Current page number |
| pageSize | Int32 | No | Records per page |
| totalPages | Int32 | No | Total number of pages |

---

**Status Code: 200 OK (Empty Results)**

**Response Example:**
```json
{
  "items": [],
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 0
}
```

---

#### Edge Cases
- No sales in the system
- Invalid pageNumber (< 1)
- Invalid pageSize (< 1 or > max allowed)
- Search returns no results
- Date range has no matching sales
- fromDate is after toDate

#### Business Rules
- Default page size is 10 (may be limited to max 100)
- Page numbers start at 1
- Search is case-insensitive and matches partial strings
- Date range is inclusive
- Results are sorted by sale date (descending - newest first)
- Only completed sales are returned

#### Side Effects
- None (read-only operation)

#### Idempotent
Yes - Multiple identical requests return the same data

---

### GET Sale by Bill Number

**Endpoint Name:** Get Sale by Bill Number  
**HTTP Method:** GET  
**Route URL:** `/api/sales/{billNumber}`  
**Short Description:** Retrieve detailed sale information by bill number  
**Detailed Description:** Fetches comprehensive details of a specific sale transaction including vehicle, customer, payment, and profit information.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (GET request)

**Route Parameters:**

| Parameter Name | Data Type | Required | Format | Description |
|---|---|---|---|---|
| billNumber | Int32 | Mandatory | Integer | Unique bill number |

**Example:** `/api/sales/10001`

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
{
  "billNumber": 10001,
  "saleDate": "2026-02-20T10:00:00Z",
  "vehicleId": 1,
  "brand": "Hyundai",
  "model": "Creta",
  "year": 2022,
  "registrationNumber": "DL01AB1234",
  "chassisNumber": "MA1FA7YA2M2123456",
  "engineNumber": "G4LA987654",
  "sellingPrice": 850000,
  "customerName": "Rajesh Kumar",
  "customerPhone": "9876543210",
  "customerAddress": "123 Main Street, New Delhi",
  "customerPhotoUrl": "https://storage.example.com/photo.jpg",
  "purchaseDate": "2026-01-15T08:00:00Z",
  "buyingCost": 650000,
  "expense": 50000,
  "colour": "Silver",
  "paymentMode": 1,
  "cashAmount": 500000,
  "upiAmount": 350000,
  "financeAmount": null,
  "financeCompany": null,
  "profit": 150000,
  "totalReceived": 850000
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| billNumber | Int32 | No | Unique bill number |
| saleDate | DateTime | No | Sale date and time |
| vehicleId | Int32 | No | ID of sold vehicle |
| brand | String | No | Vehicle brand |
| model | String | No | Vehicle model |
| year | Int32 | No | Vehicle year |
| registrationNumber | String | No | Vehicle registration number |
| chassisNumber | String | Yes | Vehicle chassis number |
| engineNumber | String | Yes | Vehicle engine number |
| sellingPrice | Decimal | No | Selling price of vehicle |
| customerName | String | No | Customer name |
| customerPhone | String | No | Customer phone |
| customerAddress | String | Yes | Customer address |
| customerPhotoUrl | String | No | Customer photo URL |
| purchaseDate | DateTime | No | Date vehicle was purchased |
| buyingCost | Decimal | No | Original purchase cost |
| expense | Decimal | No | Total expenses incurred |
| colour | String | Yes | Vehicle color |
| paymentMode | Int32 | No | Payment method (1=Cash, 2=UPI, 3=Finance) |
| cashAmount | Decimal | Yes | Cash payment amount |
| upiAmount | Decimal | Yes | UPI payment amount |
| financeAmount | Decimal | Yes | Finance payment amount |
| financeCompany | String | Yes | Finance company name |
| profit | Decimal | No | Profit from sale |
| totalReceived | Decimal | No | Total payment received |

---

**Status Code: 404 Not Found**

**Response Example:**
```json
{
  "message": "Sale not found"
}
```

---

#### Edge Cases
- Bill number does not exist
- Invalid bill number format

#### Business Rules
- Bill numbers are unique and sequential
- Only existing bills can be retrieved
- Contains complete historical information for audit trail

#### Side Effects
- None (read-only operation)

#### Idempotent
Yes - Multiple identical requests return the same data

---

### GET Sale Invoice

**Endpoint Name:** Get Sale Invoice  
**HTTP Method:** GET  
**Route URL:** `/api/sales/{billNumber}/invoice`  
**Short Description:** Retrieve sale invoice data  
**Detailed Description:** Fetches formatted invoice data for a sale, including all necessary information for generating delivery note/invoice documents.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (GET request)

**Route Parameters:**

| Parameter Name | Data Type | Required | Format | Description |
|---|---|---|---|---|
| billNumber | Int32 | Mandatory | Integer | Unique bill number |

**Example:** `/api/sales/10001/invoice`

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
{
  "billNumber": 10001,
  "saleDate": "2026-02-20T10:00:00Z",
  "deliveryTime": "14:30:00",
  "customerName": "Rajesh Kumar",
  "fatherName": null,
  "phone": "9876543210",
  "address": "123 Main Street, New Delhi",
  "photoUrl": "https://storage.example.com/photo.jpg",
  "idProofNumber": null,
  "customerPhone": "9876543210",
  "customerAddress": "123 Main Street, New Delhi",
  "customerPhotoUrl": "https://storage.example.com/photo.jpg",
  "vehicleBrand": "Hyundai",
  "vehicleModel": "Creta",
  "registrationNumber": "DL01AB1234",
  "chassisNumber": "MA1FA7YA2M2123456",
  "engineNumber": "G4LA987654",
  "colour": "Silver",
  "sellingPrice": 850000,
  "paymentMode": 1,
  "cashAmount": 500000,
  "upiAmount": 350000,
  "financeAmount": null,
  "financeCompany": null,
  "rcBookReceived": true,
  "ownershipTransferAccepted": true,
  "vehicleAcceptedInAsIsCondition": true,
  "profit": 150000
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| billNumber | Int32 | No | Bill number |
| saleDate | DateTime | No | Sale date |
| deliveryTime | TimeSpan | Yes | Delivery time of vehicle |
| customerName | String | No | Customer name |
| fatherName | String | Yes | Customer father's name |
| phone | String | No | Customer phone |
| address | String | Yes | Customer address |
| photoUrl | String | No | Customer photo |
| idProofNumber | String | Yes | Customer ID proof number |
| customerPhone | String | No | Duplicate of phone field |
| customerAddress | String | Yes | Duplicate of address field |
| customerPhotoUrl | String | No | Duplicate of photoUrl field |
| vehicleBrand | String | No | Vehicle brand |
| vehicleModel | String | No | Vehicle model |
| registrationNumber | String | No | Vehicle registration |
| chassisNumber | String | Yes | Vehicle chassis number |
| engineNumber | String | Yes | Vehicle engine number |
| colour | String | Yes | Vehicle color |
| sellingPrice | Decimal | No | Selling price |
| paymentMode | Int32 | No | Payment method |
| cashAmount | Decimal | Yes | Cash amount |
| upiAmount | Decimal | Yes | UPI amount |
| financeAmount | Decimal | Yes | Finance amount |
| financeCompany | String | Yes | Finance company |
| rcBookReceived | Boolean | No | RC book received flag |
| ownershipTransferAccepted | Boolean | No | Ownership transfer flag |
| vehicleAcceptedInAsIsCondition | Boolean | No | As-is condition flag |
| profit | Decimal | No | Profit amount |

---

**Status Code: 404 Not Found**

**Response Example:**
```json
{
  "message": "Sale not found"
}
```

---

#### Edge Cases
- Bill number does not exist
- Delivery note settings not configured

#### Business Rules
- Contains all information needed for invoice generation
- Used by PDF generation service
- Includes delivery note settings from DeliveryNoteSettings

#### Side Effects
- None (read-only operation)

#### Idempotent
Yes - Multiple identical requests return the same data

---

### POST Send Invoice

**Endpoint Name:** Send Invoice  
**HTTP Method:** POST  
**Route URL:** `/api/sales/{billNumber}/send-invoice`  
**Short Description:** Generate and send invoice via WhatsApp  
**Detailed Description:** Generates a PDF invoice from the sale data and sends it to the customer via WhatsApp. The operation is asynchronous and may take a few seconds to complete.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (POST request with no body)

**Route Parameters:**

| Parameter Name | Data Type | Required | Format | Description |
|---|---|---|---|---|
| billNumber | Int32 | Mandatory | Integer | Unique bill number |

**Example:** `/api/sales/10001/send-invoice`

**Query Parameters:**

| Parameter Name | Data Type | Required | Default | Description |
|---|---|---|---|---|
| cancellationToken | (Implicit) | N/A | - | Server-side cancellation token (not passed by client) |

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
{
  "billNumber": 10001,
  "pdfUrl": "https://storage.example.com/invoices/10001.pdf",
  "status": "Sent"
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| billNumber | Int32 | No | Bill number of invoice |
| pdfUrl | String | No | URL to generated PDF invoice |
| status | String | No | Status of the operation (e.g., "Sent", "Pending", "Failed") |

---

**Status Code: 404 Not Found**

**Response Example:**
```json
{
  "message": "Sale not found"
}
```

---

**Status Code: 400 Bad Request**

**Response Example:**
```json
{
  "message": "Customer phone number not available for WhatsApp"
}
```

---

**Status Code: 502 Bad Gateway**

**Response Example:**
```json
{
  "message": "Failed to send WhatsApp message. Please try again later."
}
```

---

#### Error Response Examples

**Sale Not Found:**
```json
{
  "message": "Sale not found"
}
```

**No Customer Phone:**
```json
{
  "message": "Customer phone number not available"
}
```

**WhatsApp Service Unavailable:**
```json
{
  "message": "Failed to send WhatsApp message: Service temporarily unavailable"
}
```

**PDF Generation Failed:**
```json
{
  "message": "Failed to generate PDF invoice"
}
```

#### Edge Cases
- Bill number does not exist
- Customer phone number is null or invalid
- WhatsApp service is down
- PDF generation fails
- Network timeout during WhatsApp sending
- Invoice already sent (idempotency)
- Delivery note settings not configured

#### Business Rules
- Customer must have a valid phone number
- WhatsApp service must be configured and active
- PDF is generated on-the-fly
- Invoice can be sent multiple times (not idempotent)
- Requires active internet connection and WhatsApp API access
- Phone number must be in WhatsApp-supported format (with country code)

#### Side Effects
- PDF file generated and stored
- WhatsApp message sent to customer
- Operation may be logged for audit trail
- Delivery note sent timestamp may be updated (if tracked)

#### Idempotent
No - Multiple identical requests send multiple invoices

---

## Manual Billing

Standalone bills not tied to vehicle inventory. Full details: **`docs/manual-billing.md`**.

**Base path:** `/api/manual-bills`  
**Auth:** Bearer token, role `Admin`

| Method | Route | Description |
|--------|--------|-------------|
| POST | `/api/manual-bills` | Create manual bill. Returns `billNumber`, optional `pdfUrl`, `createdAt`. |
| GET | `/api/manual-bills/{billNumber}` | Get full manual bill detail. |
| GET | `/api/manual-bills/{billNumber}/invoice` | Get invoice DTO for PDF/preview. |
| GET | `/api/manual-bills/{billNumber}/pdf` | Get or create PDF URL. `?redirect=true` redirects to PDF. |
| POST | `/api/manual-bills/{billNumber}/send-invoice` | Generate PDF (if needed), send via WhatsApp. Returns `billNumber`, `pdfUrl`, `status`. |

**Environment:** Cloudinary (PDF/photos), WhatsApp (Meta) keys required for PDF storage and send-invoice. See `docs/manual-billing.md` for env vars (placeholders only), local setup, troubleshooting, and production checklist.

---

## Purchases

### POST Create Purchase

**Endpoint Name:** Create Purchase  
**HTTP Method:** POST  
**Route URL:** `/api/purchases`  
**Short Description:** Record a new vehicle purchase  
**Detailed Description:** Records the purchase of a vehicle from a seller. Creates both a Purchase record and an associated Vehicle record. Automatically calculates profit margin based on buying cost and expenses.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** `application/json`

**Request Body Example:**
```json
{
  "brand": "Hyundai",
  "model": "Creta",
  "year": 2022,
  "registrationNumber": "DL01AB1234",
  "chassisNumber": "MA1FA7YA2M2123456",
  "engineNumber": "G4LA987654",
  "colour": "Silver",
  "sellingPrice": 850000,
  "sellerName": "John Doe",
  "sellerPhone": "9876543210",
  "sellerAddress": "123 Seller Street, New Delhi",
  "buyingCost": 650000,
  "expense": 50000,
  "purchaseDate": "2026-01-15T10:00:00Z"
}
```

**Request Fields:**

| Field Name | Data Type | Required | Default | Validation Rules | Description |
|-----------|-----------|----------|---------|------------------|-------------|
| brand | String | Mandatory | - | Must not be null or empty | Vehicle manufacturer brand |
| model | String | Mandatory | - | Must not be null or empty | Vehicle model name |
| year | Int32 | Mandatory | - | Typically >= 1950 and <= current year + 1 | Manufacturing year |
| registrationNumber | String | Mandatory | - | Must be unique in system | Vehicle registration/plate number |
| chassisNumber | String | Optional | null | None | Vehicle chassis/VIN number |
| engineNumber | String | Optional | null | None | Vehicle engine number |
| colour | String | Optional | null | None | Vehicle color |
| sellingPrice | Decimal | Mandatory | - | > 0 | Intended selling price |
| sellerName | String | Mandatory | - | Must not be null or empty | Name of the seller |
| sellerPhone | String | Mandatory | - | Valid phone format | Seller's phone number |
| sellerAddress | String | Optional | null | None | Seller's address |
| buyingCost | Decimal | Mandatory | - | >= 0 | Amount paid to purchase vehicle |
| expense | Decimal | Mandatory | - | >= 0 | Additional expenses (registration, repairs, etc.) |
| purchaseDate | DateTime | Mandatory | - | Must be <= current date/time | Date of purchase |

---

#### Response

**Status Code: 201 Created**

**Response Example:**
```json
{
  "id": 1,
  "vehicleId": 1,
  "brand": "Hyundai",
  "model": "Creta",
  "year": 2022,
  "registrationNumber": "DL01AB1234",
  "colour": "Silver",
  "sellingPrice": 850000,
  "sellerName": "John Doe",
  "sellerPhone": "9876543210",
  "sellerAddress": "123 Seller Street, New Delhi",
  "buyingCost": 650000,
  "expense": 50000,
  "purchaseDate": "2026-01-15T10:00:00Z",
  "createdAt": "2026-02-20T10:30:00Z"
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| id | Int32 | No | Unique purchase record ID |
| vehicleId | Int32 | No | ID of the created vehicle |
| brand | String | No | Vehicle brand |
| model | String | No | Vehicle model |
| year | Int32 | No | Vehicle year |
| registrationNumber | String | No | Vehicle registration number |
| colour | String | Yes | Vehicle color |
| sellingPrice | Decimal | No | Intended selling price |
| sellerName | String | No | Seller name |
| sellerPhone | String | No | Seller phone |
| sellerAddress | String | Yes | Seller address |
| buyingCost | Decimal | No | Purchase cost |
| expense | Decimal | No | Additional expenses |
| purchaseDate | DateTime | No | Purchase date |
| createdAt | DateTime | No | Record creation timestamp |

---

**Status Code: 400 Bad Request**

**Response Example:**
```json
{
  "message": "Registration number already exists"
}
```

---

**Status Code: 409 Conflict**

**Response Example:**
```json
{
  "message": "A vehicle with this registration number is already in the system"
}
```

---

#### Error Response Examples

**Duplicate Registration:**
```json
{
  "message": "Registration number already exists"
}
```

**Invalid Year:**
```json
{
  "message": "Vehicle year must be between 1950 and current year + 1"
}
```

**Invalid Selling Price:**
```json
{
  "message": "Selling price must be greater than 0"
}
```

**Missing Required Fields:**
```json
{
  "message": "Brand, Model, Year, Registration Number, Seller Name, and Seller Phone are required"
}
```

#### Edge Cases
- Registration number already exists
- Invalid year (too old or in future)
- Negative buying cost or expense
- Null or empty required fields
- Selling price < buying cost + expenses (negative profit)
- Purchase date in the future

#### Business Rules
- Registration number must be unique
- Selling price should be > buying cost + expenses for profit
- Year must be realistic (1950 to current year + 1)
- All required fields must be provided
- Creates both Purchase and Vehicle records
- Vehicle status is initially set to Available (1)

#### Side Effects
- Database INSERT operation for Purchase record
- Database INSERT operation for Vehicle record
- Unique vehicleId generated and linked
- Creation timestamp recorded

#### Idempotent
No - Multiple identical requests create multiple purchases

---

### GET All Purchases

**Endpoint Name:** Get All Purchases  
**HTTP Method:** GET  
**Route URL:** `/api/purchases`  
**Short Description:** Retrieve all purchase records  
**Detailed Description:** Fetches a complete list of all vehicle purchases with details about buying cost, expenses, and current selling prices.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (GET request)

**Query Parameters:** None

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
[
  {
    "id": 1,
    "vehicleId": 1,
    "brand": "Hyundai",
    "model": "Creta",
    "year": 2022,
    "registrationNumber": "DL01AB1234",
    "colour": "Silver",
    "sellingPrice": 850000,
    "sellerName": "John Doe",
    "sellerPhone": "9876543210",
    "sellerAddress": "123 Seller Street, New Delhi",
    "buyingCost": 650000,
    "expense": 50000,
    "purchaseDate": "2026-01-15T10:00:00Z",
    "createdAt": "2026-02-20T10:30:00Z"
  }
]
```

**Response Fields:** (Same as Create Purchase Response)

---

**Status Code: 200 OK (Empty List)**

**Response Example:**
```json
[]
```

---

#### Edge Cases
- No purchases in system
- Large number of purchases (consider pagination)

#### Business Rules
- Returns all purchases regardless of vehicle status
- No pagination by default

#### Side Effects
- None (read-only operation)

#### Idempotent
Yes - Multiple identical requests return the same data

---

### GET Purchase by ID

**Endpoint Name:** Get Purchase by ID  
**HTTP Method:** GET  
**Route URL:** `/api/purchases/{id:int}`  
**Short Description:** Retrieve a specific purchase record  
**Detailed Description:** Fetches details of a specific purchase transaction by purchase ID.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (GET request)

**Route Parameters:**

| Parameter Name | Data Type | Required | Format | Description |
|---|---|---|---|---|
| id | Int32 | Mandatory | Integer | Unique purchase record ID |

**Example:** `/api/purchases/1`

---

#### Response

**Status Code: 200 OK**

**Response Example:** (Same as Create Purchase Response)

---

**Status Code: 404 Not Found**

**Response Example:**
```json
{
  "message": "Purchase not found"
}
```

---

#### Edge Cases
- Purchase ID does not exist

#### Business Rules
- Only existing purchases can be retrieved

#### Side Effects
- None (read-only operation)

#### Idempotent
Yes - Multiple identical requests return the same data

---

## Purchase Expenses

### POST Create Purchase Expense

**Endpoint Name:** Create Purchase Expense  
**HTTP Method:** POST  
**Route URL:** `/api/purchases/{vehicleId:int}/expenses`  
**Short Description:** Add an expense to a vehicle purchase  
**Detailed Description:** Records an additional expense (repairs, registration, insurance, etc.) for a purchased vehicle. These expenses affect the total cost basis and profit margin.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** `application/json`

**Route Parameters:**

| Parameter Name | Data Type | Required | Format | Description |
|---|---|---|---|---|
| vehicleId | Int32 | Mandatory | Integer | ID of the vehicle to add expense to |

**Request Body Example:**
```json
{
  "expenseType": "Repair Work",
  "amount": 15000
}
```

**Request Fields:**

| Field Name | Data Type | Required | Default | Validation Rules | Description |
|-----------|-----------|----------|---------|------------------|-------------|
| expenseType | String | Mandatory | - | Must not be null or empty | Type of expense (e.g., "Repair", "Insurance", "Registration") |
| amount | Decimal | Mandatory | - | > 0 | Amount of expense |

---

#### Response

**Status Code: 201 Created**

**Response Example:**
```json
{
  "id": 1,
  "vehicleId": 1,
  "expenseType": "Repair Work",
  "amount": 15000,
  "createdAt": "2026-02-20T10:30:00Z"
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| id | Int32 | No | Unique expense record ID |
| vehicleId | Int32 | No | ID of the vehicle |
| expenseType | String | No | Type of expense |
| amount | Decimal | No | Expense amount |
| createdAt | DateTime | No | Creation timestamp |

---

**Status Code: 404 Not Found**

**Response Example:**
```json
{
  "message": "Vehicle not found"
}
```

---

**Status Code: 400 Bad Request**

**Response Example:**
```json
{
  "message": "Expense amount must be greater than 0"
}
```

---

**Status Code: 409 Conflict**

**Response Example:**
```json
{
  "message": "Cannot add expenses to a sold vehicle"
}
```

---

#### Error Response Examples

**Vehicle Not Found:**
```json
{
  "message": "Vehicle not found"
}
```

**Invalid Amount:**
```json
{
  "message": "Expense amount must be positive"
}
```

**Vehicle Already Sold:**
```json
{
  "message": "Cannot add expenses to a sold vehicle"
}
```

#### Edge Cases
- Vehicle does not exist
- Vehicle is already sold
- Amount is zero or negative
- Expense type is empty

#### Business Rules
- Vehicle must exist and be Available (not sold)
- Amount must be positive
- Expenses are cumulative for a vehicle
- Cannot add expenses to sold vehicles
- Affects profit calculation on future sales

#### Side Effects
- Database INSERT operation
- Expense record created
- Total vehicle expenses updated

#### Idempotent
No - Multiple identical requests create multiple expense records

---

### GET Expenses by Vehicle

**Endpoint Name:** Get Expenses by Vehicle  
**HTTP Method:** GET  
**Route URL:** `/api/purchases/{vehicleId:int}/expenses`  
**Short Description:** Retrieve all expenses for a vehicle  
**Detailed Description:** Fetches all expense records associated with a specific vehicle.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (GET request)

**Route Parameters:**

| Parameter Name | Data Type | Required | Format | Description |
|---|---|---|---|---|
| vehicleId | Int32 | Mandatory | Integer | Vehicle ID |

**Example:** `/api/purchases/1/expenses`

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
[
  {
    "id": 1,
    "vehicleId": 1,
    "expenseType": "Repair Work",
    "amount": 15000,
    "createdAt": "2026-02-20T10:30:00Z"
  },
  {
    "id": 2,
    "vehicleId": 1,
    "expenseType": "Insurance",
    "amount": 5000,
    "createdAt": "2026-02-20T11:00:00Z"
  }
]
```

**Response Fields:** (Same as Create Purchase Expense Response)

---

**Status Code: 200 OK (Empty List)**

**Response Example:**
```json
[]
```

---

**Status Code: 404 Not Found**

**Response Example:**
```json
{
  "message": "Vehicle not found"
}
```

---

#### Edge Cases
- Vehicle does not exist
- Vehicle has no expenses

#### Business Rules
- Returns all expenses for a vehicle, regardless of vehicle status

#### Side Effects
- None (read-only operation)

#### Idempotent
Yes - Multiple identical requests return the same data

---

### DELETE Purchase Expense

**Endpoint Name:** Delete Purchase Expense  
**HTTP Method:** DELETE  
**Route URL:** `/api/purchases/expenses/{expenseId:int}`  
**Short Description:** Delete an expense record  
**Detailed Description:** Removes an expense record from a vehicle. This operation reverses the expense amount from the vehicle's total costs.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (DELETE request)

**Route Parameters:**

| Parameter Name | Data Type | Required | Format | Description |
|---|---|---|---|---|
| expenseId | Int32 | Mandatory | Integer | Unique expense record ID |

**Example:** `/api/purchases/expenses/1`

---

#### Response

**Status Code: 204 No Content**

*No response body is returned. The request was successful.*

---

**Status Code: 404 Not Found**

**Response Example:**
```json
{
  "message": "Expense not found"
}
```

---

**Status Code: 409 Conflict**

**Response Example:**
```json
{
  "message": "Cannot delete expense for a sold vehicle"
}
```

---

#### Error Response Examples

**Expense Not Found:**
```json
{
  "message": "Expense not found"
}
```

**Vehicle Already Sold:**
```json
{
  "message": "Cannot delete expense for a sold vehicle"
}
```

#### Edge Cases
- Expense ID does not exist
- Expense belongs to a sold vehicle

#### Business Rules
- Cannot delete expenses for sold vehicles
- Deletion reverses the expense amount
- Operation affects profit recalculation

#### Side Effects
- Database DELETE operation
- Expense record removed
- Total vehicle expenses updated
- Potential profit impact

#### Idempotent
No - Second delete attempt returns 404 if already deleted

---

## Finance Companies

### POST Create Finance Company

**Endpoint Name:** Create Finance Company  
**HTTP Method:** POST  
**Route URL:** `/api/finance-companies`  
**Short Description:** Create a new finance company  
**Detailed Description:** Registers a new financing company in the system. These companies are used when vehicles are sold with financing options.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** `application/json`

**Request Body Example:**
```json
{
  "name": "HDFC Bank Finance"
}
```

**Request Fields:**

| Field Name | Data Type | Required | Default | Validation Rules | Description |
|-----------|-----------|----------|---------|------------------|-------------|
| name | String | Mandatory | - | Must not be null or empty. Must be unique | Name of the finance company |

---

#### Response

**Status Code: 201 Created**

**Response Example:**
```json
{
  "id": 1,
  "name": "HDFC Bank Finance",
  "isActive": true,
  "createdAt": "2026-02-20T10:30:00Z"
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| id | Int32 | No | Unique finance company ID |
| name | String | No | Company name |
| isActive | Boolean | No | Whether company is active (true by default) |
| createdAt | DateTime | No | Creation timestamp |

---

**Status Code: 409 Conflict**

**Response Example:**
```json
{
  "message": "Finance company with this name already exists"
}
```

---

#### Error Response Examples

**Duplicate Name:**
```json
{
  "message": "Finance company with this name already exists"
}
```

**Missing Name:**
```json
{
  "message": "Finance company name is required"
}
```

#### Edge Cases
- Name already exists
- Name is empty or null
- Name contains special characters

#### Business Rules
- Company names must be unique
- New companies are active by default
- Cannot create duplicate companies

#### Side Effects
- Database INSERT operation
- Company record created and active

#### Idempotent
No - Multiple identical requests create multiple records

---

### GET All Finance Companies

**Endpoint Name:** Get All Finance Companies  
**HTTP Method:** GET  
**Route URL:** `/api/finance-companies`  
**Short Description:** Retrieve all finance companies  
**Detailed Description:** Fetches a list of all registered finance companies (active and inactive).

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (GET request)

**Query Parameters:** None

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
[
  {
    "id": 1,
    "name": "HDFC Bank Finance",
    "isActive": true,
    "createdAt": "2026-02-20T10:30:00Z"
  },
  {
    "id": 2,
    "name": "ICICI Financing",
    "isActive": true,
    "createdAt": "2026-02-19T14:00:00Z"
  }
]
```

**Response Fields:** (Same as Create Finance Company Response)

---

**Status Code: 200 OK (Empty List)**

**Response Example:**
```json
[]
```

---

#### Edge Cases
- No finance companies configured

#### Business Rules
- Returns both active and inactive companies

#### Side Effects
- None (read-only operation)

#### Idempotent
Yes - Multiple identical requests return the same data

---

### DELETE Finance Company

**Endpoint Name:** Delete Finance Company  
**HTTP Method:** DELETE  
**Route URL:** `/api/finance-companies/{id:int}`  
**Short Description:** Soft delete a finance company  
**Detailed Description:** Marks a finance company as inactive (soft delete). The company is no longer available for new sales but historical data is preserved.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (DELETE request)

**Route Parameters:**

| Parameter Name | Data Type | Required | Format | Description |
|---|---|---|---|---|
| id | Int32 | Mandatory | Integer | Unique finance company ID |

**Example:** `/api/finance-companies/1`

---

#### Response

**Status Code: 204 No Content**

*No response body is returned. The request was successful.*

---

**Status Code: 404 Not Found**

**Response Example:**
```json
{
  "message": "Finance company not found"
}
```

---

**Status Code: 409 Conflict**

**Response Example:**
```json
{
  "message": "Cannot delete finance company with active sales"
}
```

---

#### Error Response Examples

**Not Found:**
```json
{
  "message": "Finance company not found"
}
```

**Active Sales Exist:**
```json
{
  "message": "Cannot delete finance company that has active loan agreements"
}
```

#### Edge Cases
- Company ID does not exist
- Company has active financing agreements
- Company already deleted

#### Business Rules
- Soft delete (marks inactive)
- Cannot delete if used in active sales
- Historical data preserved for audit trail

#### Side Effects
- Database UPDATE operation
- Company marked as inactive (isActive = false)
- No longer available for new sales

#### Idempotent
No - Second delete may return 404 if already deleted

---

## Dashboard

### GET Dashboard

**Endpoint Name:** Get Dashboard  
**HTTP Method:** GET  
**Route URL:** `/api/dashboard`  
**Short Description:** Retrieve dashboard statistics  
**Detailed Description:** Fetches key business metrics including total vehicles purchased/sold, available inventory, total profit, and monthly sales figures.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (GET request)

**Query Parameters:** None

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
{
  "totalVehiclesPurchased": 15,
  "totalVehiclesSold": 12,
  "availableVehicles": 3,
  "totalProfit": 1850000,
  "salesThisMonth": 450000
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| totalVehiclesPurchased | Int32 | No | Total vehicles ever purchased |
| totalVehiclesSold | Int32 | No | Total vehicles ever sold |
| availableVehicles | Int32 | No | Current available inventory count |
| totalProfit | Decimal | No | Cumulative profit from all sales |
| salesThisMonth | Decimal | No | Total sales revenue for current month |

---

#### Edge Cases
- No sales or purchases yet
- All vehicles are sold out

#### Business Rules
- Includes all historical data (not just current month except for salesThisMonth)
- Available vehicles count = total purchased - total sold (minus deleted)
- Profit is calculated from completed sales only

#### Side Effects
- None (read-only operation)

#### Idempotent
Yes - Multiple identical requests return the same data

---

## Search

### GET Global Search

**Endpoint Name:** Global Search  
**HTTP Method:** GET  
**Route URL:** `/api/search`  
**Short Description:** Global search across sales and manual bills  
**Detailed Description:** Performs a comprehensive search across sales and manual bills. Matches bill number, customer name, phone, vehicle/model/registration (sales), item description (manual bills). Returns a unified list with **`type`**: `"Sale"` or `"ManualBill"`. **Phone in results is masked** (e.g. last four digits). For manual bills, `vehicle` and `registrationNumber` are null.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (GET request)

**Query Parameters:**

| Parameter Name | Data Type | Required | Default | Format | Description |
|---|---|---|---|---|---|
| q | String | Optional | empty string | Text | Search query string |

**Example:** `/api/search?q=Rajesh`

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
[
  {
    "type": "Sale",
    "billNumber": 10001,
    "customerName": "Rajesh Kumar",
    "customerPhone": "******3210",
    "vehicle": "Hyundai Creta 2022",
    "registrationNumber": "DL01AB1234",
    "saleDate": "2026-02-20T10:00:00Z"
  },
  {
    "type": "ManualBill",
    "billNumber": 1,
    "customerName": "Jane Smith",
    "customerPhone": "******7654",
    "vehicle": null,
    "registrationNumber": null,
    "saleDate": "2026-02-19T10:00:00Z"
  }
]
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| type | String | No | "Sale" or "ManualBill" |
| billNumber | Int32 | No | Bill number |
| customerName | String | No | Customer name |
| customerPhone | String | No | Masked phone (e.g. ******3210) |
| vehicle | String | Yes | Vehicle description (Sales); null for ManualBill |
| registrationNumber | String | Yes | Registration (Sales); null for ManualBill |
| saleDate | DateTime | No | Bill/sale date |

---

**Status Code: 200 OK (Empty Results)**

**Response Example:**
```json
[]
```

---

#### Edge Cases
- Empty search query
- No results match
- Search term is very short (may return many results)

#### Business Rules
- Case-insensitive search
- Partial string matching
- Only searches completed sales
- Searches across multiple fields

#### Side Effects
- None (read-only operation)

#### Idempotent
Yes - Multiple identical requests return the same data

---

## Upload

### POST Upload Customer Photo

**Endpoint Name:** Upload Customer Photo  
**HTTP Method:** POST  
**Route URL:** `/api/upload`  
**Short Description:** Upload customer photo  
**Detailed Description:** Uploads a customer photo file. Stores the file and returns the URL for reference in customer records and sales invoices.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** `multipart/form-data`

**Request Constraints:**
- Maximum file size: 2 MB (2,097,152 bytes)
- Supported formats: JPEG, PNG, GIF, WebP (typically)

**Form Data:**

| Field Name | Data Type | Required | Description |
|---|---|---|---|
| File | File (IFormFile) | Mandatory | The customer photo file |

**Example (using curl):**
```bash
curl -X POST http://api.example.com/api/upload \
  -H "Authorization: Bearer <token>" \
  -F "file=@photo.jpg"
```

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
{
  "url": "https://storage.example.com/customers/550e8400-e29b-41d4-a716-446655440000.jpg"
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| url | String | No | URL to uploaded photo for use in customer records |

---

**Status Code: 400 Bad Request**

**Response Example:**
```json
{
  "message": "File size exceeds 2 MB limit"
}
```

---

#### Error Response Examples

**File Too Large:**
```json
{
  "message": "File size exceeds maximum allowed size of 2 MB"
}
```

**Invalid File Type:**
```json
{
  "message": "File type not supported. Please upload JPEG, PNG, GIF, or WebP"
}
```

**No File Provided:**
```json
{
  "message": "No file provided in the request"
}
```

**Upload Failed:**
```json
{
  "message": "An error occurred while uploading the file"
}
```

#### Edge Cases
- File size exceeds 2 MB
- Invalid file format
- Corrupted file
- No file provided
- Network timeout during upload
- Storage service unavailable

#### Business Rules
- Maximum file size: 2 MB
- Only image files supported (JPEG, PNG, GIF, WebP typically)
- File is stored on configured storage (local or cloud)
- URL is returned for use in database records
- Original filename may be replaced with unique ID

#### Side Effects
- File stored on disk/cloud storage
- URL generated and returned
- May be used in customer records and sale invoices

#### Idempotent
No - Multiple uploads of the same file create separate file copies

---

## Delivery Note Settings

### GET Delivery Note Settings

**Endpoint Name:** Get Delivery Note Settings  
**HTTP Method:** GET  
**Route URL:** `/api/settings/delivery-note`  
**Short Description:** Retrieve delivery note configuration  
**Detailed Description:** Fetches the current delivery note/invoice template settings including shop details, GST information, and template text.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** N/A (GET request)

**Query Parameters:** None

---

#### Response

**Status Code: 200 OK**

**Response Example:**
```json
{
  "id": 1,
  "shopName": "Best Auto Sales",
  "shopAddress": "123 Business Street, New Delhi, India",
  "gSTNumber": "29ABCDE1234F2Z5",
  "contactNumber": "9876543210",
  "footerText": "Thank you for your business!",
  "termsAndConditions": "Vehicle sold as-is condition. No warranty provided.",
  "logoUrl": "https://storage.example.com/logo.png",
  "signatureLine": "Authorized Signatory",
  "updatedAt": "2026-02-15T10:00:00Z"
}
```

**Response Fields:**

| Field Name | Data Type | Nullable | Description |
|-----------|-----------|----------|-------------|
| id | Int32 | No | Settings record ID |
| shopName | String | No | Business/shop name |
| shopAddress | String | No | Business address |
| gSTNumber | String | Yes | GST registration number |
| contactNumber | String | Yes | Business contact phone |
| footerText | String | Yes | Custom footer text for invoices |
| termsAndConditions | String | Yes | Terms and conditions text |
| logoUrl | String | Yes | URL to business logo for invoice header |
| signatureLine | String | Yes | Signatory name for invoice |
| updatedAt | DateTime | No | Last update timestamp |

---

#### Edge Cases
- No settings configured yet (first time setup)

#### Business Rules
- Typically only one settings record exists
- Settings are used for all generated invoices
- Fields are mostly optional for flexibility

#### Side Effects
- None (read-only operation)

#### Idempotent
Yes - Multiple identical requests return the same data

---

### PUT Update Delivery Note Settings

**Endpoint Name:** Update Delivery Note Settings  
**HTTP Method:** PUT  
**Route URL:** `/api/settings/delivery-note`  
**Short Description:** Update delivery note configuration  
**Detailed Description:** Updates the delivery note/invoice template settings. All provided fields overwrite existing values.

**Authentication Requirement:** Yes, Role: `Admin`

---

#### Request

**Content-Type:** `application/json`

**Request Body Example:**
```json
{
  "shopName": "Best Auto Sales",
  "shopAddress": "123 Business Street, New Delhi, India",
  "gSTNumber": "29ABCDE1234F2Z5",
  "contactNumber": "9876543210",
  "footerText": "Thank you for your business!",
  "termsAndConditions": "Vehicle sold as-is condition. No warranty provided.",
  "logoUrl": "https://storage.example.com/logo.png",
  "signatureLine": "Authorized Signatory"
}
```

**Request Fields:**

| Field Name | Data Type | Required | Default | Validation Rules | Description |
|-----------|-----------|----------|---------|------------------|-------------|
| shopName | String | Mandatory | - | Must not be null or empty | Business/shop name |
| shopAddress | String | Mandatory | - | Must not be null or empty | Business address |
| gSTNumber | String | Optional | null | Valid GST format (15 chars typically) | GST registration number |
| contactNumber | String | Optional | null | Valid phone format | Business contact number |
| footerText | String | Optional | null | None | Custom footer for invoices |
| termsAndConditions | String | Optional | null | None | Terms and conditions |
| logoUrl | String | Optional | null | Valid URL format | Logo image URL |
| signatureLine | String | Optional | null | None | Signatory name |

---

#### Response

**Status Code: 200 OK**

**Response Example:** (Same as GET Delivery Note Settings)

---

#### Error Response Examples

**Missing Required Field:**
```json
{
  "message": "Shop name and address are required"
}
```

**Invalid GST Format:**
```json
{
  "message": "Invalid GST number format"
}
```

#### Edge Cases
- Shop name is empty
- Shop address is empty
- Invalid GST format
- Invalid logo URL

#### Business Rules
- Shop name and address are mandatory
- GST number must follow valid format if provided
- All fields are updatable
- Changes apply to all future invoices

#### Side Effects
- Database UPDATE operation
- Settings modified
- Affects all future generated invoices
- Updated timestamp set

#### Idempotent
No - Multiple updates may be non-idempotent depending on implementation

---

## Standard Error Response Format

All API endpoints may return error responses in the following standard format:

### General Error Response

```json
{
  "message": "Error description explaining what went wrong",
  "timestamp": "2026-02-20T10:30:00Z",
  "statusCode": 400
}
```

### Validation Error Response (for field-level validation)

```json
{
  "errors": {
    "fieldName": ["Error message for this field"],
    "anotherField": ["Error message 1", "Error message 2"]
  },
  "message": "One or more validation errors occurred"
}
```

### Exception-Based Error Response

```json
{
  "message": "An unexpected error occurred",
  "exceptionType": "NullReferenceException",
  "details": "Internal server error details (only in development)"
}
```

---

## Authentication & Authorization

### Bearer Token Authentication

All protected endpoints require a JWT Bearer token in the Authorization header:

```
Authorization: Bearer <token_from_login>
```

**Example:**
```bash
curl -X GET http://api.example.com/api/customers \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Role-Based Access Control

- **Admin Role (Required for most endpoints):** Only users with Admin role can access most API endpoints
- **No Authentication Required:** Login and Get Available Vehicles endpoints don't require authentication

---

## HTTP Status Codes Summary

| Status Code | Description | Common Usage |
|---|---|---|
| 200 OK | Successful GET request | Retrieve data successfully |
| 201 Created | Successful POST with resource creation | Create new records |
| 204 No Content | Successful DELETE | Deletion successful, no content to return |
| 400 Bad Request | Client error in request | Invalid input, validation failed |
| 401 Unauthorized | Authentication required or failed | Missing/invalid token, invalid credentials |
| 404 Not Found | Resource doesn't exist | Record not found |
| 409 Conflict | Request conflicts with current state | Duplicate record, cannot update sold vehicle |
| 422 Unprocessable Entity | Request validation failed | Complex validation errors |
| 500 Internal Server Error | Server error | Unexpected server-side error |
| 502 Bad Gateway | External service error | WhatsApp service down |

---

## Pagination Guidelines

For endpoints that return paginated results:

- **pageNumber:** Starts at 1 (first page)
- **pageSize:** Default is 10, max typically 100
- **totalPages:** Calculated as ceil(totalCount / pageSize)
- **hasNextPage:** Can be derived from (pageNumber < totalPages)

**Example Paginated Request:**
```
GET /api/sales?pageNumber=2&pageSize=20
```

---

## Date/Time Format

All dates and times are in **ISO 8601 format** with UTC timezone:

**Format:** `YYYY-MM-DDTHH:mm:ssZ`

**Example:** `2026-02-20T10:30:00Z`

---

## Important Notes

1. **Database Transactions:** Create endpoints (POST) typically involve multiple database operations that are atomic
2. **Profit Calculation:** Profit = (Total Received - Selling Price in DB) + (Selling Price - Buying Cost - Expenses)
3. **Soft Deletes:** Deleted records are marked as deleted but not physically removed (for audit trail)
4. **Photo Storage:** Customer photos are stored locally or on cloud storage (Cloudinary)
5. **File Uploads:** Maximum size is 2 MB with multipart/form-data encoding
6. **Concurrency:** Check for appropriate locking mechanisms in place for vehicle status updates during sales
7. **Audit Trail:** All create/update operations should timestamp records for audit purposes

---

## End of Documentation

This comprehensive API documentation covers all 11 controllers with 26 distinct endpoints. Each endpoint includes request/response specifications, validation rules, error handling, business rules, and edge cases.


