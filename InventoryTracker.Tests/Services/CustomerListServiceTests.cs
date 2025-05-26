using FluentAssertions;
using InventoryTracker.Core.DTOs;
using InventoryTracker.Core.Entities;
using InventoryTracker.Core.Services.Interfaces;
using InventoryTracker.Data.Repositories.Interfaces;
using InventoryTracker.Data.Services;
using Moq;

namespace InventoryTracker.Tests.Services;

public class CustomerListServiceTests
{
    private readonly Mock<ICustomerListRepository> _mockRepository;
    private readonly CustomerListService _service;
    private readonly Guid _testListId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _testListId2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public CustomerListServiceTests()
    {
        _mockRepository = new Mock<ICustomerListRepository>();
        _service = new CustomerListService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCustomerLists()
    {
        // Arrange
        var customerLists = new List<CustomerList>
        {
            new CustomerList { Id = _testListId1, Name = "List 1", Description = "Description 1" },
            new CustomerList { Id = _testListId2, Name = "List 2", Description = "Description 2" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(customerLists);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("List 1");
        result.Last().Name.Should().Be("List 2");
    }    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnCustomerList()
    {
        // Arrange
        var customerList = new CustomerList 
        { 
            Id = _testListId1, 
            Name = "Test List", 
            Description = "Test Description"
        };
        _mockRepository.Setup(r => r.GetByIdAsync(_testListId1)).ReturnsAsync(customerList);

        // Act
        var result = await _service.GetByIdAsync(_testListId1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test List");
        result.Description.Should().Be("Test Description");
    }

    [Fact]    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(nonExistentId)).ReturnsAsync((CustomerList?)null);

        // Act
        var result = await _service.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateAndReturnCustomerList()
    {
        // Arrange
        var dto = new CreateCustomerListDto
        {
            Name = "New List",
            Description = "New Description",            SystemRef = "SYS001"
        };
        var createdEntity = new CustomerList
        {
            Id = _testListId1,
            Name = dto.Name,
            Description = dto.Description,
            SystemRef = dto.SystemRef
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<CustomerList>()))
                      .ReturnsAsync(createdEntity);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New List");
        result.Description.Should().Be("New Description");
        result.SystemRef.Should().Be("SYS001");
        
        _mockRepository.Verify(r => r.CreateAsync(It.Is<CustomerList>(c => 
            c.Name == dto.Name && 
            c.Description == dto.Description && 
            c.SystemRef == dto.SystemRef)), Times.Once);
    }    [Fact]
    public async Task UpdateAsync_WithValidDto_ShouldUpdateAndReturnCustomerList()
    {
        // Arrange
        var existingEntity = new CustomerList
        {
            Id = _testListId1,
            Name = "Old Name",
            Description = "Old Description"
        };

        var updateDto = new UpdateCustomerListDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            SystemRef = "SYS002"
        };
        var updatedEntity = new CustomerList
        {
            Id = _testListId1,
            Name = updateDto.Name,
            Description = updateDto.Description,
            SystemRef = updateDto.SystemRef
        };

        _mockRepository.Setup(r => r.GetByIdAsync(_testListId1)).ReturnsAsync(existingEntity);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<CustomerList>()))
                      .ReturnsAsync(updatedEntity);

        // Act
        var result = await _service.UpdateAsync(_testListId1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.SystemRef.Should().Be("SYS002");
          _mockRepository.Verify(r => r.UpdateAsync(It.Is<CustomerList>(c => 
            c.Id == _testListId1 && 
            c.Name == updateDto.Name &&            c.Description == updateDto.Description)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var updateDto = new UpdateCustomerListDto
        {
            Name = "Updated Name"
        };
        
        _mockRepository.Setup(r => r.GetByIdAsync(Guid.NewGuid())).ReturnsAsync((CustomerList?)null);

        // Act
        var nonExistentId = Guid.NewGuid();
        var result = await _service.UpdateAsync(nonExistentId, updateDto);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<CustomerList>()), Times.Never);
    }

    [Fact]    public async Task DeleteAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var existingEntity = new CustomerList
        {
            Id = _testListId1,
            Name = "Test List"
        };
        
        _mockRepository.Setup(r => r.ExistsAsync(_testListId1)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DeleteAsync(_testListId1)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(_testListId1);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(_testListId1), Times.Once);    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _mockRepository.Setup(r => r.ExistsAsync(nonExistentId)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(nonExistentId));
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]    public async Task GetByNameAsync_WithValidName_ShouldReturnCustomerLists()
    {
        // Arrange
        var customerLists = new List<CustomerList>
        {
            new CustomerList { Id = _testListId1, Name = "Test List 1" },
            new CustomerList { Id = _testListId2, Name = "Test List 2" }
        };
        
        _mockRepository.Setup(r => r.GetByNameAsync("Test")).ReturnsAsync(customerLists);

        // Act
        var result = await _service.GetByNameAsync("Test");

        // Assert
        result.Should().HaveCount(2);
        result.All(r => r.Name.Contains("Test")).Should().BeTrue();
    }
}
