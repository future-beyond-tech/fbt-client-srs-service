# Meta WhatsApp Integration - Quick Setup Guide

## 🚀 Quick Start (5 minutes)

### Prerequisites
- Meta Business Account with WhatsApp Business
- WhatsApp Business Phone Number ID
- Long-lived API Access Token (36-month expiry)

### Step 1: Configure Local Development

```bash
cd /Users/bdadmin/FBT-Cients/fbt-client-srs-service/src/SRS.API

# Set your WhatsApp credentials
dotnet user-secrets set "WhatsApp:AccessToken" "YOUR_LONG_LIVED_ACCESS_TOKEN"
dotnet user-secrets set "WhatsApp:PhoneNumberId" "YOUR_PHONE_NUMBER_ID"
```

### Step 2: Verify Configuration

```bash
# View stored secrets
dotnet user-secrets list

# Should output:
# WhatsApp:AccessToken = ***
# WhatsApp:PhoneNumberId = ***
```

### Step 3: Build and Test

```bash
# Build the solution
dotnet build

# Run the API
dotnet run
```

### Step 4: Test Invoice Sending

The service is now ready to use. When `SaleService` sends an invoice:

```csharp
// This will automatically use MetaWhatsAppService
await _whatsAppService.SendInvoiceAsync(
    toPhoneNumber: "919600433056",
    customerName: "John Doe",
    mediaUrl: "https://your-domain.com/invoices/123.pdf"
);
```

---

## 🔑 Configuration Details

### appsettings.json (Default/Empty)
```json
{
  "WhatsApp": {
    "AccessToken": "",
    "PhoneNumberId": ""
  }
}
```

### User-Secrets (Development)
```
WhatsApp:AccessToken = eyJhbGc...
WhatsApp:PhoneNumberId = 123456789
```

### Environment Variables (Production/Docker)
```bash
export WhatsApp__AccessToken="YOUR_TOKEN"
export WhatsApp__PhoneNumberId="YOUR_PHONE_ID"
```

### Azure Key Vault (Production Recommended)
```
WhatsApp--AccessToken
WhatsApp--PhoneNumberId
```

---

## 📱 Phone Number Format

### Accepted Formats
```csharp
// Both work identically:
await service.SendInvoiceAsync(
    toPhoneNumber: "919600433056",     // ✅ Without + (preferred)
    ...
);

await service.SendInvoiceAsync(
    toPhoneNumber: "+919600433056",    // ✅ With + (auto-converted)
    ...
);

// Invalid formats:
toPhoneNumber: "9600433056"            // ❌ No country code
toPhoneNumber: "+91 9600433056"        // ❌ Spaces not allowed
```

### Country Codes
```
India:        91 (e.g., 919600433056)
USA:          1  (e.g., 11234567890)
UK:           44 (e.g., 441234567890)
Brazil:       55 (e.g., 551199999999)
```

---

## 📧 Message Template Setup (Meta WhatsApp Manager)

### Template Name
```
invoice_notification
```

### Template Language
```
English (en)
```

### Template Body Example
```
Hello {{1}},

Your vehicle invoice is ready. Please check the attached document.

Best regards,
SRS Billing System
```

### Important
- **Must be pre-approved** by Meta before use
- Template name is case-sensitive
- Must have at least one variable ({{1}}) for customer name

---

## 🛠️ Troubleshooting

### Error: "WhatsApp configuration is missing"
**Solution:**
```bash
# Verify secrets are set
dotnet user-secrets list

# If empty, set them:
dotnet user-secrets set "WhatsApp:AccessToken" "YOUR_TOKEN"
dotnet user-secrets set "WhatsApp:PhoneNumberId" "YOUR_PHONE_ID"
```

### Error: "Meta WhatsApp API returned non-success status 401"
**Problem:** Invalid or expired access token
**Solution:**
1. Generate new long-lived token from Meta Business Manager
2. Update user-secrets or environment variables
3. Ensure token has `whatsapp_business_messaging` permission

### Error: "Meta WhatsApp API returned non-success status 400"
**Problem:** Invalid request format or phone number
**Solution:**
1. Check phone number format (country code required)
2. Verify template name matches exactly
3. Check template variable count matches parameters
4. Verify phone number is WhatsApp-capable

### Error: "Meta WhatsApp API returned non-success status 403"
**Problem:** Phone number ID invalid or not associated with token
**Solution:**
1. Verify phone number ID is correct
2. Check phone number is in the same business account
3. Ensure business account is in good standing

### Error: "Failed to communicate with Meta WhatsApp API"
**Problem:** Network connectivity issue
**Solution:**
1. Check internet connection
2. Verify Meta API is accessible
3. Check firewall/proxy settings
4. See exception InnerException for details

---

## 🧪 Manual Testing

### Using cURL
```bash
# Test message send
curl -X POST \
  "https://graph.facebook.com/v18.0/YOUR_PHONE_NUMBER_ID/messages" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "messaging_product": "whatsapp",
    "to": "919600433056",
    "type": "template",
    "template": {
      "name": "invoice_notification",
      "language": {
        "code": "en"
      },
      "components": [
        {
          "type": "body",
          "parameters": [
            {
              "type": "text",
              "text": "John Doe"
            }
          ]
        }
      ]
    }
  }'
```

### Using Postman
1. Create new POST request
2. URL: `https://graph.facebook.com/v18.0/YOUR_PHONE_NUMBER_ID/messages`
3. Header: `Authorization: Bearer YOUR_ACCESS_TOKEN`
4. Header: `Content-Type: application/json`
5. Body (raw JSON): [See cURL example above]
6. Click Send

### Expected Response
```json
{
  "messaging_product": "whatsapp",
  "contacts": [
    {
      "input": "919600433056",
      "wa_id": "919600433056"
    }
  ],
  "messages": [
    {
      "id": "wamid.xxxxxxxxxxxxx=",
      "message_status": "accepted"
    }
  ]
}
```

---

## 📊 Monitoring & Logging

### Log Levels
```csharp
// Successful send:
// Microsoft.Extensions.Http: "Sending HTTP request POST https://graph.facebook.com/v18.0/..."

// Error send:
// SRS.Infrastructure.Services.MetaWhatsAppService: "InvalidOperationException: Meta WhatsApp API returned non-success status 400..."
```

### Key Metrics to Monitor
- API response time (typically < 1s)
- Success rate (target > 99%)
- Token refresh cycles (every 30-35 months)
- Rate limits (1000 messages/day limit)

---

## 🔐 Security Checklist

- ✅ Never commit tokens to version control
- ✅ Use user-secrets in development
- ✅ Use Azure Key Vault or AWS Secrets Manager in production
- ✅ Rotate access tokens every 2 years (if possible)
- ✅ Monitor token usage in Meta Business Manager
- ✅ Use environment-specific configuration
- ✅ Enable API audit logging in Meta Business Manager
- ✅ Restrict IP addresses for API access (if possible)

---

## 📚 Resources

### Official Meta Documentation
- https://developers.facebook.com/docs/whatsapp/cloud-api/
- https://developers.facebook.com/docs/whatsapp/business-platform-get-started

### Getting Access Token
- https://developers.facebook.com/docs/whatsapp/business-platform-get-started#create-system-user

### Template Messages
- https://developers.facebook.com/docs/whatsapp/message-templates/

### API Reference
- https://developers.facebook.com/docs/whatsapp/cloud-api/reference/message

---

## ✨ Code Example

```csharp
// In SaleService or any injected service:
private readonly IWhatsAppService _whatsAppService;

public SaleService(IWhatsAppService whatsAppService)
{
    _whatsAppService = whatsAppService;
}

public async Task SendInvoiceAsync(string phoneNumber, string customerName, string pdfUrl)
{
    try
    {
        var response = await _whatsAppService.SendInvoiceAsync(
            toPhoneNumber: phoneNumber,
            customerName: customerName,
            mediaUrl: pdfUrl,
            cancellationToken: CancellationToken.None
        );
        
        _logger.LogInformation("Invoice sent successfully. Response: {Response}", response);
    }
    catch (InvalidOperationException ex)
    {
        _logger.LogError(ex, "Failed to send WhatsApp invoice");
        throw;
    }
}
```

---

## 🎯 Success Criteria

You know the setup is correct when:

1. ✅ `dotnet build` completes without errors
2. ✅ `dotnet user-secrets list` shows WhatsApp keys
3. ✅ Application starts without configuration errors
4. ✅ Manual cURL test successfully sends message
5. ✅ Customer receives invoice on WhatsApp
6. ✅ Message shows with template formatting

---

**Last Updated:** February 20, 2026  
**Version:** 1.0.0

