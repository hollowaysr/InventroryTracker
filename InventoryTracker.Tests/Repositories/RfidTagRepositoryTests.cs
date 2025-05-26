using FluentAssertions;
using InventoryTracker.Core.Entities;
using InventoryTracker.Data;
using InventoryTracker.Data.Context;
using InventoryTracker.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventoryTracker.Tests.Repositories;

public class RfidTagRepositoryTests : IDisposable
{
    private readonly InventoryTrackerDbContext _context;
    private readonly RfidTagRepository _repository;
    private readonly CustomerList _testCustomerList;

    public RfidTagRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<InventoryTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new InventoryTrackerDbContext(options);
        _repository = new RfidTagRepository(_context);

        // Create a test customer list for RFID tags
        _testCustomerList = new CustomerList
        {
            Name = "Test Customer List",
            Description = "Test Description"
        };
        _context.CustomerLists.Add(_testCustomerList);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddRfidTagToDatabase()
    {
        // Arrange
        var rfidTag = new RfidTag
        {
            Rfid = "TAG001",
            Name = "Test Tag",
            Description = "Test Description",
            ListId = _testCustomerList.Id,
            Color = "Blue",
            Size = "Medium"
        };

        // Act
        var result = await _repository.CreateAsync(rfidTag);        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Rfid.Should().Be("TAG001");
        result.Name.Should().Be("Test Tag");

        var dbEntity = await _context.RfidTags.FindAsync(result.Id);
        dbEntity.Should().NotBeNull();
        dbEntity!.Rfid.Should().Be("TAG001");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnRfidTag()
    {
        // Arrange
        var rfidTag = new RfidTag
        {
            Rfid = "TAG002",
            Name = "Test Tag",
            Description = "Test Description",
            ListId = _testCustomerList.Id
        };
        _context.RfidTags.Add(rfidTag);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(rfidTag.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Rfid.Should().Be("TAG002");
        result.Name.Should().Be("Test Tag");
        result.Description.Should().Be("Test Description");
    }    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRfidTags()
    {
        // Arrange
        var tags = new List<RfidTag>
        {
            new RfidTag { Rfid = "TAG003", Name = "Tag 1", ListId = _testCustomerList.Id },
            new RfidTag { Rfid = "TAG004", Name = "Tag 2", ListId = _testCustomerList.Id },
            new RfidTag { Rfid = "TAG005", Name = "Tag 3", ListId = _testCustomerList.Id }
        };
        _context.RfidTags.AddRange(tags);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(t => t.Rfid == "TAG003");
        result.Should().Contain(t => t.Rfid == "TAG004");
        result.Should().Contain(t => t.Rfid == "TAG005");
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingRfidTag()
    {
        // Arrange
        var rfidTag = new RfidTag
        {
            Rfid = "TAG006",
            Name = "Original Name",
            Description = "Original Description",
            ListId = _testCustomerList.Id
        };
        _context.RfidTags.Add(rfidTag);
        await _context.SaveChangesAsync();

        rfidTag.Name = "Updated Name";
        rfidTag.Description = "Updated Description";
        rfidTag.Color = "Red";

        // Act
        var result = await _repository.UpdateAsync(rfidTag);

        // Assert        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.Color.Should().Be("Red");

        var dbEntity = await _context.RfidTags.FindAsync(rfidTag.Id);
        dbEntity!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldRemoveRfidTag()
    {
        // Arrange
        var rfidTag = new RfidTag
        {
            Rfid = "TAG007",
            Name = "Test Tag",
            ListId = _testCustomerList.Id
        };
        _context.RfidTags.Add(rfidTag);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(rfidTag.Id);

        // Assert
        result.Should().BeTrue();

        var dbEntity = await _context.RfidTags.FindAsync(rfidTag.Id);
        dbEntity.Should().BeNull();
    }

    [Fact]
    public async Task GetByListIdAsync_ShouldReturnTagsForSpecificList()
    {
        // Arrange
        var secondCustomerList = new CustomerList
        {
            Name = "Second Customer List",
            Description = "Second Description"
        };
        _context.CustomerLists.Add(secondCustomerList);
        await _context.SaveChangesAsync();

        var tags = new List<RfidTag>
        {
            new RfidTag { Rfid = "TAG008", Name = "Tag 1", ListId = _testCustomerList.Id },
            new RfidTag { Rfid = "TAG009", Name = "Tag 2", ListId = _testCustomerList.Id },
            new RfidTag { Rfid = "TAG010", Name = "Tag 3", ListId = secondCustomerList.Id }
        };
        _context.RfidTags.AddRange(tags);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByListIdAsync(_testCustomerList.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.ListId == _testCustomerList.Id);
        result.Should().Contain(t => t.Rfid == "TAG008");
        result.Should().Contain(t => t.Rfid == "TAG009");
    }

    [Fact]
    public async Task GetByRfidAsync_ShouldReturnMatchingTag()
    {
        // Arrange
        var rfidTag = new RfidTag
        {
            Rfid = "TAG011",
            Name = "Unique Tag",
            ListId = _testCustomerList.Id
        };
        _context.RfidTags.Add(rfidTag);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByRfidAsync("TAG011");

        // Assert
        result.Should().NotBeNull();
        result!.Rfid.Should().Be("TAG011");
        result.Name.Should().Be("Unique Tag");
    }

    [Fact]
    public async Task GetByRfidAsync_WithNonExistingRfid_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByRfidAsync("NONEXISTENT");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnMatchingTags()
    {
        // Arrange
        var tags = new List<RfidTag>
        {
            new RfidTag { Rfid = "TAG012", Name = "Test Tag 1", ListId = _testCustomerList.Id },
            new RfidTag { Rfid = "TAG013", Name = "Test Tag 2", ListId = _testCustomerList.Id },
            new RfidTag { Rfid = "TAG014", Name = "Other Tag", ListId = _testCustomerList.Id }
        };
        _context.RfidTags.AddRange(tags);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Test");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.Name.Contains("Test"));
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateRfid_ShouldThrowException()
    {
        // Arrange
        var firstTag = new RfidTag
        {
            Rfid = "DUPLICATE",
            Name = "First Tag",
            ListId = _testCustomerList.Id
        };
        _context.RfidTags.Add(firstTag);
        await _context.SaveChangesAsync();

        var duplicateTag = new RfidTag
        {
            Rfid = "DUPLICATE",
            Name = "Second Tag",
            ListId = _testCustomerList.Id
        };

        // Act & Assert
        var act = async () => await _repository.CreateAsync(duplicateTag);
        await act.Should().ThrowAsync<Exception>();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
