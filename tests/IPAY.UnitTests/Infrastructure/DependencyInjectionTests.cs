using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Interfaces.Payments;
using IPAY.Application.Services.Payments;
using IPAY.Domain.Entities.Media;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Interfaces.ForRepos;
using IPAY.Domain.Interfaces.ForRepos.Auth;
using IPAY.Domain.Interfaces.ForRepos.Media;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using IPAY.Infrastructure;
using IPAY.Infrastructure.Caching;
using IPAY.Infrastructure.Cloudinary;
using IPAY.Infrastructure.Email;
using IPAY.Infrastructure.Identity;
using IPAY.Infrastructure.Persistence;
using IPAY.Infrastructure.Persistence.Documents;
using IPAY.Infrastructure.Payments;
using IPAY.Infrastructure.Repositories.Auth;
using IPAY.Infrastructure.Repositories.Media;
using IPAY.Infrastructure.Repositories.Shop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure;

[TestFixture]
public class DependencyInjectionTests
{
    private static ServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        services.AddFirebaseInfrastructure(configuration);
        return services;
    }

    private static ServiceDescriptor? Find(ServiceCollection services, Type serviceType) =>
        services.LastOrDefault(descriptor => descriptor.ServiceType == serviceType);

    [Test]
    public void AddFirebaseInfrastructure_RegistersFirebaseOptionsAndFirestoreClient()
    {
        var services = CreateServices();

        Assert.Multiple(() =>
        {
            Assert.That(Find(services, typeof(Microsoft.Extensions.Options.IConfigureOptions<FirebaseOptions>)), Is.Not.Null);
            Assert.That(Find(services, typeof(Google.Cloud.Firestore.FirestoreDb)), Is.Not.Null);
        });
    }

    [Test]
    public void AddFirebaseInfrastructure_RegistersAuthAndInfrastructureServices()
    {
        var services = CreateServices();

        Assert.Multiple(() =>
        {
            Assert.That(Find(services, typeof(IFirebaseAuthService))?.ImplementationType, Is.EqualTo(typeof(FirebaseAuthService)));
            Assert.That(Find(services, typeof(IEmailSender))?.ImplementationType, Is.EqualTo(typeof(SmtpEmailSender)));
            Assert.That(Find(services, typeof(ITwoFactorStore))?.ImplementationType, Is.EqualTo(typeof(MemoryTwoFactorStore)));
            Assert.That(Find(services, typeof(CloudinaryImageService))?.ImplementationType, Is.EqualTo(typeof(CloudinaryImageService)));
        });
    }

    [Test]
    public void AddFirebaseInfrastructure_RegistersMediaRepositoriesAndGenericRepositories()
    {
        var services = CreateServices();

        Assert.Multiple(() =>
        {
            Assert.That(Find(services, typeof(ISeriesRepo)), Is.Not.Null);
            Assert.That(Find(services, typeof(IEpisodeRepo)), Is.Not.Null);
            Assert.That(Find(services, typeof(IGenreRepo)), Is.Not.Null);
            Assert.That(Find(services, typeof(IRepository<Series>)), Is.Not.Null);
            Assert.That(Find(services, typeof(IRepository<Episode>)), Is.Not.Null);
            Assert.That(Find(services, typeof(IRepository<Genre>)), Is.Not.Null);
            Assert.That(Find(services, typeof(IRepository<Category>)), Is.Not.Null);
            Assert.That(Find(services, typeof(IRepository<Manufacturer>)), Is.Not.Null);
            Assert.That(Find(services, typeof(IRepository<Guest>)), Is.Not.Null);
            Assert.That(Find(services, typeof(IRepository<Customer>)), Is.Not.Null);
            Assert.That(Find(services, typeof(IRepository<Cart>)), Is.Not.Null);
            Assert.That(Find(services, typeof(IRepository<CartItem>)), Is.Not.Null);
            Assert.That(Find(services, typeof(IRepository<SellerRequest>)), Is.Not.Null);
        });
    }

    [Test]
    public void AddFirebaseInfrastructure_RegistersShopAndPaymentRepositories()
    {
        var services = CreateServices();

        Assert.Multiple(() =>
        {
            Assert.That(Find(services, typeof(IOrderRepository))?.ImplementationType, Is.EqualTo(typeof(OrderRepository)));
            Assert.That(Find(services, typeof(IOrderItemRepository))?.ImplementationType, Is.EqualTo(typeof(OrderItemRepository)));
            Assert.That(Find(services, typeof(IAddressRepository))?.ImplementationType, Is.EqualTo(typeof(AddressRepository)));
            Assert.That(Find(services, typeof(IRefreshTokenRepository))?.ImplementationType, Is.EqualTo(typeof(RefreshTokenRepository)));
            Assert.That(Find(services, typeof(IOrderStatusHistoryRepository))?.ImplementationType, Is.EqualTo(typeof(OrderStatusHistoryRepository)));
            Assert.That(Find(services, typeof(IPaymentProvider))?.ImplementationType, Is.EqualTo(typeof(MockPaymentProvider)));
            Assert.That(Find(services, typeof(IPaymentService))?.ImplementationType, Is.EqualTo(typeof(PaymentService)));
            Assert.That(Find(services, typeof(IReviewRepository))?.ImplementationType, Is.EqualTo(typeof(ReviewRepository)));
            Assert.That(Find(services, typeof(IProductRatingRepo)), Is.Not.Null);
            Assert.That(Find(services, typeof(IProductRepo)), Is.Not.Null);
        });
    }

    [Test]
    public void AddFirebaseInfrastructure_ReturnsSameServiceCollectionForChaining()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();

        var returned = services.AddFirebaseInfrastructure(configuration);

        Assert.That(returned, Is.SameAs(services));
    }
}
