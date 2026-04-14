namespace CodeExamplesPract1;
    
public interface ICurrencySubscriber
{ 
 void Update(string currency, decimal rate);
}

public class MobileAppClient : ICurrencySubscriber
{
    public void Update(string currency, decimal rate)
    {
        Console.WriteLine($"[Mobile] New currency {currency}: {rate}");
    }
}

public class WebDashboard : ICurrencySubscriber
{
    public void Update(string currency, decimal rate)
    {
        Console.WriteLine($"[Web] Updated chart for {currency}: {rate}");
    }
}

public class AnalyticsService : ICurrencySubscriber
{
    public void Update(string currency, decimal rate)
    {
        Console.WriteLine($"[Analytics] Currency display {currency}: {rate} added to DB");
    }
}

public class CurrencyRateService
{
    private List<ICurrencySubscriber> _subscribers = new();

    public void Subscribe(ICurrencySubscriber subscriber)
    {
        _subscribers.Add(subscriber);
    }

    public void Unsubscribe(ICurrencySubscriber subscriber)
    {
        _subscribers.Remove(subscriber);
    }

    public void UpdateRate(string currency, decimal newRate)
    {
        Console.WriteLine($"\n[Service] Currency {currency} updated to {newRate}");
        NotifySubscribers(currency, newRate);
    }

    private void NotifySubscribers(string currency, decimal rate)
    {
        foreach (var subscriber in _subscribers)
        {
            subscriber.Update(currency, rate);
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var rateService = new CurrencyRateService();

        var mobile = new MobileAppClient();
        var web = new WebDashboard();
        var analytics = new AnalyticsService();
        
        rateService.Subscribe(mobile);
        rateService.Subscribe(web);
        rateService.Subscribe(analytics);
        
        rateService.UpdateRate("USD", 41.25m);
    }
}
