using CoreAPI2024.Models;
using CoreAPI2024.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI2024.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpotsController : ControllerBase
    {
        private readonly MyDBContext _context;

        public SpotsController(MyDBContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<SpotsPagingDTO>> GetSpots([FromBody] SearchDTO searchDTO)
        {
            //categoryId為0就回傳所有資料，不為0就根據分類讀出資料
            var spots =  searchDTO.categoryId == 0 ? _context.SpotImagesSpots :
                _context.SpotImagesSpots.Where(s => s.CategoryId == searchDTO.categoryId);
            
            //關鍵字搜尋
            if (!string.IsNullOrEmpty(searchDTO.keyword))
            {
                spots = spots.Where(s => s.SpotTitle.Contains(searchDTO.keyword) ||
                       s.SpotDescription.Contains(searchDTO.keyword));
            }
            //排序
            switch (searchDTO.sortBy)
            {
                case "spotTitle":
                    spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.SpotTitle) : spots.OrderByDescending(s => s.SpotTitle);
                    break;
                case "categoryId":
                    spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.CategoryId) : spots.OrderByDescending(s => s.CategoryId);
                    break;
                default:
                    spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.SpotId) : spots.OrderByDescending(s => s.SpotId);
                    break;
            }


            //分頁
            int TotalCount = spots.Count(); //總共有多少筆資料
            int pageSize = searchDTO.pageSize ?? 9; //每頁多少筆資料
            int page = searchDTO.page ?? 1;   //第幾頁

            int TotalPages = 1;
            if (pageSize > 0)
            {
                TotalPages = (int)Math.Ceiling((decimal)TotalCount / pageSize);  //計算總共有幾頁
                                                                                 //page = 1, Skip(0), take(9)
                                                                                 //page = 2, Skip(1*9), take(9)
                                                                                 //page = 3 , Skie(2*9), take(9)
                spots = spots.Skip((int)((page - 1) * pageSize)).Take(pageSize);
            }


            

            SpotsPagingDTO spotsPaging = new SpotsPagingDTO();
            spotsPaging.TotalPages = TotalPages;           
            spotsPaging.SpotsResult = await spots.ToListAsync();

            return spotsPaging;
        }
    }
}
