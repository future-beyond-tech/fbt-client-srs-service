# Swagger JWT Authorization Testing Guide

## Setup Complete ✅

Your Swagger is now fully configured with JWT Bearer authentication. All controllers are protected and require authorization.

## How to Test Controllers in Swagger

### Step 1: Start the Application
```bash
cd /Users/bdadmin/FBT-Cients/fbt-client-srs-service/src
dotnet run --project SRS.API
```

### Step 2: Access Swagger UI
Open your browser and navigate to:
- Development: `https://localhost:7xxx/swagger` (check console for actual port)
- Or: `http://localhost:5xxx/swagger`

### Step 3: Get a JWT Token

1. In Swagger UI, find the **Auth** section
2. Expand `POST /api/auth/login`
3. Click **"Try it out"**
4. Enter credentials in the request body:
   ```json
   {
     "username": "your_username",
     "password": "your_password"
   }
   ```
5. Click **"Execute"**
6. Copy the `token` value from the response

### Step 4: Authorize Swagger

1. Look for the **"Authorize"** button at the top right of the Swagger UI (it has a lock icon 🔒)
2. Click the **"Authorize"** button
3. In the popup dialog, enter your token in this format:
   ```
   Bearer YOUR_TOKEN_HERE
   ```
   **Important:** The word "Bearer" followed by a space and then your token
   
   OR just paste the token directly (without "Bearer") as the system will add it automatically

4. Click **"Authorize"**
5. Click **"Close"**

### Step 5: Test Protected Endpoints

Now you can test any protected endpoint:
- The lock icon 🔒 next to each endpoint will appear closed/locked
- All requests will automatically include your JWT token in the Authorization header
- You can test:
  - **Vehicles:**
    - `GET /api/vehicles` - Get all vehicles
    - `GET /api/vehicles/available` - Get only available vehicles
  - **Purchases:**
    - `GET /api/purchases` - Get all purchase records
    - `GET /api/purchases/{id}` - Get specific purchase details
    - `POST /api/purchases` - Create a new purchase (adds vehicle to inventory)
  - **Sales:**
    - `POST /api/sales` - Create a new sale
    - `GET /api/sales/{billNumber}` - Get sale details by bill number
    - `GET /api/sales/{billNumber}/invoice` - Get invoice details
    - `POST /api/sales/{billNumber}/send-invoice` - Send invoice via WhatsApp
  - **Dashboard & Search:**
    - `GET /api/dashboard` - Get business dashboard metrics
    - `GET /api/search?q={query}` - Search across vehicles and sales
  - **Upload:**
    - `POST /api/upload` - Upload customer photo (multipart/form-data)

### Step 6: Logout (Optional)

To clear the authorization:
1. Click the **"Authorize"** button again
2. Click **"Logout"**
3. The lock icons will appear open/unlocked again

## Configuration Details

### What Was Configured:

1. **Swagger Security Definition** (`SwaggerExtensions.cs`):
   - Added JWT Bearer authentication scheme
   - Configured to accept tokens via the Authorization header
   - Format: `Bearer {token}`

2. **Global Security Requirement**:
   - All endpoints automatically require Bearer token
   - Applied globally in Swagger configuration

3. **Controller Protection**:
   - All controllers have `[Authorize(Roles = "Admin")]` attribute
   - Only authenticated Admin users can access endpoints
   - Auth controller login endpoint is public (no authorize attribute)

### Request & Response DTOs

To help you test, here are the primary DTO structures:

#### 1. Authentication (`POST /api/auth/login`)
**Request (LoginRequest):**
```json
{
  "username": "admin",
  "password": "password123"
}
```

#### 2. Purchases (`POST /api/purchases`)
**Request (PurchaseCreateDto):**
```json
{
  "brand": "Honda",
  "model": "Activa 6G",
  "year": 2023,
  "registrationNumber": "TN-01-AB-1234",
  "chassisNumber": "MD2JF...",
  "engineNumber": "JF51E...",
  "colour": "Black",
  "sellingPrice": 85000,
  "sellerName": "John Doe",
  "sellerPhone": "9876543210",
  "sellerAddress": "Chennai",
  "buyingCost": 70000,
  "expense": 2000,
  "purchaseDate": "2024-02-19T10:00:00Z"
}
```

#### 3. Sales (`POST /api/sales`)
**Request (SaleCreateDto):**
```json
{
  "vehicleId": 1,
  "customerName": "Jane Smith",
  "customerPhone": "9123456789",
  "paymentMode": 1, 
  "cashAmount": 85000,
  "saleDate": "2024-02-19T15:00:00Z"
}
```
*Note: `paymentMode`: 1 (Cash), 2 (UPI), 3 (Finance)*

#### 4. Dashboard Statistics (`GET /api/dashboard`)
**Response (DashboardDto):**
```json
{
  "totalVehiclesPurchased": 10,
  "totalVehiclesSold": 5,
  "availableVehicles": 5,
  "totalProfit": 75000,
  "salesThisMonth": 250000
}
```

#### 5. Search Results (`GET /api/search?q=...`)
**Response (List<SearchResultDto>):**
```json
[
  {
    "billNumber": 1001,
    "customerName": "Jane Smith",
    "customerPhone": "9123456789",
    "vehicle": "Honda Activa 6G",
    "registrationNumber": "TN-01-AB-1234",
    "saleDate": "2024-02-19T15:00:00Z"
  }
]
```

#### 6. Send Invoice (`POST /api/sales/{billNumber}/send-invoice`)
**Response (SendInvoiceResponseDto):**
```json
{
  "billNumber": 1001,
  "pdfUrl": "https://cloudinary.com/...",
  "status": "Sent Successfully"
}
```

#### 7. Sale Invoice Details (`GET /api/sales/{billNumber}/invoice`)
**Response (SaleInvoiceDto):**
```json
{
  "billNumber": 1001,
  "saleDate": "2024-02-19T15:00:00Z",
  "customerName": "Jane Smith",
  "phone": "9123456789",
  "vehicleBrand": "Honda",
  "vehicleModel": "Activa 6G",
  "registrationNumber": "TN-01-AB-1234",
  "sellingPrice": 85000,
  "paymentMode": 1,
  "cashAmount": 85000
}
```

### JWT Configuration:

The JWT settings are defined in `appsettings.json`:
- Issuer
- Audience  
- Secret Key
- Token expiration time

## Troubleshooting

### Token Expired
If you get 401 Unauthorized errors:
- Your token may have expired
- Get a new token by calling `/api/auth/login` again
- Re-authorize with the new token

### Invalid Token
- Ensure you copied the complete token
- Check for extra spaces or characters
- Make sure "Bearer" prefix is included (or omitted if system adds it)

### Unauthorized (401)
- Verify your user has the "Admin" role
- Check that the token is correctly formatted
- Ensure JWT settings in `appsettings.json` are correct

## Notes

- All controllers require the "Admin" role
- The login endpoint (`POST /api/auth/login`) does not require authorization
- Tokens are included automatically in all requests after authorization
- You only need to authorize once per Swagger session

