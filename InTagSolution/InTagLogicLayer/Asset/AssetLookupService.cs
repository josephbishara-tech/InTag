using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.Asset;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Asset;

namespace InTagLogicLayer.Asset
{
    public class AssetLookupService : IAssetLookupService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AssetLookupService> _logger;

        public AssetLookupService(IUnitOfWork uow, ILogger<AssetLookupService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        // ═══════════════════════════════════════
        //  ASSET TYPES
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<AssetTypeListVm>> GetAssetTypesAsync()
        {
            var items = await _uow.AssetTypes.Query()
                .OrderBy(t => t.Name).ToListAsync();

            return items.Select(t => MapAssetType(t)).ToList();
        }

        public async Task<AssetTypeUpdateVm> GetAssetTypeByIdAsync(int id)
        {
            var item = await _uow.AssetTypes.GetByIdAsync(id);
            if (item == null) throw new KeyNotFoundException("Asset type not found.");

            return new AssetTypeUpdateVm
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                DefaultDepreciationMethod = item.DefaultDepreciationMethod,
                UsefulLifeMonths = item.UsefulLifeMonths,
                Category = item.Category,
                DefaultSalvageValuePercent = item.DefaultSalvageValuePercent
            };
        }

        public async Task<AssetTypeListVm> CreateAssetTypeAsync(AssetTypeCreateVm model)
        {
            if (await _uow.AssetTypes.ExistsAsync(t => t.Name == model.Name))
                throw new InvalidOperationException($"Asset type '{model.Name}' already exists.");

            var entity = new AssetType
            {
                Name = model.Name,
                Description = model.Description,
                DefaultDepreciationMethod = model.DefaultDepreciationMethod,
                UsefulLifeMonths = model.UsefulLifeMonths,
                Category = model.Category,
                DefaultSalvageValuePercent = model.DefaultSalvageValuePercent
            };

            await _uow.AssetTypes.AddAsync(entity);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Asset type created: {Name}", entity.Name);
            return MapAssetType(entity);
        }

        public async Task UpdateAssetTypeAsync(AssetTypeUpdateVm model)
        {
            var item = await _uow.AssetTypes.GetByIdAsync(model.Id);
            if (item == null) throw new KeyNotFoundException("Asset type not found.");

            if (await _uow.AssetTypes.ExistsAsync(t => t.Name == model.Name && t.Id != model.Id))
                throw new InvalidOperationException($"Asset type '{model.Name}' already exists.");

            item.Name = model.Name;
            item.Description = model.Description;
            item.DefaultDepreciationMethod = model.DefaultDepreciationMethod;
            item.UsefulLifeMonths = model.UsefulLifeMonths;
            item.Category = model.Category;
            item.DefaultSalvageValuePercent = model.DefaultSalvageValuePercent;

            _uow.AssetTypes.Update(item);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Asset type updated: {Name}", item.Name);
        }

        // ═══════════════════════════════════════
        //  LOCATIONS
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<LocationListVm>> GetLocationsAsync()
        {
            var items = await _uow.Locations.Query()
                .Include(l => l.ParentLocation)
                .OrderBy(l => l.Name).ToListAsync();

            return items.Select(l => MapLocation(l)).ToList();
        }

        public async Task<LocationUpdateVm> GetLocationByIdAsync(int id)
        {
            var item = await _uow.Locations.GetByIdAsync(id);
            if (item == null) throw new KeyNotFoundException("Location not found.");

            return new LocationUpdateVm
            {
                Id = item.Id,
                Name = item.Name,
                Code = item.Code,
                Address = item.Address,
                Building = item.Building,
                Floor = item.Floor,
                Room = item.Room,
                ParentLocationId = item.ParentLocationId
            };
        }

        public async Task<LocationListVm> CreateLocationAsync(LocationCreateVm model)
        {
            if (!string.IsNullOrEmpty(model.Code) && await _uow.Locations.ExistsAsync(l => l.Code == model.Code))
                throw new InvalidOperationException($"Location code '{model.Code}' already exists.");

            if (model.ParentLocationId.HasValue)
            {
                var parent = await _uow.Locations.GetByIdAsync(model.ParentLocationId.Value);
                if (parent == null) throw new KeyNotFoundException("Parent location not found.");
            }

            var entity = new Location
            {
                Name = model.Name,
                Code = model.Code,
                Address = model.Address,
                Building = model.Building,
                Floor = model.Floor,
                Room = model.Room,
                ParentLocationId = model.ParentLocationId
            };

            await _uow.Locations.AddAsync(entity);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Location created: {Name} ({Code})", entity.Name, entity.Code);
            return MapLocation(entity);
        }

        public async Task UpdateLocationAsync(LocationUpdateVm model)
        {
            var item = await _uow.Locations.GetByIdAsync(model.Id);
            if (item == null) throw new KeyNotFoundException("Location not found.");

            if (!string.IsNullOrEmpty(model.Code) && await _uow.Locations.ExistsAsync(l => l.Code == model.Code && l.Id != model.Id))
                throw new InvalidOperationException($"Location code '{model.Code}' already exists.");

            if (model.ParentLocationId.HasValue && model.ParentLocationId.Value == model.Id)
                throw new InvalidOperationException("A location cannot be its own parent.");

            item.Name = model.Name;
            item.Code = model.Code;
            item.Address = model.Address;
            item.Building = model.Building;
            item.Floor = model.Floor;
            item.Room = model.Room;
            item.ParentLocationId = model.ParentLocationId;

            _uow.Locations.Update(item);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Location updated: {Name} ({Code})", item.Name, item.Code);
        }

        // ═══════════════════════════════════════
        //  DEPARTMENTS
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<DepartmentListVm>> GetDepartmentsAsync()
        {
            var items = await _uow.Departments.Query()
                .Include(d => d.ParentDepartment)
                .OrderBy(d => d.Name).ToListAsync();

            return items.Select(d => MapDepartment(d)).ToList();
        }

        public async Task<DepartmentUpdateVm> GetDepartmentByIdAsync(int id)
        {
            var item = await _uow.Departments.GetByIdAsync(id);
            if (item == null) throw new KeyNotFoundException("Department not found.");

            return new DepartmentUpdateVm
            {
                Id = item.Id,
                Name = item.Name,
                Code = item.Code,
                Description = item.Description,
                ParentDepartmentId = item.ParentDepartmentId
            };
        }

        public async Task<DepartmentListVm> CreateDepartmentAsync(DepartmentCreateVm model)
        {
            if (!string.IsNullOrEmpty(model.Code) && await _uow.Departments.ExistsAsync(d => d.Code == model.Code))
                throw new InvalidOperationException($"Department code '{model.Code}' already exists.");

            if (model.ParentDepartmentId.HasValue)
            {
                var parent = await _uow.Departments.GetByIdAsync(model.ParentDepartmentId.Value);
                if (parent == null) throw new KeyNotFoundException("Parent department not found.");
            }

            var entity = new Department
            {
                Name = model.Name,
                Code = model.Code,
                Description = model.Description,
                ParentDepartmentId = model.ParentDepartmentId
            };

            await _uow.Departments.AddAsync(entity);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Department created: {Name} ({Code})", entity.Name, entity.Code);
            return MapDepartment(entity);
        }

        public async Task UpdateDepartmentAsync(DepartmentUpdateVm model)
        {
            var item = await _uow.Departments.GetByIdAsync(model.Id);
            if (item == null) throw new KeyNotFoundException("Department not found.");

            if (!string.IsNullOrEmpty(model.Code) && await _uow.Departments.ExistsAsync(d => d.Code == model.Code && d.Id != model.Id))
                throw new InvalidOperationException($"Department code '{model.Code}' already exists.");

            if (model.ParentDepartmentId.HasValue && model.ParentDepartmentId.Value == model.Id)
                throw new InvalidOperationException("A department cannot be its own parent.");

            item.Name = model.Name;
            item.Code = model.Code;
            item.Description = model.Description;
            item.ParentDepartmentId = model.ParentDepartmentId;

            _uow.Departments.Update(item);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Department updated: {Name} ({Code})", item.Name, item.Code);
        }

        // ═══════════════════════════════════════
        //  VENDORS
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<VendorListVm>> GetVendorsAsync()
        {
            var items = await _uow.Vendors.Query()
                .OrderBy(v => v.Name).ToListAsync();

            return items.Select(v => MapVendor(v)).ToList();
        }

        public async Task<VendorUpdateVm> GetVendorByIdAsync(int id)
        {
            var item = await _uow.Vendors.GetByIdAsync(id);
            if (item == null) throw new KeyNotFoundException("Vendor not found.");

            return new VendorUpdateVm
            {
                Id = item.Id,
                Name = item.Name,
                Code = item.Code,
                ContactPerson = item.ContactPerson,
                Email = item.Email,
                Phone = item.Phone,
                Address = item.Address,
                Website = item.Website,
                Notes = item.Notes
            };
        }

        public async Task<VendorListVm> CreateVendorAsync(VendorCreateVm model)
        {
            if (!string.IsNullOrEmpty(model.Code) && await _uow.Vendors.ExistsAsync(v => v.Code == model.Code))
                throw new InvalidOperationException($"Vendor code '{model.Code}' already exists.");

            var entity = new Vendor
            {
                Name = model.Name,
                Code = model.Code,
                ContactPerson = model.ContactPerson,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                Website = model.Website,
                Notes = model.Notes
            };

            await _uow.Vendors.AddAsync(entity);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Vendor created: {Name} ({Code})", entity.Name, entity.Code);
            return MapVendor(entity);
        }

        public async Task UpdateVendorAsync(VendorUpdateVm model)
        {
            var item = await _uow.Vendors.GetByIdAsync(model.Id);
            if (item == null) throw new KeyNotFoundException("Vendor not found.");

            if (!string.IsNullOrEmpty(model.Code) && await _uow.Vendors.ExistsAsync(v => v.Code == model.Code && v.Id != model.Id))
                throw new InvalidOperationException($"Vendor code '{model.Code}' already exists.");

            item.Name = model.Name;
            item.Code = model.Code;
            item.ContactPerson = model.ContactPerson;
            item.Email = model.Email;
            item.Phone = model.Phone;
            item.Address = model.Address;
            item.Website = model.Website;
            item.Notes = model.Notes;

            _uow.Vendors.Update(item);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Vendor updated: {Name} ({Code})", item.Name, item.Code);
        }

        // ═══════════════════════════════════════
        //  MAPPING
        // ═══════════════════════════════════════

        private static AssetTypeListVm MapAssetType(AssetType t) => new()
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            DefaultDepreciationMethod = t.DefaultDepreciationMethod,
            UsefulLifeMonths = t.UsefulLifeMonths,
            Category = t.Category,
            DefaultSalvageValuePercent = t.DefaultSalvageValuePercent
        };

        private static LocationListVm MapLocation(Location l) => new()
        {
            Id = l.Id,
            Name = l.Name,
            Code = l.Code,
            Address = l.Address,
            Building = l.Building,
            Floor = l.Floor,
            Room = l.Room,
            ParentLocationName = l.ParentLocation?.Name
        };

        private static DepartmentListVm MapDepartment(Department d) => new()
        {
            Id = d.Id,
            Name = d.Name,
            Code = d.Code,
            Description = d.Description,
            ParentDepartmentName = d.ParentDepartment?.Name
        };

        private static VendorListVm MapVendor(Vendor v) => new()
        {
            Id = v.Id,
            Name = v.Name,
            Code = v.Code,
            ContactPerson = v.ContactPerson,
            Email = v.Email,
            Phone = v.Phone,
            Website = v.Website
        };
    }
}
