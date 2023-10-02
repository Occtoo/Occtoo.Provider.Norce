using Occtoo.Provider.Norce.Model;
using Occtoo.Onboarding.Sdk.Models;
using System.Collections.Generic;

namespace Occtoo.Provider.Norce.Formatter
{
    public interface IStockFormatter
    {
        List<StockModel> FormatToStockModel(List<NorceProductModel> norceProducts);
        List<StockModel> FormatToStockModel(NorceProductModel norceProducts);
        List<StockModel> FormatToStockModel(List<NorcePriceStockModel> norceProducts);
        List<DynamicEntity> FormatToEntites(List<StockModel> stocks);

    }
    public class StockFormatter : IStockFormatter
    {
        public List<DynamicEntity> FormatToEntites(List<StockModel> stocks)
        {
            var response = new List<DynamicEntity>();

            foreach (var stock in stocks)
            {
                var key = $"{stock.ProductPartNo}-{stock.WarehouseCode}";
                var entity = new DynamicEntity
                {
                    Key = key
                };

                var properties = typeof(StockModel).GetProperties();
                foreach (var property in properties)
                {
                    entity.Properties.Add(new DynamicProperty
                    {
                        Id = property.Name,
                        Value = property.GetValue(stock)?.ToString()
                    });
                }

                response.Add(entity);
            }

            return response;
        }

        public List<StockModel> FormatToStockModel(List<NorceProductModel> norceProducts)
        {
            var stocks = new List<StockModel>();
            foreach (var norceProduct in norceProducts)
            {
                foreach (var variant in norceProduct.Variants)
                {
                    stocks.AddRange(OnhandsToStockModel(variant.OnHands, variant.PartNo));
                }
            }

            return stocks;
        }

        public List<StockModel> FormatToStockModel(NorceProductModel norceProduct)
        {
            var stocks = new List<StockModel>();
            foreach (var variant in norceProduct.Variants)
            {
                stocks.AddRange(OnhandsToStockModel(variant.OnHands, variant.PartNo));
            }

            return stocks;
        }

        public List<StockModel> FormatToStockModel(List<NorcePriceStockModel> norceProducts)
        {
            var stocks = new List<StockModel>();
            foreach (var norceProduct in norceProducts)
            {
                stocks.AddRange(OnhandsToStockModel(norceProduct.OnHands, norceProduct.PartNo));
            }

            return stocks;
        }

        private IEnumerable<StockModel> OnhandsToStockModel<T>(T[] onHands, string partNo) where T : IOnHand
        {
            foreach (var onHand in onHands)
            {
                yield return new StockModel
                {
                    ProductPartNo = partNo,
                    WarehouseCode = onHand.Warehouse.Code,
                    WarehouseLocationCode = onHand.Warehouse.LocationCode,
                    WarehouseType = onHand.WarehouseType,
                    Value = onHand.Value.ToString(),
                    LeadTimeDayCount = onHand.LeadTimeDayCount?.ToString(),
                    AvailableOnStores = GetAvailableOnStores(onHand),
                    AvailableOnPriceList = GetAvailableOnPriceList(onHand),
                    NextDelivery = onHand.NextDelivery?.ToString()
                };
            }
        }

        private string GetAvailableOnStores(IOnHand onHand)
        {
            string availableOnStores = string.Empty;

            foreach (var a in onHand.AvailableOnStores)
            {
                availableOnStores += $"{a}|";
            }

            return availableOnStores.TrimEnd('|');
        }

        private string GetAvailableOnPriceList(IOnHand onHand)
        {
            string availableOnPriceList = string.Empty;

            foreach (var p in onHand.AvailableOnPriceLists)
            {
                availableOnPriceList += $"{p}|";
            }

            return availableOnPriceList.TrimEnd('|');
        }
    }
}
