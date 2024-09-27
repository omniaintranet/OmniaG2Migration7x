using Dapper;
using Microsoft.Extensions.Options;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Mappers;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.Migration.Models.Input.Social;
using Omnia.WebContentManagement.Models.Navigation;
using Omnia.WebContentManagement.Models.Pages;
using Omnia.WebContentManagement.Models.Social;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Omnia.Fx.Models.Social;
using System.ComponentModel.Design;
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Queries;
using Omnia.Workplace.Models.Social;
using DocumentFormat.OpenXml.Vml;
using System.Linq;

namespace Omnia.Migration.Core.Services
{
    public class UserService
    {
       

        private IdentityApiHttpClient IdentityApiHttpClient{ get; }
        public UserService(
            

             IdentityApiHttpClient identityApiHttpClient
           )
        {
           
            IdentityApiHttpClient = identityApiHttpClient;
        }


        public async Task<ItemQueryResult<IResolvedIdentity>> LoadUserIdentity()
        {

            var userFirstpage = await IdentityApiHttpClient.GetUserall(1, 5000);
            
            if (userFirstpage == null || userFirstpage.Data.Total == 0)
            {
                return null;
                throw new Exception("Can not get Identities Please check again");
                

            }
            var userall = new List<ResolvedUserIdentity>();
            userall = userFirstpage.Data.Value.ToList();

            int totalnumber = userFirstpage.Data.Total;

            int pagetotal = totalnumber / 5000;
            if (pagetotal == 1)
            {
                var userPage = await IdentityApiHttpClient.GetUserall(2, 5000);
                userall.AddRange(userPage.Data.Value);
                Console.WriteLine("Resolved " + (userPage.Data.Value.Count() + 5000).ToString());

            }
            if (pagetotal > 1)
            {
                for (int i = 2; i <= pagetotal + 1; i++)
                {
                    var userPage = await IdentityApiHttpClient.GetUserall(i, 5000);
                    userall.AddRange(userPage.Data.Value);
                    Console.WriteLine("Resolved " + (i * 5000).ToString());

                }
            }
            Console.WriteLine("Resolved done");

            IList<IResolvedIdentity> s = userall.Cast<IResolvedIdentity>().ToList();
            var a = new ItemQueryResult<IResolvedIdentity>();
            a.Items = s;

            return a;

           
        }
         public  Identity GetIdentitybyEmail(ItemQueryResult<IResolvedIdentity> Identities, string email)
        {
            foreach (ResolvedUserIdentity item in Identities.Items)
            {
                if (item.Username.Value.Text.ToLower() == email.ToLower())
                {
                    return (Identity)item;
                }
            }
            return null;
        }



       


    }
}
