// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using AspNetCoreODataSample.Web.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreODataSample.Web.Controllers
{
    public class PeopleController : ODataController
    {
        [EnableQuery]
        public IActionResult Get([FromODataUri]string keyFirstName, [FromODataUri]string keyLastName)
        {
            Person m = new Person
            {
                FirstName = keyFirstName,
                LastName = keyLastName,
                DynamicProperties = new Dictionary<string, object>
                {
                    { "abc", "abcValue" }
                },
                MyLevel = Level.High
            };

            return Ok(m);
        }

        [EnableQuery]
        public IActionResult Post([FromBody]Person person)
        {
            return Created(person);
        }
    }

    public class CustomersController : ODataController
    {
        //[EnableQuery]
        public IActionResult Get(ODataQueryOptions<Customer> queryOptions)
        {
            Customer m = new Customer
            {
                Id = 1,
                Name = "Ok"
            };
            IList<Customer> customers = new List<Customer> { m };
            var queryable = customers.AsQueryable();
            var query = queryOptions.ApplyTo(queryable, new ODataQuerySettings()) as IQueryable<Customer>;

            var result = new List<Customer>();
            foreach(var item in query)
            {
                item.OrdStat = "asf";
                result.Add(item);
            }

            return Ok(result.AsQueryable());
        }

        [EnableQuery]
        public IActionResult Post([FromBody]Customer person)
        {
            return Created(person);
        }
    }
}
