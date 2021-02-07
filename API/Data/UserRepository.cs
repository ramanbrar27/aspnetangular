using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using API.Helpers;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MemberDto> GetMemberByUsernameAsync(string username)
        {
            return await _context.Users
            .Where(x=>x.UserName==username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
        }

        // public async Task<IEnumerable<MemberDto>> GetMembersAsync()
        // {
        //     return await _context.Users
        //     .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
        //     .ToListAsync();
        // }
        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userparams)
        {
            // var query= _context.Users
            // .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            // .AsNoTracking();

            var query=_context.Users.AsQueryable();
            query=query.Where(u=>u.UserName!=userparams.CurrentUsername);
            query=query.Where(u=>u.Gender==userparams.Gender);
            
            var minDob=DateTime.Today.AddYears(-userparams.MaxAge-1);
            var maxDob=DateTime.Today.AddYears(-userparams.MinAge);
            query=query.Where(u=>u.DateOfBirth>=minDob &&u.DateOfBirth<=maxDob);

            query=userparams.Orderby switch 
            {
                "created"=>query.OrderByDescending(u=>u.Created),
                _=>query.OrderByDescending(u=>u.LastActive)
                
            };
            return await PagedList<MemberDto>.CreateAsync(query
             .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
             .AsNoTracking(),userparams.PageNumber,
            userparams.PageSize);
        }
        public async Task<AppUser> GetUserByIdAsync(int Id)
        {
            return await _context.Users.FindAsync(Id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
            .Include(p=>p.Photos)
            .SingleOrDefaultAsync(x=>x.UserName==username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
            .Include(p=>p.Photos)
            .ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync()>0;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State=EntityState.Modified;
        }
    }
}