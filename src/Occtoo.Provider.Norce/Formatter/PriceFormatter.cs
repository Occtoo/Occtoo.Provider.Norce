using Occtoo.Provider.Norce.Model;
using Occtoo.Onboarding.Sdk.Models;
using System.Collections.Generic;

namespace Occtoo.Provider.Norce.Formatter
{
    public interface IPriceFormatter
    {
        List<PriceModel> FormatToPriceModel(List<NorceProductModel> norceProducts);
        List<PriceModel> FormatToPriceModel(NorceProductModel norceProduct);
        List<PriceModel> FormatToPriceModel(List<NorcePriceStockModel> norceProducts);
        List<DynamicEntity> FormatToEntites(List<PriceModel> prices);
    }
    public class PriceFormatter : IPriceFormatter
    {
        public List<DynamicEntity> FormatToEntites(List<PriceModel> prices)
        {
            var response = new List<DynamicEntity>();

            foreach (var price in prices)
            {
                var key = $"{price.ProductPartNo}-{price.PriceListCode}-{price.Currency}";
                var entity = new DynamicEntity
                {
                    Key = key
                };

                var properties = typeof(PriceModel).GetProperties();
                foreach (var property in properties)
                {
                    entity.Properties.Add(new DynamicProperty
                    {
                        Id = property.Name,
                        Value = property.GetValue(price)?.ToString()
                    });
                }

                response.Add(entity);
            }

            return response;
        }

        public List<PriceModel> FormatToPriceModel(List<NorceProductModel> norceProducts)
        {
            var prices = new List<PriceModel>();

            foreach (var norceProduct in norceProducts)
            {
                foreach (var variant in norceProduct.Variants)
                {
                    prices.AddRange(PricesToPriceModel(variant.PartNo, variant.Prices));
                }
            }

            return prices;
        }

        public List<PriceModel> FormatToPriceModel(NorceProductModel norceProduct)
        {
            var prices = new List<PriceModel>();

            foreach (var variant in norceProduct.Variants)
            {
                prices.AddRange(PricesToPriceModel(variant.PartNo, variant.Prices));
            }

            return prices;
        }

        public List<PriceModel> FormatToPriceModel(List<NorcePriceStockModel> norceProducts)
        {
            var prices = new List<PriceModel>();

            foreach (var norceProduct in norceProducts)
            {
                prices.AddRange(PricesToPriceModel(norceProduct.PartNo, norceProduct.Prices));
            }

            return prices;
        }

        private IEnumerable<PriceModel> PricesToPriceModel(string partNo, Model.Price[] prices)
        {
            foreach (var price in prices)
            {
                yield return new PriceModel
                {
                    ProductPartNo = partNo,
                    SalesArea = price.SalesArea,
                    PriceListCode = price.PriceListCode,
                    Currency = price.Currency,
                    Value = price.Value.ToString(),
                    IsDiscountable = price.IsDiscountable.ToString(),
                    Original = price.Original.ToString(),
                    VatRate = price.VatRate.ToString(),
                    AvailableOnWarehouseCode = GetWarehouseCode(price.AvailableOnWarehouses),
                    AvailableOnWarehouseCodeLocation = GetWarehouseCodeLocation(price.AvailableOnWarehouses),
                    PurchaseCost = price.PurchaseCost.ToString(),
                    UnitCost = price.UnitCost.ToString(),
                    IsActive = price.IsActive.ToString(),
                    ValueIncVat = price.ValueIncVat.ToString()
                };
            }
        }

        private string GetWarehouseCode(Availableonwarehous[] availableOnWarehouses)
        {
            var code = string.Empty;
            foreach (var warehouse in availableOnWarehouses)
            {
                code += $"{warehouse.Code}|";
            }
            return code.Trim('|');
        }

        private string GetWarehouseCodeLocation(Availableonwarehous[] availableOnWarehouses)
        {
            var codeLocation = string.Empty;
            foreach (var warehouse in availableOnWarehouses)
            {
                codeLocation += $"{warehouse.LocationCode}|";
            }
            return codeLocation.Trim('|');
        }
    }
}
