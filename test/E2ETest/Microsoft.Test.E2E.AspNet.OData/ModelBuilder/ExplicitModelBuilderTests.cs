﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.ProductFamilies;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    public class ExplicitModelBuilderTests : WebHostTestBase
    {
        public ExplicitModelBuilderTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetExplicitEdmModel());
        }

        private static IEdmModel GetExplicitEdmModel()
        {
            var modelBuilder = new ODataModelBuilder();

            var enumContry = modelBuilder.EnumType<CountryOrRegion>();
            enumContry.Member(CountryOrRegion.Canada);
            enumContry.Member(CountryOrRegion.China);
            enumContry.Member(CountryOrRegion.India);
            enumContry.Member(CountryOrRegion.Japen);
            enumContry.Member(CountryOrRegion.USA);

            var products = modelBuilder.EntitySet<Product>("Products");
            products.HasEditLink(entityContext =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("ID", out id);
                    return new Uri(entityContext.GetUrlHelper().Link(ODataTestConstants.DefaultRouteName,
                        new
                        {
                            odataPath = ResourceContextHelper.CreateODataLink(entityContext,
                                new EntitySetSegment(entityContext.NavigationSource as IEdmEntitySet),
                                new KeySegment(new[] { new KeyValuePair<string, object>("ID", id) }, entityContext.StructuredType as IEdmEntityType, null))
                        }));
                }, true);

            var suppliers = modelBuilder.EntitySet<Supplier>("Suppliers");
            suppliers.HasEditLink(entityContext =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("ID", out id);
                    return new Uri(entityContext.GetUrlHelper().Link(ODataTestConstants.DefaultRouteName,
                        new
                        {
                            odataPath = ResourceContextHelper.CreateODataLink(entityContext,
                                new EntitySetSegment(entityContext.NavigationSource as IEdmEntitySet),
                                new KeySegment(new[] { new KeyValuePair<string, object>("ID", id) }, entityContext.StructuredType as IEdmEntityType, null))
                        }));
                }, true);

            var families = modelBuilder.EntitySet<ProductFamily>("ProductFamilies");
            families.HasEditLink(entityContext =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("ID", out id);
                    return new Uri(entityContext.GetUrlHelper().Link(ODataTestConstants.DefaultRouteName, 
                        new
                        {
                            odataPath = ResourceContextHelper.CreateODataLink(entityContext,
                                new EntitySetSegment(entityContext.NavigationSource as IEdmEntitySet),
                                new KeySegment(new[] { new KeyValuePair<string, object>("ID", id) }, entityContext.StructuredType as IEdmEntityType, null))
                        }));
                }, true);

            var product = products.EntityType;

            product.HasKey(p => p.ID);
            product.Property(p => p.Name);
            product.Property(p => p.ReleaseDate);
            product.Property(p => p.SupportedUntil);

            var address = modelBuilder.ComplexType<Address>();
            address.Property(a => a.City);
            address.Property(a => a.CountryOrRegion);
            address.Property(a => a.State);
            address.Property(a => a.Street);
            address.Property(a => a.ZipCode);

            var supplier = suppliers.EntityType;
            supplier.HasKey(s => s.ID);
            supplier.Property(s => s.Name);
            supplier.CollectionProperty(s => s.Addresses);
            supplier.CollectionProperty(s => s.Tags);
            supplier.EnumProperty(s => s.CountryOrRegion);

            var productFamily = families.EntityType;
            productFamily.HasKey(pf => pf.ID);
            productFamily.Property(pf => pf.Name);
            productFamily.Property(pf => pf.Description);

            // Create relationships and bindings in one go
            products.HasRequiredBinding(p => p.Family, families);
            families.HasManyBinding(pf => pf.Products, products);
            families.HasOptionalBinding(pf => pf.Supplier, suppliers);
            suppliers.HasManyBinding(s => s.ProductFamilies, families);

            // Create navigation Link builders
            products.HasNavigationPropertiesLink(
                product.NavigationProperties,
                (entityContext, navigationProperty) =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("ID", out id);
                    return new Uri(entityContext.GetUrlHelper().Link(ODataTestConstants.DefaultRouteName,
                new
                {
                    odataPath = ResourceContextHelper.CreateODataLink(entityContext,
                        new EntitySetSegment(entityContext.NavigationSource as IEdmEntitySet),
                        new KeySegment(new[] { new KeyValuePair<string, object>("ID", id) }, entityContext.StructuredType as IEdmEntityType, null),
                        new NavigationPropertySegment(navigationProperty, null))
                }));
                }, true);

            families.HasNavigationPropertiesLink(
                productFamily.NavigationProperties,
                (entityContext, navigationProperty) =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("ID", out id);
                    return new Uri(entityContext.GetUrlHelper().Link(ODataTestConstants.DefaultRouteName,
                new
                {
                    odataPath = ResourceContextHelper.CreateODataLink(entityContext,
                        new EntitySetSegment(entityContext.NavigationSource as IEdmEntitySet),
                        new KeySegment(new[] { new KeyValuePair<string, object>("ID", id) }, entityContext.StructuredType as IEdmEntityType, null),
                        new NavigationPropertySegment(navigationProperty, null))
                }));
                }, true);

            suppliers.HasNavigationPropertiesLink(
                supplier.NavigationProperties,
                (entityContext, navigationProperty) =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("ID", out id);
                    return new Uri(entityContext.GetUrlHelper().Link(
                ODataTestConstants.DefaultRouteName,
                new
                {
                    odataPath = ResourceContextHelper.CreateODataLink(entityContext,
                        new EntitySetSegment(entityContext.NavigationSource as IEdmEntitySet),
                        new KeySegment(new[] { new KeyValuePair<string, object>("ID", id) }, entityContext.StructuredType as IEdmEntityType, null),
                        new NavigationPropertySegment(navigationProperty, null))
                }));
                }, true);

            return modelBuilder.GetEdmModel();
        }

        [Fact]

        public async Task VerifyMetaDataIsGeneratedCorrectly()
        {
            var response = await Client.GetAsync(this.BaseAddress + "/$metadata");
            var stream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
            var reader = new ODataMessageReader(message);
            var edmModel = reader.ReadMetadataDocument();

            Assert.Equal(4, edmModel.GetEdmVersion().Major);

            var container = edmModel.EntityContainer;
            Assert.Equal("Container", container.Name);
            Assert.Equal(3, container.Elements.Count());

            var address = edmModel.SchemaElements.OfType<IEdmComplexType>().First();
            Assert.Equal("Address", address.Name);
            Assert.Equal(5, address.Properties().Count());

            var product = edmModel.SchemaElements.First(e => e.Name == "Product") as IEdmEntityType;
            Assert.Single(product.Key());
            Assert.Equal("ID", product.Key().First().Name);
            Assert.Equal(5, product.Properties().Count());

            var supplier = edmModel.SchemaElements.First(e => e.Name == "Supplier") as IEdmEntityType;
            Assert.Single(supplier.Key());
            Assert.Equal("ID", supplier.Key().First().Name);
            Assert.Equal(6, supplier.Properties().Count());

            var addressesProperty = supplier.Properties().First(p => p.Name == "Addresses").Type.AsCollection();
            Assert.Equal(typeof(Address).FullName, addressesProperty.CollectionDefinition().ElementType.FullName());
            Assert.False(addressesProperty.IsNullable);

            var tagsProperty = supplier.Properties().First(p => p.Name == "Tags").Type.AsCollection();
            Assert.Equal("Edm.String", tagsProperty.CollectionDefinition().ElementType.FullName());
            Assert.True(tagsProperty.IsNullable);
        }
    }
}