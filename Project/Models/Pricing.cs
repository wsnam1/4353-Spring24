namespace Project.Models;

public class Pricing
{
    public decimal CalculatePrice(int gallonsRequested, bool inState, bool hasHistory)
    {
        decimal currentPrice = 1.50m;
        decimal locationFactor = inState ? 0.02m : 0.04m;
        decimal rateHistoryFactor = hasHistory ? 0.01m : 0m;
        decimal gallonsFactor = gallonsRequested > 1000 ? 0.02m : 0.03m;
        decimal companyProfitFactor = 0.10m;

        decimal margin = currentPrice * (locationFactor - rateHistoryFactor + gallonsFactor + companyProfitFactor);
        decimal price = currentPrice + margin;

        return price;
    }
}