using CloudLogic.RepoModels.Shop;
using CloudLogic.Repositories;
using CloudLogic.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShopModel = CloudLogic.RepoModels.Shop.Shop;

namespace CloudLogic.Services.Shop
{
    public class DailyDealsService
    {
        public const string Prefix = "deal_";

        private readonly DailyDealsRepo _repo;
        private readonly ShopRepo _shopRepo;

        public DailyDealsService(DailyDealsRepo repo, ShopRepo shopRepo)
        {
            _repo = repo;
            _shopRepo = shopRepo;
        }

        public async Task<List<DailyDeal>> Today(List<ShopModel> shop = null)
        {
            var day = DateTime.UtcNow.Date;
            var id = day.ToString("yyyy-MM-dd");
            var saved = await _repo.GetByDay(id);

            if (saved?.Deals?.Count > 0)
            {
                return saved.Deals;
            }

            var deals = Pick(shop ?? await _shopRepo.GetAll(), day, GameConstants.DailyDealCount);

            if (deals.Count > 0)
            {
                await _repo.Save(new DailyDeals { Id = id, Deals = deals });
            }

            return deals;
        }

        public static List<DailyDeal> Pick(List<ShopModel> shop, DateTime day, int count)
        {
            var random = new Random((int)(day.Date.Ticks / TimeSpan.TicksPerDay));

            return (shop ?? new())
                .Where(s => s.Discount > 0 && s.Discount < 100 && s.Cost > 0)
                .OrderBy(s => s.Id)
                .ToList()
                .OrderBy(_ => random.Next())
                .Take(count)
                .Select(s => new DailyDeal { ShopId = s.Id, Discount = s.Discount })
                .ToList();
        }

        public static long Price(long cost, int discount)
        {
            return cost * (100 - discount) / 100;
        }
    }
}
