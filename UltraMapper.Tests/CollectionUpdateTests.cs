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

            var isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
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

            Assert.IsTrue( source.Count == target.Count );
            Assert.IsTrue( target.First().Id == 13 );
            Assert.IsTrue( target.First().Drawings.First().Id == 11 );
            Assert.IsTrue( Object.ReferenceEquals( targetMedia, target.First() ) );
            Assert.IsTrue( Object.ReferenceEquals( targetDrawing, target.First().Drawings.First() ) );
        }

        [TestMethod]
        public void ParentTypeConfigurationDirectMultipleNestedCollectionsUpdate()
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
            var targetDrawing = target.First().Drawings.First();

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<ICollection<Media>, ICollection<Media>, Media, Media>(
                    ( itemA, itemB ) => itemA.HashCode == itemB.HashCode );

                cfg.MapTypes<ICollection<Drawing>, ICollection<Drawing>, Drawing, Drawing>(
                    ( itemA, itemB ) => itemA.Data == itemB.Data );
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( source.Count == target.Count );
            Assert.IsTrue( target.First().Id == 13 );
            Assert.IsTrue( target.First().Drawings.First().Id == 11 );
            Assert.IsTrue( Object.ReferenceEquals( targetMedia, target.First() ) );
            Assert.IsTrue( Object.ReferenceEquals( targetDrawing, target.First().Drawings.First() ) );
        }

        [TestMethod]
        public void UpdateCapacityRemovingItems()
        {
            var source = new List<Media>
            {
                new Media()
                {
                    HashCode = "a",
                    Id = 13
                }
            };

            var target = new List<Media>
            {
                new Media() { HashCode = "a", Id = 17 },
                new Media() { HashCode = "b", Id = 18 },
                new Media() { HashCode = "c", Id = 19 },
                new Media() { HashCode = "d", Id = 20 },
                new Media() { HashCode = "e", Id = 21 },
                new Media() { HashCode = "f", Id = 22 },
            };

            var targetMedia = target.First();

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<List<Media>, List<Media>, Media, Media>(
                        ( itemA, itemB ) => itemA.HashCode == itemB.HashCode )
                   .IgnoreSourceMember( s => s.Capacity );

                cfg.MapTypes<Media, Media>()
                    .MapMember( s => s.Drawings, t => t.Drawings,
                        ( itemA, itemB ) => itemA.Id == itemB.Id );
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( source.Count == target.Count );
            Assert.IsTrue( target.First().Id == 13 );
            Assert.IsTrue( Object.ReferenceEquals( targetMedia, target.First() ) );
        }

        [TestMethod]
        public void UpdateCapacityRemovingItems2()
        {
            //In this test the collection update removes elements from the target.
            //This works if the capacity of the target list is updated AFTER adding elements.

            var source = new Media()
            {
                HashCode = "a",
                Id = 13,
                Drawings = new List<Drawing>()
                {
                    new Drawing() { Id=18, Data="18" }
                }
            };

            var target = new Media()
            {
                HashCode = "a",
                Id = 17,
                Drawings = new List<Drawing>()
                {
                    new Drawing() { Id=1, Data="1" },
                    new Drawing() { Id=2, Data="2" },
                    new Drawing() { Id=3, Data="3" },
                    new Drawing() { Id=4, Data="4" },
                    new Drawing() { Id=5, Data="5" },
                    new Drawing() { Id=6, Data="6" },
                    new Drawing() { Id=7, Data="7" },
                    new Drawing() { Id=8, Data="8" }
                }
            };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<List<Drawing>, List<Drawing>>()
                    .IgnoreSourceMember( a => a.Capacity );

                cfg.MapTypes<Media, Media>()
                    .MapMember( s => s.Drawings, t => t.Drawings, ( itemA, itemB ) => itemA.Id == itemB.Id );
            } );

            ultraMapper.Map( source, target );

            var result = ultraMapper.VerifyMapperResult( source, target );

            Assert.IsTrue( result );
            Assert.IsTrue( source.Drawings.Count == target.Drawings.Count );
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
