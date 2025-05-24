using FluentAssertions;
using InventoryTracker.Core.DTOs;
using InventoryTracker.Core.Entities;
using InventoryTracker.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace InventoryTracker.Tests.Controllers;

public class RfidTagsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly InventoryTrackerDbContext _context;

    public RfidTagsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<InventoryTrackerDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<InventoryTrackerDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
            });
        });

        _client = _factory.CreateClient();
        
        // Get the in-memory database context
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<InventoryTrackerDbContext>();
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        var customerList = new CustomerList
        {
            Id = 1,
            Name = "Test Customer List",
            Description = "Test Description",
            SystemRef = "SYS001"
        };

        _context.CustomerLists.Add(customerList);

        var rfidTags = new List<RfidTag>
        {
            new RfidTag { Id = 1, Rfid = "TAG001", Name = "Tag 1", ListId = 1, Description = "Description 1" },
            new RfidTag { Id = 2, Rfid = "TAG002", Name = "Tag 2", ListId = 1, Description = "Description 2" }
        };

        _context.RfidTags.AddRange(rfidTags);
        _context.SaveChanges();
    }

    [Fact]
    public async Task BulkCreateFromCsv_WithValidData_ShouldReturn201()
    {
        // Arrange
        var csvDto = new BulkCreateFromCsvDto
        {
            ListId = 1,
            CommaSeparatedRfids = "TAG101, TAG102, TAG103",
            DefaultName = "Bulk Tag",
            DefaultDescription = "Imported via CSV",
            DefaultColor = "Green",
            DefaultSize = "Large"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/rfidtags/bulk-csv", csvDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdTags = await response.Content.ReadFromJsonAsync<List<RfidTagDto>>();
        createdTags.Should().HaveCount(3);
        createdTags.Should().OnlyContain(t => t.Name == "Bulk Tag");
        createdTags.Should().OnlyContain(t => t.Color == "Green");
        createdTags.Select(t => t.Rfid).Should().Contain(new[] { "TAG101", "TAG102", "TAG103" });
    }

    [Fact]
    public async Task BulkCreateFromCsv_WithDuplicateRfids_ShouldReturn409()
    {
        // Arrange
        var csvDto = new BulkCreateFromCsvDto
        {
            ListId = 1,
            CommaSeparatedRfids = "TAG001, TAG104, TAG105" // TAG001 already exists
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/rfidtags/bulk-csv", csvDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task BulkCreateFromCsv_WithInvalidListId_ShouldReturn400()
    {
        // Arrange
        var csvDto = new BulkCreateFromCsvDto
        {
            ListId = 999, // Non-existent list
            CommaSeparatedRfids = "TAG201, TAG202"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/rfidtags/bulk-csv", csvDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task BulkCreateFromCsv_WithEmptyRfids_ShouldReturn400()
    {
        // Arrange
        var csvDto = new BulkCreateFromCsvDto
        {
            ListId = 1,
            CommaSeparatedRfids = "   ,   ,   " // Only whitespace and commas
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/rfidtags/bulk-csv", csvDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExportTags_WithCsvFormat_ShouldReturnCsvFile()
    {
        // Arrange
        var exportDto = new ExportRfidTagsDto
        {
            ListId = 1,
            Format = ExportFormat.Csv,
            IncludeMetadata = true
        };

        // Act
        var response = await _client.PostAsync("/api/rfidtags/export", JsonContent.Create(exportDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("RFID,Name,Description");
        content.Should().Contain("TAG001");
        content.Should().Contain("TAG002");
    }

    [Fact]
    public async Task ExportTags_WithJsonFormat_ShouldReturnJsonFile()
    {
        // Arrange
        var exportDto = new ExportRfidTagsDto
        {
            ListId = 1,
            Format = ExportFormat.Json,
            IncludeMetadata = true
        };

        // Act
        var response = await _client.PostAsync("/api/rfidtags/export", JsonContent.Create(exportDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("CustomerList");
        content.Should().Contain("Tags");
        content.Should().Contain("TAG001");
    }

    [Fact]
    public async Task ExportTags_WithXmlFormat_ShouldReturnXmlFile()
    {
        // Arrange
        var exportDto = new ExportRfidTagsDto
        {
            ListId = 1,
            Format = ExportFormat.Xml,
            IncludeMetadata = false // Test without metadata
        };

        // Act
        var response = await _client.PostAsync("/api/rfidtags/export", JsonContent.Create(exportDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/xml");
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("<?xml version=\"1.0\"");
        content.Should().Contain("<RfidExport>");
        content.Should().Contain("TAG001");
        content.Should().NotContain("<Name>"); // Metadata should not be included
    }

    [Fact]
    public async Task ExportTags_WithInvalidListId_ShouldReturn400()
    {
        // Arrange
        var exportDto = new ExportRfidTagsDto
        {
            ListId = 999,
            Format = ExportFormat.Csv
        };

        // Act
        var response = await _client.PostAsync("/api/rfidtags/export", JsonContent.Create(exportDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExportAndEmailTags_WithValidEmail_ShouldReturn200()
    {
        // Arrange
        var exportDto = new ExportRfidTagsDto
        {
            ListId = 1,
            Format = ExportFormat.Csv,
            EmailAddress = "test@example.com",
            IncludeMetadata = true
        };

        // Act
        var response = await _client.PostAsync("/api/rfidtags/export-email", JsonContent.Create(exportDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("Export sent successfully");
        result.Should().Contain("test@example.com");
    }

    [Fact]
    public async Task ExportAndEmailTags_WithoutEmail_ShouldReturn400()
    {
        // Arrange
        var exportDto = new ExportRfidTagsDto
        {
            ListId = 1,
            Format = ExportFormat.Csv
            // EmailAddress not set
        };

        // Act
        var response = await _client.PostAsync("/api/rfidtags/export-email", JsonContent.Create(exportDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByListId_ShouldReturnTagsForList()
    {
        // Act
        var response = await _client.GetAsync("/api/rfidtags/by-list/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tags = await response.Content.ReadFromJsonAsync<List<RfidTagDto>>();
        tags.Should().HaveCount(2);
        tags.Should().OnlyContain(t => t.ListId == 1);
    }

    public void Dispose()
    {
        _context.Dispose();
        _client.Dispose();
        _factory.Dispose();
    }
}
