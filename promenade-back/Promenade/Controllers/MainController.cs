using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Promenade.Models;
using Promenade.Models.Web.Request;
using Promenade.Models.Web.Response;
using Promenade.Services;
using Promenade.Services.Abstract;

namespace Promenade.Controllers
{
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly ISocialService _socialService;
        private readonly ConcurrencyService _concurrencyService;
        private readonly ILogger<MainController> _logger;
        private readonly GeoService _geoService;

        public MainController(
            ISocialService socialService,
            ConcurrencyService concurrencyService,
            ILogger<MainController> logger,
            GeoService geoService
        )
        {
            _socialService = socialService;
            _concurrencyService = concurrencyService;
            _logger = logger;
            _geoService = geoService;
        }
        
        [HttpGet("/test")]
        public ContentResult Test()
        {
            return new ContentResult
            {
                ContentType = "text/html",
                Content = "<h1>It works!</h1>"
            };
        }

        [HttpPost("/init")]
        public Task Init()
        {
            return HandleRequest<BaseRequest, State>(r => _geoService.GetState(r.UserId));
        }

        [HttpPost("/find")]
        public Task Find()
        {
            return HandleRequest<FindRequest, State>(r => _geoService.Find(r.UserId, r.Lat, r.Lng, r.RangeId));
        }

        [HttpPost("/move")]
        public Task Move()
        {
            return HandleRequest<CoordinatesRequest, State>(r => _geoService.Move(r.UserId, r.Lat, r.Lng));
        }
        
        [HttpPost("/stop")]
        public Task Stop()
        {
            return HandleRequest<BaseRequest, State>(r => _geoService.Stop(r.UserId));
        }

        [HttpPost("/toggle")]
        public Task Toggle()
        {
            return HandleRequest<CategoryIdRequest, State>(r => _geoService.ToggleCategory(r.UserId, r.CategoryId));
        }

        private Task HandleRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler, bool limitRatio = false) where TRequest : BaseRequest
        {
            // setup response
            Response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            
            // load request body
            using var reader = new StreamReader(Request.Body);
            var body = reader.ReadToEnd();
            
            _logger.LogInformation($"REQUEST {typeof(TRequest)}:\n{body}");
            TRequest request;

            try
            {
                request = JsonConvert.DeserializeObject<TRequest>(body, Utils.ConverterSettings);
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Invalid request: {e.Message}");
                Response.StatusCode = 400;
                return Response.WriteAsync(new ErrorResponse($"Invalid request: {e.Message}").ToString());
            }

            // check sign
            if (request == null || !_socialService.IsSignValid(request.UserId, request.Params, request.Sign))
            {
                _logger.LogWarning("Signature calculation failed");
                Response.StatusCode = 400;
                return Response.WriteAsync(new ErrorResponse("Signature calculation failed").ToString());
            }
            
            // check requests ratio
            if (limitRatio)
            {
                if (!_concurrencyService.CheckAddRequest(request.UserId))
                {
                    _logger.LogWarning("Too many requests");
                    Response.StatusCode = 400;
                    return Response.WriteAsync(
                        new ErrorResponse("Вы вызываете запросы слишком часто, попробуйте через несколько секунд")
                            .ToString()
                    );
                }
            }
            
            // handle request
            try
            {
                var response = handler(request);
                var stringResponse = JsonConvert.SerializeObject(response, Utils.ConverterSettings);

                _logger.LogInformation($"RESPONSE:\n{stringResponse.SafeSubstring(300)}");
                return Response.WriteAsync(stringResponse);
            }
            catch (Exception e)
            {
#if DEBUG
                throw e;
#endif
                _logger.LogWarning(e.Message);
                _logger.LogWarning(e.StackTrace);
                Response.StatusCode = 400;
                return Response.WriteAsync(new ErrorResponse(e.Message).ToString());
            }
        }
    }
}