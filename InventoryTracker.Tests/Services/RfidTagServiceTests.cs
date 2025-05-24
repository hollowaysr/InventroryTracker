using FluentAssertions;
using InventoryTracker.Core.DTOs;
using InventoryTracker.Core.Entities;
using InventoryTracker.Core.Interfaces.Repositories;
using InventoryTracker.Data.Services;
using Moq;

namespace InventoryTracker.Tests.Services;

public class RfidTagServiceTests
{
    private readonly Mock<IRfidTagRepository> _mockRepository;
    private readonly Mock<ICustomerListRepository> _mockCustomerListRepository;
    private readonly RfidTagService _service;

    public RfidTagServiceTests()
    {
        _mockRepository = new Mock<IRfidTagRepository>();
        _mockCustomerListRepository = new Mock<ICustomerListRepository>();
        _service = new RfidTagService(_mockRepository.Object, _mockCustomerListRepository.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRfidTags()
    {
        // Arrange
        var rfidTags = new List<RfidTag>
        {
            new RfidTag { Id = 1, Rfid = "TAG001", Name = "Tag 1", ListId = 1 },
            new RfidTag { Id = 2, Rfid = "TAG002", Name = "Tag 2", ListId = 1 }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rfidTags);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Rfid.Should().Be("TAG001");
        result.Last().Rfid.Should().Be("TAG002");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnRfidTag()
    {
        // Arrange
        var rfidTag = new RfidTag 
        { 
            Id = 1, 
            Rfid = "TAG001",
            Name = "Test Tag", 
            Description = "Test Description",
            ListId = 1,
            CreatedAt = DateTime.UtcNow
        };
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rfidTag);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Tag");
        result.Rfid.Should().Be("TAG001");
        result.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateAndReturnRfidTag()
    {
        // Arrange
        var dto = new RfidTagCreateDto
        {
            Rfid = "TAG003",
            Name = "New Tag",
            Description = "New Description",
            ListId = 1,
            Color = "Blue",
            Size = "Medium"
        };

        var createdEntity = new RfidTag
        {
            Id = 1,
            Rfid = dto.Rfid,
            Name = dto.Name,
            Description = dto.Description,
            ListId = dto.ListId,
            Color = dto.Color,
            Size = dto.Size,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<RfidTag>()))
                      .ReturnsAsync(createdEntity);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Rfid.Should().Be("TAG003");
        result.Name.Should().Be("New Tag");
        result.Color.Should().Be("Blue");
        result.Size.Should().Be("Medium");
        
        _mockRepository.Verify(r => r.CreateAsync(It.Is<RfidTag>(t => 
            t.Rfid == dto.Rfid && 
            t.Name == dto.Name && 
            t.ListId == dto.ListId)), Times.Once);
    }

    [Fact]
    public async Task GetByListIdAsync_WithValidListId_ShouldReturnRfidTags()
    {
        // Arrange
        var rfidTags = new List<RfidTag>
        {
            new RfidTag { Id = 1, Rfid = "TAG001", Name = "Tag 1", ListId = 1 },
            new RfidTag { Id = 2, Rfid = "TAG002", Name = "Tag 2", ListId = 1 }
        };
        
        _mockRepository.Setup(r => r.GetByListIdAsync(1)).ReturnsAsync(rfidTags);

        // Act
        var result = await _service.GetByListIdAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.All(r => r.ListId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task GetByRfidAsync_WithValidRfid_ShouldReturnRfidTag()
    {
        // Arrange
        var rfidTag = new RfidTag 
        { 
            Id = 1, 
            Rfid = "TAG001",
            Name = "Test Tag",
            ListId = 1
        };
        
        _mockRepository.Setup(r => r.GetByRfidAsync("TAG001")).ReturnsAsync(rfidTag);

        // Act
        var result = await _service.GetByRfidAsync("TAG001");

        // Assert
        result.Should().NotBeNull();
        result!.Rfid.Should().Be("TAG001");
        result.Name.Should().Be("Test Tag");
    }

    [Fact]
    public async Task UpdateAsync_WithValidDto_ShouldUpdateAndReturnRfidTag()
    {
        // Arrange
        var existingEntity = new RfidTag
        {
            Id = 1,
            Rfid = "TAG001",
            Name = "Old Name",
            Description = "Old Description",
            ListId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updateDto = new RfidTagUpdateDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Color = "Red",
            Size = "Large"
        };

        var updatedEntity = new RfidTag
        {
            Id = 1,
            Rfid = existingEntity.Rfid,
            Name = updateDto.Name,
            Description = updateDto.Description,
            ListId = existingEntity.ListId,
            Color = updateDto.Color,
            Size = updateDto.Size,
            CreatedAt = existingEntity.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingEntity);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<RfidTag>()))
                      .ReturnsAsync(updatedEntity);

        // Act
        var result = await _service.UpdateAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Color.Should().Be("Red");
        result.Size.Should().Be("Large");
        
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<RfidTag>(t => 
            t.Id == 1 && 
            t.Name == updateDto.Name && 
            t.Color == updateDto.Color)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var existingEntity = new RfidTag
        {
            Id = 1,
            Rfid = "TAG001",
            Name = "Test Tag",
            ListId = 1
        };
        
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingEntity);
        _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetByNameAsync_WithValidName_ShouldReturnRfidTags()
    {
        // Arrange
        var rfidTags = new List<RfidTag>
        {
            new RfidTag { Id = 1, Rfid = "TAG001", Name = "Test Tag 1", ListId = 1 },
            new RfidTag { Id = 2, Rfid = "TAG002", Name = "Test Tag 2", ListId = 1 }
        };
        
        _mockRepository.Setup(r => r.GetByNameAsync("Test")).ReturnsAsync(rfidTags);

        // Act
        var result = await _service.GetByNameAsync("Test");

        // Assert
        result.Should().HaveCount(2);
        result.All(r => r.Name.Contains("Test")).Should().BeTrue();
    }

    [Fact]
    public async Task CreateBulkFromCsvAsync_WithValidCsvString_ShouldCreateRfidTags()
    {
        // Arrange
        var csvDto = new BulkCreateFromCsvDto
        {
            ListId = 1,
            CommaSeparatedRfids = "TAG001, TAG002, TAG003",
            DefaultName = "CSV Tag",
            DefaultDescription = "Imported from CSV",
            DefaultColor = "Blue",
            DefaultSize = "Medium"
        };

        var createdTags = new List<RfidTag>
        {
            new RfidTag { Id = 1, Rfid = "TAG001", Name = "CSV Tag", ListId = 1 },
            new RfidTag { Id = 2, Rfid = "TAG002", Name = "CSV Tag", ListId = 1 },
            new RfidTag { Id = 3, Rfid = "TAG003", Name = "CSV Tag", ListId = 1 }
        };

        _mockCustomerListRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.ExistsByRfidAsync(It.IsAny<string>())).ReturnsAsync(false);
        _mockRepository.Setup(r => r.CreateBulkAsync(It.IsAny<IEnumerable<RfidTag>>())).ReturnsAsync(createdTags);
        _mockRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(createdTags);

        // Act
        var result = await _service.CreateBulkFromCsvAsync(csvDto);

        // Assert
        result.Should().HaveCount(3);
        result.All(r => r.Name == "CSV Tag").Should().BeTrue();
        result.All(r => r.Color == "Blue").Should().BeTrue();
        result.Select(r => r.Rfid).Should().Contain(new[] { "TAG001", "TAG002", "TAG003" });
    }

    [Fact]
    public async Task CreateBulkFromCsvAsync_WithEmptyString_ShouldThrowArgumentException()
    {
        // Arrange
        var csvDto = new BulkCreateFromCsvDto
        {
            ListId = 1,
            CommaSeparatedRfids = "   ,   ,   "
        };

        _mockCustomerListRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateBulkFromCsvAsync(csvDto));
    }

    [Fact]
    public async Task CreateBulkFromCsvAsync_WithDuplicateRfids_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var csvDto = new BulkCreateFromCsvDto
        {
            ListId = 1,
            CommaSeparatedRfids = "TAG001, TAG002, TAG003"
        };

        _mockCustomerListRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.ExistsByRfidAsync("TAG001")).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateBulkFromCsvAsync(csvDto));
        exception.Message.Should().Contain("TAG001");
    }

    [Fact]
    public async Task ExportAsync_WithValidParameters_ShouldReturnExportData()
    {
        // Arrange
        var exportDto = new ExportRfidTagsDto
        {
            ListId = 1,
            Format = ExportFormat.Csv,
            IncludeMetadata = true
        };

        var tags = new List<RfidTag>
        {
            new RfidTag { Id = 1, Rfid = "TAG001", Name = "Tag 1", ListId = 1, CreatedAt = DateTime.UtcNow },
            new RfidTag { Id = 2, Rfid = "TAG002", Name = "Tag 2", ListId = 1, CreatedAt = DateTime.UtcNow }
        };

        var customerList = new CustomerList { Id = 1, Name = "Test List", Description = "Test" };

        _mockCustomerListRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockCustomerListRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customerList);
        _mockRepository.Setup(r => r.GetByListIdAsync(1)).ReturnsAsync(tags);

        // Act
        var result = await _service.ExportAsync(exportDto);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
        
        var csvContent = System.Text.Encoding.UTF8.GetString(result);
        csvContent.Should().Contain("TAG001");
        csvContent.Should().Contain("TAG002");
        csvContent.Should().Contain("Name"); // Header should be present
    }

    [Fact]
    public async Task ExportAsync_WithJsonFormat_ShouldReturnJsonData()
    {
        // Arrange
        var exportDto = new ExportRfidTagsDto
        {
            ListId = 1,
            Format = ExportFormat.Json,
            IncludeMetadata = true
        };

        var tags = new List<RfidTag>
        {
            new RfidTag { Id = 1, Rfid = "TAG001", Name = "Tag 1", ListId = 1, CreatedAt = DateTime.UtcNow }
        };

        var customerList = new CustomerList { Id = 1, Name = "Test List", Description = "Test" };

        _mockCustomerListRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockCustomerListRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customerList);
        _mockRepository.Setup(r => r.GetByListIdAsync(1)).ReturnsAsync(tags);

        // Act
        var result = await _service.ExportAsync(exportDto);

        // Assert
        result.Should().NotBeNull();
        
        var jsonContent = System.Text.Encoding.UTF8.GetString(result);
        jsonContent.Should().Contain("TAG001");
        jsonContent.Should().Contain("CustomerList");
        jsonContent.Should().Contain("Tags");
    }

    [Fact]
    public async Task ExportAndEmailAsync_WithValidEmail_ShouldReturnTrue()
    {
        // Arrange
        var exportDto = new ExportRfidTagsDto
        {
            ListId = 1,
            Format = ExportFormat.Csv,
            EmailAddress = "test@example.com",
            IncludeMetadata = true
        };

        var tags = new List<RfidTag>
        {
            new RfidTag { Id = 1, Rfid = "TAG001", Name = "Tag 1", ListId = 1 }
        };

        var customerList = new CustomerList { Id = 1, Name = "Test List" };

        _mockCustomerListRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockCustomerListRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customerList);
        _mockRepository.Setup(r => r.GetByListIdAsync(1)).ReturnsAsync(tags);

        // Act
        var result = await _service.ExportAndEmailAsync(exportDto);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExportAndEmailAsync_WithoutEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var exportDto = new ExportRfidTagsDto
        {
            ListId = 1,
            Format = ExportFormat.Csv,
            EmailAddress = null
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ExportAndEmailAsync(exportDto));
    }
}
