using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SRS.Application.Interfaces;

namespace SRS.Infrastructure.Services;

/// <summary>
/// WhatsApp integration service using Meta Graph API.
/// Sends invoice PDFs as document messages.
/// </summary>
public class MetaWhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MetaWhatsAppService> _logger;
    private readonly string _accessToken;
    private readonly string _phoneNumberId;
    private const string MetaGraphApiBaseUrl = "https://graph.facebook.com/v18.0";
    private const string MessageEndpoint = "/messages";

    /// <summary>
    /// Initializes a new instance of the MetaWhatsAppService.
    /// </summary>
    /// <param name="httpClient">HttpClient instance for making API calls.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when WhatsApp configuration is missing or incomplete.
    /// </exception>
    public MetaWhatsAppService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<MetaWhatsAppService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _accessToken = configuration["WhatsApp:AccessToken"] ?? string.Empty;
        _phoneNumberId = configuration["WhatsApp:PhoneNumberId"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(_accessToken) || string.IsNullOrWhiteSpace(_phoneNumberId))
        {
            throw new InvalidOperationException(
                "WhatsApp configuration is missing. Please configure 'WhatsApp:AccessToken' and 'WhatsApp:PhoneNumberId' in user-secrets or configuration.");
        }
    }

    /// <summary>
    /// Sends an invoice notification via WhatsApp with a PDF document link.
    /// </summary>
    /// <param name="toPhoneNumber">
    /// Recipient phone number in E.164 format without + (e.g., 919600433056).
    /// </param>
    /// <param name="customerName">Customer name used in the document caption.</param>
    /// <param name="mediaUrl">URL of the invoice PDF to be sent.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// API response containing message status or ID.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the Meta API returns a non-success status code.
    /// </exception>
    public async Task<string> SendInvoiceAsync(
        string toPhoneNumber,
        string customerName,
        string mediaUrl,
        CancellationToken cancellationToken = default)
    {
        // Validate input parameters
        ValidateInputParameters(toPhoneNumber, customerName, mediaUrl);

        // Ensure phone number is in correct format (no + sign, with country code)
        var formattedPhoneNumber = FormatPhoneNumber(toPhoneNumber);

        // Build the API endpoint URL
        var apiUrl = $"{MetaGraphApiBaseUrl}/{_phoneNumberId}{MessageEndpoint}";

        // Create the request payload
        var requestPayload = CreateDocumentMessagePayload(formattedPhoneNumber, customerName, mediaUrl);

        // Make the API request
        var response = await SendMessageToMetaApiAsync(apiUrl, requestPayload, cancellationToken);

        _logger.LogInformation(
            "WhatsApp invoice message sent to {PhoneNumber} with media {MediaUrl}.",
            formattedPhoneNumber,
            mediaUrl);

        return response;
    }

    /// <summary>
    /// Validates that all required input parameters are provided.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when parameters are null or empty.</exception>
    private static void ValidateInputParameters(string toPhoneNumber, string customerName, string mediaUrl)
    {
        if (string.IsNullOrWhiteSpace(toPhoneNumber))
        {
            throw new ArgumentException("Phone number cannot be null or empty.", nameof(toPhoneNumber));
        }

        if (string.IsNullOrWhiteSpace(customerName))
        {
            throw new ArgumentException("Customer name cannot be null or empty.", nameof(customerName));
        }

        if (string.IsNullOrWhiteSpace(mediaUrl))
        {
            throw new ArgumentException("Media URL cannot be null or empty.", nameof(mediaUrl));
        }

        if (!Uri.TryCreate(mediaUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Media URL must be a valid absolute URI.", nameof(mediaUrl));
        }
    }

    /// <summary>
    /// Formats the phone number to ensure it doesn't contain a + sign and includes country code.
    /// </summary>
    /// <param name="phoneNumber">Raw phone number.</param>
    /// <returns>Formatted phone number (e.g., 919600433056).</returns>
    private static string FormatPhoneNumber(string phoneNumber)
    {
        return phoneNumber.TrimStart('+');
    }

    /// <summary>
    /// Creates the document message payload for the Meta API.
    /// </summary>
    /// <param name="phoneNumber">Formatted phone number.</param>
    /// <param name="customerName">Customer name used in the caption.</param>
    /// <param name="mediaUrl">Public invoice PDF URL.</param>
    /// <returns>JSON string representing the message payload.</returns>
    private static string CreateDocumentMessagePayload(string phoneNumber, string customerName, string mediaUrl)
    {
        var payload = new
        {
            messaging_product = "whatsapp",
            to = phoneNumber,
            type = "document",
            document = new
            {
                link = mediaUrl,
                filename = $"invoice-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf",
                caption = $"Invoice for {customerName}"
            }
        };

        return JsonSerializer.Serialize(payload);
    }

    /// <summary>
    /// Sends the message to the Meta API and handles the response.
    /// </summary>
    /// <param name="apiUrl">Complete API endpoint URL.</param>
    /// <param name="jsonPayload">JSON-serialized request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>API response as JSON string.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the API returns a non-success HTTP status code.
    /// </exception>
    private async Task<string> SendMessageToMetaApiAsync(
        string apiUrl,
        string jsonPayload,
        CancellationToken cancellationToken)
    {
        using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        // Add authorization header
        var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
        {
            Content = content
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Meta WhatsApp API returned status {StatusCode}. Response: {ResponseContent}",
                    (int)response.StatusCode,
                    responseContent);

                throw new InvalidOperationException(
                    $"Meta WhatsApp API returned non-success status {(int)response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            return responseContent;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Meta WhatsApp API.");
            throw new InvalidOperationException(
                "Failed to communicate with Meta WhatsApp API. Please check your network connection and API configuration.",
                ex);
        }
    }
}

