using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public UserController(
			IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		[HttpGet(nameof(Me))]
		public IEnumerable<XClaim> Me()
		{
			if (_httpContextAccessor.HttpContext.User?.Claims != null)
			{
				foreach (var claim in _httpContextAccessor.HttpContext.User?.Claims)
				{
					yield return new XClaim
					{
						Type = claim.Type,
						Value = claim.Value,
						Issuer = claim.Issuer,
						OrgIssuer = claim.OriginalIssuer,
					};
				}
			}
		}

		public class XClaim
		{
			public string Type { get; set; }

			public string Value { get; set; }

			public string Issuer { get; set; }

			public string OrgIssuer { get; set; }
		}
	}
}
