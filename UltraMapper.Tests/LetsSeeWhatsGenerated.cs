using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Tests
{
    [TestClass]
    public class LetsSeeWhatsGenerated
    {
        public class SpotifyAlbum
        {
            public string AlbumType { get; set; }
            public Artist[] Artists { get; set; }
            public string[] AvailableMarkets { get; set; }
            public Copyright[] Copyrights { get; set; }
            public ExternalIds ExternalIds { get; set; }
            public ExternalUrls ExternalUrls { get; set; }
            public object[] Genres { get; set; }
            public string Href { get; set; }
            public string Id { get; set; }
            public Image[] Images { get; set; }
            public string Name { get; set; }
            public long Popularity { get; set; }
            public string ReleaseDate { get; set; }
            public string ReleaseDatePrecision { get; set; }
            public Tracks Tracks { get; set; }
            public string Type { get; set; }
            public string Uri { get; set; }
        }

        public class Tracks
        {
            public string Href { get; set; }
            public Item[] Items { get; set; }
            public long Limit { get; set; }
            public object Next { get; set; }
            public long Offset { get; set; }
            public object Previous { get; set; }
            public long Total { get; set; }
        }

        public class Item
        {
            public Artist[] Artists { get; set; }
            public string[] AvailableMarkets { get; set; }
            public long DiscNumber { get; set; }
            public long DurationMs { get; set; }
            public bool Explicit { get; set; }
            public ExternalUrls ExternalUrls { get; set; }
            public string Href { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string PreviewUrl { get; set; }
            public long TrackNumber { get; set; }
            public string Type { get; set; }
            public string Uri { get; set; }
        }

        public class Image
        {
            public long Height { get; set; }
            public string Url { get; set; }
            public long Width { get; set; }
        }

        public class ExternalIds
        {
            public string Upc { get; set; }
        }

        public class Copyright
        {
            public string Text { get; set; }
            public string Type { get; set; }
        }

        public class Artist
        {
            public ExternalUrls ExternalUrls { get; set; }
            public string Href { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Uri { get; set; }
        }

        public class ExternalUrls
        {
            public string Spotify { get; set; }
        }

        #region DTO


        public partial class SpotifyAlbumDto
        {
            [JsonProperty( "album_type" )]
            public string AlbumType { get; set; }

            [JsonProperty( "artists" )]
            public ArtistDto[] Artists { get; set; }

            [JsonProperty( "available_markets" )]
            public string[] AvailableMarkets { get; set; }

            [JsonProperty( "copyrights" )]
            public CopyrightDto[] Copyrights { get; set; }

            [JsonProperty( "external_ids" )]
            public ExternalIdsDto ExternalIds { get; set; }

            [JsonProperty( "external_urls" )]
            public ExternalUrlsDto ExternalUrls { get; set; }

            [JsonProperty( "genres" )]
            public object[] Genres { get; set; }

            [JsonProperty( "href" )]
            public string Href { get; set; }

            [JsonProperty( "id" )]
            public string Id { get; set; }

            [JsonProperty( "images" )]
            public ImageDto[] Images { get; set; }

            [JsonProperty( "name" )]
            public string Name { get; set; }

            [JsonProperty( "popularity" )]
            public long Popularity { get; set; }

            [JsonProperty( "release_date" )]
            public string ReleaseDate { get; set; }

            [JsonProperty( "release_date_precision" )]
            public string ReleaseDatePrecision { get; set; }

            [JsonProperty( "tracks" )]
            public TracksDto Tracks { get; set; }

            [JsonProperty( "type" )]
            public string Type { get; set; }

            [JsonProperty( "uri" )]
            public string Uri { get; set; }
        }

        public class TracksDto
        {
            [JsonProperty( "href" )]
            public string Href { get; set; }

            [JsonProperty( "items" )]
            public ItemDto[] Items { get; set; }

            [JsonProperty( "limit" )]
            public long Limit { get; set; }

            [JsonProperty( "next" )]
            public object Next { get; set; }

            [JsonProperty( "offset" )]
            public long Offset { get; set; }

            [JsonProperty( "previous" )]
            public object Previous { get; set; }

            [JsonProperty( "total" )]
            public long Total { get; set; }
        }

        public class ItemDto
        {
            [JsonProperty( "artists" )]
            public ArtistDto[] Artists { get; set; }

            [JsonProperty( "available_markets" )]
            public string[] AvailableMarkets { get; set; }

            [JsonProperty( "disc_number" )]
            public long DiscNumber { get; set; }

            [JsonProperty( "duration_ms" )]
            public long DurationMs { get; set; }

            [JsonProperty( "explicit" )]
            public bool Explicit { get; set; }

            [JsonProperty( "external_urls" )]
            public ExternalUrlsDto ExternalUrls { get; set; }

            [JsonProperty( "href" )]
            public string Href { get; set; }

            [JsonProperty( "id" )]
            public string Id { get; set; }

            [JsonProperty( "name" )]
            public string Name { get; set; }

            [JsonProperty( "preview_url" )]
            public string PreviewUrl { get; set; }

            [JsonProperty( "track_number" )]
            public long TrackNumber { get; set; }

            [JsonProperty( "type" )]
            public string Type { get; set; }

            [JsonProperty( "uri" )]
            public string Uri { get; set; }
        }

        public class ImageDto
        {
            [JsonProperty( "height" )]
            public long Height { get; set; }

            [JsonProperty( "url" )]
            public string Url { get; set; }

            [JsonProperty( "width" )]
            public long Width { get; set; }
        }

        public class ExternalIdsDto
        {
            [JsonProperty( "upc" )]
            public string Upc { get; set; }
        }

        public class CopyrightDto
        {
            [JsonProperty( "text" )]
            public string Text { get; set; }

            [JsonProperty( "type" )]
            public string Type { get; set; }
        }

        public class ArtistDto
        {
            [JsonProperty( "external_urls" )]
            public ExternalUrlsDto ExternalUrls { get; set; }

            [JsonProperty( "href" )]
            public string Href { get; set; }

            [JsonProperty( "id" )]
            public string Id { get; set; }

            [JsonProperty( "name" )]
            public string Name { get; set; }

            [JsonProperty( "type" )]
            public string Type { get; set; }

            [JsonProperty( "uri" )]
            public string Uri { get; set; }
        }

        public class ExternalUrlsDto
        {
            [JsonProperty( "spotify" )]
            public string Spotify { get; set; }
        }

        public partial class SpotifyAlbumDto
        {
            public static SpotifyAlbumDto FromJson( string json )
            {
                return JsonConvert.DeserializeObject<SpotifyAlbumDto>( json, Converter.Settings );
            }
        }

        public class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None
            };
        }
        #endregion

        [TestMethod]
        public void LookAtGeneratedCodeForPossiblePerformanceBoosts()
        {
            var json = File.ReadAllText( "spotifyAlbum.json" );
            var spotifyAlbumDto = SpotifyAlbumDto.FromJson( json );

            var ultramapper = new UltraMapper.Mapper( cfg => { cfg.IsReferenceTrackingEnabled = false; } );
            ultramapper.Map<SpotifyAlbum>( spotifyAlbumDto );


            //Automapper Configuration 
            var mapperConfig = new MapperConfiguration( cfg =>
            {
                cfg.CreateMap<SpotifyAlbumDto, SpotifyAlbum>();
                cfg.CreateMap<CopyrightDto, Copyright>();
                cfg.CreateMap<ArtistDto, Artist>();
                cfg.CreateMap<ExternalIdsDto, ExternalIds>();
                cfg.CreateMap<ExternalUrlsDto, ExternalUrls>();
                cfg.CreateMap<TracksDto, Tracks>();
                cfg.CreateMap<ImageDto, Image>();
                cfg.CreateMap<ItemDto, Item>();
                cfg.CreateMap<SpotifyAlbum, SpotifyAlbumDto>();
                cfg.CreateMap<Copyright, CopyrightDto>();
                cfg.CreateMap<Artist, ArtistDto>();
                cfg.CreateMap<ExternalIds, ExternalIdsDto>();
                cfg.CreateMap<ExternalUrls, ExternalUrlsDto>();
                cfg.CreateMap<Tracks, TracksDto>();
                cfg.CreateMap<Image, ImageDto>();
                cfg.CreateMap<Item, ItemDto>();
            } );
            var autoMapper = mapperConfig.CreateMapper();
            var result = autoMapper.Map<SpotifyAlbum>( spotifyAlbumDto );
        }
    }
}
