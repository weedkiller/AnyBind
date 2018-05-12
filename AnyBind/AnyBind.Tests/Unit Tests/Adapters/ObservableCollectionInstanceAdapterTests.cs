﻿using AnyBind.Adapters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests.Unit_Tests.Adapters
{
    public class ObservableCollectionInstanceAdapterTests
    {
        ObservableCollection<string> Object = new ObservableCollection<string>();
        private ObservableCollectionInstanceAdapter<string> Adapter;

        public ObservableCollectionInstanceAdapterTests()
        {
            Adapter = new ObservableCollectionInstanceAdapter<string>(Object);
        }

        [Theory]
        [InlineData()]
        [InlineData("One")]
        [InlineData("One", "Count*")]
        [InlineData("Count*")]
        [InlineData("Count*", "One", "[]*")]
        [InlineData("[FooBar]")]
        [InlineData("[1]*", "[2]*")]
        [InlineData("[1]*", "[2]*", "[Foo]", "Bar")]
        public void NotifyPropertyChangedInstanceAdapter_Success_SubscribeProperties(params string[] propertyNames)
        {
            (string, bool) stringIncluded(string str)
            {
                if (str.EndsWith("*"))
                    return (str.Substring(0, str.Length - 1), true);
                return (str, false);
            }

            //Arrange
            var input = propertyNames.Select(prop => stringIncluded(prop).Item1);
            var expectedOutput = propertyNames.Select(prop => stringIncluded(prop))
                .Where(obj => obj.Item2)
                .Select(obj => obj.Item1);
            
            // Act
            var result = Adapter.SubscribeToProperties(input.ToArray());

            // Assert
            Assert.True(result.SequenceEqual(expectedOutput));
        }

        [Fact]
        public void NotifyPropertyChangedInstanceAdapter_EventsRaised_Add()
        {
            // Arrange
            Dictionary<string, int> raiseCounts = new Dictionary<string, int>()
            { ["Count"] = 0, ["[]"] = 0, ["[0]"] = 0, ["[00]"] = 0, ["[1]"] = 0 };
            Adapter.PropertyChanged += (s, e) =>
            {
                if (raiseCounts.ContainsKey(e.PropertyName))
                    raiseCounts[e.PropertyName]++;
            };

            foreach(var prop in raiseCounts.Keys)
            {
                Adapter.SubscribeToProperties(prop);
            }

            // Act
            Object.Add("Test1");

            // Assert
            Assert.Equal(expected: 1, actual: raiseCounts["Count"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[0]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[00]"]);
            Assert.Equal(expected: 0, actual: raiseCounts["[1]"]);
        }
    }
}