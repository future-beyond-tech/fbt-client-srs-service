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
  - `GET /api/vehicles` - Get all vehicles
  - `POST /api/vehicles` - Create a vehicle
  - `GET /api/sales` - Get all sales
  - `POST /api/sales` - Create a sale
  - `GET /api/dashboard` - Get dashboard data
  - `GET /api/search` - Search functionality
  - `POST /api/upload` - Upload files

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

