using DynamicQuery.AspNetCore;
using DynamicQuery.AspNetCore.Test.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace DynamicQuery.Test.Unit
{
    public class DynamicQueryAspNetCoreTests
    {
        [Fact]
        public void BinderProvider_Returns_Binder_For_DynamicQuery()
        {
            var provider = new DynamicQueryBinderProvider();
            var context = new TestModelBinderProviderContext(typeof(DynamicQuery<User>));

            var binder = provider.GetBinder(context);

            Assert.NotNull(binder);
        }

        [Fact]
        public void BinderProvider_Returns_Null_For_Other_Types()
        {
            var provider = new DynamicQueryBinderProvider();
            var context = new TestModelBinderProviderContext(typeof(string));

            var binder = provider.GetBinder(context);

            Assert.Null(binder);
        }

        [Fact]
        public void BinderProvider_Throws_When_Context_Is_Null()
        {
            var provider = new DynamicQueryBinderProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null!));
        }

        [Fact]
        public async Task Binder_Throws_When_BindingContext_Is_Null()
        {
            var binder = new DynamicQueryBinder();
            await Assert.ThrowsAsync<ArgumentNullException>(() => binder.BindModelAsync(null!));
        }

        [Fact]
        public void AddDynamicQuery_Registers_BinderProvider()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddControllers().AddDynamicQuery();

            using var provider = services.BuildServiceProvider();
            var mvcOptions = provider.GetRequiredService<IOptions<MvcOptions>>().Value;

            Assert.Contains(mvcOptions.ModelBinderProviders, binderProvider => binderProvider is DynamicQueryBinderProvider);
        }

        private sealed class TestModelBinderProviderContext : ModelBinderProviderContext
        {
            private readonly IModelMetadataProvider _metadataProvider = new EmptyModelMetadataProvider();

            public TestModelBinderProviderContext(Type modelType)
            {
                Metadata = _metadataProvider.GetMetadataForType(modelType);
                BindingInfo = BindingInfo.GetBindingInfo(Array.Empty<object>());
            }

            public override ModelMetadata Metadata { get; }

            public override BindingInfo BindingInfo { get; }

            public override IModelMetadataProvider MetadataProvider => _metadataProvider;

            public override IModelBinder CreateBinder(ModelMetadata metadata) => null!;
        }
    }
}
