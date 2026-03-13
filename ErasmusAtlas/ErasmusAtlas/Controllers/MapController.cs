using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.ViewModels.MapViewModels;

namespace ErasmusAtlas.Controllers;

public class MapController(
    IMapService mapService) 
    : Controller
{
    [HttpGet]
    [Authorize]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> PostsMarkers(
        string? bbox,
        int? cityId,
        string? topic,
        int precision = 3,
        int maxTotal = 1500,
        int maxPerMarker = 25,
        int bodyMaxLen = 0)
    {
        var request = new PostMapMarkersRequestViewModel
        {
            BoundingBox = bbox,
            CityId = cityId,
            Topic = topic,
            CoordinateRoundingPrecision = precision,
            MaxPostsReturnedTotal = maxTotal,
            MaxPostsReturnedPerMarker = maxPerMarker,
            PostBodyMaxLength = bodyMaxLen
        };

        var response = await mapService.GetPostMapMarkersAsync(request);
        return Json(response);
    }
}
