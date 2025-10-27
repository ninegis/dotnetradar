using Microsoft.EntityFrameworkCore;
using RadarSystem.Data.Context;
using RadarSystem.Data.Models;

namespace RadarSystem.Data.Repositories
{
    public interface ILayerRepository
    {
        Task<List<LayerEntity>> GetByOrgIdAsync(string orgId);
        Task<LayerEntity?> GetByOidAsync(string oid);
        Task<string> AddAsync(LayerEntity entity);
        Task UpdateAsync(LayerEntity entity);
        Task DeleteAsync(string oid);
        Task SetEnableAsync(string oid, bool enable);
        Task SetShowAsync(string oid, bool show);
    }

    public class LayerRepository : ILayerRepository
    {
        private readonly RadarDbContext _context;

        public LayerRepository(RadarDbContext context)
        {
            _context = context;
        }

        public async Task<List<LayerEntity>> GetByOrgIdAsync(string orgId)
        {
            return await _context.Layers
                .Where(x => x.OrgId == orgId && !x.IsDeleted)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.CreateTime)
                .ToListAsync();
        }

        public async Task<LayerEntity?> GetByOidAsync(string oid)
        {
            return await _context.Layers
                .FirstOrDefaultAsync(x => x.Oid == oid && !x.IsDeleted);
        }

        public async Task<string> AddAsync(LayerEntity entity)
        {
            entity.Id = Guid.NewGuid().ToString();
            entity.CreateTime = DateTime.Now;
            entity.IsDeleted = false;
            
            await _context.Layers.AddAsync(entity);
            await _context.SaveChangesAsync();
            
            return entity.Id;
        }

        public async Task UpdateAsync(LayerEntity entity)
        {
            entity.UpdateTime = DateTime.Now;
            _context.Layers.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string oid)
        {
            var entity = await GetByOidAsync(oid);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdateTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetEnableAsync(string oid, bool enable)
        {
            var entity = await GetByOidAsync(oid);
            if (entity != null)
            {
                entity.Enable = enable;
                entity.UpdateTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetShowAsync(string oid, bool show)
        {
            var entity = await GetByOidAsync(oid);
            if (entity != null)
            {
                entity.Show = show;
                entity.UpdateTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}

