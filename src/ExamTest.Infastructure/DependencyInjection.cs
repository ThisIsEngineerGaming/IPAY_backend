using ExamTest.Domain.Entities.Auth;
using ExamTest.Domain.Entities.Media;
using ExamTest.Domain.Entities.Shop;
using ExamTest.Domain.Interfaces.ForRepos;
using ExamTest.Infastructure.Firebase;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExamTest.Infastructure
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

            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<FirebaseOptions>>().Value;
                var credential = GoogleCredential.FromFile(options.ServiceAccountKeyPath);
                return StorageClient.Create(credential);
            });

            services.AddSingleton<FirebaseStorageService>();

            // One Firestore collection per entity type - add a line here for any new entity.
            services.AddScoped<IRepository<Series>>(sp =>
                new FirestoreRepository<Series>(sp.GetRequiredService<FirestoreDb>(), "series"));
            services.AddScoped<IRepository<Episode>>(sp =>
                new FirestoreRepository<Episode>(sp.GetRequiredService<FirestoreDb>(), "episodes"));
            services.AddScoped<IRepository<Genre>>(sp =>
                new FirestoreRepository<Genre>(sp.GetRequiredService<FirestoreDb>(), "genres"));
            services.AddScoped<IRepository<Film>>(sp =>
                new FirestoreRepository<Film>(sp.GetRequiredService<FirestoreDb>(), "films"));
            services.AddScoped<IRepository<Category>>(sp =>
                new FirestoreRepository<Category>(sp.GetRequiredService<FirestoreDb>(), "categories"));
            services.AddScoped<IRepository<Product>>(sp =>
                new FirestoreRepository<Product>(sp.GetRequiredService<FirestoreDb>(), "products"));
            services.AddScoped<IRepository<Seller>>(sp =>
               new FirestoreRepository<Seller>(sp.GetRequiredService<FirestoreDb>(), "sellers"));

            return services;
        }
    }
}
