using IPAY.Domain.Entities.Users;
using IPAY.Domain.Entities.Media;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Interfaces.ForRepos;
using IPAY.Domain.Interfaces.ForRepos.Media;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using IPAY.Infrastructure.Repositories.Media;
using IPAY.Infrastructure.Cloudinary;
using IPAY.Infrastructure.Persistence;
using IPAY.Infrastructure.Persistence.Documents;
using IPAY.Infrastructure.Repositories.Shop;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IPAY.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFirebaseInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FirebaseOptions>(configuration.GetSection(FirebaseOptions.SectionName));

            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<FirebaseOptions>>().Value;
                var credential = GoogleCredential.FromFile(options.ServiceAccountKeyPath);
                return new FirestoreDbBuilder
                {
                    ProjectId = options.ProjectId,
                    GoogleCredential = credential
                }.Build();
            });

            // Image hosting - Cloudinary (replaced Firebase Storage).
            services.Configure<CloudinaryOptions>(configuration.GetSection(CloudinaryOptions.SectionName));
            services.AddSingleton<CloudinaryImageService>();

            // One Firestore collection per entity type - add a line here for any new entity
            // (plus a matching *Document class in Firebase/Documents).
            services.AddDocumentRepository<Series, SeriesDocument>(
                "series", SeriesDocument.FromEntity, document => document.ToEntity());
            services.AddDocumentRepository<Episode, EpisodeDocument>(
                "episodes", EpisodeDocument.FromEntity, document => document.ToEntity());
            services.AddDocumentRepository<Genre, GenreDocument>(
                "genres", GenreDocument.FromEntity, document => document.ToEntity());
            // Films use a dedicated repo (paging / filtering / sorting); it also serves IRepository<Film>.
            services.AddScoped<IFilmRepo>(sp =>
                new FilmRepo(sp.GetRequiredService<FirestoreDb>(), "films"));
            services.AddScoped<IRepository<Film>>(sp => sp.GetRequiredService<IFilmRepo>());
            services.AddDocumentRepository<Category, CategoryDocument>(
                "categories", CategoryDocument.FromEntity, document => document.ToEntity());

            services.AddDocumentRepository<Manufacturer, ManufacturerDocument>(
                "manufacturers", ManufacturerDocument.FromEntity, document => document.ToEntity());


            services.AddDocumentRepository<Customer, CustomerDocument>(
                "customers", CustomerDocument.FromEntity, document => document.ToEntity());

            services.AddDocumentRepository<Cart, CartDocument>(
               "carts", CartDocument.FromEntity, document => document.ToEntity());

            services.AddDocumentRepository<CartItem, CartItemDocument>(
                "cartItems", CartItemDocument.FromEntity, document => document.ToEntity());


            services.AddScoped<IProductRepo>(sp =>
              new ProductRepo(
                  sp.GetRequiredService<FirestoreDb>(),
                  "products"));

            return services;
        }

        private static IServiceCollection AddDocumentRepository<TEntity, TDocument>(
            this IServiceCollection services,
            string collectionName,
            Func<TEntity, TDocument> toDocument,
            Func<TDocument, TEntity> toEntity)
            where TEntity : class
            where TDocument : class, new()
        {
            return services.AddScoped<IRepository<TEntity>>(sp =>
                new FirestoreRepository<TEntity, TDocument>(
                    sp.GetRequiredService<FirestoreDb>(),
                    collectionName,
                    toDocument,
                    toEntity));
        }
    }
}
