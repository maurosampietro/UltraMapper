using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace UltraMapper.Tests
{
    [TestClass]
    public class CollectionUpdates
    {
        private class Case
        {
            public int Id { get; set; }
            public string CaseId { get; set; }

            public virtual ICollection<Media> Media { get; set; }
        }

        private class Case2
        {
            public int Id { get; set; }
            public string CaseId { get; set; }

            public virtual ICollection<Media2> Media { get; set; }
        }

        private class Media
        {
            public int Id { get; set; }
            public string HashCode { get; set; }

            public virtual ICollection<Drawing> Drawings { get; set; }
        }

        private class Media2
        {
            public int Id { get; set; }
            public string HashCode { get; set; }

            public virtual ICollection<Drawing2> Drawings { get; set; }
        }

        private class Drawing
        {
            public int Id { get; set; }
            public virtual Media Image { get; set; }
            public string Data { get; set; }
        }

        private class Drawing2
        {
            public int Id { get; set; }
            public virtual Media2 Image { get; set; }
            public string Data { get; set; }
        }

        [TestMethod]
        public void MultipleNestedCollectionsUpdate()
        {
            var source = new Case()
            {
                Media = new Collection<Media>
                {
                    new Media()
                    {
                        HashCode = "a",
                        Id = 13,
                        Drawings = new BindingList<Drawing>()
                        {
                            new Drawing() { Id=11, Data="bo" }
                        }
                    },
                }
            };

            var target = new Case()
            {
                Media = new Collection<Media>
                {
                    new Media()
                    {
                        HashCode ="a",
                        Id =17,
                        Drawings = new BindingList<Drawing>()
                        {
                            new Drawing() { Id=19, Data="bo" }
                        }
                    }
                }
            };

            var targetMedia = target.Media.First();
            var targetDrawing = targetMedia.Drawings.First();

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<Case, Case>()
                    .MapMember( a => a.Media, b => b.Media, ( itemA, itemB ) => itemA.HashCode == itemB.HashCode );

                cfg.MapTypes<Media, Media>()
                    .MapMember( a => a.Drawings, b => b.Drawings, ( itemA, itemB ) => itemA.Data == itemB.Data );
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( target.Media.First().Id == 13 );
            Assert.IsTrue( target.Media.First().Drawings.First().Id == 11 );
            Assert.IsTrue( Object.ReferenceEquals( targetMedia, target.Media.First() ) );
            Assert.IsTrue( Object.ReferenceEquals( targetDrawing, target.Media.First().Drawings.First() ) );
        }

        [TestMethod]
        public void DirectMultipleNestedCollectionsUpdate()
        {
            var source = new Collection<Media>
            {
                new Media()
                {
                    HashCode = "a",
                    Id = 13,
                    Drawings = new Collection<Drawing>()
                    {
                        new Drawing() { Id=11, Data="bo" }
                    }
                }
            };

            var target = new Collection<Media>
            {
                new Media()
                {
                    HashCode ="a",
                    Id =17,
                    Drawings = new Collection<Drawing>()
                    {
                        new Drawing() { Id=19, Data="bo" }
                    }
                }
            };

            var targetMedia = target.First();
            var targetDrawing = targetMedia.Drawings.First();

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<Collection<Media>, Collection<Media>, Media, Media>(
                    ( itemA, itemB ) => itemA.HashCode == itemB.HashCode );

                cfg.MapTypes<ICollection<Drawing>, ICollection<Drawing>, Drawing, Drawing>(
                    ( itemA, itemB ) => itemA.Data == itemB.Data );
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( target.First().Id == 13 );
            Assert.IsTrue( target.First().Drawings.First().Id == 11 );
            Assert.IsTrue( Object.ReferenceEquals( targetMedia, target.First() ) );
            Assert.IsTrue( Object.ReferenceEquals( targetDrawing, target.First().Drawings.First() ) );
        }

        [TestMethod]
        public void MultipleNestedCollectionsUpdateDifferentTypes()
        {
            var source = new Case()
            {
                Media = new Collection<Media>
                {
                    new Media()
                    {
                        HashCode = "a",
                        Id = 13,
                        Drawings = new BindingList<Drawing>()
                        {
                            new Drawing() { Id=11, Data="bo" }
                        }
                    }
                }
            };

            var target = new Case2()
            {
                Media = new Collection<Media2>
                {
                    new Media2()
                    {
                        HashCode ="a",
                        Id =17,
                        Drawings = new BindingList<Drawing2>()
                        {
                            new Drawing2() { Id=19, Data="bo" }
                        }
                    }
                }
            };

            var targetMedia = target.Media.First();
            var targetDrawing = targetMedia.Drawings.First();

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<Case, Case2>()
                    .MapMember( a => a.Media, b => b.Media, ( itemA, itemB ) => itemA.HashCode == itemB.HashCode );

                cfg.MapTypes<Media, Media2>()
                    .MapMember( a => a.Drawings, b => b.Drawings, ( itemA, itemB ) => itemA.Data == itemB.Data );
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( target.Media.First().Id == 13 );
            Assert.IsTrue( target.Media.First().Drawings.First().Id == 11 );
            Assert.IsTrue( Object.ReferenceEquals( targetMedia, target.Media.First() ) );
            Assert.IsTrue( Object.ReferenceEquals( targetDrawing, target.Media.First().Drawings.First() ) );
        }   
    }
}
