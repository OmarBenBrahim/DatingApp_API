using API.DTOs;
using API.Entity;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public PhotoRepository(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<Photo> GetPhotoById(int id)
        {
            return await context.Photos
                .Include(u => u.AppUser)
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.Id == id);


        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
        {
            return await context.Photos
               .Include(x => x.AppUser)
               .IgnoreQueryFilters()
               .Where(x => x.IsApproved == false)
               .ProjectTo<PhotoForApprovalDto>(mapper.ConfigurationProvider)
               .ToListAsync(); 
         
        }

        public void RemovePhoto(Photo photo)
        {
            context.Photos.Remove(photo);
        }
    }
}
