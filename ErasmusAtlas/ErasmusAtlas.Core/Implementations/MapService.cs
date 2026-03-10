using System.Globalization;

using NetTopologySuite;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore;

using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.ViewModels.MapViewModels;
using ErasmusAtlas.Infrastructure.Repository.Interfaces;

namespace ErasmusAtlas.Core.Implementations;

public class MapService(
    IRepository<Post, Guid> postsRepository)
    : IMapService
{
    public async Task<PostMapMarkersResponseViewModel> GetPostMapMarkersAsync(PostMapMarkersRequestViewModel request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        int precision = Math.Clamp(request.CoordinateRoundingPrecision, 2, 6);
        int maxTotal = Math.Clamp(request.MaxPostsReturnedTotal, 1, 5000);
        int maxPerMarker = Math.Clamp(request.MaxPostsReturnedPerMarker, 1, 200);

        var query = postsRepository
            .GetAllAttached()
            .Where(p => p.Location != null);

        if (request.CityId.HasValue)
        {
            query = query.Where(p => p.CityId == request.CityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Topic))
        {
            query = query.Where(p =>
                p.PostTopics.Any(pt => pt.Topic.Name == request.Topic));
        }

        if (!string.IsNullOrWhiteSpace(request.BoundingBox) &&
            TryParseBoundingBox(request.BoundingBox, out var bbox))
        {
            var gf = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var polygon = CreateBboxPolygon(gf, bbox.minLng, bbox.minLat, bbox.maxLng, bbox.maxLat);

            query = query.Where(p => p.Location!.Intersects(polygon));
        }

        var raw = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Body,
                p.CreatedAt,
                Lat = p.Location!.Y,
                Lng = p.Location!.X,
                Topics = p.PostTopics
                    .Select(pt => pt.Topic.Name)
                    .OrderBy(t => t)
                    .ToList()
            })
            .Take(maxTotal)
            .ToListAsync();

        var items = raw
            .GroupBy(x => new
            {
                Lat = Math.Round(x.Lat, precision),
                Lng = Math.Round(x.Lng, precision)
            })
            .Select(g => new PostMapMarkerViewModel
            {
                Lat = g.Key.Lat,
                Lng = g.Key.Lng,
                Posts = g.Take(maxPerMarker)
                    .Select(p => new PostMapMarkerPostViewModel
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Body = request.PostBodyMaxLength > 0
                            ? TrimToLength(p.Body, request.PostBodyMaxLength)
                            : null,
                        Topics = p.Topics,
                        CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    })
                    .ToList()
            })
            .ToList();

        return new PostMapMarkersResponseViewModel
        {
            Items = items
        };
    }

    private static string? TrimToLength(string? value, int maxLen)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (maxLen <= 0)
        {
            return null;
        }

        return value.Length <= maxLen
            ? value
            : value[..maxLen];
    }

    private static Polygon CreateBboxPolygon(
        GeometryFactory gf,
        double minLng,
        double minLat,
        double maxLng,
        double maxLat)
    {
        var coords = new[]
        {
            new Coordinate(minLng, minLat),
            new Coordinate(maxLng, minLat),
            new Coordinate(maxLng, maxLat),
            new Coordinate(minLng, maxLat),
            new Coordinate(minLng, minLat),
        };

        return gf.CreatePolygon(coords);
    }

    private static bool TryParseBoundingBox(
        string bbox,
        out (double minLng, double minLat, double maxLng, double maxLat) result)
    {
        result = default;

        var parts = bbox.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 4)
        {
            return false;
        }

        if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var minLng))
        {
            return false;
        }

        if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var minLat))
        {
            return false;
        }

        if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var maxLng))
        {
            return false;
        }

        if (!double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var maxLat))
        {
            return false;
        }

        if (minLat < -90 || minLat > 90 || maxLat < -90 || maxLat > 90)
        {
            return false;
        }

        if (minLng < -180 || minLng > 180 || maxLng < -180 || maxLng > 180)
        {
            return false;
        }

        result = (minLng, minLat, maxLng, maxLat);
        return true;
    }
}