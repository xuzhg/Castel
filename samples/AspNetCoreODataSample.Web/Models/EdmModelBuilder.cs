// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using System.Collections.Generic;

namespace AspNetCoreODataSample.Web.Models
{
    public static class EdmModelBuilder
    {
        private static IEdmModel _edmModel;

        public static IEdmModel GetEdmModel()
        {
            if (_edmModel == null)
            {
                var builder = new ODataConventionModelBuilder();
                builder.EntitySet<Movie>("Movies");
                _edmModel = builder.GetEdmModel();
            }

            return _edmModel;
        }

        public static IEdmModel GetCompositeModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Person>("People");
            var type = builder.EntitySet<Person>("Person").EntityType;
            type.HasKey(x => new { x.FirstName, x.LastName });
            return builder.GetEdmModel();
        }

        public static IEdmModel GetCustomerModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            return builder.GetEdmModel();
        }
    }

    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public System.Int64 Date { get; set; }

        public string OrdStat { get; set; }

        public IDictionary<string, object> Dynamics { get; set; }
    }
}
