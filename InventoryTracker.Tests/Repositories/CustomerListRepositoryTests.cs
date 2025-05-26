using FluentAssertions;
using InventoryTracker.Core.Entities;
using InventoryTracker.Data;
using InventoryTracker.Data.Context;
using InventoryTracker.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventoryTracker.Tests.Repositories;

public class CustomerListRepositoryTests : IDisposable
{
    private readonly InventoryTrackerDbContext _context;
    private readonly CustomerListRepository _repository;

    public CustomerListRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<InventoryTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new InventoryTrackerDbContext(options);
        _repository = new CustomerListRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddCustomerListToDatabase()
    {
        // Arrange
        var customerList = new CustomerList
        {
            Name = "Test List",
            Description = "Test Description",
            SystemRef = "SYS001"
        };

        // Act
        var result = await _repository.CreateAsync(customerList);        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Name.Should().Be("Test List");

        var dbEntity = await _context.CustomerLists.FindAsync(result.Id);
        dbEntity.Should().NotBeNull();
        dbEntity!.Name.Should().Be("Test List");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnCustomerList()
    {
        // Arrange
        var customerList = new CustomerList
        {
            Name = "Test List",
            Description = "Test Description"
        };
        _context.CustomerLists.Add(customerList);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(customerList.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test List");
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
    public async Task GetAllAsync_ShouldReturnAllCustomerLists()
    {
        // Arrange
        var lists = new List<CustomerList>
        {
            new CustomerList { Name = "List 1", Description = "Description 1" },
            new CustomerList { Name = "List 2", Description = "Description 2" },
            new CustomerList { Name = "List 3", Description = "Description 3" }
        };
        _context.CustomerLists.AddRange(lists);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(l => l.Name == "List 1");
        result.Should().Contain(l => l.Name == "List 2");
        result.Should().Contain(l => l.Name == "List 3");
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingCustomerList()
    {
        // Arrange
        var customerList = new CustomerList
        {
            Name = "Original Name",
            Description = "Original Description"
        };
        _context.CustomerLists.Add(customerList);
        await _context.SaveChangesAsync();

        customerList.Name = "Updated Name";
        customerList.Description = "Updated Description";

        // Act
        var result = await _repository.UpdateAsync(customerList);        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");

        var dbEntity = await _context.CustomerLists.FindAsync(customerList.Id);
        dbEntity!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldRemoveCustomerList()
    {
        // Arrange
        var customerList = new CustomerList
        {
            Name = "Test List",
            Description = "Test Description"
        };
        _context.CustomerLists.Add(customerList);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(customerList.Id);

        // Assert
        result.Should().BeTrue();

        var dbEntity = await _context.CustomerLists.FindAsync(customerList.Id);
        dbEntity.Should().BeNull();
    }    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnMatchingCustomerLists()
    {
        // Arrange
        var lists = new List<CustomerList>
        {
            new CustomerList { Name = "Test List 1", Description = "Description 1" },
            new CustomerList { Name = "Test List 2", Description = "Description 2" },
            new CustomerList { Name = "Other List", Description = "Description 3" }
        };
        _context.CustomerLists.AddRange(lists);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Test");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(l => l.Name.Contains("Test"));
    }

    [Fact]
    public async Task GetBySystemRefAsync_ShouldReturnMatchingCustomerList()
    {
        // Arrange
        var customerList = new CustomerList
        {
            Name = "Test List",
            Description = "Test Description",
            SystemRef = "SYS001"
        };
        _context.CustomerLists.Add(customerList);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySystemRefAsync("SYS001");

        // Assert
        result.Should().NotBeNull();
        result!.SystemRef.Should().Be("SYS001");
        result.Name.Should().Be("Test List");
    }

    [Fact]
    public async Task GetBySystemRefAsync_WithNonExistingRef_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetBySystemRefAsync("NONEXISTENT");

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
