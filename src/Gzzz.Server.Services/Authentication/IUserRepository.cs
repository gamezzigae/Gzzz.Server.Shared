using Gzzz.Authentication;
using Gzzz.Db.DynamoDb;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gzzz.Services.Authentication;



public interface IUserRepository
{
	Task LoadAsync(in TokenClaims claims);
}


public class DefaultUserRepository : IUserRepository
{
	public Task LoadAsync(in TokenClaims claims) => Task.CompletedTask;
}
